using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static ACMESharp.Util.StreamTokenizer;

namespace ACMESharp.Util
{
    public class ExpressionParser
    {
        private Stream _stream;
        private IDictionary<string, object> _map;

        private StreamTokenizer _tok;
        private Action<string> _outWriter;

        private StringBuilder _buff;

         public ExpressionParser(Stream stream, IDictionary<string, object> map,
                Action<string> outWriter = null)
        {
            _stream = stream;
            _map = map;

            _tok = new StreamTokenizer(_stream);
            //_tok.ResetSyntax();

            if (outWriter == null)
                outWriter = Console.WriteLine;
            _outWriter = outWriter;
            _buff = new StringBuilder();
        }
        public string ParseExpression()
        {
            while (_tok.NextToken() != TT_EOF)
            {
                _outWriter($"TOK: [{_tok.ttype}][{_tok.sval}][{_tok.nval}]");
            }

            return string.Empty;
        }
    }
}