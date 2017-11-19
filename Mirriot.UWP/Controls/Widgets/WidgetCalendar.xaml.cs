using Microsoft.Graph;
using Microsoft.Office365.OutlookServices;
using Microsoft.Toolkit.Uwp;
using Miriot.Common.Model.Widgets;
using Miriot.Core.ViewModels.Widgets;
using Miriot.Services;
using Miriot.Win10.Utils.Graph;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Miriot.Win10.Controls
{
    public sealed partial class WidgetCalendar : IWidgetOAuth, IWidgetListener
    {
        public string Token { get; set; }
        public GraphUser User { get; set; }

        private readonly CalendarModel _model;

        public WidgetCalendar(CalendarModel model) : base(model.X, model.Y)
        {
            _model = model;

            InitializeComponent();

            Loaded += WidgetCalendar_Loaded;
        }

        private async Task RetrieveData()
        {
            await _model.Load();

            User = _model.User;
        }

        public event EventHandler OnInfosChanged;

        public void RaiseOnChanged()
        {
            OnInfosChanged?.Invoke(this, new EventArgs());
        }

        private async void WidgetCalendar_Loaded(object sender, RoutedEventArgs e)
        {
            await RetrieveData();
            NotConnectedMessage.Visibility = Visibility.Collapsed;

            if (User != null)
                Init();
            else
            {
                Loader.Visibility = Visibility.Collapsed;
                NotConnectedMessage.Visibility = Visibility.Visible;
            }
        }

        private void Init()
        {
            try
            {
                GetEmails();
                GetEvents();
                //await LoadEvents();
                //await LoadMails();
            }
            catch (Exception ex)
            {
                // Something went wrong
                Debug.WriteLine(ex.Message);
                NotConnectedMessage.Visibility = Visibility.Visible;
            }
            finally
            {
                Loader.Visibility = Visibility.Collapsed;
            }
        }

        private void GetEmails()
        {
            var collection = new IncrementalLoadingCollection<MicrosoftGraphSource<Microsoft.Graph.Message>, Microsoft.Graph.Message>(
                10,
                async () =>
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Loader.Visibility = Visibility.Visible;
                    });
                },
                async () =>
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Loader.Visibility = Visibility.Collapsed;
                    });
                },
                async ex =>
                {
                    if (!Dispatcher.HasThreadAccess)
                    {
                        if (ex is ServiceException)
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                NotConnectedMessage.Visibility = Visibility.Visible;
                                Loader.Visibility = Visibility.Collapsed;
                                //await DisplayAuthorizationErrorMessageAsync(ex as ServiceException, "Read user mail");
                            });
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                });

            Mails.ItemsSource = collection;
        }

        private void GetEvents()
        {
            var collection = new IncrementalLoadingCollection<MicrosoftGraphSource<DisplayEvent>, DisplayEvent>(
                10,
                async () =>
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Loader.Visibility = Visibility.Visible;
                    });
                },
                async () =>
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Loader.Visibility = Visibility.Collapsed;
                    });
                },
                async ex =>
                {
                    if (!Dispatcher.HasThreadAccess)
                    {
                        if (ex is ServiceException)
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                NotConnectedMessage.Visibility = Visibility.Visible;
                                Loader.Visibility = Visibility.Collapsed;
                                //await DisplayAuthorizationErrorMessageAsync(ex as ServiceException, "Read user mail");
                            });
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                });

            Events.ItemsSource = collection;
        }

        private async Task LoadMails()
        {
            try
            {
                OutlookServicesClient client = new OutlookServicesClient(new Uri("https://outlook.office.com/api/v2.0"),
                   async () =>
                   {
                       await Task.Delay(0);
                       // Since we have it locally from the Session, just return it here.
                       return Token;
                   });

                var entities = await client.Me.Messages
                                    .OrderByDescending(e => e.ReceivedDateTime)
                                    .Take(10)
                                    //.Select(e => new { e.IsRead, e.Subject, e.ReceivedDateTime, e.From, e.BodyPreview, e.Body })
                                    .ExecuteAsync();

                var e2 =
                    entities.CurrentPage.Select(
                        e =>
                            new DisplayMessage(e.IsRead, e.Subject, e.ReceivedDateTime,
                                e.From == null ? string.Empty : e.From.EmailAddress.Name, e.BodyPreview, e.Body.Content));


                var es = e2.Take(3);

                Mails.ItemsSource = es.ToList();
            }
            catch (Exception ex)
            {
                Token = null;
                RaiseOnChanged();
                Debug.WriteLine(string.Format("ERROR retrieving messages: {0}", ex.Message));
            }
        }

        private async Task LoadEvents()
        {
            try
            {
                OutlookServicesClient client = new OutlookServicesClient(new Uri("https://outlook.office.com/api/v2.0"),
                   async () =>
                   {
                       await Task.Delay(0);
                       // Since we have it locally from the Session, just return it here.
                       return Token;
                   });

                var eventResults = await client.Me.Events
                                    .OrderByDescending(e => e.Start.DateTime)
                                    //.Where(e => (DateTime.Parse(e.Start.DateTime).DayOfYear >= DateTime.Now.DayOfYear))
                                    .Take(400)
                                    .Select(e => new DisplayEvent(e.Subject, e.Start.DateTime, e.End.DateTime))
                                    .ExecuteAsync();

                var currentEvents = eventResults.CurrentPage.OrderBy(e => e.Start)
                    .Where(e => e.Start.DayOfYear >= DateTime.Now.DayOfYear &&
                                e.Start.DayOfYear <= DateTime.Now.DayOfYear + 1 &&
                                e.End >= DateTime.Now);

                // Set HasBegan on passed event with active end time
                foreach (var ev in currentEvents)
                {
                    if (ev.Start.TimeOfDay <= DateTime.Now.TimeOfDay)
                        ev.HasBegan = true;
                }

                Events.ItemsSource = currentEvents.ToList();
            }
            catch (Exception ex)
            {
                Token = null;
                NotConnectedMessage.Visibility = Visibility.Visible;
                RaiseOnChanged();
                Debug.WriteLine(string.Format("ERROR retrieving messages: {0}", ex.InnerException.Message));
            }
        }

        public override void SetPosition(int x, int y)
        {
            Grid.SetColumn(this, x);
            Grid.SetRow(this, y);
            Grid.SetRowSpan(this, 2);
        }
    }

    public class DisplayEvent
    {
        public string Subject { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool HasBegan { get; set; }

        public string FriendlyStartEnd { get { return $"{Start.Hour}:{Start.Minute} - {End.Hour}:{End.Minute}"; } }

        public DisplayEvent(string subject, string start, string end)
        {
            Subject = subject;
            Start = DateTime.Parse(start);
            End = DateTime.Parse(end);
        }
    }

    public class DisplayMessage
    {
        public bool? IsRead { get; set; }
        public string Subject { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string From { get; set; }
        public string Body { get; set; }
        public string Preview { get; set; }

        public DisplayMessage(bool? isRead, string subject, DateTimeOffset? receivedDate, string from, string preview, string body)
        {
            IsRead = isRead;
            Subject = subject;
            ReceivedDate = receivedDate?.DateTime;
            From = from;
            Preview = preview;
            Body = body;
        }
    }
}
