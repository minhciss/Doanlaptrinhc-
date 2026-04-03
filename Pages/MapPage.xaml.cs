using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using VinhKhanhTour.Models;
using VinhKhanhTour.Services;
using VinhKhanhTour.Utilities;
using System.Diagnostics;

namespace VinhKhanhTour.Pages
{
    [QueryProperty(nameof(SelectedPoiId), "poiId")]
    public partial class MapPage : ContentPage
    {
        private readonly NarrationEngine _narrationEngine;
        private readonly PoiRepository _poiRepository;
        private List<Poi> _cachedPois = new();
        private bool _isTrackingLocation = false;

        private int _selectedPoiId = 0;
        public int SelectedPoiId
        {
            get => _selectedPoiId;
            set
            {
                _selectedPoiId = value;
                if (_isMapLoaded)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        UpdateMapCirclesState();
                        CenterOnSelectedPoi();
                    });
                }
            }
        }

        private int _playingPoiId = 0;

        // ===== SIMULATION MODE =====
        private bool _isSimulationMode = false;
        private Location? _simulatedLocation;
        private const double SimStepDegrees = 0.00009; // ~10 mét mỗi lần bấm

#if ANDROID || IOS || MACCATALYST
        private Microsoft.Maui.Controls.Maps.Map VinhKhanhMap;
        private Dictionary<int, Microsoft.Maui.Controls.Maps.Circle> _poiCircles = new();
        private Pin? _simulatedPin;
#endif

        private bool _isMapLoaded = false;

        public MapPage(NarrationEngine narrationEngine, PoiRepository poiRepository)
        {
            InitializeComponent();
            _narrationEngine = narrationEngine;
            _poiRepository = poiRepository;
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            
            // OnNavigatedTo được gọi SAU KHI animation chuyển trang đã hoàn tất hoàn toàn.
            if (!_isMapLoaded)
            {
                MainThread.BeginInvokeOnMainThread(async () => 
                {
                    await LoadPoisToMapAsync();
                    _isMapLoaded = true;
                });
            }
            
            // Đợi thêm 1 chút để UI bản đồ kịp render mượt mà trước khi gọi hộp thoại Quyền GPS
            Task.Delay(500).ContinueWith(async (t) => 
            {
               await MainThread.InvokeOnMainThreadAsync(async () => 
               {
                   await StartLocationTracking();
               });
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopLocationTracking();
            _narrationEngine.CancelCurrentNarration();
        }

        private IDispatcherTimer? _locationTimer;

        private async Task StartLocationTracking()
        {
            if (_isTrackingLocation) return;
            
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
                
                // Cấp quyền Location trên Android 14 đôi lúc rắc rối,
                // chuyển sang phương pháp Polling (hỏi liên tục) thay vì Foreground Service
                if (status == PermissionStatus.Granted)
                {
                    _isTrackingLocation = true;
                    _locationTimer = Dispatcher.CreateTimer();
                    _locationTimer.Interval = TimeSpan.FromSeconds(5); // Quét mỗi 5 giây
                    _locationTimer.Tick += async (s, e) => await CheckLocation();
                    _locationTimer.Start();

#if ANDROID
                    // Start Background Service
                    if (Platform.CurrentActivity is MainActivity mainActivity)
                    {
                        mainActivity.StartLocationService();
                        VinhKhanhTour.Platforms.Android.BackgroundLocationTracker.LocationChanged += OnBackgroundLocationChanged;
                    }
#endif
                }
                else
                {
                    await DisplayAlert("Lỗi Quyền", "Không thể truy cập vị trí để tự động Thuyết minh.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi bật GPS: {ex.Message}");
            }
        }

        private void OnBackgroundLocationChanged(object? sender, Location location)
        {
            if (_isSimulationMode) return; // Bỏ qua GPS thật khi đang minh họa
            MainThread.BeginInvokeOnMainThread(() => 
            {
                Geolocation_LocationChanged(this, new GeolocationLocationChangedEventArgs(location));
            });
        }

        private void StopLocationTracking()
        {
            if (!_isTrackingLocation) return;
            _isTrackingLocation = false;

#if ANDROID
            if (Platform.CurrentActivity is MainActivity mainActivity)
            {
                mainActivity.StopLocationService();
                VinhKhanhTour.Platforms.Android.BackgroundLocationTracker.LocationChanged -= OnBackgroundLocationChanged;
            }
#endif
            
            if (_locationTimer != null)
            {
                _locationTimer.Stop();
                _locationTimer = null;
            }
        }

        private async Task CheckLocation()
        {
            if (_isSimulationMode) return; // Bỏ qua GPS thật khi đang minh họa
            try
            {
                // Bắt buộc Không sử dụng Vị trí đệm (Cache) trên Android Emulator
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5));

                // Lấy vị trí trực tiếp từ Cảm biến Máy ảo
                Location? location = await Geolocation.Default.GetLocationAsync(request);

                if (location == null)
                {
                    // Fallback nếu GetLocation xịt
                    location = await Geolocation.Default.GetLastKnownLocationAsync();
                }

                if (location != null)
                {
                    Geolocation_LocationChanged(this, new GeolocationLocationChangedEventArgs(location));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi lấy tọa độ: {ex.Message}");
            }
        }

        private Location? _currentUserLocation;

        private void OnMyLocationClicked(object sender, EventArgs e)
        {
#if ANDROID || IOS || MACCATALYST
            if (_currentUserLocation != null && VinhKhanhMap != null)
            {
                VinhKhanhMap.MoveToRegion(MapSpan.FromCenterAndRadius(_currentUserLocation, Distance.FromKilometers(0.5)));
            }
#endif
        }

        private void OnZoomInClicked(object sender, EventArgs e)
        {
#if ANDROID || IOS || MACCATALYST
            if (VinhKhanhMap != null)
            {
                var currentSpan = VinhKhanhMap.VisibleRegion;
                if (currentSpan != null)
                {
                    VinhKhanhMap.MoveToRegion(MapSpan.FromCenterAndRadius(currentSpan.Center, Distance.FromKilometers(currentSpan.Radius.Kilometers / 2)));
                }
            }
#endif
        }

        private void OnZoomOutClicked(object sender, EventArgs e)
        {
#if ANDROID || IOS || MACCATALYST
            if (VinhKhanhMap != null)
            {
                var currentSpan = VinhKhanhMap.VisibleRegion;
                if (currentSpan != null)
                {
                    VinhKhanhMap.MoveToRegion(MapSpan.FromCenterAndRadius(currentSpan.Center, Distance.FromKilometers(currentSpan.Radius.Kilometers * 2)));
                }
            }
#endif
        }

        private bool _isPlaying = true;
        private void OnPlayPauseClicked(object sender, EventArgs e)
        {
            _isPlaying = !_isPlaying;
            UpdatePlayPauseButton(true);
        }

        private void UpdatePlayPauseButton(bool triggerCancelOnPause = false)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PlayPauseButton.Text = _isPlaying ? "⏸" : "▶";
            });
            
            if (triggerCancelOnPause && !_isPlaying)
            {
                _narrationEngine.CancelCurrentNarration();
            }
        }

        private void Geolocation_LocationChanged(object sender, GeolocationLocationChangedEventArgs e)
        {
            _currentUserLocation = e.Location;
            
            // Xử lý tính toán ngầm trên CPU khác, không làm treo giao diện (Background Thread)
            Task.Run(async () => 
            {
                // Dùng _cachedPois đã tải từ Repository, tránh tạo lại object mỗi lần GPS update
                var pois = _cachedPois;
                if (pois.Count == 0) return;

                // Tìm quán ốc gần nhất trong bán kính kích hoạt
                Poi? nearestTriggeredPoi = null;
                double minDistance = double.MaxValue;
                double nearestDistance = double.MaxValue;
                string closestPoiName = Services.LocalizationResourceManager.Instance["Chưa tìm thấy quán ốc"];

                foreach (var poi in pois)
                {
                    double distanceToPoi = LocationHelper.CalculateDistanceInMeters(
                        _currentUserLocation.Latitude, _currentUserLocation.Longitude,
                        poi.Latitude, poi.Longitude);

                    if (distanceToPoi < nearestDistance)
                    {
                        nearestDistance = distanceToPoi;
                        closestPoiName = poi.Name;
                    }

                    if (distanceToPoi <= poi.Radius)
                    {
                        if (distanceToPoi < minDistance || 
                            (distanceToPoi == minDistance && (nearestTriggeredPoi == null || poi.Priority > nearestTriggeredPoi.Priority)))
                        {
                            nearestTriggeredPoi = poi;
                            minDistance = distanceToPoi;
                        }
                    }
                }

                // Quay lại UI Thread để cập nhật giao diện sau khi tính toán xong
                MainThread.BeginInvokeOnMainThread(async () => 
                {
                    DebugLabel.Text = $"GPS: {_currentUserLocation.Latitude:F5}, {_currentUserLocation.Longitude:F5}";
                    DistanceLabel.Text = $"{Services.LocalizationResourceManager.Instance["Cách quán gần nhất"]}: {nearestDistance:F0}m";
                    
                    if (nearestTriggeredPoi != null)
                    {
                        PoiNameLabel.Text = nearestTriggeredPoi.Name;
                    }
                    else
                    {
                        PoiNameLabel.Text = closestPoiName;
                    }

                    if (nearestTriggeredPoi != null)
                    {
                        Debug.WriteLine($"[Geofence] Đã rơi vào bán kính quán: {nearestTriggeredPoi.Name} ({minDistance}m)");
                        
                        // Gọi Động cơ Thuyết minh (Narration Engine)
                        if (_isPlaying)
                        {
                            _playingPoiId = nearestTriggeredPoi.Id;
                            UpdateMapCirclesState();
                            await _narrationEngine.PlayPoiNarrationAsync(nearestTriggeredPoi);
                        }
                        else
                        {
                            _playingPoiId = 0;
                            UpdateMapCirclesState();
                        }
                    }
                    else
                    {
                        if (_playingPoiId != 0)
                        {
                            _playingPoiId = 0;
                            UpdateMapCirclesState();
                        }
                    }
                });
            });
        }

        private async Task LoadPoisToMapAsync()
        {
            // Tải POI từ Repository cho mọi nền tảng (phải nằm ngoài #if)
            _cachedPois = await _poiRepository.GetAllPoisAsync();

#if ANDROID || IOS || MACCATALYST
            if (_cachedPois.Count == 0)
            {
                Debug.WriteLine("[MapPage] Không có POI nào trong repository.");
                return;
            }

            VinhKhanhMap = new Microsoft.Maui.Controls.Maps.Map
            {
                IsShowingUser = true,
                MapType = MapType.Street
            };
            MapContainer.Children.Add(VinhKhanhMap);

            foreach (var poi in _cachedPois)
            {
                // Nhường luồng xử lý UI (50ms) giữa mỗi lần đính ghim
                await Task.Delay(50); 

                var pin = new Pin
                {
                    Label = poi.DisplayName,
                    Address = $"{Services.LocalizationResourceManager.Instance["Bán kính"]}: {poi.Radius}m",
                    Type = PinType.Place,
                    Location = new Location(poi.Latitude, poi.Longitude)
                };

                VinhKhanhMap.Pins.Add(pin);

                var circle = new Microsoft.Maui.Controls.Maps.Circle
                {
                    Center = new Location(poi.Latitude, poi.Longitude),
                    Radius = Distance.FromMeters(poi.Radius),
                    StrokeColor = Color.FromArgb("#808080"),
                    StrokeWidth = 2,
                    FillColor = Color.FromRgba(128, 128, 128, 50)
                };
                VinhKhanhMap.MapElements.Add(circle);
                _poiCircles[poi.Id] = circle;
            }

            UpdateMapCirclesState();
            CenterOnSelectedPoi();
#else
            if (_cachedPois.Count == 0)
                Debug.WriteLine("[MapPage] Không có POI nào trong repository.");
            MapContainer.Children.Add(new Label 
            {
                Text = "Bản đồ không khả dụng trên nền tảng này.",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(20)
            });
#endif
        }

        private void UpdateMapCirclesState()
        {
#if ANDROID || IOS || MACCATALYST
            foreach (var kvp in _poiCircles)
            {
                int poiId = kvp.Key;
                var circle = kvp.Value;

                if (poiId == _playingPoiId)
                {
                    // Đang phát (Màu Vàng)
                    circle.StrokeColor = Color.FromArgb("#FFD700");
                    circle.FillColor = Color.FromRgba(255, 215, 0, 80);
                }
                else if (poiId == _selectedPoiId)
                {
                    // Được chọn từ list (Màu Xanh Cyan/Lục)
                    circle.StrokeColor = Color.FromArgb("#00CED1");
                    circle.FillColor = Color.FromRgba(0, 206, 209, 80);
                }
                else
                {
                    // Chưa đụng tới (Xám)
                    circle.StrokeColor = Color.FromArgb("#A9A9A9");
                    circle.FillColor = Color.FromRgba(169, 169, 169, 40);
                }
            }
#endif
        }

        private void CenterOnSelectedPoi()
        {
#if ANDROID || IOS || MACCATALYST
            if (VinhKhanhMap == null) return;
            // Dùng _cachedPois thay vì tạo lại GetSampleData()
            var poiList = _cachedPois;
            Poi? targetPoi = null;
            
            if (_selectedPoiId > 0)
            {
                targetPoi = poiList.FirstOrDefault(p => p.Id == _selectedPoiId);
            }
            
            if (targetPoi == null && poiList.Any())
            {
                targetPoi = poiList[0];
            }
            
            if (targetPoi != null)
            {
                var centerLocation = new Location(targetPoi.Latitude, targetPoi.Longitude);
                var radius = _selectedPoiId > 0 ? Distance.FromKilometers(0.2) : Distance.FromKilometers(1);
                var mapSpan = MapSpan.FromCenterAndRadius(centerLocation, radius);
                
                try 
                {
                    VinhKhanhMap.MoveToRegion(mapSpan);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi zoom bản đồ: {ex.Message}");
                }
            }
#endif
        }

        // =================== SIMULATION MODE METHODS ===================

        private void OnDemoModeToggled(object sender, EventArgs e)
        {
            _isSimulationMode = !_isSimulationMode;
            DPadPanel.IsVisible = _isSimulationMode;

            if (_isSimulationMode)
            {
                DemoModeButton.Text = "🛑";
                DemoModeButton.BackgroundColor = Color.FromArgb("#FF6B35");
                DemoModeButton.TextColor = Colors.White;
                _simulatedLocation = new Location(10.76140, 106.70270);

                // Nếu cache chưa được tải (demo bật trước khi map load xong), tải người dùng nửa
                if (_cachedPois.Count == 0)
                {
                    Task.Run(async () =>
                    {
                        _cachedPois = await _poiRepository.GetAllPoisAsync();
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
#if ANDROID || IOS || MACCATALYST
                            AddSimulatedPin();
#endif
                            ProcessSimulatedLocation();
                        });
                    });
                    return;
                }

#if ANDROID || IOS || MACCATALYST
                AddSimulatedPin();
#endif
                ProcessSimulatedLocation();
            }
            else
            {
                DemoModeButton.Text = "🎮";
                bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
                DemoModeButton.BackgroundColor = isDark ? Color.FromArgb("#2A2A2A") : Colors.White;
                DemoModeButton.TextColor = Color.FromArgb("#FF6B35");
#if ANDROID || IOS || MACCATALYST
                RemoveSimulatedPin();
#endif
                _simulatedLocation = null;
                DebugLabel.Text = Services.LocalizationResourceManager.Instance["Đang tìm kiếm vệ tinh GPS..."];
            }
        }

        private void OnMoveUp(object sender, EventArgs e)    => SimulateMove(SimStepDegrees, 0);
        private void OnMoveDown(object sender, EventArgs e)  => SimulateMove(-SimStepDegrees, 0);
        private void OnMoveLeft(object sender, EventArgs e)  => SimulateMove(0, -SimStepDegrees);
        private void OnMoveRight(object sender, EventArgs e) => SimulateMove(0, SimStepDegrees);

        private void OnResetSimulation(object sender, EventArgs e)
        {
            _simulatedLocation = new Location(10.76140, 106.70270);
#if ANDROID || IOS || MACCATALYST
            UpdateSimulatedPin();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (VinhKhanhMap != null)
                    VinhKhanhMap.MoveToRegion(MapSpan.FromCenterAndRadius(_simulatedLocation, Distance.FromKilometers(0.5)));
            });
#endif
            ProcessSimulatedLocation();
        }

        private void SimulateMove(double latDelta, double lonDelta)
        {
            if (!_isSimulationMode || _simulatedLocation == null) return;

            _simulatedLocation = new Location(
                _simulatedLocation.Latitude + latDelta,
                _simulatedLocation.Longitude + lonDelta);

#if ANDROID || IOS || MACCATALYST
            UpdateSimulatedPin();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (VinhKhanhMap != null)
                    VinhKhanhMap.MoveToRegion(MapSpan.FromCenterAndRadius(_simulatedLocation, Distance.FromKilometers(0.3)));
            });
#endif
            ProcessSimulatedLocation();
        }

        private void ProcessSimulatedLocation()
        {
            if (_simulatedLocation == null) return;
            Geolocation_LocationChanged(this, new GeolocationLocationChangedEventArgs(_simulatedLocation));
        }

#if ANDROID || IOS || MACCATALYST
        private void AddSimulatedPin()
        {
            if (VinhKhanhMap == null || _simulatedLocation == null) return;
            _simulatedPin = new Pin
            {
                Label = "📍 Vị trí của bạn (Demo)",
                Type = PinType.Generic,
                Location = _simulatedLocation
            };
            MainThread.BeginInvokeOnMainThread(() =>
            {
                VinhKhanhMap.Pins.Add(_simulatedPin);
                VinhKhanhMap.MoveToRegion(MapSpan.FromCenterAndRadius(_simulatedLocation, Distance.FromKilometers(0.5)));
            });
        }

        private void UpdateSimulatedPin()
        {
            if (_simulatedPin == null || _simulatedLocation == null) return;
            MainThread.BeginInvokeOnMainThread(() => _simulatedPin.Location = _simulatedLocation);
        }

        private void RemoveSimulatedPin()
        {
            if (_simulatedPin == null || VinhKhanhMap == null) return;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                VinhKhanhMap.Pins.Remove(_simulatedPin);
                _simulatedPin = null;
            });
        }
#endif
    }
}
