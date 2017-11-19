using Miriot.Common;
using Miriot.Services;
using Miriot.Core.ViewModels.Widgets;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Streaming.Adaptive;

namespace Miriot.Win10.Controls
{
    public sealed partial class WidgetRadio : IWidgetAction, IWidgetExclusive
    {
        public WidgetRadio(RadioModel widget): base (widget)
        {
            InitializeComponent();
            IsExclusive = true;
        }

        private async Task ChangeChannel(string url, string title)
        {
            var result = await AdaptiveMediaSource.CreateFromUriAsync(new Uri(url, UriKind.Absolute));
            if (result.Status == AdaptiveMediaSourceCreationStatus.Success)
            {
                var astream = result.MediaSource;
                Player.SetMediaStreamSource(astream);
            }
            else
            {
                Player.Source = new Uri(url, UriKind.Absolute);
            }

            Title.Text = title;

            IconSb.Begin();
        }

        public void DoAction(IntentResponse intent)
        {
            TurnOn(intent);
        }

        private void TurnOn(IntentResponse intent)
        {
            var action = intent.Actions.FirstOrDefault(e => e.Triggered);

            string channel = string.Empty;

            if (action.Parameters != null && action.Parameters.Any())
                foreach (var p in action.Parameters)
                {
                    if (p.Value != null && p.Name == "Channel")
                        channel = p.Value.OrderByDescending(e => e.Score).First().Entity;
                }

            string channelUri;
            switch (channel)
            {
                case "nova":
                    channelUri = "http://novazz.ice.infomaniak.ch/novazz-128.mp3";
                    break;
                case "chérie fm":
                    channelUri = "http://adwzg3.scdn.arkena.com/8473/nrj_178499.mp3";
                    break;
                case "nrj":
                case "énergie":
                    channelUri = "http://mp3lg4.scdn.arkena.com/8432/nrj_205243.mp3";
                    break;
                case "rfm":
                    channelUri = "http://rfm-live-mp3-128.scdn.arkena.com/rfm.mp3";
                    break;
                case "inter":
                case "france inter":
                    channelUri = "http://audio.scdn.arkena.com/11008/franceinter-midfi128.mp3";
                    break;
                case "skyrock":
                    channelUri = "http://mp3lg3.tdf-cdn.com/4599/sky_151614.mp3";
                    break;
                default:
                    channel = "RTL";
                    channelUri = "http://streaming.radio.rtl.fr/rtl-1-48-192";
                    break;
            }

            ChangeChannel(channelUri, channel);
        }

        public bool IsFullscreen { get; set; }

        public bool IsExclusive { get; set; }
    }
}
