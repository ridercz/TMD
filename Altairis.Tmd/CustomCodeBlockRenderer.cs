using Markdig.Renderers.Html;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig;

namespace Altairis.Tmd; 

/// <summary>
/// An HTML renderer for a <see cref="CodeBlock"/> and <see cref="FencedCodeBlock"/>.
/// </summary>
/// <seealso cref="HtmlObjectRenderer{CodeBlock}" />
public class CustomCodeBlockRenderer : HtmlObjectRenderer<CodeBlock> {
    protected override void Write(HtmlRenderer renderer, CodeBlock obj) {
        // var info = (obj as FencedCodeBlock)?.Info;

        renderer.EnsureLine();
        renderer.WriteLine("<pre>");
        renderer.WriteLeafRawLines(obj, true, true, true);
        renderer.WriteLine("</pre>");
        renderer.EnsureLine();
    }
}


public class CustomCodeBlocksExtension() : IMarkdownExtension {

    public void Setup(MarkdownPipelineBuilder pipeline) {
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) {
        if (renderer is HtmlRenderer htmlRenderer && htmlRenderer.ObjectRenderers is not null) {
            var objectRenderers = htmlRenderer.ObjectRenderers;

            // Remove default code block renderer
            var defaultCodeBlockRenderer = objectRenderers.Find<CodeBlockRenderer>();
            if (defaultCodeBlockRenderer != null) objectRenderers.Remove(defaultCodeBlockRenderer);

            // Add custom code block renderer
            objectRenderers.AddIfNotAlready(new CustomCodeBlockRenderer());
        }
    }
}

public static class CustomCodeBlocksExtensions {

    public static MarkdownPipelineBuilder UseCustomCodeBlocks(this MarkdownPipelineBuilder pipeline) {
        var extensions = pipeline.Extensions;
        if (!extensions.Contains<CustomCodeBlocksExtension>()) extensions.Add(new CustomCodeBlocksExtension());
        return pipeline;
    }

}