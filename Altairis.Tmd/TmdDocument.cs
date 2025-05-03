using System.Text;
using Markdig;

namespace Altairis.Tmd;

/// <summary>
/// Represents a TMD (Tutorial MarkDown) document, which consists of multiple blocks of content.
/// Provides methods for loading, saving, and rendering the document.
/// </summary>
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

    /// <summary>
    /// Gets or sets the list of blocks in the document.
    /// </summary>
    public IList<TmdBlock> Blocks { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of warnings generated during document processing.
    /// </summary>
    public IList<TmdWarning> Warnings { get; set; } = [];

    /// <summary>
    /// Gets or sets the rendering options for the document.
    /// </summary>
    public TmdRenderOptions RenderOptions { get; set; } = new TmdRenderOptions();

    // Load methods

    /// <summary>
    /// Loads the document from a file.
    /// </summary>
    /// <param name="fileName">The path to the file to load.</param>
    /// <returns>True if the document was loaded successfully; otherwise, false.</returns>
    public bool LoadFile(string fileName) {
        using var reader = new StreamReader(fileName);
        return this.Load(reader);
    }

    /// <summary>
    /// Loads the document from a string source.
    /// </summary>
    /// <param name="source">The string containing the document content.</param>
    /// <returns>True if the document was loaded successfully; otherwise, false.</returns>
    public bool Load(string source) {
        using var reader = new StringReader(source);
        return this.Load(reader);
    }

    /// <summary>
    /// Loads the document from a <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> to read the document content from.</param>
    /// <returns>True if the document was loaded successfully; otherwise, false.</returns>
    public bool Load(TextReader reader) {
        this.Blocks.Clear();
        this.Warnings.Clear();

        this.LoadBlocks(reader);
        this.QualifyBlocks();
        this.NumberBlocks();

        return this.Warnings.Count == 0;
    }

    // Index methods

    public void NumberBlocks() {
        var stepNumber = 1;
        foreach (var block in this.Blocks) {
            if (block.Type == TmdBlockType.NumberedStep) {
                block.StepNumber = stepNumber;
                stepNumber++;
            } else {
                block.StepNumber = 0;
            }
        }
    }

    // Save methods

    /// <summary>
    /// Saves the document to a file.
    /// </summary>
    /// <param name="fileName">The path to the file to save the document to.</param>
    public void Save(string fileName) {
        using var writer = new StreamWriter(fileName);
        this.Save(writer);
    }

    /// <summary>
    /// Saves the document to a string.
    /// </summary>
    /// <returns>The TMD source code of document.</returns>
    public string Save() {
        using var writer = new StringWriter();
        this.Save(writer);
        return writer.ToString();
    }

    /// <summary>
    /// Saves the document using a <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write the document content to.</param>
    public void Save(TextWriter writer) => this.Save(writer, false);

    /// <summary>
    /// Saves the document using a <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write the document content to.</param>
    /// <param name="useLongQualifiers">Indicates whether to use long qualifiers(<c>true</c>) or short ones (<c>false</c>).</param>
    public void Save(TextWriter writer, bool useLongQualifiers) {
        if (this.Blocks.Count == 0) return;

        var qualifierPrefix = useLongQualifiers ? QualifierLongPrefix + " " : QualifierShortPrefix;
        var qualifierSuffix = useLongQualifiers ? " " + QualifierLongSuffix : QualifierShortSuffix;

        foreach (var block in this.Blocks) {
            var isLastBlock = block == this.Blocks.Last();
            switch (block.Type) {
                case TmdBlockType.NumberedStep:
                    // Named step
                    if (!string.IsNullOrWhiteSpace(block.Name)) writer.WriteLine($"{qualifierPrefix}{QualifierName}{block.Name.Trim()}{qualifierSuffix}");
                    writer.WriteLine(block.Markdown);
                    break;
                case TmdBlockType.PlainText:
                    if (block.Markdown.StartsWith('#')) {
                        // Implicit plaintext block by virtue of starting with heading
                        writer.WriteLine(block.Markdown);
                    } else {
                        // Explicit plain text block
                        writer.WriteLine($"{qualifierPrefix}{QualifierPlainText}{qualifierSuffix}");
                        writer.WriteLine(block.Markdown);
                    }
                    break;
                case TmdBlockType.Information:
                    writer.WriteLine($"{qualifierPrefix}{QualifierInformation}{qualifierSuffix}");
                    writer.WriteLine(block.Markdown);
                    break;
                case TmdBlockType.Warning:
                    writer.WriteLine($"{qualifierPrefix}{QualifierWarning}{qualifierSuffix}");
                    writer.WriteLine(block.Markdown);
                    break;
                case TmdBlockType.Download:
                    writer.WriteLine($"{qualifierPrefix}{QualifierDownload}{qualifierSuffix}");
                    writer.WriteLine(block.Markdown);
                    break;
                case TmdBlockType.Empty:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(block.Type), block.Type, "Unknown block type");
            }

            // Write block separator if not last or empty block
            if (!isLastBlock && block.Type != TmdBlockType.Empty) writer.WriteLine(BlockSeparator);
        }
    }

    // Rendering methods

    /// <summary>
    /// Renders the document as HTML and writes it to a <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write the HTML content to.</param>
    /// <returns>True if rendering was successful; otherwise, false.</returns>
    public bool RenderHtml(TextWriter writer) {
        var result = this.RenderHtml(out var htmlString);
        writer.Write(htmlString);
        return result;
    }

    /// <summary>
    /// Renders the document as HTML and saves it to a file.
    /// </summary>
    /// <param name="fileName">The path to the file to save the HTML content to.</param>
    /// <returns>True if rendering was successful; otherwise, false.</returns>
    public bool RenderHtml(string fileName) {
        var result = this.RenderHtml(out var htmlString);
        File.WriteAllText(fileName, htmlString);
        return result;
    }

    /// <summary>
    /// Renders the document as HTML and returns it as a string.
    /// </summary>
    /// <param name="htmlString">The rendered HTML content.</param>
    /// <returns>True if rendering was successful; otherwise, false.</returns>
    public bool RenderHtml(out string htmlString) {
        htmlString = string.Empty;
        if (this.Blocks.Count == 0) return true;

        // Renumber blocks
        this.NumberBlocks();

        // Prepare output
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
                this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), block.StartingLineNumber, TmdWarningType.Exception));
            }

            // Check for empty content
            if (string.IsNullOrWhiteSpace(html)) {
                this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), block.StartingLineNumber, TmdWarningType.ContentIsEmpty));
                continue;
            }

            // Compute SHA-256 hash of generated HTML so we can track changes on client side
            var htmlHash = this.GetHashString(html);

            if (block.Type == TmdBlockType.PlainText) {
                // Plain (non-table) step - close table if open
                if (tableOpen) {
                    sb.AppendLine(this.RenderOptions.TableEndTemplate);
                    tableOpen = false;
                }

                // Render plain text block
                sb.AppendLine(html);
            } else {
                // Table steps - open table if not already open
                if (!tableOpen) {
                    sb.AppendLine(this.RenderOptions.TableBeginTemplate);
                    tableOpen = true;
                }

                // Render appropriate table row based on block type
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

    /// <summary>
    /// Qualifies the blocks in the document by determining their type and assigning step numbers.
    /// </summary>
    private void QualifyBlocks() {
        var stepNumber = 1;
        foreach (var block in this.Blocks) {
            // Check for empty content
            if (string.IsNullOrWhiteSpace(block.Markdown)) {
                // Do nothing if this is last block
                if (block == this.Blocks.Last()) break;

                // Otherwise mark block as empty and continue
                block.Type = TmdBlockType.Empty;
                this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), block.StartingLineNumber, TmdWarningType.ContentIsEmpty));
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
                    this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), block.StartingLineNumber, TmdWarningType.ContentIsEmpty));
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
                    this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), block.StartingLineNumber, TmdWarningType.EmptyBlockName));
                } else if (this.Blocks.Any(b => b != block && b.Name == block.Name)) {
                    // Check for duplicate name
                    this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), block.StartingLineNumber, TmdWarningType.DuplicateBlockName));
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
                this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), block.StartingLineNumber, TmdWarningType.EmptyQualifier));
            } else {
                // Unknown qualifier
                block.Type = TmdBlockType.NumberedStep;
                block.StepNumber = stepNumber;
                stepNumber++;
                this.Warnings.Add(new TmdWarning(this.Blocks.IndexOf(block), block.StartingLineNumber, TmdWarningType.UnknownQualifier));
            }
        }
    }

    /// <summary>
    /// Loads the blocks from a <see cref="TextReader"/> by separating them based on the block separator.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> to read the blocks from.</param>
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
            if (line.TrimEnd().Equals(BlockSeparator, StringComparison.Ordinal) && !inCodeBlock) {
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