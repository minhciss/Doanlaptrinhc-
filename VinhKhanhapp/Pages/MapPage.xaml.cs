using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;
using VinhKhanhapp.Models;
using VinhKhanhapp.PageModels;

namespace VinhKhanhapp.Pages;

public partial class MapPage : ContentPage
{
    IAudioService audioService = new AudioService();

    MapPageModel ViewModel => (MapPageModel)BindingContext;

    readonly Location VinhKhanhLocation = new Location(10.7579, 106.7035);

    double currentZoom = 200;

    public MapPage(MapPageModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        MainMap.HandlerChanged += (_, _) => UpdateMap();
        MainMap.MapClicked += MainMap_MapClicked;

        viewModel.PropertyChanged += ViewModel_PropertyChanged;
        viewModel.Pois.CollectionChanged += Pois_CollectionChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MapPageModel vm)
        {
            await vm.InitializeAsync();
        }

        UpdateMap();

        // TEST AUDIO khi mở app
        if (ViewModel != null && ViewModel.Pois.Any())
        {
            await audioService.PlayPoiAsync(ViewModel.Pois.First(), "vi");
        }

        // tự phát POI gần nhất
        if (ViewModel != null)
            await ViewModel.PlayNearestPoiAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is MapPageModel vm)
        {
            vm.PropertyChanged -= ViewModel_PropertyChanged;
            vm.Pois.CollectionChanged -= Pois_CollectionChanged;
            vm.StopLocationUpdates();
        }
    }

    void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.CurrentLocation))
            MainThread.BeginInvokeOnMainThread(UpdateMap);
    }

    void Pois_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateMap);
    }

    void UpdateMap()
    {
        if (ViewModel.CurrentLocation != null)
        {
            var span = MapSpan.FromCenterAndRadius(
                ViewModel.CurrentLocation,
                Distance.FromMeters(currentZoom));

            MainMap.MoveToRegion(span);
        }
        else
        {
            var span = MapSpan.FromCenterAndRadius(
                VinhKhanhLocation,
                Distance.FromMeters(1000));

            MainMap.MoveToRegion(span);
        }

        MainMap.Pins.Clear();
        MainMap.MapElements.Clear();

        foreach (var poi in ViewModel.Pois)
        {
            var translation = poi.Translations.FirstOrDefault()
                ?? new PoiTranslation { Title = poi.Id };

            var location = new Location(poi.Lat, poi.Lng);

            var pin = new Pin
            {
                Label = $"🍜 {translation.Title}",
                Address = translation.Description,
                Location = location,
                Type = PinType.Place
            };

            // BẤM POI → PHÁT AUDIO
            pin.MarkerClicked += async (s, e) =>
            {
                await audioService.PlayPoiAsync(poi, "vi");
            };

            MainMap.Pins.Add(pin);

            // VÒNG POI
            var circle = new Circle
            {
                Center = location,
                Radius = new Distance(poi.RadiusMeters),
                StrokeColor = Colors.Orange,
                StrokeWidth = 3,
                FillColor = Color.FromRgba(255, 165, 0, 80)
            };

            MainMap.MapElements.Add(circle);
        }
    }

    void MainMap_MapClicked(object? sender, MapClickedEventArgs e)
    {
        try
        {
            var clicked = e.Location;

            Poi? nearest = null;
            double best = double.MaxValue;

            foreach (var poi in ViewModel.Pois)
            {
                var distance = Location.CalculateDistance(
                    clicked,
                    new Location(poi.Lat, poi.Lng),
                    DistanceUnits.Kilometers) * 1000.0;

                if (distance < best)
                {
                    best = distance;
                    nearest = poi;
                }
            }

            if (nearest != null && best <= 100)
            {
                ViewModel.NearestPoi = nearest;

                var translation = nearest.Translations.FirstOrDefault();

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(
                        translation?.Title ?? "POI",
                        translation?.Description ?? "",
                        "Nghe thuyết minh");

                    await audioService.PlayPoiAsync(nearest, "vi");
                });
            }
        }
        catch { }
    }

    public async void PoisCollectionView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            var poi = e.CurrentSelection?.FirstOrDefault() as Poi;
            if (poi == null)
                return;

            var loc = new Location(poi.Lat, poi.Lng);

            var span = MapSpan.FromCenterAndRadius(loc, Distance.FromMeters(120));
            MainMap.MoveToRegion(span);

            ViewModel.NearestPoi = poi;

            await audioService.PlayPoiAsync(poi, "vi");

            if (sender is CollectionView cv)
                cv.SelectedItem = null;
        }
        catch { }
    }

    public void LocateButton_Clicked(object? sender, EventArgs e)
    {
        var loc = ViewModel.CurrentLocation ?? VinhKhanhLocation;

        MainMap.MoveToRegion(
            MapSpan.FromCenterAndRadius(loc, Distance.FromMeters(150)));
    }

    public void ZoomIn_Clicked(object? sender, EventArgs e)
    {
        var center = MainMap.VisibleRegion?.Center
                     ?? ViewModel.CurrentLocation
                     ?? VinhKhanhLocation;

        currentZoom = Math.Max(30, currentZoom / 1.5);

        MainMap.MoveToRegion(
            MapSpan.FromCenterAndRadius(center, Distance.FromMeters(currentZoom)));
    }

    public void ZoomOut_Clicked(object? sender, EventArgs e)
    {
        var center = MainMap.VisibleRegion?.Center
                     ?? ViewModel.CurrentLocation
                     ?? VinhKhanhLocation;

        currentZoom = Math.Min(2000, currentZoom * 1.5);

        MainMap.MoveToRegion(
            MapSpan.FromCenterAndRadius(center, Distance.FromMeters(currentZoom)));
    }

    public void ResetMap_Clicked(object? sender, EventArgs e)
    {
        currentZoom = 400;

        MainMap.MoveToRegion(
            MapSpan.FromCenterAndRadius(
                VinhKhanhLocation,
                Distance.FromMeters(currentZoom)));
    }
}