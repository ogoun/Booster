using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Booster.Helpers
{
    internal static class AssembliesResolver
    {
        private static readonly ConcurrentDictionary<string, Assembly> _contractAssembliesNamesCachee = new ConcurrentDictionary<string, Assembly>();

        static AssembliesResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyResolve;
        }

        static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly asm;
            _contractAssembliesNamesCachee.TryGetValue(args.Name, out asm);
            if (asm == null)
            {
                throw new DllNotFoundException(args.Name);
            }
            return asm;
        }

        public static void Register(Assembly asm)
        {
            if (!_contractAssembliesNamesCachee.ContainsKey(asm.FullName))
            {
                _contractAssembliesNamesCachee.TryAdd(asm.FullName, asm);
            }
        }
    }
}