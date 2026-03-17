using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
using VinhKhanhapp.Models;

namespace VinhKhanhapp.Services;

public interface ILocationService
{
    Task<Location?> GetCurrentLocationAsync(CancellationToken cancellationToken = default);
}

public interface IGeofenceService
{
    void SetPois(IEnumerable<Poi> pois);
    PoiActivationResult? OnLocationUpdated(Location location, DateTime timestampUtc);
}

public class LocationService : ILocationService
{
    public async Task<Location?> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
            throw new PermissionException("Location permission not granted.");

        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            return await Geolocation.GetLocationAsync(request, cancellationToken);
        }
        catch (FeatureNotEnabledException)
        {
            // Location toggle off (device/emulator)
            return null;
        }
        catch (FeatureNotSupportedException)
        {
            return null;
        }
    }
}

public class GeofenceService : IGeofenceService
{
    readonly Dictionary<string, (DateTime lastPlayedUtc, TimeSpan cooldown)> _playState = new();
    IReadOnlyList<Poi> _pois = Array.Empty<Poi>();
    readonly TimeSpan _defaultCooldown = TimeSpan.FromMinutes(5);

    public void SetPois(IEnumerable<Poi> pois)
    {
        _pois = pois.ToList();
    }

    public PoiActivationResult? OnLocationUpdated(Location location, DateTime timestampUtc)
    {
        if (_pois.Count == 0)
            return null;

        Poi? bestPoi = null;
        double bestDistance = double.MaxValue;

        foreach (var poi in _pois)
        {
            var distance = Location.CalculateDistance(
                location,
                new Location(poi.Lat, poi.Lng),
                DistanceUnits.Kilometers) * 1000.0;

            if (distance > poi.RadiusMeters)
                continue;

            if (bestPoi == null)
            {
                bestPoi = poi;
                bestDistance = distance;
                continue;
            }

            if (poi.Priority > bestPoi.Priority ||
                (poi.Priority == bestPoi.Priority && distance < bestDistance))
            {
                bestPoi = poi;
                bestDistance = distance;
            }
        }

        if (bestPoi == null)
            return null;

        var key = bestPoi.Id;
        if (_playState.TryGetValue(key, out var state))
        {
            var cooldown = state.cooldown == default ? _defaultCooldown : state.cooldown;
            if (timestampUtc - state.lastPlayedUtc < cooldown)
                return null;
        }

        _playState[key] = (timestampUtc, _defaultCooldown);

        return new PoiActivationResult
        {
            Poi = bestPoi,
            DistanceMeters = bestDistance
        };
    }
}

