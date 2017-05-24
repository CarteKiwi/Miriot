using System;
using System.Globalization;

namespace Miriot.Common.Model.Widgets.Twitter
{
    /// <summary>Twitter Timeline item.</summary>
    public class Tweet
    {
        /// <summary>Gets or sets time item was created.</summary>
        public string CreatedAt { get; set; }

        /// <summary>Gets or sets item Id.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets text of the status.</summary>
        public string Text { get; set; }

        /// <summary>Gets or sets user who posted the status.</summary>
        public TwitterUser User { get; set; }

        /// <summary>Gets the creation date</summary>
        public DateTime CreationDate
        {
            get
            {
                DateTime result;
                if (!DateTime.TryParseExact(CreatedAt, "ddd MMM dd HH:mm:ss zzzz yyyy", (IFormatProvider)CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    result = DateTime.Today;
                return result;
            }
        }
    }
}
