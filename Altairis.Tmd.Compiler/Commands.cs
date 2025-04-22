namespace Altairis.Tmd.Compiler;
using NConsoler;

public static class Commands {

    private const int ErrorlevelSuccess = 0;
    private const int ErrorlevelFailure = 1;

    [Action(Description = "Compile TMD document to HTML")]
    public static void Compile(
        [Required] string path,
        [Optional(null, "t", Description = "Path to HTML template file")] string htmlTemplateFile,
        [Optional("<!--body-->", "tb", Description = "Placeholder where to put the compiled markup")] string templatePlaceholder,
        [Optional(false, Description = "Debug mode")] bool debug) {

        // Set debug mode
        try {
            // Load HTML template
            var htmlTemplate = Properties.Resources.DefaultTemplate;
            if (!string.IsNullOrEmpty(htmlTemplateFile)) {
                Console.Write("Loading HTML template...");
                htmlTemplate = File.ReadAllText(htmlTemplateFile);
                if (!htmlTemplate.Contains(templatePlaceholder)) {
                    Console.WriteLine("Failed!");
                    Console.WriteLine("Template file does not contain placeholder '{0}'.", templatePlaceholder);
                    Environment.Exit(ErrorlevelFailure);
                    return;
                }
                Console.WriteLine("OK");
            }

            // Check if path is file or directory
            if (File.Exists(path)) {
                // Compile single file
                CompileFile(path, htmlTemplate, templatePlaceholder);
            } else if (Directory.Exists(path)) {
                // Compile all files in directory
                var files = Directory.GetFiles(path, "*.md");
                if (files.Length == 0) {
                    Console.WriteLine("No *.md files found in directory '{0}'.", path);
                    Environment.Exit(ErrorlevelFailure);
                    return;
                }
                foreach (var file in files) {
                    CompileFile(file, htmlTemplate, templatePlaceholder);
                }
            } else {
                Console.WriteLine("Path '{0}' does not exist.", path);
                Environment.Exit(ErrorlevelFailure);
                return;
            }
            Console.WriteLine("Program terminated successfully.");
            Environment.Exit(ErrorlevelSuccess);
        } catch (Exception ex) when (!debug) {
            Console.WriteLine("Failed!");
            Console.WriteLine(ex.Message);
            Environment.Exit(ErrorlevelFailure);
            return;
        }
    }

    private static void CompileFile(string path, string htmlTemplate, string templatePlaceholder) {
        var outputPath = path + ".html";
        var doc = new TmdDocument();

        Console.WriteLine($"Processing {path}:");

        if (File.Exists(outputPath)) {
            Console.Write("  Deleting old output file...");
            File.Delete(outputPath);
            Console.WriteLine("OK");
        }

        Console.Write("  Parsing...");
        var result = doc.LoadFile(path);
        if (result) {
            Console.WriteLine("OK");
        } else {
            Console.WriteLine($"{doc.Warnings.Count} warnings");
        }

        Console.Write("  Rendering...");
        result = doc.RenderHtml(out var html);
        if (result) {
            Console.WriteLine("OK");
        } else {
            Console.WriteLine($"{doc.Warnings.Count} warnings");
        }

        Console.Write("  Saving...");
        File.WriteAllText(outputPath, htmlTemplate.Replace(templatePlaceholder, html));
        Console.WriteLine("OK");

        if (doc.Warnings.Count > 0) {
            Console.WriteLine("  Warnings:");
            foreach (var warning in doc.Warnings) {
                Console.WriteLine("    " + warning.ToString());
            }
        }

    }

}
