using System.Collections.Generic;

namespace Altairis.Tmd.Core {
    public class TmdParserOptions {

        public string DirectiveBegin { get; set; } = "<!--";

        public string DirectiveEnd { get; set; } = "-->";

        public int DirectiveMarkupLength => this.DirectiveBegin.Length + this.DirectiveEnd.Length;

        public IEnumerable<string> StepSeparators { get; set; } = new[] { "\r\n- - -\r\n", "\n- - -\n" };

        public IEnumerable<string> LineSeparators { get; set; } = new[] { "\r\n", "\n" };

    }
}
