using Markdig;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Altairis.Tmd;

/// <summary>
/// An HTML renderer for a <see cref="CodeBlock"/> and <see cref="FencedCodeBlock"/>.
/// </summary>
/// <seealso cref="HtmlObjectRenderer{CodeBlock}" />
public class CustomCodeBlockRenderer : HtmlObjectRenderer<CodeBlock> {
    protected override void Write(HtmlRenderer renderer, CodeBlock obj) {
        var info = (obj as FencedCodeBlock)?.Info;
        var tags = info?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];

        if (tags.Contains("edit")) {
            // Mark added lines with <ins> and removed lines with <del>
            renderer.WriteLine($"<pre class=\"{info}\">");
            foreach (StringLine item in obj.Lines) {
                var line = item.ToString();
                if (line.StartsWith("+ ")) {
                    renderer.Write("<ins>");
                    renderer.WriteEscape(line[2..]);
                    renderer.WriteLine("</ins>");
                } else if (line.StartsWith("- ")) {
                    renderer.Write("<del>");
                    renderer.WriteEscape(line[2..]);
                    renderer.WriteLine("</del>");
                } else {
                    renderer.WriteEscape(line);
                    renderer.WriteLine();
                }
            }
            renderer.WriteLine("</pre>");
            return;
        } else {
            // Render as plain text
            renderer.EnsureLine();
            renderer.WriteLine($"<pre class=\"{info}\">");
            renderer.WriteLeafRawLines(obj, true, true, true);
            renderer.WriteLine("</pre>");
            renderer.EnsureLine();
        }
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