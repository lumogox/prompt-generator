namespace PromptAssistant.Core.Models;

public abstract record PromptSection(SectionKind Kind)
{
    /// <summary>
    /// Whether this section is rendered into the assembled prompt at all.
    /// Default true. Set to false on optional sections the user has explicitly excluded —
    /// the renderer will skip the section entirely (no header, no lead, no empty tags).
    /// </summary>
    public bool IsIncluded { get; init; } = true;
}

public sealed record FreeTextSection(SectionKind Kind, string? Content) : PromptSection(Kind)
{
    public bool HasContent => !string.IsNullOrWhiteSpace(Content);
}

public sealed record ListSection(SectionKind Kind, IReadOnlyList<string> Items) : PromptSection(Kind)
{
    public bool HasContent => Items.Count > 0;

    public bool Equals(ListSection? other) =>
        other is not null && Kind == other.Kind && Items.SequenceEqual(other.Items);

    public override int GetHashCode()
    {
        var hash = HashCode.Combine(Kind);
        foreach (var item in Items)
        {
            hash = HashCode.Combine(hash, item);
        }
        return hash;
    }
}

public sealed record ToggleSection(SectionKind Kind, bool Enabled, string CanonicalText) : PromptSection(Kind)
{
    public bool HasContent => Enabled && !string.IsNullOrWhiteSpace(CanonicalText);
}
