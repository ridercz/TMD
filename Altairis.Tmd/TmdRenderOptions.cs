using System.Resources;
using System.Security.Cryptography;
using Markdig;

namespace Altairis.Tmd;

public class TmdRenderOptions {

    public bool SingleTableLayout { get; set; } = false;

    public string TableBeginTemplate { get; set; } = "<table class=\"steps\">";

    public string TableEndTemplate { get; set; } = "</table>";

    public string NumberedStepTemplate { get; set; } = "<tr data-step-seqid=\"{0}\" data-step-hash=\"{1}\">\r\n\t<th>{0}.</th>\r\n\t<td>{2}</td>\r\n</tr>";

    public string NamedStepTemplate { get; set; } = "<tr id=\"{0}\" data-step-seqid=\"{1}\" data-step-hash=\"{2}\">\r\n\t<th>{1}.</th>\r\n\t<td>{3}</td>\r\n</tr>";

    public string InformationTemplate { get; set; } = "<tr class=\"information\">\r\n\t<th>&#x1F6C8;</th>\r\n\t<td>{0}</td>\r\n</tr>";

    public string WarningTemplate { get; set; } = "<tr class=\"warning\">\r\n\t<th>&#x26A0;</th>\r\n\t<td>{0}</td>\r\n</tr>";

    public string DownloadTemplate { get; set; } = "<tr class=\"download\">\r\n\t<th>&#x1F5AB;</th>\r\n\t<td>{0}</td>\r\n</tr>";

    public string PlainTemplate { get; set; } = "<tr class=\"plain\">\r\n\t<td colspan=\"2\">{0}</td>\r\n</tr>";

    public string AfterStepTemplate { get; set; } = string.Empty;

    public MarkdownPipelineBuilder MarkdownPipelineBuilder { get; set; } = new MarkdownPipelineBuilder()
            .UseEmphasisExtras()
            .UsePipeTables()
            .UseGridTables()
            .UseListExtras()
            .UseMediaLinks();

    public HashAlgorithm ContentHashAlgorithm { get; set; } = MD5.Create();

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



