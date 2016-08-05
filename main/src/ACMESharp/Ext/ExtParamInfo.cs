namespace ACMESharp.Ext
{
    /// <summary>
    /// Defines the different logical types that are supported for parameters
    /// in the extension mechanism.
    /// </summary>
    public enum ExtParamType
    {
        /// <summary>
        /// Single-line string (no CR/LF).
        /// </summary>
        STRING = 0x1,
        /// <summary>
        /// Signed integer (i.e. Int32).
        /// </summary>
        NUMBER = 0x2,
        /// <summary>
        /// Boolean value.
        /// </summary>
        BOOLEAN = 0x3,

        /// <summary>
        /// Multi-line string.
        /// </summary>
        TEXT = 0xA,
        /// <summary>
        /// A string value that should be securely handled,
        /// e.g. via <see cref="System.Security.SecureString"/>.
        /// </summary>
        SECRET = 0xB,

        /// <summary>
        /// Sequence (enumeration) of values.
        /// </summary>
        SEQ = 0x10,

        /// <summary>
        /// Sequence (enumeration) of string values.
        /// </summary>
        SEQ_STRING = SEQ | STRING,

        /// <summary>
        /// Key-Value pair, with a string key and a value.
        /// </summary>
        KVP = 0x21,

        /// <summary>
        /// Key-Value pair, with a string key and a string value.
        /// </summary>
        KVP_STRING = KVP | STRING,
    }

    public struct ExtParamInfo
    {
        public ExtParamInfo(string name, ExtParamType type,
                string label = null, string description = null,
                bool isRequired = false)
        {
            Name = name;
            Type = type;
            Label = label;
            Description = description;
            IsRequired = isRequired;
        }

        public string Name
        { get; private set; }

        public ExtParamType Type
        { get; private set; }

        public string Label
        { get; private set; }

        public string Description
        { get; private set; }

        public bool IsRequired
        { get; private set; }

    }
}