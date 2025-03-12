using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;

namespace Altairis.Tmd;

public class StepLink : LeafInline {

    public string? StepName { get; set; }

}

public class StepLinkInlineParser : InlineParser {

    public StepLinkInlineParser() {
        this.OpeningCharacters = ['['];
    }

    public override bool Match(InlineProcessor processor, ref StringSlice slice) {
        // Check if next character is #
        if (slice.PeekChar() != '#') return false;

        // Set start character to the [
        var start = slice.Start;

        // Find end of the link
        var end = slice.Start;
        do {
            slice.NextChar();
            end = slice.Start;
        } while (slice.CurrentChar != ']');
        slice.NextChar();

        processor.GetSourcePosition(start, out var line, out var column);
        var stepName = slice.Text[(start + 2)..end];
        processor.Inline = new StepLink {
            Span = {
                    Start = start,
                    End = slice.End
                },
            Line = line,
            Column = column,
            StepName = stepName
        };

        return true;
    }

}

public class StepLinkRenderer(IList<TmdBlock> blocks) : HtmlObjectRenderer<StepLink> {

    protected override void Write(HtmlRenderer renderer, StepLink obj) {
        if (string.IsNullOrEmpty(obj.StepName)) return;

        var stepNumber = blocks.FirstOrDefault(x => obj.StepName.Equals(x.Name, StringComparison.Ordinal))?.StepNumber;

        if (renderer.EnableHtmlForInline && stepNumber.HasValue) {
            renderer.Write($"<a href=\"#{obj.StepName}\">{stepNumber}</a>");
        } else {
            renderer.Write($"#{obj.StepName}");
        }
    }

}

public class StepLinksExtension(IList<TmdBlock> blocks) : IMarkdownExtension {

    public void Setup(MarkdownPipelineBuilder pipeline) {
        var parsers = pipeline.InlineParsers;
        if (!parsers.Contains<StepLinkInlineParser>()) {
            // Insert the parser before the LinkInlineParser
            if (parsers.Contains<LinkInlineParser>()) {
                parsers.InsertBefore<LinkInlineParser>(new StepLinkInlineParser());
            } else {
                parsers.Add(new StepLinkInlineParser());
            }
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) {
        if (renderer is HtmlRenderer htmlRenderer && htmlRenderer.ObjectRenderers is not null) {
            var objectRenderers = htmlRenderer.ObjectRenderers;
            if (!objectRenderers.Contains<StepLinkRenderer>()) objectRenderers.Add(new StepLinkRenderer(blocks));
        }
    }
}

public static class StepLinkExtensions {

    public static MarkdownPipelineBuilder UseStepLinks(this MarkdownPipelineBuilder pipeline, IList<TmdBlock> blocks) {
        var extensions = pipeline.Extensions;
        if (!extensions.Contains<StepLinksExtension>()) extensions.Add(new StepLinksExtension(blocks));
        return pipeline;
    }

}