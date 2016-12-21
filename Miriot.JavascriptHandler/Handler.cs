using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Core;

namespace Miriot.JavascriptHandler
{
    [AllowForWeb]
    public sealed class Handler
    {
        public double Ratio { get; set; }

        public event EventHandler<Object> OnProgressChanged;
        public event EventHandler<Object> OnNext;

        public void RaiseOnChanged()
        {
            OnProgressChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Use to make the progressbar of the SoundCloud widget progress
        /// </summary>
        /// <param name="ratio"></param>
        public void progressBar(double ratio)
        {
            Ratio = ratio;
            Debug.WriteLine("Called from WebView! {0}", ratio);

            var window = Windows.UI.Core.CoreWindow.GetForCurrentThread();
            var dispatcher = window.Dispatcher;

            Task.Run(async () =>
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {
                    this.RaiseOnChanged();
                })); // end m_dispatcher.RunAsync
            }); // end Task.Run
        }

        public void next()
        {
            var window = Windows.UI.Core.CoreWindow.GetForCurrentThread();
            var dispatcher = window.Dispatcher;

            Task.Run(async () =>
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {
                    OnNext?.Invoke(this, new EventArgs());
                }));
            });
        }
    }
}
