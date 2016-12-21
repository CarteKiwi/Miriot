using Miriot.JavascriptHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Streaming.Adaptive;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Miriot.Common;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Devices.Sms;
using Windows.Phone.Notification.Management;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Miriot.Utils;
using Miriot.Common.Model;
using Newtonsoft.Json;
using Microsoft.Practices.ServiceLocation;
using Miriot.Core.ViewModels;

namespace Miriot.Controls
{
    public sealed partial class WidgetPhone : WidgetBase
    {
        private const string BackgroundTaskEntryPoint = "Miriot.Agent.PhoneBackgroundTask";
        private const string BackgroundTaskName = "PhoneBackgroundTask";
        private CoreDispatcher sampleDispatcher;

        public WidgetPhone()
        {
            OnLoad();
        }

        // Initialize state based on currently registered background tasks
        public bool InitializeRegisteredSmsBackgroundTasks()
        {
            //
            // Initialize UI elements based on currently registered background tasks
            // and associate background task completed event handler with each background task.
            //
            //UpdateBackgroundTaskUIState(false);

            foreach (var item in BackgroundTaskRegistration.AllTasks)
            {
                IBackgroundTaskRegistration task = item.Value;
                if (task.Name == BackgroundTaskName)
                {
                    //UpdateBackgroundTaskUIState(true);
                    task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
                    return true;
                }
            }

            return false;
        }



        static IBackgroundTaskRegistration Register()
        {
            var task = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(x => x.Name == BackgroundTaskEntryPoint);
            if (task != null) return task;

            var filterRule = new SmsFilterRule(SmsMessageType.Text);
            //filterRule.SenderNumbers.Add("111111111");
            //filterRule.SenderNumbers.Add("222222222");

            var filterRules = new SmsFilterRules(SmsFilterActionType.Accept);
            filterRules.Rules.Add(filterRule);

            var taskBuilder = new BackgroundTaskBuilder();
            taskBuilder.Name = BackgroundTaskName;
            taskBuilder.TaskEntryPoint = BackgroundTaskEntryPoint;
            //taskBuilder.SetTrigger(new SmsMessageReceivedTrigger(filterRules));
            taskBuilder.SetTrigger(new UserNotificationChangedTrigger(NotificationKinds.Toast));
        
            return taskBuilder.Register();
        }

        public async void OnLoad()
        {
            bool granted = await AskRequest();

            bool taskRegistered;

            taskRegistered = InitializeRegisteredSmsBackgroundTasks();

            var task = Register();

            if (!taskRegistered)
            {

               
                task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
            }
        }

        private async Task<bool> AskRequest()
        {
            // Get the listener
            UserNotificationListener listener = UserNotificationListener.Current;

            // And request access to the user's notifications (must be called from UI thread)
            UserNotificationListenerAccessStatus accessStatus = await listener.RequestAccessAsync();

            switch (accessStatus)
            {
                // This means the user has granted access.
                case UserNotificationListenerAccessStatus.Allowed:
                    return true;
                    // Yay! Proceed as normal
                    break;

                // This means the user has denied access.
                // Any further calls to RequestAccessAsync will instantly
                // return Denied. The user must go to the Windows settings
                // and manually allow access.
                case UserNotificationListenerAccessStatus.Denied:

                    // Show UI explaining that listener features will not
                    // work until user allows access.
                    break;

                // This means the user closed the prompt without
                // selecting either allow or deny. Further calls to
                // RequestAccessAsync will show the dialog again.
                case UserNotificationListenerAccessStatus.Unspecified:

                    // Show UI that allows the user to bring up the prompt again
                    break;
            }

            return false;
        }

        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var key = task.TaskId.ToString();
            var notif = settings.Values[key] as UserNotification;


        }
    }
}
