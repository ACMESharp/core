using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using static PKISharp.Asn1.Common;

namespace PKISharp.Asn1
{
    public class StructuralException : System.Exception
    {
        public StructuralException() { }
        public StructuralException( string message )
            : base( message )
        { }
        public StructuralException( string message, System.Exception inner )
            : base( message, inner ) { }
        // protected StructuralException(
        //     System.Runtime.Serialization.SerializationInfo info,
        //     System.Runtime.Serialization.StreamingContext context ) : base( info, context )
        // { }

        public string Error
        {
            get
            {
                return $"asn1: structure error: {Message}";
            }
        }
    }

    public class SyntaxException : System.Exception
    {
        public SyntaxException() { }
        public SyntaxException( string message )
            : base( message )
        { }
        public SyntaxException( string message, System.Exception inner )
            : base( message, inner ) { }
        // protected StructuralException(
        //     System.Runtime.Serialization.SerializationInfo info,
        //     System.Runtime.Serialization.StreamingContext context ) : base( info, context )
        // { }

        public string Error
        {
            get
            {
                return $"asn1: syntax error: {Message}";
            }
        }
    }

    public static class Parser
    {
        public static bool ParseBool(byte[] bytes)
        {
            if (bytes?.Length != 1)
                throw new SyntaxException("invalid boolean");

            // DER demands that "If the encoding represents the boolean value TRUE,
            // its single contents octet shall have all eight bits set to one."
            // Thus only 0 and 255 are valid encoded values.

            switch (bytes[0])
            {
                case 0:
                    return false;
                case 0xff:
                    return true;
                default:
                    throw new SyntaxException("invalid boolean");
            }
        }

        // checkInteger returns nil if the given bytes are a valid DER-encoded
        // INTEGER and an error otherwise.
        public static void CheckInteger(byte[] bytes)
        {
            if (bytes?.Length == 0)
                throw new StructuralException("empty integer");
            
            if (bytes.Length > 1)
            {
                if ((bytes[0] == 0 && ((bytes[1] & 0x80) == 0)) || (bytes[0] == 0xff && ((bytes[1] & 0x80) == 0x80)))
                    throw new StructuralException("integer not minimally-encoded");
            }
        }

        // parseInt64 treats the given bytes as a big-endian, signed integer and
        // returns the result.
        public static long ParseLong(byte[] bytes)
        {
            CheckInteger(bytes);
            if (bytes.Length > 8)
            {
                // We'll overflow an int64 in this case.
                throw new StructuralException("integer too large");
            }

            long ret = 0;

            for (var bytesRead = 0; bytesRead < bytes.Length; bytesRead++)
            {
                ret <<= 8;
                ret |= (long)bytes[bytesRead];
            }

            // Shift up and down in order to sign extend the result.
            ret <<= 64 - ((byte)(bytes.Length)) * 8;
            ret >>= 64 - ((byte)(bytes.Length)) * 8;

            return ret;
        }

        // parseInt treats the given bytes as a big-endian, signed integer and returns
        // the result.
        public static int ParseInteger(byte[] bytes)
        {
            CheckInteger(bytes);
            
            var ret64 = ParseLong(bytes);
            if (ret64 != (long)((int)ret64))
                throw new StructuralException("integer too large");

            return (int)ret64;
        }

        // parseBigInt treats the given bytes as a big-endian, signed integer and returns
        // the result.
        public static BigInteger ParseBigInt(byte[] bytes)
        {
            CheckInteger(bytes);
            BigInteger ret;

            // NOTE:  this implementation is *NOT* a translation of the
            // original GO-lang library translation but instead uses the
            // built-in features of the .NET BigInteger type to implement

            var bytesReversed = (byte[])bytes.Clone();
            Array.Reverse(bytesReversed);
            ret = new BigInteger(bytesReversed);

            return ret;
        }

        // BitString is the structure to use when you want an ASN.1 BIT STRING type. A
        // bit string is padded up to the nearest byte in memory and the number of
        // valid bits is recorded. Padding bits will be zero.
        public struct BitString
        {
            public BitString(byte[] bytes, int bitLength)
            {
                Bytes = bytes;
                BitLength = bitLength;
            }

            public byte[] Bytes;     // bits packed into bytes.
            public int BitLength;    // length in bits.

            // At returns the bit at the given index. If the index is out of range it
            // returns false.
            public int At(int i)
            {
                if (i < 0 || i >= BitLength)
                    return 0;

                var x = i / 8;
                var y = 7 - i % 8;
                return ((int)(Bytes[x] >> y)) & 1;
            }

            // RightAlign returns a slice where the padding bits are at the beginning. The
            // slice may share memory with the BitString.
            public byte[] RightAlign()
            {
                var shift = (8 - (BitLength % 8));
                if (shift == 8 || Bytes.Length == 0)
                    return Bytes;

                var a = new byte[Bytes.Length];
                a[0] = (byte)(Bytes[0] >> shift);
                for (var i = 1; i < Bytes.Length; i++)
                {
                    a[i] = (byte)(Bytes[i - 1] << (8 - shift));
                    a[i] |= (byte)(Bytes[i] >> shift);
                }

                return a;
            }
        }

        // parseBitString parses an ASN.1 bit string from the given byte slice and returns it.
        public static BitString ParseBitString(byte[] bytes)
        {
            BitString ret;

            if (bytes.Length == 0)
                throw new SyntaxException("zer length BIT STRING");
            
            var paddingBits = (int)bytes[0];
            if ((paddingBits > 7)
                    || (bytes.Length == 1 && paddingBits > 0)
                    || (bytes[bytes.Length - 1] & ((1 << bytes[0]) - 1)) != 0)
            {
                throw new SyntaxException("invalide padding bits in BIT STRING");
            }

            ret.BitLength = (bytes.Length - 1) * 8 - paddingBits;
            ret.Bytes = new byte[bytes.Length - 1];
            Array.Copy(bytes, 1, ret.Bytes, 0, ret.Bytes.Length);

            return ret;
        }

        // An ObjectIdentifier represents an ASN.1 OBJECT IDENTIFIER.
        public struct ObjectIdentifier
        {
            public int[] Value;

            // override object.Equals
            public override bool Equals (object obj)
            {
                //
                // See the full list of guidelines at
                //   http://go.microsoft.com/fwlink/?LinkID=85237
                // and also the guidance for operator== at
                //   http://go.microsoft.com/fwlink/?LinkId=85238
                //
                
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                
                return Equals((ObjectIdentifier)obj);
            }

            public bool Equals(ObjectIdentifier other)
            {
                if (Value.Length != other.Value.Length)
                    return false;

                for (var i = 0; i < Value.Length; i++)
                    if (Value[i] != other.Value[i])
                        return false;

                return true;
            }

            public override string ToString()
            {
                return string.Join(".", Value);
            }
        }

        // parseObjectIdentifier parses an OBJECT IDENTIFIER from the given bytes and
        // returns it. An object identifier is a sequence of variable length integers
        // that are assigned in a hierarchy.
        public static ObjectIdentifier ParseObjectIdentifier(byte[] bytes)
        {
            ObjectIdentifier ret;

            if (bytes.Length == 0)
                throw new SyntaxException("zero length OBJECT IDENTIFIER");

            // In the worst case, we get two elements from the first byte (which is
            // encoded differently) and then every varint is a single byte long.
            ret.Value = new int[bytes.Length + 1];

            // The first varint is 40*value1 + value2:
            // According to this packing, value1 can take the values 0, 1 and 2 only.
            // When value1 = 0 or value1 = 1, then value2 is <= 39. When value1 = 2,
            // then there are no restrictions on value2.
            var offset = 0;
            var v = ParseBase128Int(bytes, ref offset);

            if (v < 80)
            {
                ret.Value[0] = v / 40;
                ret.Value[1] = v % 40;
            }
            else
            {
                ret.Value[0] = 2;
                ret.Value[1] = v - 80;
            }

            var i = 2;
            for ( ; offset < bytes.Length; i++)
            {
                v = ParseBase128Int(bytes, ref offset);
                ret.Value[i] = v;
            }

            var s = new int[i];
            Array.Copy(ret.Value, 0, s, 0, i);
            ret.Value = s;

            return ret;            
        }

        // An Enumerated is represented as a plain int.
        public struct Enumerated
        {
            public int Value;
        }

        // A Flag accepts any data and is set to true if present.
        public struct Flag
        {
            public bool Value;
        }

        // TODO:  This is incomplete because of lacking support for custom TimeZones in Core
        //        We'll pby have to pull in advanced support from something like NodaTime
        public static DateTime ParseUTCTime(byte[] bytes)
        {
            var s = Encoding.UTF8.GetString(bytes);
            DateTime ret;

            var fmt = "yyMMddHHmmzzz";
            if (!DateTime.TryParseExact(s, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out ret))
            {
                fmt = "yyMMddHHmmsszzz";
                ret = DateTime.ParseExact(s, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }

            var serialized = ret.ToString(fmt);
            if (serialized != s)
                throw new Exception($"asn1: time did not serialize back to the original value and may be invalid: given {s}, but serialized as {serialized}");

            if (ret.Year >= 2050)
                // UTCTime only encodes times prior to 2050. See https://tools.ietf.org/html/rfc5280#section-4.1.2.5.1
                ret = ret.AddYears(-100);

            return ret;
        }

        // TODO: add routine for GeneralizedTime

        // parseBase128Int parses a base-128 encoded int from the given offset in the
        // given byte slice. It returns the value and the new offset.
        private static int ParseBase128Int(byte[] bytes, ref int offset)
        {
            var ret = 0;
            for (var shifted = 0; offset < bytes.Length; shifted++)
            {
                if (shifted == 4)
                    throw new StructuralException("base 128 integer too large");
                
                ret <<= 7;
                var b = bytes[offset];
                ret |= b & 0x7f;
                offset++;
                if ((b & 0x80) == 0)
                    return ret;
            }

            throw new SyntaxException("truncated base 128 integer");
        }

        // parsePrintableString parses a ASN.1 PrintableString from the given byte
        // array and returns it.
        public static string ParsePrintableString(byte[] bytes)
        {
            foreach (var b in bytes)
                if (!b.IsPrintable())
                    throw new SyntaxException("PrintableString contains invalid character");

            return Encoding.UTF8.GetString(bytes);
        }

        // isPrintable reports whether the given b is in the ASN.1 PrintableString set.
        public static bool IsPrintable(this byte b)
        {
            return 'a' <= b && b <= 'z'
                || 'A' <= b && b <= 'Z'
                || '0' <= b && b <= '9'
                || '\'' <= b && b <= ')'
                || '+' <= b && b <= '/'
                || b == ' '
                || b == ':'
                || b == '='
                || b == '?'
                // This is technically not allowed in a PrintableString.
                // However, x509 certificates with wildcard strings don't
                // always use the correct string type so we permit it.
                || b == '*';
        }

        /// <summary>
        /// From https://golang.org/pkg/unicode/utf8/
        /// </summary>
        public const int UTF8_RUNE_SELF = 0x80;

        // parseIA5String parses a ASN.1 IA5String (ASCII string) from the given
        // byte slice and returns it.
        public static string ParseIA5String(byte[] bytes)
        {
            foreach (var b in bytes)
                if (b >= UTF8_RUNE_SELF)
                    throw new SyntaxException("IA5String contains invalid character");

            return Encoding.UTF8.GetString(bytes);
        }

        // parseT61String parses a ASN.1 T61String (8-bit clean string) from the given
        // byte slice and returns it.
        public static string  ParseT61String(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        // parseUTF8String parses a ASN.1 UTF8String (raw UTF-8) from the given byte
        // array and returns it.
        public static string ParseUTF8String(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        // A RawValue represents an undecoded ASN.1 object.
        public struct RawValue
        {
            public int Class;
            public int Tag;
            public bool IsCompound;
            public byte[] Bytes;
            public byte[] FullBytes;// includes the tag and length
        }

        // RawContent is used to signal that the undecoded, DER data needs to be
        // preserved for a struct. To use it, the first field of the struct must have
        // this type. It's an error for any of the other fields to have this type.
        public struct RawContent
        {
            public byte[] Value;
        }


        // parseTagAndLength parses an ASN.1 tag and length pair from the given offset
        // into a byte slice. It returns the parsed data and the new offset. SET and
        // SET OF (tag 17) are mapped to SEQUENCE and SEQUENCE OF (tag 16) since we
        // don't distinguish between ordered and unordered objects in this code.
        public static TagAndLength ParseTagAndLength(byte[] bytes, ref int offset)
        {
            TagAndLength ret;

            // parseTagAndLength should not be called without at least a single
            // byte to read. Thus this check is for robustness:
            if (offset >= bytes.Length)
                throw new Exception("asn1: internal error in parseTagAndLength");
            var b = bytes[offset];
            offset++;
            ret.Class = b >> 6;
            ret.IsCompound = (b & 0x20) == 0x20;
            ret.Tag = b & 0x1f;

            // If the bottom five bits are set, then the tag number is actually base 128
            // encoded afterwards
            if (ret.Tag == 0x1f)
            {
                ret.Tag = ParseBase128Int(bytes, ref offset);
                // Tags should be encoded in minimal form.
                if (ret.Tag < 0x1f)
                    throw new SyntaxException("non-minimal tag");
            }
            if (offset >= bytes.Length)
                throw new SyntaxException("truncated tag or length");
            b = bytes[offset];
            offset++;
            if ((b & 0x80) == 0)
            {
                // The length is encoded in the bottom 7 bits.
                ret.Length = b & 0x7f;
            }
            else
            {
                // Bottom 7 bits give the number of length bytes to follow.
                var numBytes = b & 0x7f;
                if (numBytes == 0)
                    throw new SyntaxException("indefinite length found (not DER)");
                ret.Length = 0;
                for (var i = 0; i < numBytes; i++)
                {
                    if (offset >= bytes.Length)
                        throw new SyntaxException("truncated tag or length");
                    b = bytes[offset];
                    offset++;
                    if (ret.Length >= (1 << 23))
                        // We can't shift ret.length up without
                        // overflowing.
                        throw new StructuralException("length too large");
                    ret.Length <<= 8;
                    ret.Length |= b;
                    if (ret.Length == 0)
                        // DER requires that lengths be minimal.
                        throw new StructuralException("superfluous leading zeros in length");
                }
                // Short lengths must be encoded in short form.
                if (ret.Length < 0x80)
                    throw new StructuralException("non-minimal length");
            }

            return ret;
        }

        // parseSequenceOf is used for SEQUENCE OF and SET OF values. It tries to parse
        // a number of ASN.1 values from the given byte slice and returns them as a
        // slice of Go values of the given type.
        // public static object ParseSequenceOf(byte[] bytes, Type sliceType, Type elemType)
        // {
        //     object ret;
            
        //     var tuple = Common.GetUniversalType(elemType);

        //     expectedTag, compoundType, ok := getUniversalType(elemType)
        //     if !ok {
        //         err = StructuralError{"unknown Go type for slice"}
        //         return
        //     }

        //     // First we iterate over the input and count the number of elements,
        //     // checking that the types are correct in each case.
        //     numElements := 0
        //     for offset := 0; offset < len(bytes); {
        //         var t tagAndLength
        //         t, offset, err = parseTagAndLength(bytes, offset)
        //         if err != nil {
        //             return
        //         }
        //         switch t.tag {
        //         case TagIA5String, TagGeneralString, TagT61String, TagUTF8String:
        //             // We pretend that various other string types are
        //             // PRINTABLE STRINGs so that a sequence of them can be
        //             // parsed into a []string.
        //             t.tag = TagPrintableString
        //         case TagGeneralizedTime, TagUTCTime:
        //             // Likewise, both time types are treated the same.
        //             t.tag = TagUTCTime
        //         }

        //         if t.class != ClassUniversal || t.isCompound != compoundType || t.tag != expectedTag {
        //             err = StructuralError{"sequence tag mismatch"}
        //             return
        //         }
        //         if invalidLength(offset, t.length, len(bytes)) {
        //             err = SyntaxError{"truncated sequence"}
        //             return
        //         }
        //         offset += t.length
        //         numElements++
        //     }
        //     ret = reflect.MakeSlice(sliceType, numElements, numElements)
        //     params := fieldParameters{}
        //     offset := 0
        //     for i := 0; i < numElements; i++ {
        //         offset, err = parseField(ret.Index(i), bytes, offset, params)
        //         if err != nil {
        //             return
        //         }
        //     }
        //     return
        // }

        public static readonly Type BitStringType = typeof(BitString);
        public static readonly Type ObjectIdentifierType = typeof(ObjectIdentifier);
        public static readonly Type EnumeratedType = typeof(Enumerated);
        public static readonly Type FlagType = typeof(Flag);
        public static readonly Type TimeType = typeof(DateTime);
        public static readonly Type RawValueType = typeof(RawValue);
        public static readonly Type RawContentsType = typeof(RawContent);
        public static readonly Type BigIntType = typeof(BigInteger);

    }
}