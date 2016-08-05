using System;
using System.Text;

namespace ACMESharp.Jose
{
    public class EncodingHelper
    {
        /// <summary>
        /// URL-safe Base64 encoding as prescribed in RFC 7515 Appendix C.
        /// </summary>
        public static string Base64UrlEncode(string raw, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            return Base64UrlEncode(encoding.GetBytes(raw));
        }

        /// <summary>
        /// URL-safe Base64 encoding as prescribed in RFC 7515 Appendix C.
        /// </summary>
        public static string Base64UrlEncode(byte[] raw)
        {
            string enc = Convert.ToBase64String(raw);  // Regular base64 encoder
            enc = enc.Split('=')[0];                   // Remove any trailing '='s
            enc = enc.Replace('+', '-');               // 62nd char of encoding
            enc = enc.Replace('/', '_');               // 63rd char of encoding
            return enc;
        }

        /// <summary>
        /// URL-safe Base64 decoding as prescribed in RFC 7515 Appendix C.
        /// </summary>
        public static byte[] Base64UrlDecode(string enc)
        {
            string raw = enc;
            raw = raw.Replace('-', '+');  // 62nd char of encoding
            raw = raw.Replace('_', '/');  // 63rd char of encoding
            switch (raw.Length % 4)       // Pad with trailing '='s
            {
                case 0: break;               // No pad chars in this case
                case 2: raw += "=="; break;  // Two pad chars
                case 3: raw += "="; break;   // One pad char
                default:
                    throw new System.Exception("Illegal base64url string!");
            }
            return Convert.FromBase64String(raw); // Standard base64 decoder
        }

        public static string Base64UrlDecodeToString(string enc, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            return encoding.GetString(Base64UrlDecode(enc));
        }
    }
}