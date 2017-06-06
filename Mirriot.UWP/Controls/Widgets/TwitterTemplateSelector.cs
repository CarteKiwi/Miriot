using Microsoft.Toolkit.Uwp.Services.Twitter;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Miriot.Controls.Widgets
{
    public class TwitterTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TweetTemplate { get; set; }

        public DataTemplate DirectMessageTemplate { get; set; }

        public DataTemplate EventTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var currentFrame = Window.Current.Content as Frame;

            if (currentFrame != null)
            {
                var currentPage = currentFrame.Content as Page;

                if (item != null && currentPage != null)
                {
                    if (item is Tweet)
                    {
                        return TweetTemplate;
                    }

                    if (item is TwitterDirectMessage)
                    {
                        return DirectMessageTemplate;
                    }

                    if (item is TwitterStreamEvent)
                    {
                        return EventTemplate;
                    }
                }
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}