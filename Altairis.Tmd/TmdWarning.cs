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

public record TmdWarning(int BlockNumber, TmdWarningType Type, string? ContextValue = null);
