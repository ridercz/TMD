namespace Altairis.Tmd;

public enum TmdWarningType {
    ContentIsEmpty,
    EmptyBlockName,
    DuplicateBlockName,
    UnknownQualifier
}

public record TmdWarning(int BlockNumber, TmdWarningType Type);
