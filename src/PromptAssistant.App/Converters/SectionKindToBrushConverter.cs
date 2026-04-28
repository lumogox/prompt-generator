using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PromptAssistant.App.Converters;

public sealed class SectionKindToBrushConverter : IValueConverter
{
    /// <summary>
    /// Converter parameter selects which slot of the section palette to return:
    /// "Bg" (default) for background tint, "Fg" for the matching foreground/label color.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SectionKind kind) return AvaloniaProperty.UnsetValue;

        var slot = parameter as string ?? "Bg";
        var key = $"Section.{kind}.{slot}";

        if (Application.Current is { } app && app.TryFindResource(key, out var brush))
        {
            return brush;
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
