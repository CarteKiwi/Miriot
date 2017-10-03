using Microsoft.Toolkit.Uwp.Services.MicrosoftGraph;
using Miriot.Common;
using Miriot.Core.Services.Interfaces;
using Miriot.Core.ViewModels.Widgets;
using System.Linq;

namespace Miriot.Controls
{
    public sealed partial class WidgetReminder: IWidgetAction
    {
        public WidgetReminder(ReminderModel widget): base(widget)
        {
            InitializeComponent();
        }

        private async void AddReminder(IntentResponse intent)
        {
            MicrosoftGraphService mgService = new MicrosoftGraphService();
            mgService.Initialize("1a383460-c136-44e4-be92-aa8a379f3265");
            var isConnected = await mgService.LoginAsync();

            var action = intent.Actions.FirstOrDefault(e => e.Triggered);

            string channel = string.Empty;

            if (action.Parameters != null && action.Parameters.Any())
                foreach (var p in action.Parameters)
                {
                    if (p.Value != null)
                    {
                        if (p.Name == "Channel")
                            channel = p.Value.OrderByDescending(e => e.Score).First().Entity;
                    }
                }

            switch (channel.ToLowerInvariant())
            {
                default:
                case "tf1":
                    
                    
                    break;
            }
        }

        public void DoAction(IntentResponse intent)
        {
            AddReminder(intent);
        }
    }
}
