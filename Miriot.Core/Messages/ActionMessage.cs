using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Miriot.Common;
using Miriot.Common.Model;

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
