using System;

namespace Minh2.Models
{
    public class POI
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusMeters { get; set; } = 50; // trigger radius
        public int Priority { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string AudioScript { get; set; } = string.Empty;
        public DateTime? LastTriggeredUtc { get; set; }
    }
}