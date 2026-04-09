using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using VinhKhanhTour.Models;
using VinhKhanhTour.Services;
using VinhKhanhTour.Utilities;
using System.Diagnostics;

namespace VinhKhanhTour.Pages;

[QueryProperty(nameof(SelectedPoiId), "poiId")]
public partial class MapPage : ContentPage
{
    private readonly NarrationEngine _narrationEngine;
    private readonly ApiService _apiService;

    private List<Poi> _poisFromApi = new();
    private bool _isTrackingLocation = false;
    private bool _isMapLoaded = false;

    private int _selectedPoiId = 0;
    private int _playingPoiId = 0;

#if ANDROID || IOS || MACCATALYST
    private Microsoft.Maui.Controls.Maps.Map VinhKhanhMap;
    private Dictionary<int, Microsoft.Maui.Controls.Maps.Circle> _poiCircles = new();
#endif

    public int SelectedPoiId
    {
        get => _selectedPoiId;
        set => _selectedPoiId = value;
    }

    public MapPage(NarrationEngine narrationEngine, ApiService apiService)
    {
        InitializeComponent();
        _narrationEngine = narrationEngine;
        _apiService = apiService;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (!_isMapLoaded)
        {
            await LoadPoisToMapAsync();
            _isMapLoaded = true;
        }

        await StartLocationTracking();
    }

    private async Task LoadPoisToMapAsync()
    {
#if ANDROID || IOS || MACCATALYST
        VinhKhanhMap = new Microsoft.Maui.Controls.Maps.Map
        {
            IsShowingUser = true,
            MapType = MapType.Street
        };

        MapContainer.Children.Add(VinhKhanhMap);

        // 🔥 LOAD API
        _poisFromApi = await _apiService.GetPoisAsync();

        foreach (var poi in _poisFromApi)
        {
            var pin = new Pin
            {
                Label = poi.Name,
                Location = new Location(poi.Latitude, poi.Longitude),
                Type = PinType.Place
            };

            VinhKhanhMap.Pins.Add(pin);

            var circle = new Microsoft.Maui.Controls.Maps.Circle
            {
                Center = new Location(poi.Latitude, poi.Longitude),
                Radius = Distance.FromMeters(poi.Radius),
                StrokeColor = Colors.Gray,
                StrokeWidth = 2,
                FillColor = Color.FromRgba(128, 128, 128, 50)
            };

            VinhKhanhMap.MapElements.Add(circle);
            _poiCircles[poi.Id] = circle;
        }
#endif
    }

    private async Task StartLocationTracking()
    {
        if (_isTrackingLocation) return;

        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
        {
            _isTrackingLocation = true;

            var timer = Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += async (s, e) => await CheckLocation();
            timer.Start();
        }
    }

    private async Task CheckLocation()
    {
        try
        {
            var location = await Geolocation.Default.GetLocationAsync();

            if (location == null) return;

            HandleLocation(location);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private async void HandleLocation(Location userLocation)
    {
        if (_poisFromApi.Count == 0) return;

        Poi? nearest = null;
        double minDistance = double.MaxValue;

        foreach (var poi in _poisFromApi)
        {
            double distance = LocationHelper.CalculateDistanceInMeters(
                userLocation.Latitude,
                userLocation.Longitude,
                poi.Latitude,
                poi.Longitude);

            if (distance <= poi.Radius && distance < minDistance)
            {
                nearest = poi;
                minDistance = distance;
            }
        }

        if (nearest != null)
        {
            if (_playingPoiId != nearest.Id)
            {
                _playingPoiId = nearest.Id;

                await _narrationEngine.PlayPoiNarrationAsync(nearest);
            }
        }
        else
        {
            _playingPoiId = 0;
        }
    }

    // Added missing Clicked event handlers referenced from MapPage.xaml
    private async void OnMyLocationClicked(object sender, EventArgs e)
    {
#if ANDROID || IOS || MACCATALYST
        try
        {
            var location = await Geolocation.Default.GetLocationAsync();
            if (location == null || VinhKhanhMap == null) return;

            VinhKhanhMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(location.Latitude, location.Longitude),
                Distance.FromMeters(500)));
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
#endif
    }

    private void OnZoomInClicked(object sender, EventArgs e)
    {
#if ANDROID || IOS || MACCATALYST
        try
        {
            if (VinhKhanhMap?.VisibleRegion == null) return;
            var center = VinhKhanhMap.VisibleRegion.Center;
            VinhKhanhMap.MoveToRegion(MapSpan.FromCenterAndRadius(center, Distance.FromMeters(500)));
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
#endif
    }

    private void OnZoomOutClicked(object sender, EventArgs e)
    {
#if ANDROID || IOS || MACCATALYST
        try
        {
            if (VinhKhanhMap?.VisibleRegion == null) return;
            var center = VinhKhanhMap.VisibleRegion.Center;
            VinhKhanhMap.MoveToRegion(MapSpan.FromCenterAndRadius(center, Distance.FromMeters(2000)));
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
#endif
    }

    private void OnPlayPauseClicked(object sender, EventArgs e)
    {
        // Simple behavior: stop current narration
        _narrationEngine.CancelCurrentNarration();
    }
}