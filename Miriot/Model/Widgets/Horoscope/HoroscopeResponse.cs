using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miriot.Common.Model.Widgets.Horoscope
{
    public class HoroscopeResponse
    {
        public Config config { get; set; }
        public Content[] content { get; set; }
    }

    public class Config
    {
        public string sign_id { get; set; }
        public string sign_title { get; set; }
        public string fb_description { get; set; }
        public string fb_option_title_1 { get; set; }
        public string fb_option_text_1 { get; set; }
        public string fb_option_url_1 { get; set; }
        public string fb_option_title_2 { get; set; }
        public string fb_option_text_2 { get; set; }
        public string fb_option_url_2 { get; set; }
        public Navigation[] navigation { get; set; }
    }

    public class Navigation
    {
        public string id { get; set; }
        public string title { get; set; }
        public string site_url { get; set; }
    }

    public class Content
    {
        public string day { get; set; }
        public string timestamp { get; set; }
        public string date_title { get; set; }
        public Section[] section { get; set; }
        public string intro { get; set; }
        public Decan[] decans { get; set; }
    }

    public class Section
    {
        public string section_order { get; set; }
        public string section_title { get; set; }
        public string section_content { get; set; }
        public string section_rate { get; set; }
    }

    public class Decan
    {
        public string decan_title { get; set; }
        public string decan_date { get; set; }
        public string decan_content { get; set; }
    }
}
