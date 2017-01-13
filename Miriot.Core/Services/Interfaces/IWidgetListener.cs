using System;

namespace Miriot.Core.Services.Interfaces
{
    public interface IWidgetListener
    {
        event EventHandler OnInfosChanged;

        void RaiseOnChanged();
    }
}
