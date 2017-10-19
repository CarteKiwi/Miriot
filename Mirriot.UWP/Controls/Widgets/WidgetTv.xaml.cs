using GalaSoft.MvvmLight.Ioc;
using Miriot.Common;
using Miriot.Core.Services.Interfaces;
using Miriot.Core.ViewModels;
using Miriot.Core.ViewModels.Widgets;
using Miriot.JavascriptHandler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Streaming.Adaptive;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Miriot.Win10.Controls
{
    public sealed partial class WidgetTv : IWidgetAction, IWidgetExclusive
    {
        //private FFmpegInteropMSS FFmpegMSS;
        private string _urlHub;
        private string _currentChannelKey;
        private Dictionary<string, string> _cachedUrls;
        private bool _isFullscreen;

        public bool IsFullscreen
        {
            get { return _isFullscreen; }

            set
            {
                _isFullscreen = value;

                if (value)
                {
                    Grid.SetColumnSpan(this, 4);
                    Grid.SetRowSpan(this, 4);
                }
                else
                {
                    Grid.SetColumnSpan(this, 1);
                    Grid.SetRowSpan(this, 1);
                }
            }
        }

        public bool IsExclusive { get; set; }

        public WidgetTv(WidgetModel widget, Dictionary<string, string> cachedUrls) : base(widget)
        {
            InitializeComponent();

            _cachedUrls = cachedUrls ?? new Dictionary<string, string>();
        }

        public async void LoadChannel(string channelKey, string url)
        {
            _currentChannelKey = channelKey;

            if (_cachedUrls.ContainsKey(channelKey))
                try
                {
                    await LoadStream(_cachedUrls[channelKey]);
                }
                catch (Exception)
                {
                    _cachedUrls[channelKey] = null;
                    LoadFromHub(url);
                }
            else
                LoadFromHub(url);
        }

        private async Task LoadStream(string url)
        {
            //try
            //{
            //    Player.Source = new Uri(url, UriKind.Absolute);
            //}
            //catch (Exception ex)
            //{

            //}
            var result = await AdaptiveMediaSource.CreateFromUriAsync(new Uri(url, UriKind.Absolute));
            if (result.Status == AdaptiveMediaSourceCreationStatus.Success)
            {
                var astream = result.MediaSource;
                Player.SetMediaStreamSource(astream);
            }
            else
            {
                throw new Exception("TV token expired");
            }
        }


        private void LoadFFmpeg()
        {
            try

            {

                // Read toggle switches states and use them to setup FFmpeg MSS

                //bool forceDecodeAudio = toggleSwitchAudioDecode.IsOn;

                //bool forceDecodeVideo = toggleSwitchVideoDecode.IsOn;



                // Set FFmpeg specific options. List of options can be found in https://www.ffmpeg.org/ffmpeg-protocols.html

                PropertySet options = new PropertySet();



                // Below are some sample options that you can set to configure RTSP streaming

                // options.Add("rtsp_flags", "prefer_tcp");

                // options.Add("stimeout", 100000);

                // Instantiate FFmpegInteropMSS using the URI
                Player.Stop();

                //FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromUri(uri, forceDecodeAudio, forceDecodeVideo, options);

                //if (FFmpegMSS != null)
                //{
                //    MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();

                //    if (mss != null)
                //    {
                //        // Pass MediaStreamSource to Media Element
                //        Player.SetMediaStreamSource(mss);

                //        // Close control panel after opening media
                //        Splitter.IsPaneOpen = false;
                //    }
                //    else
                //    {
                //        Debug.WriteLine("Cannot open media");
                //    }
                //}
                //else
                //{
                //    Debug.WriteLine("Cannot open media");
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void LoadFromHub(string url)
        {
            try
            {
                TvView.NavigationStarting += TvView_NavigationStarting;
                TvView.NavigationCompleted += TvView_NavigationCompleted;
                TvView.Source = new Uri(url, UriKind.Absolute);
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        private async void TvView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            TvView.NavigationCompleted -= TvView_NavigationCompleted;

            // Get HTML Content
            var html = await TvView.InvokeScriptAsync("eval", new[] { "document.documentElement.innerHTML" });

            // Retrieve video source
            var splitted = html.Split(new[] { "iframe src=" }, StringSplitOptions.RemoveEmptyEntries);
            var last = splitted[1].Trim().Split('"');
            var source = last[1];

            TvView.Visibility = Visibility.Collapsed;
            TvView.Stop();
            MainGrid.Children.Remove(TvView);
            GC.Collect();
            TvView.Source = new Uri("http://Miriot.Win10.suismoi.fr", UriKind.Absolute);

            if (_cachedUrls.ContainsKey(_currentChannelKey))
                _cachedUrls[_currentChannelKey] = source;
            else
                _cachedUrls.Add(_currentChannelKey, source);

            try
            {
                var vm = SimpleIoc.Default.GetInstance<MainViewModel>();
                vm.User.UserData.CachedTvUrls = _cachedUrls;
                Task.Run(async () => await vm.UpdateUserAsync());
            }
            catch (Exception)
            {
                // To be removed
            }

            LoadStream(source);
        }

        private void TvView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            TvView.NavigationStarting -= TvView_NavigationStarting;

            // WebView native object must be inserted in the OnNavigationStarting event handler
            Handler winRTObject = new Handler();

            // Expose the native WinRT object on the page's global object
            TvView.AddWebAllowedObject("NotifyApp", winRTObject);
        }

        internal void TurnOn(IntentResponse intent)
        {
            var action = intent.Actions.FirstOrDefault(e => e.Triggered);

            string channel = string.Empty;

            if (action.Parameters != null && action.Parameters.Any())
                foreach (var p in action.Parameters)
                {
                    if (p.Value != null)
                    {
                        if (p.Name == "Channel")
                            channel = p.Value.OrderByDescending(e => e.Score).First().Entity;
                    }
                }

            var channelUri = "http://streaming-hub.com/";
            string key;
            switch (channel.ToLowerInvariant())
            {
                default:
                case "tf1":
                    key = "tf1";
                    channelUri += "tf1-live";
                    break;
                case "bfm":
                case "bfm tv":
                case "bfmtv":
                    key = "bfmtv";
                    channelUri += "bfmtv-live";
                    break;
                case "m6":
                case "m six":
                    key = "m6";
                    channelUri += "m6-live";
                    break;
                case "france 2":
                case "france de":
                case "france deux":
                    key = "fr2";
                    channelUri += "france-2-live";
                    break;
                case "france 3":
                case "france trois":
                    key = "fr3";
                    channelUri += "france-3-live";
                    break;
                case "d8":
                case "des huit":
                case "d huit":
                    key = "d8";
                    channelUri += "d8-live";
                    break;
                case "w9":
                case "w neuf":
                    key = "w9";
                    channelUri += "w9-live";
                    break;
            }

            _urlHub = channelUri;
            LoadChannel(key, channelUri);
        }

        public void DoAction(IntentResponse intent)
        {
            if (intent.Intent == "TurnOnTv")
            {
                TurnOn(intent);
            }

            if (intent.Intent == "FullScreenTv")
            {
                IsFullscreen = true;
            }

            if (intent.Intent == "ReduceScreenTv")
            {
                IsFullscreen = false;
            }
        }
    }
}
