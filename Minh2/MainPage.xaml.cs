using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel; // for MainThread
using Minh2.Models;
using Minh2.Services;

namespace Minh2
{
    public partial class MainPage : ContentPage
    {
        private readonly POIRepository _repo = new();
        private readonly LocationService _locationService;
        private readonly GeofenceService _geofenceService;
        private readonly TtsService _ttsService;
        private POI? _currentNearest;

        public MainPage()
        {
            InitializeComponent();

            var pois = _repo.GetAll();
            PoiList.ItemsSource = pois;

            // assign readonly fields here (inside constructor) — allowed
            _location_service_init();
            _locationService = new LocationService(TimeSpan.FromSeconds(5));
            _geofenceService = new GeofenceService(pois);

            // Create TTS service instance so field is not null
            _ttsService = new TtsService();

            // wire events
            _locationService.LocationChanged += LocationService_LocationChanged;
            _geofenceService.POIEntered += GeofenceService_POIEntered;
        }

        // keep small helper but DO NOT assign readonly fields here — only used to factor code (no assignment)
        private void _location_service_init()
        {
            // placeholder for future init logic; do not assign readonly fields here
        }

        private void OnStartClicked(object sender, EventArgs e)
        {
            try
            {
                _locationService.Start();
                StatusLabel.Text = "Running";
                StartBtn.IsEnabled = false;
                StopBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error: {ex.Message}";
            }
        }

        private void OnStopClicked(object sender, EventArgs e)
        {
            _locationService.Stop();
            StatusLabel.Text = "Stopped";
            StartBtn.IsEnabled = true;
            StopBtn.IsEnabled = false;
        }

        private void LocationService_LocationChanged(object? sender, LocationChangedEventArgs e)
        {
            var loc = e.Location;
            if (loc == null) return;

            var pois = _repo.GetAll();
            var nearest = pois
                .Select(p => new { Poi = p, Distance = DistanceMeters(loc.Latitude, loc.Longitude, p.Latitude, p.Longitude) })
                .OrderBy(x => x.Distance)
                .FirstOrDefault();

            if (nearest != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _currentNearest = nearest.Poi;
                    NearestPoiLabel.Text = $"Nearest: {nearest.Poi.Name}";
                    NearestDistLabel.Text = $"{nearest.Distance:F0} m";
                    PlayPoiBtn.IsEnabled = true;
                });
            }

            // feed geofence engine (safe: loc is not null here)
            _geofenceService.OnLocation(loc);
        }

        private void GeofenceService_POIEntered(object? sender, POIEventArgs e)
        {
            var text = $"{e.Poi.Name}. {e.Poi.AudioScript}";
            _ttsService?.Enqueue(text);
        }

        private void OnPlayPoi(object sender, EventArgs e)
        {
            if (_currentNearest is not null)
            {
                var text = !string.IsNullOrWhiteSpace(_currentNearest.AudioScript)
                    ? _currentNearest.AudioScript
                    : (!string.IsNullOrWhiteSpace(_currentNearest.Description) ? _currentNearest.Description : _currentNearest.Name);
                _tts_service_enqueue_safe(text);
            }
        }

        private void _tts_service_enqueue_safe(string text)
        {
            _ttsService?.Enqueue(text);
        }

        private void PoiList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sel = e.CurrentSelection.FirstOrDefault() as POI;
            if (sel != null)
            {
                _currentNearest = sel;
                NearestPoiLabel.Text = $"Selected: {sel.Name}";
                PlayPoiBtn.IsEnabled = true;
            }
        }

        private async void OnScanQr(object sender, EventArgs e)
        {
            await DisplayAlertAsync("QR", "QR scan not implemented in POC", "OK");
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await DisplayAlertAsync("Settings", "Open CMS / Settings (not implemented)", "OK");
        }

        private static double DegreesToRadians(double deg) => deg * Math.PI / 180.0;
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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _locationService.Stop();
            _ttsService?.Dispose();
        }
    }
}
