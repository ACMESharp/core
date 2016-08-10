using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;

namespace PKISharp.Asn1
{
    public class Asn1Tests
    {
        static TestData<bool>[] BoolTestData = new[]
        {
            new TestData<bool>(new byte[] { 0x00 }, true, false ),
            new TestData<bool>(new byte[] { 0xff }, true, true ),
            new TestData<bool>(new byte[] { 0x00, 0x00 }, false, false ),
            new TestData<bool>(new byte[] { 0xff, 0xff }, false, false ),
            new TestData<bool>(new byte[] { 0x01 }, false, false ),
        };

        static TestData<long>[] LongTestData = new []
        {
            new TestData<long>(new byte[] { 0x00 }, true, 0 ),
            new TestData<long>(new byte[] { 0x7f }, true, 127 ),
            new TestData<long>(new byte[] { 0x00, 0x80 }, true, 128 ),
            new TestData<long>(new byte[] { 0x01, 0x00 }, true, 256 ),
            new TestData<long>(new byte[] { 0x80 }, true, -128 ),
            new TestData<long>(new byte[] { 0xff, 0x7f }, true, -129 ),
            new TestData<long>(new byte[] { 0xff}, true, -1 ),
            new TestData<long>(new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, true, -9223372036854775808 ),
            new TestData<long>(new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, false, 0 ),
            new TestData<long>(new byte[] { }, false, 0 ),
            new TestData<long>(new byte[] { 0x00, 0x7f }, false, 0 ),
            new TestData<long>(new byte[] { 0xff, 0xf0 }, false, 0 ),
        };

        static TestData<int>[] IntTestData = new []
        {
            new TestData<int>(new byte[] {0x00}, true, 0),
            new TestData<int>(new byte[] {0x7f}, true, 127),
            new TestData<int>(new byte[] {0x00, 0x80}, true, 128),
            new TestData<int>(new byte[] {0x01, 0x00}, true, 256),
            new TestData<int>(new byte[] {0x80}, true, -128),
            new TestData<int>(new byte[] {0xff, 0x7f}, true, -129),
            new TestData<int>(new byte[] {0xff}, true, -1),
            new TestData<int>(new byte[] {0x80, 0x00, 0x00, 0x00}, true, -2147483648),
            new TestData<int>(new byte[] {0x80, 0x00, 0x00, 0x00, 0x00}, false, 0),
            new TestData<int>(new byte[] {}, false, 0),
            new TestData<int>(new byte[] {0x00, 0x7f}, false, 0),
            new TestData<int>(new byte[] {0xff, 0xf0}, false, 0),
        };

        static TestData<string>[] BigIntTestData = new []
        {
            new TestData<string>(new byte[] { 0xff},       true,  "-1"),
            new TestData<string>(new byte[] { 0x00},       true,  "0"),
            new TestData<string>(new byte[] { 0x01},       true,  "1"),
            new TestData<string>(new byte[] { 0x00, 0xff}, true,  "255"),
            new TestData<string>(new byte[] { 0xff, 0x00}, true,  "-256"),
            new TestData<string>(new byte[] { 0x01, 0x00}, true,  "256"),
            new TestData<string>(new byte[] { },           false, ""),
            new TestData<string>(new byte[] { 0x00, 0x7f}, false, ""),
            new TestData<string>(new byte[] { 0xff, 0xf0}, false, ""),
        };

        // public TestBitString[] BitStringTestData = new []
        // {
        //     new TestBitString(new byte[] {},           false, new byte[] {},     0),
        //     new TestBitString(new byte[] {0x00},       true,  new byte[] {},     0),
        //     new TestBitString(new byte[] {0x07, 0x00}, true,  new byte[] {0x00}, 1),
        //     new TestBitString(new byte[] {0x07, 0x01}, false, new byte[] {},     0),
        //     new TestBitString(new byte[] {0x07, 0x40}, false, new byte[] {},     0),
        //     new TestBitString(new byte[] {0x08, 0x00}, false, new byte[] {},     0),
        // };

        public TestData<Parser.BitString>[] BitStringTestData = new []
        {
            new TestData<Parser.BitString>(new byte[] {},           false, new Parser.BitString(new byte[] {},     0)),
            new TestData<Parser.BitString>(new byte[] {0x00},       true,  new Parser.BitString(new byte[] {},     0)),
            new TestData<Parser.BitString>(new byte[] {0x07, 0x00}, true,  new Parser.BitString(new byte[] {0x00}, 1)),
            new TestData<Parser.BitString>(new byte[] {0x07, 0x01}, false, new Parser.BitString(new byte[] {},     0)),
            new TestData<Parser.BitString>(new byte[] {0x07, 0x40}, false, new Parser.BitString(new byte[] {},     0)),
            new TestData<Parser.BitString>(new byte[] {0x08, 0x00}, false, new Parser.BitString(new byte[] {},     0)),
        };

        public TestBitStringAlign[] BitStringAlignTestData = new []
        {
	        new TestBitStringAlign { In = new byte[] {0x80},       InLen = 1,  Out = new byte[] {0x01}},
	        new TestBitStringAlign { In = new byte[] {0x80, 0x80}, InLen = 9,  Out = new byte[] {0x01, 0x01}},
	        new TestBitStringAlign { In = new byte[] {},           InLen = 0,  Out = new byte[] {}},
	        new TestBitStringAlign { In = new byte[] {0xce},       InLen = 8,  Out = new byte[] {0xce}},
	        new TestBitStringAlign { In = new byte[] {0xce, 0x47}, InLen = 16, Out = new byte[] {0xce, 0x47}},
	        new TestBitStringAlign { In = new byte[] {0x34, 0x50}, InLen = 12, Out = new byte[] {0x03, 0x45}},
        };

        public TestData<int[]>[] ObjectIdentifierTestData = new[]
        {
            new TestData<int[]>(new byte[] {},                                       false, new int[] {}),
            new TestData<int[]>(new byte[] {85},                                     true,  new int[] {2, 5}),
            new TestData<int[]>(new byte[] {85, 0x02},                               true,  new int[] {2, 5, 2}),
            new TestData<int[]>(new byte[] {85, 0x02, 0xc0, 0x00},                   true,  new int[] {2, 5, 2, 0x2000}),
            new TestData<int[]>(new byte[] {0x81, 0x34, 0x03},                       true,  new int[] {2, 100, 3}),
            new TestData<int[]>(new byte[] {85, 0x02, 0xc0, 0x80, 0x80, 0x80, 0x80}, false, new int[] {}),
        };

        public TestData<DateTime>[] TimeTestData = new[]
        {
            new TestData<DateTime>(Encoding.UTF8.GetBytes("910506164540-0700"), true,  new DateTime(1991, 05, 06, 16, 45, 40, 0, DateTimeKind.Unspecified)), // time.FixedZone("", -7*60*60)
            new TestData<DateTime>(Encoding.UTF8.GetBytes("910506164540+0730"), true,  new DateTime(1991, 05, 06, 16, 45, 40, 0, DateTimeKind.Unspecified)), // time.FixedZone("", 7*60*60+30*60)
            new TestData<DateTime>(Encoding.UTF8.GetBytes("910506234540Z"),     true,  new DateTime(1991, 05, 06, 23, 45, 40, 0, DateTimeKind.Utc)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("9105062345Z"),       true,  new DateTime(1991, 05, 06, 23, 45,  0, 0, DateTimeKind.Utc)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("5105062345Z"),       true,  new DateTime(1951, 05, 06, 23, 45,  0, 0, DateTimeKind.Utc)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("a10506234540Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("91a506234540Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("9105a6234540Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("910506a34540Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("910506334a40Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("91050633444aZ"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("910506334461Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("910506334400Za"),    false, default(DateTime)),
            // THIS COMMENT IS FROM THE ORIGINAL GOLANG code:
            /* These are invalid times. However, the time package normalises times
            * and they were accepted in some versions. See #11134. */
            new TestData<DateTime>(Encoding.UTF8.GetBytes("000100000000Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("101302030405Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("100002030405Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("100100030405Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("100132030405Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("100231030405Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("100102240405Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("100102036005Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("100102030460Z"),     false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("-100102030410Z"),    false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("10-0102030410Z"),    false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("10-0002030410Z"),    false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("1001-02030410Z"),    false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("100102-030410Z"),    false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("10010203-0410Z"),    false, default(DateTime)),
            new TestData<DateTime>(Encoding.UTF8.GetBytes("1001020304-10Z"),    false, default(DateTime)),
        };

        [Fact]
        public void ParseBool() 
        {
            ParseTestData(BoolTestData, x => Parser.ParseBool(x));
        }

        [Fact]
        public void ParseLong()
        {
            ParseTestData(LongTestData, x => Parser.ParseLong(x));
        }

        [Fact]
        public void ParseInteger()
        {
            ParseTestData(IntTestData, x => Parser.ParseInteger(x));
        }

        [Fact]
        public void ParseBigInt()
        {
            ParseTestData(BigIntTestData, x => Parser.ParseBigInt(x).ToString());


            // TODO: complete this test with Marshal Big Int

        }

        [Fact]
        public void ParseBitString()
        {
            ParseTestData(BitStringTestData,
                    x => Parser.ParseBitString(x),
                    (x,y) => x.BitLength == y.BitLength 
                            && BitConverter.ToString(x.Bytes) == BitConverter.ToString(y.Bytes));

            // foreach (var td in TestBitStrings)
            // {
            //     if (td.Ok)
            //     {
            //         var bs = Parser.ParseBitString(td.In);
            //         Assert.Equal(td.BitLength, bs.BitLength);
            //         Assert.Equal(td.Out, bs.Bytes);
            //     }
            //     else
            //     {
            //         Assert.ThrowsAny<Exception>(() => Parser.ParseBitString(td.In));
            //     }
            // }
        }

        [Fact]
        public void BitStringAt()
        {
            var bs = new Parser.BitString
            {
                Bytes = new byte[] { 0x82, 0x40 }, // ‭1000 0010 0100 0000‬
                BitLength = 16,
            };

            Assert.Equal(bs.At(0), 1);
            Assert.Equal(bs.At(1), 0);
            Assert.Equal(bs.At(6), 1);
            Assert.Equal(bs.At(9), 1);
            Assert.Equal(bs.At(-1), 0);
            Assert.Equal(bs.At(17), 0);
        }

        [Fact]
        public void BitStringRightAlign()
        {
            foreach (var td in BitStringAlignTestData)
            {
                var bs = new Parser.BitString { Bytes = td.In, BitLength = td.InLen };
                var @out = bs.RightAlign();
                Assert.Equal(td.Out, @out);
            }
        }

        [Fact]
        public void ParseObjectIdentifier()
        {
            ParseTestData(ObjectIdentifierTestData, x => Parser.ParseObjectIdentifier(x).Value);
        }

        // TODO:  This is incomplete because of lacking support for custom TimeZones in Core
        //        We'll pby have to pull in advanced support from something like NodaTime
        //[Fact]
        public void ParseUTCTime()
        {
            ParseTestData(TimeTestData, x => Parser.ParseUTCTime(x));
        }

        // TODO: add tests for GeneralizedTime

        private void ParseTestData<T>(TestData<T>[] testData, Func<byte[], T> parseFunc, Func<T, T, bool> comparer = null)
        {
            IEqualityComparer<T> ec = null;
            if (comparer != null)
                ec = new EquComparer<T> { Comparer = comparer };

            foreach (var td in testData)
            {
                if (td.Ok)
                {
                    if (ec == null)
                        Assert.Equal(td.Out, parseFunc(td.In));
                    else
                        Assert.Equal(td.Out, parseFunc(td.In), ec);
                }
                else
                {
                    Assert.ThrowsAny<Exception>(() => parseFunc(td.In));
                }
            }
        }
    }

    public class EquComparer<T> : IEqualityComparer<T>
    {
        public Func<T, T, bool> Comparer
        { get; set; }

        public bool Equals(T x, T y)
        {
            return Comparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }

    public struct TestData<T>
    {
        public TestData(byte[] @in, bool ok, T @out)
        {
            In = @in;
            Ok = ok;
            Out = @out;
        }

        public byte[] In;
        public bool Ok;
        public T Out;
    }

    // public struct TestBitString
    // {
    //     public TestBitString(byte[] @in, bool ok, byte[] @out, int bitLength)
    //     {
    //         In = @in;
    //         Ok = ok;
    //         Out = @out;
    //         BitLength = bitLength;
    //     }

    //     public byte[] In;
    //     public bool Ok;
    //     public byte[] Out;
    //     public int BitLength;
    // }

    public struct TestBitStringAlign
    {
        public byte[] In;
        public int InLen;
        public byte[] Out;
    }
}
