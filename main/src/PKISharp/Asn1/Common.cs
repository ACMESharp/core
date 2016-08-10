using System;
using System.Collections.Generic;
using System.Reflection;

namespace PKISharp.Asn1
{
    /// <summary>
    /// This class is a C# adaptation of the ASN1 support from the
    /// <see chref="https://github.com/golang/go/blob/master/src/encoding/asn1/common.go"
    /// >Go language base library</see>.
    /// </summary>
    public static class Common
    {
        // ASN.1 objects have metadata preceding them:
        //   the tag: the type of the object
        //   a flag denoting if this object is compound or not
        //   the class type: the namespace of the tag
        //   the length of the object, in bytes

        // Here are some standard tags and classes

        // ASN.1 tags represent the type of the following object.
        public enum CommonTag
        {
            TagBoolean         = 1,
            TagInteger         = 2,
            TagBitString       = 3,
            TagOctetString     = 4,
            TagOID             = 6,
            TagEnum            = 10,
            TagUTF8String      = 12,
            TagSequence        = 16,
            TagSet             = 17,
            TagPrintableString = 19,
            TagT61String       = 20,
            TagIA5String       = 22,
            TagUTCTime         = 23,
            TagGeneralizedTime = 24,
            TagGeneralString   = 27,
        }

        // ASN.1 class types represent the namespace of the tag.
        public enum ClassType
        {
            ClassUniversal       = 0,
            ClassApplication     = 1,
            ClassContextSpecific = 2,
            ClassPrivate         = 3,
        }

        public struct  TagAndLength
        {
            public int Class;
            public int Tag;
            public int Length;
            public bool IsCompound;
        }

        // ASN.1 has IMPLICIT and EXPLICIT tags, which can be translated as "instead
        // of" and "in addition to". When not specified, every primitive type has a
        // default tag in the UNIVERSAL class.
        //
        // For example: a BIT STRING is tagged [UNIVERSAL 3] by default (although ASN.1
        // doesn't actually have a UNIVERSAL keyword). However, by saying [IMPLICIT
        // CONTEXT-SPECIFIC 42], that means that the tag is replaced by another.
        //
        // On the other hand, if it said [EXPLICIT CONTEXT-SPECIFIC 10], then an
        // /additional/ tag would wrap the default tag. This explicit tag will have the
        // compound flag set.
        //
        // (This is used in order to remove ambiguity with optional elements.)
        //
        // You can layer EXPLICIT and IMPLICIT tags to an arbitrary depth, however we
        // don't support that here. We support a single layer of EXPLICIT or IMPLICIT
        // tagging with tag strings on the fields of a structure.

        // fieldParameters is the parsed representation of tag string from a structure field.
        public struct FieldParameters
        {
            public bool Optional;        // true iff the field is OPTIONAL
            public bool Explicit;        // true iff an EXPLICIT tag is in use.
            public bool Application;     // true iff an APPLICATION tag is in use.
            public long? DefaultValue;   // a default value for INTEGER typed fields (maybe nil).
            public int? Tag;             // the EXPLICIT or IMPLICIT tag (maybe nil).
            public int StringType;       // the string tag to use when marshaling.
            public int TimeType;         // the time tag to use when marshaling.
            public bool Set;             // true iff this should be encoded as a SET
            public bool OmitEmpty;       // true iff this should be omitted if empty when marshaling.

            // Invariants:
            //   if explicit is set, tag is non-nil.
        }

        private static Dictionary<string, Action<FieldParameters>> FIELD_PARAMETERS_PARSE_MAP =
                new Dictionary<string, Action<FieldParameters>>
        {
            ["optional"] = (fp) =>
                fp.Optional = true,
            ["explicit"] = (fp) => {
                fp.Explicit = true;
                if (fp.Tag == null)
                    fp.Tag = default(int);
            },
            ["generalized"] = (fp) =>
                fp.TimeType = (int)CommonTag.TagGeneralizedTime,
            ["utc"] = (fp) =>
                fp.TimeType = (int)CommonTag.TagUTCTime,
            ["ia5"] = (fp) =>
                fp.StringType = (int)CommonTag.TagIA5String,
            ["printable"] = (fp) =>
                fp.StringType = (int)CommonTag.TagPrintableString,
            ["utf8"] = (fp) =>
                fp.StringType = (int)CommonTag.TagUTF8String,
            ["set"] = (fp) =>
                fp.Set = true,
            ["application"] = (fp) => {
                fp.Application = true;
                if (fp.Tag == null)
                    fp.Tag = default(int);
            },
            ["omitempty"] = (fp) =>
                fp.OmitEmpty = true,
        };

        /// Given a tag string with the format specified in the package comment,
        /// parseFieldParameters will parse it into a fieldParameters structure,
        /// ignoring unknown parts of the string.

        // func parseFieldParameters(str string) (ret fieldParameters) {
        public static FieldParameters ParseFieldParameters(this string str)
        {
            var ret = new FieldParameters();

            foreach (var part in str.Split(','))
            {


                if (FIELD_PARAMETERS_PARSE_MAP.ContainsKey(part))
                {
                    FIELD_PARAMETERS_PARSE_MAP[part](ret);
                }
                else if (part.StartsWith("default:"))
                {
                    long l;
                    if (long.TryParse(part.Substring(8), out l))
                        ret.DefaultValue = l;
                }
                else if (part.StartsWith("tag:"))
                {
                    int i;
                    if (int.TryParse(part.Substring(4), out i))
                        ret.Tag = i;
                }
            }

            return ret;
        }

        class ObjectIdentiferType {}
        class BitStringType {}

        class TimeType {}
        class EnumeratedType {}
        class BigIntType {}




        private static Dictionary<Type, Tuple<int, bool, bool>> TYPE_TAG_MAP =
                new Dictionary<Type, Tuple<int, bool, bool>>
        {
            [typeof(ObjectIdentiferType)] = new Tuple<int, bool, bool>(
                    (int)CommonTag.TagOID, false, true),
            [typeof(BitStringType)] = new Tuple<int, bool, bool>(
                    (int)CommonTag.TagBitString, false, true),
            [typeof(TimeType)] = new Tuple<int, bool, bool>(
                    (int)CommonTag.TagUTCTime, false, true),
            [typeof(EnumeratedType)] = new Tuple<int, bool, bool>(
                    (int)CommonTag.TagEnum, false, true),
            [typeof(BigIntType)] = new Tuple<int, bool, bool>(
                    (int)CommonTag.TagInteger, false, true),

            [typeof(bool)] = new Tuple<int, bool, bool>(
                    (int)CommonTag.TagBoolean, false, true),

            [typeof(sbyte)] = new Tuple<int, bool, bool>(
                    (int)CommonTag.TagInteger, false, true),
            [typeof(short)] = new Tuple<int, bool, bool>(
                    (int)CommonTag.TagInteger, false, true),
            [typeof(int)] = new Tuple<int, bool, bool>(
                    (int)CommonTag.TagInteger, false, true),
            [typeof(long)] = new Tuple<int, bool, bool>(
                    (int)CommonTag.TagInteger, false, true),

            [typeof(string)] = new Tuple<int, bool, bool>(
                    (int)CommonTag.TagPrintableString, false, true),
        };

        /// Given a reflected <!--Go--> type, getUniversalType returns the default tag number
        /// and expected compound flag.
        //func getUniversalType(t reflect.Type) (tagNumber int, isCompound, ok bool) {
        public static Tuple<int, bool, bool> GetUniversalType(this Type t)
        {
            Tuple<int, bool, bool> ret;

            if (!TYPE_TAG_MAP.TryGetValue(t, out ret))
            {
                if (t.GetTypeInfo().IsValueType)
                {
                    ret = new Tuple<int, bool, bool>(
                            (int)CommonTag.TagSequence, true, true);
                }
                else if (t.IsArray)
                {
                    var et = t.GetElementType();
                    if (typeof(byte) == et)
                        ret = new Tuple<int, bool, bool>(
                                (int)CommonTag.TagOctetString, false, true);
                    // TODO: NOT SURE HOW THIS SHOULD BE TRANSLATED YET!
                    // 	if strings.HasSuffix(t.Name(), "SET") {
                    // 		return TagSet, true, true
                    // 	}
                    else
                        ret = new Tuple<int, bool, bool>(
                                (int)CommonTag.TagSequence, true, true);
                }
                else
                {
                    ret = new Tuple<int, bool, bool>(0, false, false);
                }
            }

            return ret;
        }
    }
}