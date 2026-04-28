namespace PromptAssistant.App.ViewModels;

/// <summary>
/// User-supplied steering for a Refine operation. Returned by the RefineDialog and consumed
/// by MainWindowViewModel when building the refinement meta-prompt.
/// </summary>
/// <param name="LensInstructions">
/// Pre-resolved instruction strings for the selected lenses (e.g. "Cut wordiness aggressively…").
/// Empty means: no lenses selected → today's baseline-only refinement.
/// </param>
/// <param name="Guidance">
/// Free-text guidance the user typed. Null/empty means: no extra guidance.
/// </param>
public sealed record RefineOptions(
    IReadOnlyList<string> LensInstructions,
    string? Guidance);
