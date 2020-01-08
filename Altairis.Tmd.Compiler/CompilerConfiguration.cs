using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Altairis.Tmd.Core;

namespace Altairis.Tmd.Compiler {
    public class CompilerConfiguration {
        public TmdParserOptions ParserOptions { get; set; }

        public TmdRenderOptions RenderOptions { get; set; }
    }
}
