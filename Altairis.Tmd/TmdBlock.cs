namespace Altairis.Tmd;

public enum TmdBlockType { Empty, NumberedStep, PlainText, Information, Warning, Download }

/// <summary>
/// Represents a block of content in the TMD (Tutorial MarkDown).
/// </summary>
public class TmdBlock {

    /// <summary>
    /// Gets or sets the type of the block.
    /// </summary>
    public TmdBlockType Type { get; set; } = TmdBlockType.Empty;

    /// <summary>
    /// Gets or sets the step number for numbered steps. 
    /// This property is relevant only when <see cref="Type"/> is <see cref="TmdBlockType.NumberedStep"/>.
    /// </summary>
    public int StepNumber { get; set; }

    /// <summary>
    /// Gets or sets the starting line number of the block in the source document.
    /// </summary>
    public int StartingLineNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the name of the block. 
    /// This can be used to identify or label the block.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the Markdown content of the block.
    /// </summary>
    public string Markdown { get; set; } = string.Empty;

}
