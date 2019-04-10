using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace elb_utilities.Addins
{
    [Serializable]
    public class AddinLoader : MarshalByRefObject
    {
        public static List<Type> Load<T>(string pluginPath, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (!typeof(T).IsInterface) throw new Exception($"{nameof(AddinLoader)}.{nameof(Load)} called with non-interface type: {typeof(T).Name}");

            var addins = new List<Type>();

            // scan for plugins in other dlls in a new appdomain so any assemblies scanned and not containing plugins are unloaded
            using (var appDomain = new AppDomainWithType<AddinLoader>())
            {
                var addinLoader = appDomain.TypeObject;

                addins.AddRange(addinLoader.GetImplementationsFromPath<T>(pluginPath, "*.dll", searchOption));
            }

            return addins;
        }

        private List<Type> GetImplementationsFromPath<T>(string path, string searchPattern, SearchOption searchOption)
        {
            var list = new List<Type>();

            if (Directory.Exists(path))
            {
                list.AddRange(GetImplementationsFromAssemblies<T>(Directory.GetFiles(path, searchPattern, searchOption).Select(file => Assembly.LoadFrom(file)))); 
            }

            return list;
        }

        public static IEnumerable<Type> GetImplementationsFromAssemblies<T>(IEnumerable<Assembly> assemblies)
        {
            if (!typeof(T).IsInterface) throw new Exception($"{nameof(AddinLoader)}.{nameof(GetImplementationsFromAssemblies)} called with non-interface type: {typeof(T).Name}");

            return assemblies.SelectMany(a => GetImplementationsFromAssembly<T>(a));
        }

        public static List<Type> GetImplementationsFromAssembly<T>(Assembly assembly)
        {
            return assembly.GetTypes().Where(t => typeof(T).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).ToList();
        }
    }
}
