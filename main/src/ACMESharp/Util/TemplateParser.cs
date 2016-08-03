using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static ACMESharp.Util.StreamTokenizer;

namespace ACMESharp.Util
{
    public class TemplateParser : IEmitContext
    {
        private Stream _stream;
        private IDictionary<string, object> _map;

        private StreamTokenizer _tok;
        private Action<string> _outWriter;

        private StringBuilder _buff;

        private int _indentSize = 0;
        private string _indent = "";

        public TemplateParser(Stream stream, IDictionary<string, object> map,
                Action<string> outWriter = null)
        {
            _stream = stream;
            _map = map;

            _tok = new StreamTokenizer(_stream);
            _tok.ResetSyntax();

            if (outWriter == null)
                outWriter = Console.WriteLine;
            _outWriter = outWriter;
            _buff = new StringBuilder();
        }

        public void Emit(string fmt, params object[] args)
        {
            _buff.AppendFormat(fmt, args);
        }

        public void EmitLine(string fmt, params object[] args)
        {
            _buff.AppendLine(string.Format(fmt, args)).Append(_indent);
        }

        public void Indent()
        {
            ++_indentSize;
            _indent = string.Empty.PadLeft(_indentSize * 4);
        }

        public void Outdent()
        {
            if (_indentSize > 0)
                --_indentSize;
            _indent = string.Empty.PadLeft(_indentSize * 4);
        }

        public string ParseStream()
        {
            EmitLine(String.Empty);
            EmitLine("var @out = new TextWriter();");
            Emit("@out.Write(\"");

            bool eolLast = false;

            while (_tok.NextToken() != TT_EOF)
            {
                if (_tok.ttype == '\r')
                {
                    Emit("\\r");
                    eolLast = true;
                    continue;
                }

                if (_tok.ttype == '\n')
                {
                    Emit("\\n");
                    eolLast = true;
                    continue;
                }

                if (eolLast)
                {
                    EmitLine("\" +");
                    Indent();
                    Emit("\"");
                    Outdent();
                    eolLast = false;
                }

                if (_tok.ttype == '$')
                {
                    EnableWords(true);
                    switch (_tok.NextToken())
                    {
                        case TT_EOF:
                        case '$':
                            Emit("$");
                            EnableWords(false);
                            break;

                        case '{':
                            EnableWords(false);
                            EmitLine("\");");
                            Emit("Expression(\"");
                            while (_tok.NextToken() != TT_EOF)
                            {
                                if (_tok.ttype == '}')
                                {
                                    EmitLine("\");");
                                    Emit("@out.Write(\"");
                                    break;
                                }

                                if (_tok.ttype == '\r')
                                    _buff.Append("\\r");
                                else if (_tok.ttype == '\n')
                                    _buff.Append("\\n");
                                else
                                    _buff.Append((char)_tok.ttype);
                            }
                            break;

                        case TT_WORD:
                            EnableWords(false);
                            if (char.IsLetter(_tok.sval[0]))
                            {
                                var mapKey = _tok.sval;
                                var mapVal = (object)null;
                                var emitVal = $"Variable(\"{mapKey}\");";

                                EmitLine("\");");
                                if (_map.TryGetValue(mapKey, out mapVal))
                                {
                                    if (mapVal is Action<IEmitContext>)
                                    {
                                        ((Action<IEmitContext>)mapVal)(this);
                                    }
                                    else
                                    {
                                        EmitLine(emitVal);
                                    }
                                }
                                else
                                {
                                    EmitLine(emitVal);
                                }

                                Emit("@out.Write(\"");
                            }
                            else
                            {
                                _buff.Append("$").Append(_tok.sval);
                            }
                            break;

                        default:
                            _buff.Append("$").Append((char)_tok.ttype);
                            break;
                    }
                }
                else
                {
                    _buff.Append((char)_tok.ttype);
                }
            }

            EmitLine("\");");

            return _buff.ToString();
        }

        public string ParseExpression(Stream stm)
        {
            var buff = new StringBuilder();
            var st = new StreamTokenizer(stm);

            return buff.ToString();
        }
        private void EnableWords(bool flag)
        {
            if (flag)
            {
                _tok.WordChars('a', 'z');
                _tok.WordChars('A', 'Z');
                _tok.WordChars('0', '9');
                _tok.WordChars('.', '.');
                _tok.WordChars(':', ':');
            }
            else
            {
                _tok.OrdinaryChars('a', 'z');
                _tok.OrdinaryChars('A', 'Z');
                _tok.OrdinaryChars('0', '9');
                _tok.OrdinaryChars('.', '.');
                _tok.OrdinaryChars(':', ':');
            }
        }

        public void Foo()
        {
            switch (_tok.ttype)
            {
                case StreamTokenizer.TT_EOL:
                    _outWriter($"TT_EOL: [{_tok.ttype}][{_tok.sval?.ToCharArray()}]");
                    break;
                case StreamTokenizer.TT_WORD:
                    _outWriter($"TT_WORD: [{_tok.ttype}][{_tok.sval}]");
                    break;
                case StreamTokenizer.TT_NUMBER:
                    _outWriter($"TT_NUMBER: [{_tok.ttype}][{_tok.sval}][{_tok.nval}]");
                    break;
                case StreamTokenizer.TT_NOTHING:
                    throw new Exception("Unexpected TT_NOTHING");

                default:
                    _outWriter($"DEFAULT: [{_tok.ttype}]");
                    break;
            }
        }
    }

    public interface IEmitContext
    {
        void Emit(string fmt, params object[] args);
        void EmitLine(string fmt, params object[] args);
        void Indent();
        void Outdent();
    }

    public interface IExecContext
    {

    }
}