using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace Minh2.Services
{
    public class LocationChangedEventArgs : EventArgs
    {
        // Make Location nullable to satisfy nullable-check rules in constructor flow
        public Location? Location { get; set; }
    }

    // Simple foreground poller POC. Replace/extend with platform-specific background implementation.
    public class LocationService
    {
        private CancellationTokenSource? _cts;
        private readonly TimeSpan _interval;

        public event EventHandler<LocationChangedEventArgs>? LocationChanged;

        public LocationService(TimeSpan? interval = null)
        {
            _interval = interval ?? TimeSpan.FromSeconds(5);
        }

        public void Start()
        {
            if (_cts != null) return;
            _cts = new CancellationTokenSource();
            _ = PollLoop(_cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts = null;
        }

        private async Task PollLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Best, _interval);
                    var location = await Geolocation.Default.GetLocationAsync(request, token);
                    if (location != null)
                    {
                        LocationChanged?.Invoke(this, new LocationChangedEventArgs { Location = location });
                    }
                }
                catch (Exception)
                {
                    // handle permissions / hardware errors in real app
                }

                await Task.Delay(_interval, token);
            }
        }
    }
}