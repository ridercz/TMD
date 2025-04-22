namespace Altairis.Tmd;

public enum TmdBlockType { Empty, NumberedStep, PlainText, Information, Warning, Download }

public class TmdBlock {

    public TmdBlockType Type { get; set; } = TmdBlockType.Empty;

    public int StepNumber { get; set; }

    public int StartingLineNumber { get; set; } = 1;

    public string? Name { get; set; }

    public string Markdown { get; set; } = string.Empty;

}