using System.Resources;
using System.Security.Cryptography;
using Markdig;

namespace Altairis.Tmd;

/// <summary>
/// Options for rendering TMD (Table Markdown) content.
/// </summary>
public class TmdRenderOptions {

    /// <summary>
    /// Gets or sets a value indicating whether to use a single table layout for all steps.
    /// </summary>
    public bool SingleTableLayout { get; set; } = false;

    /// <summary>
    /// Gets or sets the HTML template used to begin the steps table.
    /// </summary>
    public string TableBeginTemplate { get; set; } = "<table class=\"steps\">";

    /// <summary>
    /// Gets or sets the HTML template used to end the steps table.
    /// </summary>
    public string TableEndTemplate { get; set; } = "</table>";

    /// <summary>
    /// Gets or sets the HTML template for a numbered step row.
    /// <para>
    /// Placeholders: {0} = step sequence ID, {1} = step hash, {2} = step content, {3} = block index.
    /// </para>
    /// </summary>
    public string NumberedStepTemplate { get; set; } = "<tr data-block-index=\"{3}\" data-step-seqid=\"{0}\" data-step-hash=\"{1}\">\r\n\t<th>{0}.</th>\r\n\t<td>{2}</td>\r\n</tr>";

    /// <summary>
    /// Gets or sets the HTML template for a named step row.
    /// <para>
    /// Placeholders: {0} = step name, {1} = step sequence ID, {2} = step hash, {3} = step content, {4} = block index.
    /// </para>
    /// </summary>
    public string NamedStepTemplate { get; set; } = "<tr data-block-index=\"{4}\" id=\"{0}\" data-step-seqid=\"{1}\" data-step-hash=\"{2}\">\r\n\t<th>{1}.</th>\r\n\t<td>{3}</td>\r\n</tr>";

    /// <summary>
    /// Gets or sets the HTML template for an information row.
    /// <para>
    /// Placeholders: {0} = information content, {1} = block index.
    /// </para>
    /// </summary>
    public string InformationTemplate { get; set; } = "<tr data-block-index=\"{1}\" class=\"information\">\r\n\t<th>&#x1F6C8;</th>\r\n\t<td>{0}</td>\r\n</tr>";

    /// <summary>
    /// Gets or sets the HTML template for a warning row.
    /// <para>
    /// Placeholders: {0} = warning content, {1} = block index.
    /// </para>
    /// </summary>
    public string WarningTemplate { get; set; } = "<tr data-block-index=\"{1}\" class=\"warning\">\r\n\t<th>&#x26A0;</th>\r\n\t<td>{0}</td>\r\n</tr>";

    /// <summary>
    /// Gets or sets the HTML template for a download row.
    /// <para>
    /// Placeholders: {0} = download content, {1} = block index.
    /// </para>
    /// </summary>
    public string DownloadTemplate { get; set; } = "<tr data-block-index=\"{1}\" class=\"download\">\r\n\t<th>&#x1F5AB;</th>\r\n\t<td>{0}</td>\r\n</tr>";

    /// <summary>
    /// Gets or sets the HTML template for a plain row.
    /// <para>
    /// Placeholders: {0} = plain content, {1} = block index.
    /// </para>
    /// </summary>
    public string PlainTemplate { get; set; } = "<tr data-block-index=\"{1}\" class=\"plain\">\r\n\t<td colspan=\"2\">{0}</td>\r\n</tr>";

    /// <summary>
    /// Gets or sets the HTML template to be inserted after each step.
    /// <para>
    /// Placeholders: {0} = block index.
    /// </para>
    /// </summary>
    public string AfterStepTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Markdown pipeline builder used for rendering Markdown content.
    /// </summary>
    public MarkdownPipelineBuilder MarkdownPipelineBuilder { get; set; } = new MarkdownPipelineBuilder()
            .UseEmphasisExtras()
            .UsePipeTables()
            .UseGridTables()
            .UseListExtras()
            .UseMediaLinks();

    /// <summary>
    /// Gets or sets the hash algorithm used for content hashing.
    /// </summary>
    public HashAlgorithm ContentHashAlgorithm { get; set; } = MD5.Create();

    /// <summary>
    /// Creates a <see cref="TmdRenderOptions"/> instance with templates loaded from the specified resource manager.
    /// </summary>
    /// <param name="rm">The resource manager to load templates from.</param>
    /// <returns>A <see cref="TmdRenderOptions"/> instance with resource-based templates.</returns>
    public static TmdRenderOptions FromResource(ResourceManager rm) {
        // Create default options
        var options = new TmdRenderOptions();

        // Load templates from resource if present
        options.AfterStepTemplate = rm.GetString(nameof(AfterStepTemplate)) ?? options.AfterStepTemplate;
        options.DownloadTemplate = rm.GetString(nameof(DownloadTemplate)) ?? options.DownloadTemplate;
        options.InformationTemplate = rm.GetString(nameof(InformationTemplate)) ?? options.InformationTemplate;
        options.NamedStepTemplate = rm.GetString(nameof(NamedStepTemplate)) ?? options.NamedStepTemplate;
        options.NumberedStepTemplate = rm.GetString(nameof(NumberedStepTemplate)) ?? options.NumberedStepTemplate;
        options.PlainTemplate = rm.GetString(nameof(PlainTemplate)) ?? options.PlainTemplate;
        options.TableBeginTemplate = rm.GetString(nameof(TableBeginTemplate)) ?? options.TableBeginTemplate;
        options.TableEndTemplate = rm.GetString(nameof(TableEndTemplate)) ?? options.TableEndTemplate;
        options.WarningTemplate = rm.GetString(nameof(WarningTemplate)) ?? options.WarningTemplate;

        return options;
    }

}



