using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace ACMESharp.Ext
{
    /// <summary>
    /// Base class and entry-point into the dynamically-loaded Extentsion
    /// Mechanism.
    /// </summary>
    /// <remarks>
    /// Because this system is based on MEF2, the support for dynamic
    /// loading from a directory is implemented as described here:
    /// http://weblogs.asp.net/ricardoperes/using-mef-in-net-core
    /// </remarks>
    public class ExtManager<E, EP>
        where E : IExtension
        where EP : IExtProvider<E>
    {
        private static readonly ILogger LOG = LogManager.GetLogger<ExtManager<E, EP>>();

        private List<Assembly> _BuiltInAssemblies = new List<Assembly>();

        private EP[] _foundProviders = null;

        public ExtManager()
        {
            AddBuiltIn(true,
                this.GetType().GetTypeInfo().Assembly,
                typeof(E).GetTypeInfo().Assembly,
                typeof(ExtManager<E, EP>).GetTypeInfo().Assembly);
        }

        public IEnumerable<Assembly> BuiltInAssemblies
        {
            get { return _BuiltInAssemblies; }
        }

        public IEnumerable<EP> FoundProviders
        {
            get
            {
                if (_foundProviders == null)
                    FindProviders();
                return _foundProviders;
            }
        }

        public IEnumerable<ExtInfo> ExtInfos
        {
            get
            {
                return FoundProviders.Select(ep => ep.Describe());
            }
        }

        public void AddBuiltIn(params Assembly[] asms)
        {
            AddBuiltIn(false, asms);
        }

        public void AddBuiltIn(bool clear, params Assembly[] asms)
        {
            if (clear)
                _BuiltInAssemblies.Clear();

            foreach (var a in asms)
            {
                if (_BuiltInAssemblies.Contains(a))
                    continue;
                _BuiltInAssemblies.Add(a);
            }
        }

        // public void Init()
        // {
        //     var asms = new[] {
        //       typeof(E).GetTypeInfo().Assembly,
        //     };

        //     // var conv = new ConventionBuilder();
        //     // conv.ForTypesDerivedFrom<IExtProvider<E>>()
        //     //     .Export<IExtProvider<E>>()
        //     //     .Shared();
            
        //     var conf = new ContainerConfiguration()
        //         .WithAssemblies(asms);

        //     conf.CreateContainer().GetExports<EP>();
        // }

        // public void ComputeRelativeRoot(string relPath = "ext", Assembly relToAsm = null)
        // {
        //     if (relToAsm == null)
        //         relToAsm = this.GetType().GetTypeInfo().Assembly;
            
        //     if (string.IsNullOrWhiteSpace(relToAsm.Location))
        //         throw new InvalidOperationException(AsmRes.EXCEPTIONS["invalid assembly is missing location"]);
        // }

        // public static string Foo()
        // {
        //     var asm = typeof(ExtManager<E, EP>).GetTypeInfo().Assembly;

        //     return Newtonsoft.Json.JsonConvert.SerializeObject(asm);
        // }

        public IEnumerable<EP> FindProviders(string relPath = "ext", Assembly relToAsm = null)
        {
            if (relToAsm == null)
                relToAsm = typeof(EP).GetTypeInfo().Assembly;
            
            if (string.IsNullOrWhiteSpace(relToAsm.Location))
                throw new InvalidOperationException(AsmRes.EXCEPTIONS["invalid assembly is missing location"]);

            var asmDir = Path.GetDirectoryName(relToAsm.Location);
            var extDir = Path.Combine(asmDir, relPath);

            var conventions = new ConventionBuilder();
            conventions.ForTypesDerivedFrom<EP>()
                    .Export<EP>()
                    .Shared();

            var configuration = new ContainerConfiguration()
                    .WithAssemblies(_BuiltInAssemblies, conventions);
            
            if (Directory.Exists(extDir))
                configuration.WithAssembliesInPath(extDir, conventions);
            
            using (var container = configuration.CreateContainer())
            {
                _foundProviders = container.GetExports<EP>().ToArray();
            }

            return _foundProviders;
        }

        public EP GetProvider(string name)
        {
            return FoundProviders.FirstOrDefault(
                    ep => name.Equals(ep.Describe().Name));
        }
    }
}