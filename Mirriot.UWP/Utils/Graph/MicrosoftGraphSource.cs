using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Services.MicrosoftGraph;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Miriot.Utils.Graph
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
                    //items = (IEnumerable<T>)await MicrosoftGraphService.Instance.User.Event.GetEventsAsync(cancellationToken, pageSize);
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
                    //items = (IEnumerable<T>)await MicrosoftGraphService.Instance.User.Event.NextPageEventsAsync(cancellationToken);
                }
            }

            return items;
        }
    }

}
