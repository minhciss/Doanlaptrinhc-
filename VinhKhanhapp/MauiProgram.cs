using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Maps;
using Syncfusion.Maui.Toolkit.Hosting;
using ZXing.Net.Maui.Controls;

namespace VinhKhanhapp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiMaps()
                .UseMauiCommunityToolkit()
                .UseBarcodeReader()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
#if WINDOWS
    				Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
    				{
    					handler.PlatformView.SingleSelectionFollowsFocus = false;
    				});

    				Microsoft.Maui.Handlers.ContentViewHandler.Mapper.AppendToMapping(nameof(Pages.Controls.CategoryChart), (handler, view) =>
    				{
    					if (view is Pages.Controls.CategoryChart && handler.PlatformView is Microsoft.Maui.Platform.ContentPanel contentPanel)
    					{
    						contentPanel.IsTabStop = true;
    					}
    				});
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            // Tourism app services
            builder.Services.AddSingleton<Services.ILocationService, Services.LocationService>();
            builder.Services.AddSingleton<Services.IGeofenceService, Services.GeofenceService>();
            builder.Services.AddSingleton<Services.IAudioService, Services.AudioService>();
            builder.Services.AddSingleton<Services.IPoiRepository, Services.MockPoiRepository>();
            builder.Services.AddSingleton<Services.IAnalyticsService, Services.AnalyticsService>();
            builder.Services.AddSingleton<PageModels.MapPageModel>();

            builder.Services.AddTransientWithShellRoute<Pages.MapPage, PageModels.MapPageModel>("map");

            return builder.Build();
        }
    }
}
