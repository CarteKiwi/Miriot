using GalaSoft.MvvmLight.Ioc;
using Miriot.Common.Model;
using Miriot.Common.Model.Widgets;
using Miriot.Model;
using Miriot.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Miriot.Core.ViewModels.Widgets
{
    public class CalendarModel : WidgetModel<GraphUser>
    {
        public GraphUser User
        {
            get { return Model; }
            set { Model = value; }
        }

        public override string Title => "Calendrier & mails";

        public override WidgetStates State => WidgetStates.Compact;

        public override WidgetType Type => WidgetType.Calendar;

        public CalendarModel(Widget widget) : base(widget) { }

        public override async Task Load()
        {
            await base.Load();
            await Initialize();
        }

        public override void OnActivated()
        {
            if (User == null)
            {
                var dispatcher = SimpleIoc.Default.GetInstance<IDispatcherService>();
                dispatcher.Invoke(async () =>
                {
                    var remoteService = SimpleIoc.Default.GetInstance<RemoteService>();
                    var auth = SimpleIoc.Default.GetInstance<IGraphService>();

                    // Tell the mirror to display code
                    await remoteService.SendAsync(RemoteCommands.GraphService_Initialize);

                    // Redirect the user to the login page
                    await auth.AuthenticateForDeviceAsync();

                    // Tell the mirror to retrieve user
                    User = await remoteService.CommandAsync<GraphUser>(RemoteCommands.GraphService_GetUser);
                });
            }
        }

        public override async void OnDisabled()
        {
            User = null;

            try
            {
                var auth = SimpleIoc.Default.GetInstance<IGraphService>();
                await auth.LogoutAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task Initialize()
        {
            var auth = SimpleIoc.Default.GetInstance<IGraphService>();
            auth.Initialize();

            try
            {
                if (await auth.LoginAsync(true))
                {
                    User = await auth.GetUserAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
