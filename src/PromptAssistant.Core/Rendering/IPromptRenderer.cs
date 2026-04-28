using PromptAssistant.Core.Models;

namespace PromptAssistant.Core.Rendering;

public interface IPromptRenderer
{
    RenderedPrompt Render(ref readonly PromptTemplate template);
}
