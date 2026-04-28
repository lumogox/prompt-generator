using PromptAssistant.Core.Tests.Fixtures;

namespace PromptAssistant.Core.Tests;

public class AnthropicPromptRendererTests
{
    private readonly AnthropicPromptRenderer _sut = new();

    [Fact]
    public void Renders_full_template_to_expected_string()
    {
        var template = SampleTemplates.FullyPopulated();

        var rendered = _sut.Render(in template);

        Assert.Equal(ReadExpected("full-template.expected.txt"), rendered.UserMessage);
        Assert.Equal("<response>", rendered.AssistantPrefill);
    }

    [Fact]
    public void Renders_minimum_template_to_expected_string()
    {
        var template = SampleTemplates.MinimumRequired();

        var rendered = _sut.Render(in template);

        Assert.Equal(ReadExpected("minimum-template.expected.txt"), rendered.UserMessage);
        Assert.Null(rendered.AssistantPrefill);
    }

    private static string ReadExpected(string filename)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Expected", filename);
        return File.ReadAllText(path).Replace("\r\n", "\n");
    }
}
