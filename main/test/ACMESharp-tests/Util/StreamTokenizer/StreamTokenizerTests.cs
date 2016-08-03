using System;
using Xunit;
using System.Reflection;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace ACMESharp.Util
{
    //public 
    class StreamTokenizerTests
    {
        private readonly ITestOutputHelper _out;
        private readonly Action<string> _outWriter;

        public StreamTokenizerTests(ITestOutputHelper output)
        {
            _out = output;
            _outWriter = Console.WriteLine;
        }

        [Fact]
        public void TestComments()
        {
            var res = "ACMESharp.Util.StreamTokenizer.input.txt";
            var asm = typeof(StreamTokenizerTests).GetTypeInfo().Assembly;

            int slashIsCommentStart = 1;
            int slashSlashComment = 2;
            int slashStarComment = 4;

            for (int i = 0; i < 8; i++)
            {
                using (var stm = asm.GetManifestResourceStream(res))
                {
                    var st = new StreamTokenizer(stm);

                    /* decide the state of this run */
                    bool slashCommentFlag = ((i & slashIsCommentStart) != 0);
                    bool slashSlashCommentFlag = ((i & slashSlashComment) != 0);
                    bool slashStarCommentFlag = ((i & slashStarComment) != 0);

                    /* set the initial state of the tokenizer */
                    if (!slashCommentFlag)
                    {
                        st.OrdinaryChars('/', '/');
                    }
                    st.SlashSlashComments(slashSlashCommentFlag);
                    st.SlashStarComments(slashStarCommentFlag);

                    /* now go throgh the input file */
                    while (st.NextToken() != StreamTokenizer.TT_EOF)
                    {
                        String token = st.sval;
                        if (token == null)
                        {
                            continue;
                        }
                        else
                        {
                            if ((token.CompareTo("Error1") == 0) && slashStarCommentFlag)
                            {
                                throw new Exception("Failed to pass one line C comments!");
                            }
                            if ((token.CompareTo("Error2") == 0) && slashStarCommentFlag)
                            {
                                throw new Exception("Failed to pass multi line C comments!");
                            }
                            if ((token.CompareTo("Error3") == 0) && slashSlashCommentFlag)
                            {
                                throw new Exception("Failed to pass C++ comments!");
                            }
                            if ((token.CompareTo("Error4") == 0) && slashCommentFlag)
                            {
                                throw new Exception("Failed to pass / comments!");
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public void TestNullConstruct()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                StreamTokenizer st = new StreamTokenizer(null);
            });
        }

        [Fact]
        public void TestQuote()
        {
            const string testStr = "token1 token2 \"The test string\" token4";

            _outWriter("Parsing String: " + testStr);
            StreamTokenizer st = new StreamTokenizer(new MemoryStream(
                    Encoding.UTF8.GetBytes(testStr)));
            bool foundToken = false;
            String matchStr = null;
            while (st.NextToken() != StreamTokenizer.TT_EOF)
            {
                switch (st.ttype)
                {
                    case '\"':
                        foundToken = true;
                        matchStr = st.ToString();
                        _outWriter("Found token " + matchStr);
                        break;
                    default:
                        _outWriter("Found token " + st);
                        break;
                }
            }
            if (!foundToken)
                throw new Exception("Test failed to recognize Quote type");
            if (!matchStr.Equals("Token[The test string], line 1"))
                throw new Exception("Test failed parse quoted string");

        }

        [Fact]
        public void TestReadAhead()
        {
            foreach (var s in new[] { "foo\nx", "foo\r\nx" })
            {
                _outWriter("Testing:");
                _outWriter(s);
                using (var lis = new LimitedInputStream(s, 4))
                {
                    var st = new StreamTokenizer(lis);
                    st.EolIsSignificant(true);

                    int tt = st.NextToken();
                    Assert.Equal(StreamTokenizer.TT_WORD, tt);
                    Assert.Equal("foo", st.sval);

                    tt = st.NextToken();
                    Assert.Equal(StreamTokenizer.TT_EOL, tt);
                }
            }
        }

        [Fact]
        public void TestReset()
        {
            var stm = new MemoryStream(Encoding.UTF8.GetBytes("[ #"));
            var scan = new StreamTokenizer(stm);

            scan.NextToken();
            scan.NextToken();

            stm.Seek(0, SeekOrigin.Begin);
            int token = scan.NextToken();
            Assert.Equal('[', token);
        }

        class LimitedInputStream : MemoryStream
        {

            private String input;
            private int limit;      /* Do not allow input[limit] to be read */
            private int next = 0;

            public LimitedInputStream(string input, int limit)
                : base(Encoding.UTF8.GetBytes(input))
            {
                this.input = input;
                this.limit = limit;
            }

            public override int ReadByte()
            {
                if (next >= limit)
                    throw new IOException("Attempted to read too far in stream");

                var b = base.ReadByte();
                if (b != 0)
                    ++next;
                return b;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                count = 1;
                if (next + count >= limit)
                    throw new IOException("Attempted to read too far in stream");

                var n = base.Read(buffer, offset, count);
                next += n;
                return n;
            }
        }
    }
}
