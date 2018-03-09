using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miriot.Common.Model
{
    public class Pagination
    {
        public int start_page { get; set; }
        public int items_on_page { get; set; }
        public int items_per_page { get; set; }
        public int total_result { get; set; }
    }

    public class Link
    {
        public string href { get; set; }
        public string type { get; set; }
        public string rel { get; set; }
        public bool templated { get; set; }
    }

    public class FeedPublisher
    {
        public string url { get; set; }
        public string id { get; set; }
        public string license { get; set; }
        public string name { get; set; }
    }

    public class DisplayInformations
    {
        public string direction { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public List<object> links { get; set; }
        public string color { get; set; }
        public string physical_mode { get; set; }
        public string headsign { get; set; }
        public string commercial_mode { get; set; }
        public List<object> equipments { get; set; }
        public string label { get; set; }
        public string network { get; set; }
    }

    public class Coord
    {
        public string lat { get; set; }
        public string lon { get; set; }
    }

    public class AdministrativeRegion
    {
        public string insee { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public Coord coord { get; set; }
        public string label { get; set; }
        public string id { get; set; }
        public string zip_code { get; set; }
    }

    public class StopArea
    {
        public string name { get; set; }
        public List<object> links { get; set; }
        public Coord coord { get; set; }
        public string label { get; set; }
        public List<AdministrativeRegion> administrative_regions { get; set; }
        public string timezone { get; set; }
        public string id { get; set; }
    }

    public class StopPoint
    {
        public string name { get; set; }
        public List<object> links { get; set; }
        public Coord coord { get; set; }
        public string label { get; set; }
        public List<object> equipments { get; set; }
        public List<AdministrativeRegion> administrative_regions { get; set; }
        public string id { get; set; }
        public StopArea stop_area { get; set; }
    }

    public class Direction
    {
        public string embedded_type { get; set; }
        public int quality { get; set; }
        public StopArea stop_area { get; set; }
        public string name { get; set; }
        public string id { get; set; }
    }

    public class Geojson
    {
        public string type { get; set; }
        public List<object> coordinates { get; set; }
    }

    public class Line
    {
        public string code { get; set; }
        public string name { get; set; }
        public List<object> links { get; set; }
        public string color { get; set; }
        public Geojson geojson { get; set; }
        public string text_color { get; set; }
        public string closing_time { get; set; }
        public string opening_time { get; set; }
        public string id { get; set; }
    }

    public class Route
    {
        public Direction direction { get; set; }
        public string name { get; set; }
        public List<object> links { get; set; }
        public string is_frequence { get; set; }
        public Geojson geojson { get; set; }
        public string direction_type { get; set; }
        public Line line { get; set; }
        public string id { get; set; }
    }

    public class StopDateTime
    {
        public string arrival_date_time { get; set; }
        public List<object> additional_informations { get; set; }
        public string departure_date_time { get; set; }
        public List<object> links { get; set; }
    }

    public class Departure
    {
        public DisplayInformations display_informations { get; set; }
        public StopPoint stop_point { get; set; }
        public Route route { get; set; }
        public StopDateTime stop_date_time { get; set; }
    }

    public class SncfResponse
    {
        public Pagination pagination { get; set; }
        public List<Link> links { get; set; }
        public List<object> disruptions { get; set; }
        public List<object> notes { get; set; }
        public List<FeedPublisher> feed_publishers { get; set; }
        public List<Departure> departures { get; set; }
        public List<object> exceptions { get; set; }
    }
}
