using System;

namespace Miriot.Services
{
    public interface IWidgetListener
    {
        event EventHandler OnInfosChanged;

        void RaiseOnChanged();
    }
}
