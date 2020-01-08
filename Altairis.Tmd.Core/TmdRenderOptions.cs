using System;
using System.Resources;
using System.Security.Cryptography;
using Markdig;
using System.Text.Json.Serialization;

namespace Altairis.Tmd.Core {
    public class TmdRenderOptions {

        public int FirstStepNumber { get; set; } = 1;

        public string TableBeginTemplate { get; set; } = "<table class=\"steps\">";

        public string TableEndTemplate { get; set; } = "</table>";

        public string StepTemplate { get; set; } = "<tr data-step-seqid=\"{0}\" data-step-sha256=\"{1}\">\r\n\t<th>{0}.</th>\r\n\t<td>{2}</td>\r\n</tr>";

        public string NamedStepTemplate { get; set; } = "<tr id=\"{0}\" data-step-seqid=\"{1}\" data-step-sha256=\"{2}\">\r\n\t<th>{1}.</th>\r\n\t<td>{3}</td>\r\n</tr>";

        public string InformationTemplate { get; set; } = "<tr class=\"information\">\r\n\t<th><i class=\"fal fa-info-circle\"></i></th>\r\n\t<td>{0}</td>\r\n</tr>";

        public string WarningTemplate { get; set; } = "<tr class=\"warning\">\r\n\t<th><i class=\"fal fa-exclamation-triangle\"></i></th>\r\n\t<td>{0}</td>\r\n</tr>";

        public string DownloadTemplate { get; set; } = "<tr class=\"download\">\r\n\t<th><i class=\"fal fa-file-download\"></i></th>\r\n\t<td>{0}</td>\r\n</tr>";

        public string StepLinkTemplate { get; set; } = "<a href=\"{0}\">{1}</a>";

        [JsonIgnore]
        public MarkdownPipeline MarkdownPipeline { get; set; } = new MarkdownPipelineBuilder()
                .UseAbbreviations()
                .UseCitations()
                .UseDefinitionLists()
                .UseDiagrams()
                .UseEmphasisExtras()
                .UsePipeTables()
                .UseGridTables()
                .UseListExtras()
                .UseMediaLinks()
                .Build();

        [JsonIgnore]
        public HashAlgorithm ContentHashAlgorithm { get; set; } = new SHA256Managed();

        public static TmdRenderOptions FromResource(ResourceManager rm) {
            if (rm == null) throw new ArgumentNullException(nameof(rm));

            // Create default options
            var options = new TmdRenderOptions();

            // Load templates from resource if present
            options.DownloadTemplate = rm.GetString("DownloadTemplate") ?? options.DownloadTemplate;
            options.InformationTemplate = rm.GetString("InformationTemplate") ?? options.InformationTemplate;
            options.NamedStepTemplate = rm.GetString("NamedStepTemplate") ?? options.NamedStepTemplate;
            options.StepLinkTemplate = rm.GetString("StepLinkTemplate") ?? options.StepLinkTemplate;
            options.StepTemplate = rm.GetString("StepTemplate") ?? options.StepTemplate;
            options.TableBeginTemplate = rm.GetString("TableBeginTemplate") ?? options.TableBeginTemplate;
            options.TableEndTemplate = rm.GetString("TableEndTemplate") ?? options.TableEndTemplate;
            options.WarningTemplate = rm.GetString("WarningTemplate") ?? options.WarningTemplate;

            return options;
        }

    }
}
