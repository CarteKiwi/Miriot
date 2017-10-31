using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace Miriot.Agent
{
    using System.Diagnostics;
    using Windows.ApplicationModel.AppService;
    using Windows.ApplicationModel.Background;
    using Windows.Foundation.Collections;

    public sealed class SettingsBackgroundTask : IBackgroundTask
    {
        private BackgroundTaskDeferral backgroundTaskDeferral;
        private AppServiceConnection appServiceconnection;
        private String[] inventoryItems = new string[] { "Robot vacuum", "Chair" };
        private double[] inventoryPrices = new double[] { 129.99, 88.99 };

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            this.backgroundTaskDeferral = taskInstance.GetDeferral(); // Get a deferral so that the service isn't terminated.
            taskInstance.Canceled += OnTaskCanceled; // Associate a cancellation handler with the background task.

            // Retrieve the app service connection and set up a listener for incoming app service requests.
            var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            appServiceconnection = details.AppServiceConnection;
            appServiceconnection.RequestReceived += OnRequestReceived;
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // Get a deferral because we use an awaitable API below to respond to the message
            // and we don't want this call to get cancelled while we are waiting.
            var messageDeferral = args.GetDeferral();

            ValueSet message = args.Request.Message;
            ValueSet returnData = new ValueSet();

            string command = message["Command"] as string;

            switch (command)
            {
                case "Hello":
                    {
                        returnData.Add("Result", inventoryPrices[inventoryIndex.Value]);
                        returnData.Add("Status", "OK");
                        break;
                    }

                default:
                    {
                        returnData.Add("Status", "Fail: unknown command");
                        break;
                    }
            }

            await args.Request.SendResponseAsync(returnData); // Return the data to the caller.
                                                              // Complete the deferral so that the platform knows that we're done responding to the app service call.
                                                              // Note for error handling: this must be called even if SendResponseAsync() throws an exception.
            messageDeferral.Complete();

        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this.backgroundTaskDeferral != null)
            {
                // Complete the service deferral.
                this.backgroundTaskDeferral.Complete();
            }
        }

    }
}
