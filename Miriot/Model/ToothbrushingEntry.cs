using System;

namespace Miriot.Model
{
    public class ToothbrushingEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Duration { get; set; }
        public Guid UserId { get; set; }
    }
}
