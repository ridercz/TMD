using System.Text;
using Markdig;

namespace Altairis.Tmd;

public class TmdDocument {
    private const string BlockSeparator = "- - -";
    private const string QualifierShortPrefix = "(";
    private const string QualifierShortSuffix = ")";
    private const string QualifierLongPrefix = "<!--";
    private const string QualifierLongSuffix = "-->";
    private const string QualifierName = "#";
    private const string QualifierInformation = "i";
    private const string QualifierWarning = "!";
    private const string QualifierDownload = "dl";
    private const string QualifierPlainText = "$";

    // Properties

    public IList<TmdBlock> Blocks { get; set; } = [];

    public IList<TmdWarning> Warnings { get; set; } = [];

    public TmdRenderOptions RenderOptions { get; set; } = new TmdRenderOptions();

    // Load methods

    public bool LoadFile(string fileName) {
        using var reader = new StreamReader(fileName);
        return this.Load(reader);
    }

    public bool Load(string source) {
        using var reader = new StringReader(source);
        return this.Load(reader);
    }

    public bool Load(TextReader reader) {
        this.Blocks.Clear();
        this.Warnings.Clear();

        // Separate all blocks and load them
        this.LoadBlocks(reader);

        // Qualify blocks
        this.QualifyBlocks();

        return this.Warnings.Count == 0;
    }

    // Rendering methods

    public bool RenderHtml(TextWriter writer) {
        var result = this.RenderHtml(out var htmlString);
        writer.Write(htmlString);
        return result;
    }

    public bool RenderHtml(string fileName) {
        var result = this.RenderHtml(out var htmlString);
        File.WriteAllText(fileName, htmlString);
        return result;
    }

    public bool RenderHtml(out string htmlString) {
        htmlString = string.Empty;
        if (this.Blocks.Count == 0) return true;

        // Prepare ouptut
        var sb = new StringBuilder();
        var tableOpen = false;

        // Prepare Markdown pipeline builder
        var pipeline = this.RenderOptions.MarkdownPipelineBuilder
            .UseStepLinks(this.Blocks)
            .UseCustomCodeBlocks()
            .Build();

        

        // Render all steps
        foreach (var block in this.Blocks) {
            // Skip empty blocks
            if (block.Type == TmdBlockType.Empty) continue;

            // Render markdown to HTML
            var html = string.Empty;
            try {
                html = Markdown.ToHtml(block.Markdown, pipeline).Trim();
            } catch (Exception ex) {
                html = $"<p><b>{ex.Message}</b></p><pre>{ex}</pre>";
                this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), TmdWarningType.Exception, ex.Message));
            }

            // Check for empty content
            if (string.IsNullOrWhiteSpace(html)) {
                this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), TmdWarningType.ContentIsEmpty));
                continue;
            }

            // Compute SHA-256 hash of generated HTML so we can track changes on client side
            var htmlHash = this.GetHashString(html);

            if (block.Type == TmdBlockType.PlainText) {
                // Plain (non-table) step
                if (tableOpen) {
                    sb.AppendLine(this.RenderOptions.TableEndTemplate);
                    tableOpen = false;
                }
                sb.AppendLine(html);
            } else {
                // Table steps
                if (!tableOpen) {
                    sb.AppendLine(this.RenderOptions.TableBeginTemplate);
                    tableOpen = true;
                }

                if (block.Type == TmdBlockType.Information) {
                    sb.AppendLine(string.Format(this.RenderOptions.InformationTemplate, html));
                } else if (block.Type == TmdBlockType.Warning) {
                    sb.AppendLine(string.Format(this.RenderOptions.WarningTemplate, html));
                } else if (block.Type == TmdBlockType.Download) {
                    sb.AppendLine(string.Format(this.RenderOptions.DownloadTemplate, html));
                } else if (string.IsNullOrWhiteSpace(block.Name)) {
                    sb.AppendLine(string.Format(this.RenderOptions.NumberedStepTemplate, block.StepNumber, htmlHash, html));
                } else {
                    sb.AppendLine(string.Format(this.RenderOptions.NamedStepTemplate, block.Name, block.StepNumber, htmlHash, html));
                }
            }
        }

        // Close table if open
        if (tableOpen) sb.AppendLine(this.RenderOptions.TableEndTemplate);

        // Return HTML
        htmlString = sb.ToString();
        return this.Warnings.Count == 0;
    }

    // Helper methods

    private void QualifyBlocks() {
        var stepNumber = 1;
        foreach (var block in this.Blocks) {
            // Check for empty content
            if (string.IsNullOrWhiteSpace(block.Markdown)) {
                block.Type = TmdBlockType.Empty;
                this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), TmdWarningType.ContentIsEmpty));
                continue;
            }

            // Read first line of block, which may end with \n or \r\n
            var blockLines = block.Markdown.Split('\n');

            // Check for block type qualifiers
            string? qualifier = null;
            if (blockLines[0].StartsWith(QualifierLongPrefix, StringComparison.Ordinal) && blockLines[0].EndsWith(QualifierLongSuffix, StringComparison.Ordinal)) {
                qualifier = blockLines[0][(QualifierLongPrefix.Length)..^QualifierLongSuffix.Length].Trim();
            } else if (blockLines[0].StartsWith(QualifierShortPrefix, StringComparison.Ordinal) && blockLines[0].EndsWith(QualifierShortSuffix, StringComparison.Ordinal)) {
                qualifier = blockLines[0][(QualifierShortPrefix.Length)..^QualifierShortSuffix.Length].Trim();
            }

            if (qualifier != null) {
                // Check if there is some content after qualifier
                if (blockLines.Length == 1) {
                    // No content, remove qualifier
                    block.Markdown = string.Empty;
                    block.Type = TmdBlockType.Empty;
                    this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), TmdWarningType.ContentIsEmpty));
                    continue;
                } else {
                    // Remove first line from block content
                    block.Markdown = string.Join('\n', blockLines[1..]);
                }
            }

            // Determine block type
            if (qualifier is null) {
                if (blockLines[0].StartsWith('#')) {
                    // Plaintext block by virtue of starting with heading
                    block.Type = TmdBlockType.PlainText;
                } else {
                    // Common numbered step
                    block.Type = TmdBlockType.NumberedStep;
                    block.StepNumber = stepNumber;
                    stepNumber++;
                }
            } else if (qualifier.StartsWith(QualifierName, StringComparison.Ordinal)) {
                // Named step
                block.Type = TmdBlockType.NumberedStep;
                block.Name = qualifier[(QualifierName.Length)..].Trim();
                block.StepNumber = stepNumber;
                stepNumber++;

                if (string.IsNullOrWhiteSpace(block.Name)) {
                    // Check for empty name
                    block.Name = null;
                    this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), TmdWarningType.EmptyBlockName));
                } else if (this.Blocks.Any(b => b != block && b.Name == block.Name)) {
                    // Check for duplicate name
                    this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), TmdWarningType.DuplicateBlockName, block.Name));
                }
            } else if (qualifier.Equals(QualifierInformation, StringComparison.Ordinal)) {
                // Information block
                block.Type = TmdBlockType.Information;
            } else if (qualifier.Equals(QualifierWarning, StringComparison.Ordinal)) {
                // Warning block
                block.Type = TmdBlockType.Warning;
            } else if (qualifier.Equals(QualifierDownload, StringComparison.Ordinal)) {
                // Download block
                block.Type = TmdBlockType.Download;
            } else if (qualifier.Equals(QualifierPlainText, StringComparison.Ordinal)) {
                // Plain text block
                block.Type = TmdBlockType.PlainText;
            } else if (qualifier == string.Empty) {
                // Empty qualifier
                block.Type = TmdBlockType.NumberedStep;
                block.StepNumber = stepNumber;
                stepNumber++;
                this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), TmdWarningType.EmptyQualifier));
            } else {
                // Unknown qualifier
                block.Type = TmdBlockType.NumberedStep;
                block.StepNumber = stepNumber;
                stepNumber++;
                this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), TmdWarningType.UnknownQualifier, qualifier));
            }
        }
    }

    private void LoadBlocks(TextReader reader) {
        var block = new TmdBlock();
        var inCodeBlock = false;
        var sb = new StringBuilder();
        var lineNumber = 0;
        while (true) {
            // Read next line
            var line = reader.ReadLine();
            if (line == null) break;
            lineNumber++;

            // Check if line is step separator
            if (line.Trim().Equals(BlockSeparator, StringComparison.Ordinal) && !inCodeBlock) {
                // Save current step
                block.Markdown = sb.ToString().Trim('\n');
                this.Blocks.Add(block);

                // Prepare new step
                block = new TmdBlock { StartingLineNumber = lineNumber };
                sb.Clear();
                continue;
            }

            // Check if line is code block start/end
            if (line.StartsWith("```", StringComparison.Ordinal)) {
                inCodeBlock = !inCodeBlock;
            }

            // Append line to current step
            sb.Append(line);
            sb.Append('\n');
        }

        // Save last step
        block.Markdown = sb.ToString().Trim('\n');
        this.Blocks.Add(block);
    }

    protected string GetHashString(string s) {
        if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(s));

        var data = System.Text.Encoding.UTF8.GetBytes(s);
        var hash = this.RenderOptions.ContentHashAlgorithm.ComputeHash(data);
        return Convert.ToBase64String(hash).Trim('=');
    }

}