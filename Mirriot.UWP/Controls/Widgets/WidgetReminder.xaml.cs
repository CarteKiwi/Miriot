using Miriot.Common;
using Miriot.Core.ViewModels.Widgets;
using Miriot.Services;
using System.Linq;

namespace Miriot.Win10.Controls
{
    public sealed partial class WidgetReminder : IWidgetAction
    {
        public WidgetReminder(ReminderModel widget) : base(widget)
        {
            InitializeComponent();
        }

        private async void AddReminder(LuisEntity entity)
        {
            //MicrosoftGraphService mgService = new MicrosoftGraphService();
            //mgService.Initialize("1a383460-c136-44e4-be92-aa8a379f3265");
            //var isConnected = await mgService.LoginAsync();
        }

        public void DoAction(LuisResponse luis)
        {
            AddReminder(luis.Entities.OrderByDescending(e => e.Score).FirstOrDefault());
        }
    }
}
