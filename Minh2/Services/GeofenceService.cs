using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Minh2.Models;
using Microsoft.Maui.Devices.Sensors;

namespace Minh2.Services
{
    public class POIEventArgs : EventArgs
    {
        public POI Poi { get; set; } = null!;
        public double DistanceMeters { get; set; }
    }

    public class GeofenceService
    {
        private readonly IEnumerable<POI> _pois;
        private readonly TimeSpan _cooldown;
        private readonly TimeSpan _debounceWindow;
        private readonly Dictionary<string, DateTime> _lastTriggered = new();

        // hold last detection to debounce rapid flips
        private readonly Dictionary<string, DateTime> _lastCandidate = new();

        public event EventHandler<POIEventArgs>? POIEntered;

        public GeofenceService(IEnumerable<POI> pois, TimeSpan? cooldown = null, TimeSpan? debounceWindow = null)
        {
            _pois = pois;
            _cooldown = cooldown ?? TimeSpan.FromMinutes(2);
            _debounceWindow = debounceWindow ?? TimeSpan.FromSeconds(6);
        }

        public void OnLocation(Location location)
        {
            var now = DateTime.UtcNow;
            // compute distances
            var candidates = _pois
                .Select(p => new { Poi = p, Distance = DistanceMeters(location.Latitude, location.Longitude, p.Latitude, p.Longitude) })
                .Where(x => x.Distance <= x.Poi.RadiusMeters + 5) // small margin
                .OrderByDescending(x => x.Poi.Priority)
                .ThenBy(x => x.Distance)
                .ToList();

            if (!candidates.Any()) return;

            var top = candidates.First();
            var poi = top.Poi;
            var dist = top.Distance;

            // cooldown check
            if (_lastTriggered.TryGetValue(poi.Id, out var last) && now - last < _cooldown)
                return;

            // debounce: require stable candidate for debounce window
            if (_lastCandidate.TryGetValue(poi.Id, out var candidateAt))
            {
                if (now - candidateAt >= _debounceWindow)
                {
                    // considered entered
                    _lastTriggered[poi.Id] = now;
                    POIEntered?.Invoke(this, new POIEventArgs { Poi = poi, DistanceMeters = dist });
                    _lastCandidate.Remove(poi.Id);
                }
                // else keep waiting
            }
            else
            {
                _lastCandidate[poi.Id] = now;
            }

            // prune old candidates
            var keys = _lastCandidate.Keys.ToList();
            foreach (var k in keys)
            {
                if (now - _lastCandidate[k] > _debounceWindow * 3)
                    _lastCandidate.Remove(k);
            }
        }

        private static double DegreesToRadians(double deg) => deg * Math.PI / 180.0;

        // Haversine formula
        private static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371000.0;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}