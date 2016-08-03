using System;
using System.Collections.Generic;
using ACMESharp.Vault;
using Newtonsoft.Json;

namespace ACMESharp.Cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var vem = new VaultExtManager();
            Console.WriteLine("Providers:");
            foreach (var p in vem.FoundProviders)
            {
                Console.WriteLine($"  * {p}");
            }

            Console.WriteLine("ExtInfo:");
            foreach (var ei in vem.ExtInfos)
            {
                Console.WriteLine($"  * {JsonConvert.SerializeObject(ei)}");
                var vp = vem.GetProvider(ei.Name);
                Console.WriteLine("  * Params:");
                foreach (var p in vp.DescribeParams())
                {
                    Console.WriteLine($"    - {JsonConvert.SerializeObject(p)}");
                }
            }

            using (var v = vem.GetProvider("local").Get(new Dictionary<string, object>
                    {
                        ["RootPath"] = ".\\VAULT_ROOT",
                    }))
            {
                Console.WriteLine(v);
            }
        }
    }
}
