using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Devices.Sms;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Miriot.Common.Model;

namespace Miriot.Agent
{
    using System.Diagnostics;
    using Windows.ApplicationModel.Background;

    public sealed class PhoneBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral; // Note: defined at class scope so we can mark it complete inside the OnCancel() callback if we choose to support cancellation
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            //var sms = new SmsMessageReceivedTrigger(new SmsFilterRules(SmsFilterActionType.Accept));
            //SmsMessageRegistration s = SmsMessageRegistration.Register("1", new SmsFilterRules(SmsFilterActionType.Accept));
            //s.MessageReceived += (a, b) =>
            //{
            //    var text = b.TextMessage.Body;
            //};

            //BackgroundTaskBuilder task = new BackgroundTaskBuilder();
            //task.SetTrigger(sms);


            Debug.WriteLine("Background " + taskInstance.Task.Name + " Starting...");
            //
            // Associate a cancellation handler with the background task.
            //
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

            //
            // Do the background task activity.
            //
            var notif = await GetNotification();


            //
            // Provide status to application via local settings storage
            //
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            settings.Values[taskInstance.Task.TaskId.ToString()] = ToMiriotNotification(notif);

            Debug.WriteLine("Background " + taskInstance.Task.Name + ("process ran"));

            _deferral.Complete();
        }

        private void DisplayToast(UserNotification notif)
        {
            try
            {
                // Just registered for text messages
                var smsTextMessage = notif.AppInfo;

                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

                XmlNodeList stringElements = toastXml.GetElementsByTagName("text");

                stringElements.Item(0).AppendChild(toastXml.CreateTextNode(smsTextMessage.PackageFamilyName));

                stringElements.Item(1).AppendChild(toastXml.CreateTextNode(smsTextMessage.DisplayInfo.DisplayName));

                ToastNotification notification = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier().Show(notification);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error displaying toast: " + ex.Message);
            }
        }


        //
        // Handles background task cancellation.
        //
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //
            // Indicate that the background task is canceled.
            //
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            settings.Values[sender.Task.TaskId.ToString()] = "Canceled";

            Debug.WriteLine("Background " + sender.Task.Name + " Cancel Requested...");
        }

        private async Task<UserNotification> GetNotification()
        {
            // Get the listener
            UserNotificationListener listener = UserNotificationListener.Current;

            // Get the toast notifications
            IReadOnlyList<UserNotification> notifs = await listener.GetNotificationsAsync(NotificationKinds.Toast);

            return notifs.First();
            // For each notification in the platform
            foreach (UserNotification userNotification in notifs)
            {
                // If we've already displayed this notification
                //DisplayToast(userNotification);
            }


        }

        private MiriotNotification ToMiriotNotification(UserNotification notif)
        {
            NotificationBinding toastBinding = notif.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);

            if (toastBinding != null)
            {
                // And then get the text elements from the toast binding
                IReadOnlyList<AdaptiveNotificationText> textElements = toastBinding.GetTextElements();

                // Treat the first text element as the title text
                string titleText = textElements.FirstOrDefault()?.Text;

                // We'll treat all subsequent text elements as body text,
                // joining them together via newlines.
                string bodyText = string.Join("\n", textElements.Skip(1).Select(t => t.Text));

                var mn = new MiriotNotification()
                {
                    SenderName = titleText,
                    Content = bodyText
                };

                return mn;
            }

            return null;
        }
    }
}
