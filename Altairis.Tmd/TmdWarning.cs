namespace Altairis.Tmd;

public enum TmdWarningType {
    ContentIsEmpty,
    EmptyBlockName,
    DuplicateBlockName,
    UnknownQualifier,
    UnknownBlockNameLink,
}

public record TmdWarning(int BlockNumber, TmdWarningType Type);
