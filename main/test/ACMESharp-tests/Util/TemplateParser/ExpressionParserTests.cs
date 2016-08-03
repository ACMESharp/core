
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace ACMESharp.Util
{
    public class ExpressionParserTests
    {
        private readonly ITestOutputHelper _out;
        private readonly Action<string> _outWriter;

        public ExpressionParserTests(ITestOutputHelper output)
        {
            _out = output;
            _outWriter = Console.WriteLine;
        }

        [Fact]
        public void TestTemplate()
        {
            var map = new Dictionary<string, object>();

            map["var1"] = "VAR1";
            map["var2"] = 54321;
            map["now"] = (Action<IEmitContext>)(ctx =>
            {
                ctx.EmitLine("@out.Write(DateTime.Now);");
            });

            // map["now"] = 
            // map["foreach"] = (Action<IEmitContext>)(ctx => {
            //     ctx.Emit("foreach (var _xyz1234) {\r\n");
            //     ctx.Emit("}");
            // });
            // map["fun2"] = (Action<IExecContext>)(ctx => {
            // });

            var asm = typeof(TemplateParserTests).GetTypeInfo().Assembly;
            var stm = asm.GetManifestResourceStream("ACMESharp.Util.TemplateParser.expr1.str");
            var ep = new ExpressionParser(stm, map);

            ep.ParseExpression();
        }
    }
}
