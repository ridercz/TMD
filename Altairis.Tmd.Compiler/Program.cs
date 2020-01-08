using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Altairis.Tmd.Core;
using NConsoler;

namespace Altairis.Tmd.Compiler {
    internal class Program {
        private const int ERRORLEVEL_SUCCESS = 0;
        private const int ERRORLEVEL_FAILURE = 1;

        private static bool debugMode;

        private static void Main(string[] args) {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"TMDC: Tutorial Markdown Compiler version {version:4}");
            Console.WriteLine("Copyright (c) Michal Altair Valasek - Altairis, 2019-2020");
            Console.WriteLine("www.altairis.cz | www.rider.cz | github.com/ridercz/TMD");
            Console.WriteLine();
            Consolery.Run();
        }

        [Action("Compile TMD file to HTML")]
        public static void Compile(
            [Required(Description = "Input file name")] string inputFileName,
            [Optional(null, "o", Description = "Output file name")] string outputFileName,
            [Optional(false, "os", Description = "Open output file in default browser")] bool startOutputFile,
            [Optional(null, "c", Description = "Path to configuration file")] string configFileName,
            [Optional(null, "t", Description = "Document template file name")] string templateFileName,
            [Optional("<!--body-->", "tp", Description = "Template placeholder for body")] string templatePlaceholder,
            [Optional(false, Description = "Show detailed exception messages")] bool debug
            ) {

            debugMode = debug;

            // Read or write configuration
            var renderOptions = new TmdRenderOptions {
                InformationTemplate = "<tr class=\"information\">\r\n\t<th>&#9432;</th>\r\n\t<td>{0}</td>\r\n</tr>",
                WarningTemplate = "<tr class=\"warning\">\r\n\t<th>&#9888;</th>\r\n\t<td>{0}</td>\r\n</tr>",
                DownloadTemplate = "<tr class=\"download\">\r\n\t<th>&#128426;</th>\r\n\t<td>{0}</td>\r\n</tr>"
            };
            var parserOptions = new TmdParserOptions();
            var config = new CompilerConfiguration { RenderOptions = renderOptions, ParserOptions = parserOptions };

            if (!string.IsNullOrWhiteSpace(configFileName)) {
                if (File.Exists(configFileName)) {
                    TryDo(
                        $"Reading configuration from {configFileName}...",
                        () => {
                            var json = File.ReadAllText(configFileName);
                            config = JsonSerializer.Deserialize<CompilerConfiguration>(json);
                        }
                    );
                } else {
                    TryDo(
                        $"Writing default configuration to {configFileName}...",
                        () => {
                            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(configFileName, json);
                        }
                    );
                }
            }

            // Read source
            var source = TryDo(
                $"Reading source from {inputFileName}...",
                () => File.ReadAllText(inputFileName)
            );

            // Read template
            var templateHtml = Properties.Resources.DefaultTemplate;
            if (!string.IsNullOrWhiteSpace(templateFileName)) TryDo(
                $"Reading template from {templateFileName}...",
                () => File.ReadAllText(templateFileName)
            );

            // Compile
            var parser = new TmdParser(config.ParserOptions, config.RenderOptions);
            var html = TryDo(
                "Compiling...",
                () => parser.Render(source)
            );

            // Use template
            html = templateHtml.Replace(templatePlaceholder, html);

            // Save to output file
            if (string.IsNullOrWhiteSpace(outputFileName)) outputFileName = Path.Combine(Path.GetDirectoryName(inputFileName), Path.GetFileNameWithoutExtension(inputFileName) + ".html");
            TryDo(
                $"Saving to {outputFileName}...",
                () => File.WriteAllText(outputFileName, html)
            );

            // Start output file
            if (startOutputFile) TryDo(
                  "Opening output file...",
                  () => Process.Start(new ProcessStartInfo { FileName = outputFileName, UseShellExecute = true })
              ); ;

            Environment.Exit(ERRORLEVEL_SUCCESS);
        }

        private static T TryDo<T>(string message, Func<T> func) {
            try {
                Console.Write(message);
                var result = func();
                Console.WriteLine("OK");
                return result;
#pragma warning disable CA1031 // Do not catch general exception types
            } catch (Exception ex) {
                CrashExit("FAILED: {0}", ex);
                return default;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private static void TryDo(string message, Action action) {
            try {
                Console.Write(message);
                action();
                Console.WriteLine("OK");
#pragma warning disable CA1031 // Do not catch general exception types
            } catch (Exception ex) {
                CrashExit("FAILED: {0}", ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private static void CrashExit(string message, Exception ex = null) {
            if (ex == null) {
                Console.Error.WriteLine(message);
            } else {
                Console.Error.WriteLine(message, ex.Message);
                if (debugMode) {
                    Console.Error.WriteLine(new string('-', Console.BufferWidth));
                    Console.Error.WriteLine(ex.ToString());
                    Console.Error.WriteLine(new string('-', Console.BufferWidth));
                }
            }

            Console.Error.WriteLine("Program execution terminated.");
            Environment.Exit(ERRORLEVEL_FAILURE);
        }

    }
}
