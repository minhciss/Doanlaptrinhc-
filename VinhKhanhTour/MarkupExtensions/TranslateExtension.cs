using System;
using Microsoft.Maui.Controls;
using VinhKhanhTour.Services;

namespace VinhKhanhTour.MarkupExtensions
{
    [ContentProperty(nameof(Text))]
    public class TranslateExtension : IMarkupExtension<BindingBase>
    {
        public string Text { get; set; } = string.Empty;

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            var key = Text ?? string.Empty;

            // Use a converter-based binding to avoid building indexer paths in XAML.
            // This lets keys contain any character (including ']' or quotes).
            return new Binding
            {
                Mode = BindingMode.OneWay,
                Path = ".", // pass the LocalizationResourceManager instance as the value
                Source = LocalizationResourceManager.Instance,
                Converter = new TranslateConverter(),
                ConverterParameter = key,
            };
        }

        // Local converter that looks up the key on the LocalizationResourceManager provided as the value.
        private class TranslateConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var manager = value as VinhKhanhTour.Services.LocalizationResourceManager ?? VinhKhanhTour.Services.LocalizationResourceManager.Instance;
                var key = parameter?.ToString() ?? string.Empty;
                try
                {
                    return manager[key] ?? key;
                }
                catch
                {
                    return key;
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }
    }
}
