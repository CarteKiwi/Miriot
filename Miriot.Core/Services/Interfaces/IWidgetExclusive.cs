﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miriot.Common.Model;

namespace Miriot.Core.Services.Interfaces
{
    public interface IWidgetExclusive
    {
        bool IsFullscreen { get; set; }
        bool IsExclusive { get; set; }
    }
}
