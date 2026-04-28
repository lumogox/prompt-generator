using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using PromptAssistant.App.ViewModels;

namespace PromptAssistant.App.Converters;

public sealed class HealthToBrushConverter : IValueConverter
{
    // Muted semaphore palette — hues chosen to fit the paper-warm editorial aesthetic without clashing.
    private static readonly IBrush Green = new SolidColorBrush(Color.Parse("#5C8550"));
    private static readonly IBrush Yellow = new SolidColorBrush(Color.Parse("#C49B3F"));
    private static readonly IBrush Red = new SolidColorBrush(Color.Parse("#B84545"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            ProviderHealth.Authenticated => Green,
            ProviderHealth.NotFound => Red,
            ProviderHealth.Unknown or ProviderHealth.Failed => Yellow,
            _ => AvaloniaProperty.UnsetValue,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
