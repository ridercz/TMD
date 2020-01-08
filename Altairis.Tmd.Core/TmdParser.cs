using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;

namespace Altairis.Tmd.Core {
    public class TmdParser {
        private readonly TmdParserOptions parserOptions;
        private readonly TmdRenderOptions renderOptions;

        public TmdParser(TmdParserOptions parserOptions = null, TmdRenderOptions renderOptions = null) {
            this.parserOptions = parserOptions ?? new TmdParserOptions();
            this.renderOptions = renderOptions ?? new TmdRenderOptions();
        }

        public static string RenderWithDefaults(string source) {
            var p = new TmdParser();
            return p.Render(source);
        }

        public string Render(string source) {
            if (string.IsNullOrWhiteSpace(source)) return null;

            var steps = this.ParseSteps(source);
            var html = this.RenderSteps(steps);
            return html;
        }

        protected IList<TmdStep> ParseSteps(string source) {
            var steps = new List<TmdStep>();
            if (string.IsNullOrWhiteSpace(source)) return steps;

            // Get the raw steps
            var rawSteps = source.Split(this.parserOptions.StepSeparators.ToArray(), StringSplitOptions.RemoveEmptyEntries);

            // First-stage process the raw steps
            var nextNumber = this.renderOptions.FirstStepNumber;
            foreach (var stepSource in rawSteps) {
                if (string.IsNullOrWhiteSpace(stepSource)) continue;
                var normalizedSource = stepSource.Trim();
                this.GetDirective(ref normalizedSource, out var directive);
                var step = ParseSingleStep(normalizedSource, directive);

                if (step.Type == TmdStepType.Normal) {
                    step.SeqId = nextNumber;
                    nextNumber++;
                }
                steps.Add(step);
            }
            return steps;
        }

        protected static TmdStep ParseSingleStep(string source, string directive) {
            if (string.IsNullOrEmpty(directive)) { // No directive
                return new TmdStep {
                    Type = TmdStepType.Normal,
                    SourceText = source
                };
            } else if (directive.Equals("$")) { // Plain text
                return new TmdStep {
                    Type = TmdStepType.Plain,
                    SourceText = source
                };
            } else if (directive.Equals("i", StringComparison.OrdinalIgnoreCase)) { // Information
                return new TmdStep {
                    Type = TmdStepType.Information,
                    SourceText = source
                };
            } else if (directive.Equals("!")) { // Warning
                return new TmdStep {
                    Type = TmdStepType.Warning,
                    SourceText = source
                };
            } else if (directive.Equals("dl", StringComparison.OrdinalIgnoreCase)) { // Download
                return new TmdStep {
                    Type = TmdStepType.Download,
                    SourceText = source
                };
            } else if (directive.Length > 2 && directive.StartsWith("#")) { // Named step
                var id = directive.Substring(1);
                return new TmdStep {
                    Type = TmdStepType.Normal,
                    Name = id,
                    SourceText = source
                };
            } else { // Unknown directive
                return new TmdStep {
                    Type = TmdStepType.Normal,
                    SourceText = $"**WARNING: Unknown T/MD directive `{directive}`**\r\n{source}"
                };
            }
        }

        protected string RenderSteps(IList<TmdStep> steps) {
            if (steps == null) throw new ArgumentNullException(nameof(steps));
            if (!steps.Any()) return string.Empty;

            // Prepare ouptut
            var sb = new StringBuilder();
            var tableOpen = false;

            // Render all steps
            foreach (var step in steps) {
                // Render markdown to HTML
                var src = Regex.Replace(step.SourceText, @"\[\#([0-9a-zA-Z_-]+)\]", m => {
                    var targetName = m.Groups[1].Value;
                    var targetNumber = steps.FirstOrDefault(x => targetName.Equals(x.Name, StringComparison.Ordinal))?.SeqId ?? 0;
                    return targetNumber == 0 ? m.Value : string.Format(this.renderOptions.StepLinkTemplate, "#" + targetName, targetNumber);
                });
                var html = Markdown.ToHtml(src, this.renderOptions.MarkdownPipeline).Trim();
                html = html.Replace("\r\n</code></pre>", "</code></pre>");
                html = html.Replace("\n</code></pre>", "</code></pre>");

                // Compute SHA-256 hash of generated HTML so we can track changes on client side
                var htmlHash = this.GetHashString(html);

                if (step.Type == TmdStepType.Plain) {
                    // Plain (non-table) step
                    if (tableOpen) {
                        sb.AppendLine(this.renderOptions.TableEndTemplate);
                        tableOpen = false;
                    }
                    sb.AppendLine(html);
                } else {
                    // Table steps
                    if (!tableOpen) {
                        sb.AppendLine(this.renderOptions.TableBeginTemplate);
                        tableOpen = true;
                    }

                    if (step.Type == TmdStepType.Information) {
                        sb.AppendLine(string.Format(this.renderOptions.InformationTemplate, html));
                    } else if (step.Type == TmdStepType.Warning) {
                        sb.AppendLine(string.Format(this.renderOptions.WarningTemplate, html));
                    } else if (step.Type == TmdStepType.Download) {
                        sb.AppendLine(string.Format(this.renderOptions.DownloadTemplate, html));
                    } else { // step.Type == TmdStepType.Normal
                        if (string.IsNullOrWhiteSpace(step.Name)) {
                            sb.AppendLine(string.Format(this.renderOptions.StepTemplate, step.SeqId, htmlHash, html));
                        } else {
                            sb.AppendLine(string.Format(this.renderOptions.NamedStepTemplate, step.Name, step.SeqId, htmlHash, html));
                        }
                    }
                }
            }

            // Close table if open
            if (tableOpen) sb.AppendLine(this.renderOptions.TableEndTemplate);

            // Return HTML
            return sb.ToString();
        }

        protected void GetDirective(ref string source, out string dir) {
            if (string.IsNullOrWhiteSpace(source)) {
                dir = null;
                return;
            }

            var lines = source.Split(this.parserOptions.LineSeparators.ToArray(), 2, StringSplitOptions.None);
            var dirLine = lines[0].Trim();
            if (dirLine.Length > this.parserOptions.DirectiveMarkupLength && dirLine.StartsWith(this.parserOptions.DirectiveBegin, StringComparison.Ordinal) && dirLine.EndsWith(this.parserOptions.DirectiveEnd, StringComparison.Ordinal)) {
                // Get directive content
                dir = dirLine.Substring(this.parserOptions.DirectiveBegin.Length, dirLine.Length - this.parserOptions.DirectiveMarkupLength).Trim();
                source = lines[1];
            } else if (source.StartsWith("#")) {
                dir = "$";
            } else if (source.StartsWith("(i)", StringComparison.OrdinalIgnoreCase)) {
                dir = "i";
                source = source.Substring(3).Trim();
            } else if (source.StartsWith("(!)", StringComparison.Ordinal)) {
                dir = "!";
                source = source.Substring(3).Trim();
            } else if (source.StartsWith("(dl)", StringComparison.OrdinalIgnoreCase)) {
                dir = "dl";
                source = source.Substring(4).Trim();
            } else {
                // No directive
                dir = null;
            }
        }

        protected string GetHashString(string s) {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(s));

            var data = System.Text.Encoding.UTF8.GetBytes(s);
            var hash = this.renderOptions.ContentHashAlgorithm.ComputeHash(data);
            return Convert.ToBase64String(hash).Trim('=');
        }

    }
}
