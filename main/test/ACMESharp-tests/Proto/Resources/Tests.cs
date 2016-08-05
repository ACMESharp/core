using ACMESharp.Json;
using Xunit;
using Xunit.Abstractions;

namespace ACMESharp.Proto.Resources
{
    public
    class Tests
    {
        const string FakeUrl = "http://example.com/acme";

        public static readonly string DirectoryWithMeta_Json = ("{"
                + "'new-reg':'nr','new-app':'na','revoke-cert':'rc'"
                + ",'key-change':'kc'"
                + ",'meta':{'terms-of-service':'tos','website':'ws'"
                + ",'caa-identities':['caa1','caa2']"
                + "}}").Replace("'", "\"");
        public static readonly string DirectoryWithMetaExtended_Json = ("{"
                + "'new-reg':'nr','new-app':'na','revoke-cert':'rc'"
                + ",'key-change':'kc'"
                + ",'meta':{'terms-of-service':'tos','website':'ws'"
                + ",'caa-identities':['caa1','caa2'],'ext-prop':'ext-val'"
                + "}}").Replace("'", "\"");

        public static readonly string DirectoryNoMeta_Json = ("{"
                + "'new-reg':'nr','new-app':'na','revoke-cert':'rc'"
                + ",'key-change':'kc'"
                + "}").Replace("'", "\"");


        private readonly ITestOutputHelper _out;
        public Tests(ITestOutputHelper output)
        {
            _out = output;
        }

        [Fact]
        public void DirectorySerialization() 
        {
            Directory d = new Directory
            {
                NewReg = "nr",
                NewApp = "na",
                RevokeCert = "rc",
                KeyChange = "kc",
                Meta = new Directory.DirectoryMeta
                {
                    TermsOfService = "tos",
                    Website = "ws",
                    CaaIdentities = new[] { "caa1", "caa2" },
                }
            };

            var expWithMeta = DirectoryWithMeta_Json;
            var expNoMeta = DirectoryNoMeta_Json;

            var actWithMeta = JsonSerializer.Serialize(d);
            d.Meta = null;
            var actNoMeta = JsonSerializer.Serialize(d);

            _out.WriteLine($"expWithMeta: {expWithMeta}");
            _out.WriteLine($"expWithMeta: {expWithMeta}");
            Assert.Equal(expWithMeta, actWithMeta);

            _out.WriteLine($"expNoMeta: {expNoMeta}");
            _out.WriteLine($"actNoMeta: {actNoMeta}");
            Assert.Equal(expNoMeta, actNoMeta);
        }

        [Fact]
        public void DirectoryDeserialization()
        {
            Directory d = new Directory
            {
                NewReg = "nr",
                NewApp = "na",
                RevokeCert = "rc",
                KeyChange = "kc",
                Meta = new Directory.DirectoryMeta
                {
                    TermsOfService = "tos",
                    Website = "ws",
                    CaaIdentities = new[] { "caa1", "caa2" },
                }
            };

            var actWithMeta = JsonSerializer.Deserialize<Directory>(
                    DirectoryWithMeta_Json);
            var actNoMeta = JsonSerializer.Deserialize<Directory>(
                    DirectoryNoMeta_Json);

            Assert.Equal(DirectoryWithMeta_Json,
                    JsonSerializer.Serialize(actWithMeta));
            Assert.Equal(DirectoryNoMeta_Json,
                    JsonSerializer.Serialize(actNoMeta));
        }

        [Fact]
        public void DirectoryExtendedDeserialization()
        {


            var actWithMeta = JsonSerializer.Deserialize<Directory>(
                    DirectoryWithMetaExtended_Json);
            var actNoMeta = JsonSerializer.Deserialize<Directory>(
                    DirectoryNoMeta_Json);

            _out.WriteLine(actWithMeta.Meta.GetExtDataCount().ToString());

            // Assert.Equal(DirectoryWithMeta_Json,
            //         JsonSerializer.Serialize(actWithMeta));
            // Assert.Equal(DirectoryNoMeta_Json,
            //         JsonSerializer.Serialize(actNoMeta));
            Assert.Equal("ext-val", (string)actWithMeta.Meta["ext-prop"]);
        }
    }
}
