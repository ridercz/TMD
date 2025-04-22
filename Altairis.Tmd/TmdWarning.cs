namespace Altairis.Tmd;

public enum TmdWarningType {
    ContentIsEmpty,
    EmptyBlockName,
    DuplicateBlockName,
    UnknownQualifier,
    EmptyQualifier,
    UnknownBlockNameLink,
    Exception
}

public record TmdWarning(int BlockNumber, int LineNumber, TmdWarningType Type, string? ContextValue = null) {

    public override string ToString() => this.Type switch {
        TmdWarningType.ContentIsEmpty => $"Empty block {this.BlockNumber} starting at line {this.LineNumber}",
        TmdWarningType.EmptyBlockName => $"Empty name of block {this.BlockNumber} starting at line {this.LineNumber}",
        TmdWarningType.DuplicateBlockName => $"Duplicate name '{this.ContextValue}' of block {this.BlockNumber} starting at line {this.LineNumber}",
        TmdWarningType.UnknownQualifier => $"Unknown qualifier '{this.ContextValue}' in block {this.BlockNumber} starting at line {this.LineNumber}",
        TmdWarningType.EmptyQualifier => $"Empty qualifier in block {this.BlockNumber} starting at line {this.LineNumber}",
        TmdWarningType.UnknownBlockNameLink => $"Unknown link to block named '{this.ContextValue}' in block {this.BlockNumber} starting at line {this.LineNumber}",
        TmdWarningType.Exception => $"Exception '{this.ContextValue}' in block {this.BlockNumber} starting at line {this.LineNumber}",
        _ => this.ContextValue == null
            ? $"{this.Type} in block {this.BlockNumber} starting at line {this.LineNumber}"
            : $"{this.Type} ({this.ContextValue}) in block {this.BlockNumber} starting at line {this.LineNumber}",
    };

}

