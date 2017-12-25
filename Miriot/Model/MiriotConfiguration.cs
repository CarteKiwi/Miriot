using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;

namespace Miriot.Common.Model
{
    public class MiriotConfiguration : ObservableObject
    {
        public MiriotConfiguration()
        {

        }

        public MiriotConfiguration(string id, string name)
        {
            DeviceIdentifier = id;
            Name = name;
            Widgets = new List<Widget>();
        }

        public int Id { get; set; }

        public string DeviceIdentifier { get; }

        private string _name;
        public string Name { get { return _name; } set { Set(ref _name, value); } }

        public List<Widget> Widgets { get; set; }

        public Guid UserId { get; set; }
    }
}
