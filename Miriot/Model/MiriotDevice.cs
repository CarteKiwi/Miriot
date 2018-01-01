using GalaSoft.MvvmLight;
using System.Collections.Generic;

namespace Miriot.Common.Model
{
    public class MiriotDevice : ObservableObject
    {
        public MiriotDevice()
        {
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public ICollection<User> Owners { get; set; }
    }
}
