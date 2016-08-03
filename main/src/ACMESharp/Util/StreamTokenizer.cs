using System;
using System.IO;

namespace ACMESharp.Util
{
    public class StreamTokenizer
    {
        private const int NEED_CHAR = int.MaxValue;
        private const int SKIP_LF = int.MaxValue - 1;

        private const byte CT_WHITESPACE = 1;
        private const byte CT_DIGIT = 2;
        private const byte CT_ALPHA = 4;
        private const byte CT_QUOTE = 8;
        private const byte CT_COMMENT = 16;

        public const int TT_EOF = -1;
        public const int TT_EOL = '\n';
        public const int TT_NUMBER = -2;
        public const int TT_WORD = -3;
        public const int TT_NOTHING = -4;

        private Stream _stream = null;

        private char[] _buf = new char[20];

        private int _peekc = NEED_CHAR;

        private bool _pushedBack;
        private bool _forceLower;
        private int _lineNumber = 1;

        private bool _eolIsSignificant = false;
        private bool _slashSlashComments = false;
        private bool _slashStarComments = false;

        private byte[] _ctype = new byte[256];

        public int ttype = TT_NOTHING;
        public string sval;
        public double nval;

        private StreamTokenizer()
        {
            WordChars('a', 'z');
            WordChars('A', 'Z');
            WordChars(128 + 32, 255);
            WhitespaceChars(0, ' ');
            CommentChar('/');
            QuoteChar('"');
            QuoteChar('\'');
            ParseNumbers();
        }

        public StreamTokenizer(Stream s) : this()
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            _stream = s;
        }

        public void ResetSyntax()
        {
            for (int i = _ctype.Length; --i >= 0;)
                _ctype[i] = 0;
        }

        public void WordChars(int lwr, int upr)
        {
            SetCType(lwr, upr, CT_ALPHA);
        }

        public void WhitespaceChars(int lwr, int upr)
        {
            SetCType(lwr, upr, CT_WHITESPACE);
        }

        public void OrdinaryChars(int lwr, int upr)
        {
            SetCType(lwr, upr, 0);
        }

        public void CommentChar(int chr)
        {
            SetCType(chr, CT_COMMENT);
        }

        public void QuoteChar(int chr)
        {
            SetCType(chr, CT_QUOTE);
        }

        public void ParseNumbers()
        {
            SetCType('0', '9', CT_DIGIT);
            SetCType('.', CT_DIGIT);
            SetCType('-', CT_DIGIT);
        }

        public void EolIsSignificant(bool flag)
        {
            _eolIsSignificant = flag;
        }

        public void SlashStarComments(bool flag)
        {
            _slashStarComments = flag;
        }

        public void SlashSlashComments(bool flag)
        {
            _slashSlashComments = flag;
        }

        public void LowerCaseMode(bool flag)
        {
            _forceLower = flag;
        }

        public void PushBack()
        {
            if (ttype != TT_NOTHING)
                _pushedBack = true;
        }

        public int LineNumber()
        {
            return _lineNumber;
        }

        public int NextToken()
        {
            if (_pushedBack)
            {
                _pushedBack = false;
                return ttype;
            }
            byte[] ct = _ctype;
            sval = null;

            int c = _peekc;
            if (c < 0)
                c = NEED_CHAR;
            if (c == SKIP_LF)
            {
                c = Read();
                if (c < 0)
                    return ttype = TT_EOF;
                if (c == '\n')
                    c = NEED_CHAR;
            }
            if (c == NEED_CHAR)
            {
                c = Read();
                if (c < 0)
                    return ttype = TT_EOF;
            }
            ttype = c;              /* Just to be safe */

            /* Set peekc so that the next invocation of nextToken will read
             * another character unless peekc is reset in this invocation
             */
            _peekc = NEED_CHAR;

            int ctype = c < 256 ? ct[c] : CT_ALPHA;
            while ((ctype & CT_WHITESPACE) != 0)
            {
                if (c == '\r')
                {
                    _lineNumber++;
                    if (_eolIsSignificant)
                    {
                        _peekc = SKIP_LF;
                        return ttype = TT_EOL;
                    }
                    c = Read();
                    if (c == '\n')
                        c = Read();
                }
                else
                {
                    if (c == '\n')
                    {
                        _lineNumber++;
                        if (_eolIsSignificant)
                        {
                            return ttype = TT_EOL;
                        }
                    }
                    c = Read();
                }
                if (c < 0)
                    return ttype = TT_EOF;
                ctype = c < 256 ? ct[c] : CT_ALPHA;
            }

            if ((ctype & CT_DIGIT) != 0)
            {
                bool neg = false;
                if (c == '-')
                {
                    c = Read();
                    if (c != '.' && (c < '0' || c > '9'))
                    {
                        _peekc = c;
                        return ttype = '-';
                    }
                    neg = true;
                }
                double v = 0;
                int decexp = 0;
                int seendot = 0;
                while (true)
                {
                    if (c == '.' && seendot == 0)
                        seendot = 1;
                    else if ('0' <= c && c <= '9')
                    {
                        v = v * 10 + (c - '0');
                        decexp += seendot;
                    }
                    else
                        break;
                    c = Read();
                }
                _peekc = c;
                if (decexp != 0)
                {
                    double denom = 10;
                    decexp--;
                    while (decexp > 0)
                    {
                        denom *= 10;
                        decexp--;
                    }
                    /* Do one division of a likely-to-be-more-accurate number */
                    v = v / denom;
                }
                nval = neg ? -v : v;
                return ttype = TT_NUMBER;
            }

            if ((ctype & CT_ALPHA) != 0)
            {
                int i = 0;
                do
                {
                    if (i >= _buf.Length)
                    {
                        var newBuf = new char[_buf.Length * 2];
                        Array.Copy(_buf, newBuf, _buf.Length);
                        _buf = newBuf;
                    }
                    _buf[i++] = (char)c;
                    c = Read();
                    ctype = c < 0 ? CT_WHITESPACE : c < 256 ? ct[c] : CT_ALPHA;
                } while ((ctype & (CT_ALPHA | CT_DIGIT)) != 0);
                _peekc = c;
                sval = new String(_buf, 0, i);
                if (_forceLower)
                    sval = sval.ToLower();
                return ttype = TT_WORD;
            }

            if ((ctype & CT_QUOTE) != 0)
            {
                ttype = c;
                int i = 0;
                /* Invariants (because \Octal needs a lookahead):
                 *   (i)  c contains char value
                 *   (ii) d contains the lookahead
                 */
                int d = Read();
                while (d >= 0 && d != ttype && d != '\n' && d != '\r')
                {
                    if (d == '\\')
                    {
                        c = Read();
                        int first = c;   /* To allow \377, but not \477 */
                        if (c >= '0' && c <= '7')
                        {
                            c = c - '0';
                            int c2 = Read();
                            if ('0' <= c2 && c2 <= '7')
                            {
                                c = (c << 3) + (c2 - '0');
                                c2 = Read();
                                if ('0' <= c2 && c2 <= '7' && first <= '3')
                                {
                                    c = (c << 3) + (c2 - '0');
                                    d = Read();
                                }
                                else
                                    d = c2;
                            }
                            else
                                d = c2;
                        }
                        else
                        {
                            switch (c)
                            {
                                case 'a':
                                    c = 0x7;
                                    break;
                                case 'b':
                                    c = '\b';
                                    break;
                                case 'f':
                                    c = 0xC;
                                    break;
                                case 'n':
                                    c = '\n';
                                    break;
                                case 'r':
                                    c = '\r';
                                    break;
                                case 't':
                                    c = '\t';
                                    break;
                                case 'v':
                                    c = 0xB;
                                    break;
                            }
                            d = Read();
                        }
                    }
                    else
                    {
                        c = d;
                        d = Read();
                    }
                    if (i >= _buf.Length)
                    {
                        var newBuf = new char[_buf.Length * 2];
                        Array.Copy(_buf, newBuf, _buf.Length);
                        _buf = newBuf;
                    }
                    _buf[i++] = (char)c;
                }

                /* If we broke out of the loop because we found a matching quote
                 * character then arrange to read a new character next time
                 * around; otherwise, save the character.
                 */
                _peekc = (d == ttype) ? NEED_CHAR : d;

                sval = new String(_buf, 0, i);
                return ttype;
            }

            if (c == '/' && (_slashSlashComments || _slashStarComments))
            {
                c = Read();
                if (c == '*' && _slashStarComments)
                {
                    int prevc = 0;
                    while ((c = Read()) != '/' || prevc != '*')
                    {
                        if (c == '\r')
                        {
                            _lineNumber++;
                            c = Read();
                            if (c == '\n')
                            {
                                c = Read();
                            }
                        }
                        else
                        {
                            if (c == '\n')
                            {
                                _lineNumber++;
                                c = Read();
                            }
                        }
                        if (c < 0)
                            return ttype = TT_EOF;
                        prevc = c;
                    }
                    return NextToken();
                }
                else if (c == '/' && _slashSlashComments)
                {
                    while ((c = Read()) != '\n' && c != '\r' && c >= 0) ;
                    _peekc = c;
                    return NextToken();
                }
                else
                {
                    /* Now see if it is still a single line comment */
                    if ((ct['/'] & CT_COMMENT) != 0)
                    {
                        while ((c = Read()) != '\n' && c != '\r' && c >= 0) ;
                        _peekc = c;
                        return NextToken();
                    }
                    else
                    {
                        _peekc = c;
                        return ttype = '/';
                    }
                }
            }

            if ((ctype & CT_COMMENT) != 0)
            {
                while ((c = Read()) != '\n' && c != '\r' && c >= 0) ;
                _peekc = c;
                return NextToken();
            }

            return ttype = c;
        }

        /** Read the next character */
        private int Read()
        {
            if (_stream != null)
                return _stream.ReadByte();
            else
                throw new InvalidOperationException();
        }
        
        public override string ToString()
        {
            string ret;
            switch (ttype)
            {
                case TT_EOF:
                    ret = "EOF";
                    break;
                case TT_EOL:
                    ret = "EOL";
                    break;
                case TT_WORD:
                    ret = sval;
                    break;
                case TT_NUMBER:
                    ret = "n=" + nval;
                    break;
                case TT_NOTHING:
                    ret = "NOTHING";
                    break;
                default:
                    {
                        /*
                         * ttype is the first character of either a quoted string or
                         * is an ordinary character. ttype can definitely not be less
                         * than 0, since those are reserved values used in the previous
                         * case statements
                         */
                        if (ttype < 256 &&
                            ((_ctype[ttype] & CT_QUOTE) != 0))
                        {
                            ret = sval;
                            break;
                        }

                        char[] s = new char[3];
                        s[0] = s[2] = '\'';
                        s[1] = (char)ttype;
                        ret = new String(s);
                        break;
                    }
            }
            return "Token[" + ret + "], line " + _lineNumber;
        }

        private void SetCType(int lwr, int upr, byte ct, bool bitwiseOr = false)
        {
            if (lwr < 0)
                lwr = 0;
            if (upr >= _ctype.Length)
                upr = _ctype.Length - 1;
            while (lwr <= upr)
                if (bitwiseOr)
                    _ctype[lwr++] |= ct;
                else
                    _ctype[lwr++] = ct;
        }

        private void SetCType(int chr, byte ct)
        {
            if (chr >= 0 && chr < _ctype.Length)
                _ctype[chr] = ct;
        }
    }
}
