using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Security;

namespace ACMESharp.Ext
{
    public static class ExtUtil
    {
        public static readonly IEnumerable<KeyValuePair<string, object>> NO_TRAITS =
                new KeyValuePair<string, object>[0];

        public static readonly IEnumerable<ExtParamInfo> NO_PARAMS =
                new ExtParamInfo[0];

        public const string TRAIT_VERSION = "Version";
        public const string TRAIT_LICENSE = "License";
        public const string TRAIT_URL = "URL";

        public static readonly IEnumerable<string> COMMON_TRAITS = new string[]
        {
            TRAIT_VERSION,
            TRAIT_LICENSE,
            TRAIT_URL,
        };

        /// <summary>
        /// Utility method to validate the supplied parameter values
        /// dictionary against the specification of parameters provided
        /// in the parameter info collection.
        /// </summary>
        /// <remarks>
        /// Specifically, this routine validates that all provided parameter
        /// values respect the specification of the required and optional
        /// parameters, and that the parameter value types are compliant
        /// with the specification of their paramter type. 
        /// </remarks>
        public static void ResolveParams(
                IEnumerable<ExtParamInfo> paramInfos,
                IDictionary<string, object> paramVals)
        {
            ResolveParams<object>(paramInfos, paramVals);
        }

        /// <summary>
        /// Performs the same resolution and validation as
        /// <see cref="ResolveParams"/> but also assigns the resolved
        /// parameter values to their corresponding same-named properties
        /// of the target object.
        /// </summary>
        /// <param name="strictTarget">
        /// If <c>true</c> and the target is missing one of the resolved
        /// parameters, an exception will be thrown.
        /// </param>
        public static T ResolveParams<T>(
                IEnumerable<ExtParamInfo> paramInfos,
                IDictionary<string, object> paramVals,
                T target = default(T), bool strictTarget = false)
        {
            Type targetType = null;
            TypeInfo targetTypeInfo = null;
            if (target != null)
            {
                targetType = target.GetType();
                targetTypeInfo = targetType.GetTypeInfo();
            }

            foreach (var pi in paramInfos)
            {
                object val = null;
                if (!(paramVals?.TryGetValue(pi.Name, out val)).GetValueOrDefault())
                {
                    if (pi.IsRequired)
                        throw new ArgumentNullException(pi.Name,
                                AsmRes.EXCEPTIONS["missing required provider parameter"]);
                }
                else
                {
                    try
                    {
                        switch (pi.Type)
                        {
                            case ExtParamType.STRING:
                            case ExtParamType.TEXT:
                                val = (string)val;
                                break;
                            case ExtParamType.NUMBER:
                                val = (int)val;
                                break;
                            case ExtParamType.BOOLEAN:
                                val = (bool)val;
                                break;
                            case ExtParamType.SECRET:
                                val = (SecureString)val;
                                break;
                            case ExtParamType.SEQ_STRING:
                                val = (IEnumerable<string>)val;
                                break;
                            case ExtParamType.KVP_STRING:
                                val = (IDictionary<string, string>)val;
                                break;
                            
                            default:
                                throw new NotImplementedException(AsmRes.EXCEPTIONS.With(
                                        "parameter type [{0}] is not supported", pi.Type));
                        }
                    }
                    catch (InvalidCastException)
                    {
                        throw new ArgumentException(
                                AsmRes.EXCEPTIONS["incompatible type for provider parameter"],
                                pi.Name);
                    }

                    if (targetTypeInfo != null)
                    {
                        var prop = targetTypeInfo.GetProperty(pi.Name,
                                BindingFlags.Public | BindingFlags.Instance);
                        if (prop == null)
                        {
                            if (strictTarget)
                                throw new MissingMemberException(AsmRes.EXCEPTIONS.With(
                                        "target object is missing expected property [{0}]", pi.Name));
                        }
                        else
                        {
                            prop.SetValue(target, val);
                        }
                    }
                }
            }

            return target;
        }

        // Based on:
        //    http://weblogs.asp.net/ricardoperes/using-mef-in-net-core
        internal static ContainerConfiguration WithAssembliesInPath(
                this ContainerConfiguration configuration,
                string path,
                SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return WithAssembliesInPath(configuration, path, null, searchOption);
        }

        // Based on:
        //    http://weblogs.asp.net/ricardoperes/using-mef-in-net-core
        internal static ContainerConfiguration WithAssembliesInPath(
                this ContainerConfiguration configuration, string path,
                AttributedModelProvider conventions,
                SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var assemblies = Directory
                .GetFiles(path, "*.dll", searchOption)
                .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                // TODO:  This didn't seem to work, LoadFromAsmName kept throwing
                // FileNotFoundException even though the AsmName was legit
                // .Select(AssemblyLoadContext.GetAssemblyName)
                // .Select(AssemblyLoadContext.Default.LoadFromAssemblyName)
                .ToList();

            configuration.WithAssemblies(assemblies, conventions);

            return configuration;
        }
    }
}