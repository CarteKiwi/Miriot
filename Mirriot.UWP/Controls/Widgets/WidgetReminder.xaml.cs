using Miriot.Common;
using System.Linq;
using Windows.ApplicationModel.Appointments;
using Windows.UI.Xaml.Controls;
using Miriot.Common.Model;
using Miriot.Core.Services.Interfaces;

namespace Miriot.Controls
{
    public sealed partial class WidgetReminder
    {
        public WidgetReminder(IntentResponse intent)
        {
            InitializeComponent();
            AddReminder(intent);
        }

        internal void AddReminder(IntentResponse intent)
        {
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
    }
}
