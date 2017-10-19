using GalaSoft.MvvmLight.Messaging;
using Miriot.Common;

namespace Miriot.Core.Messages
{
    public class ActionMessage : MessageBase
    {
        public readonly IntentResponse Intent;

        public ActionMessage(IntentResponse intent)
        {
            Intent = intent;
        }
    }
}
