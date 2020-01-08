namespace Altairis.Tmd.Core {

    public enum TmdStepType {
        Normal = 0,
        Plain = 1,
        Information = 2,
        Warning = 3,
        Download = 4
    }

    public class TmdStep {

        public int SeqId { get; set; }

        public TmdStepType Type { get; set; }

        public string Name { get; set; }

        public string SourceText { get; set; }

    }
}
