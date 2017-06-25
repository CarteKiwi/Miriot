using Windows.UI.Xaml.Media.Imaging;

namespace Miriot.Common.Model.Widgets
{
    public class GraphUser
    {
        /// <summary>Gets or sets user Id.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets user name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets user photo.</summary>
        public BitmapImage Photo { get; set; }
    }
}
