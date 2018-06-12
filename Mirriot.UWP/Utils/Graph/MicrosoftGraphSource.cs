using Microsoft.Graph;
using Microsoft.Toolkit.Collections;
using Microsoft.Toolkit.Uwp.Services.MicrosoftGraph;
using Miriot.Win10.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Miriot.Win10.Utils.Graph
{
    public class MicrosoftGraphSource<T> : IIncrementalSource<T>
    {
        private bool isFirstCall = true;

        public async Task<IEnumerable<T>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<T> items = null;

            if (isFirstCall)
            {
                if (typeof(T) == typeof(Message))
                {
                    items = (IEnumerable<T>)await MicrosoftGraphService.Instance.User.Message.GetEmailsAsync(cancellationToken, pageSize);
                }
                else
                {
                    var events = await MicrosoftGraphService.Instance.User.Event.GetEventsAsync(cancellationToken, pageSize);

                    var currentEvents = ToDisplayEvents(events);

                    items = (IEnumerable<T>)currentEvents;
                }

                isFirstCall = false;
            }
            else
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return items;
                }

                if (typeof(T) == typeof(Message))
                {
                    items = (IEnumerable<T>)await MicrosoftGraphService.Instance.User.Message.NextPageEmailsAsync(cancellationToken);
                }
                else
                {
                    var events = await MicrosoftGraphService.Instance.User.Event.NextPageEventsAsync(cancellationToken);
                    var currentEvents = ToDisplayEvents(events);
                    items = (IEnumerable<T>)currentEvents;
                }
            }

            return items;
        }

        private IEnumerable<DisplayEvent> ToDisplayEvents(IUserEventsCollectionPage events)
        {
            var eventResults = events.Select(e => new DisplayEvent(e.Subject, e.Start.DateTime, e.End.DateTime));

            var currentEvents = eventResults.OrderBy(e => e.Start)
                .Where(e => e.Start.DayOfYear >= DateTime.Now.DayOfYear &&
                            e.Start.DayOfYear <= DateTime.Now.DayOfYear + 1 &&
                            e.End >= DateTime.Now);

            // Set HasBegan on passed event with active end time
            foreach (var ev in currentEvents)
            {
                if (ev.Start.TimeOfDay <= DateTime.Now.TimeOfDay)
                    ev.HasBegan = true;
            }

            return currentEvents;
        }
    }

}
