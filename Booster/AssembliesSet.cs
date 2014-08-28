using Booster.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Booster
{
    internal class AssembliesSet
    {
        private readonly List<Assembly> _assemblies = new List<Assembly>();

        internal void TryAppendAssembly(byte[] rawAssembly)
        {
            Assembly asm;
            try
            {
                asm = Assembly.Load(rawAssembly);
                AssembliesResolver.Register(asm);
                _assemblies.Add(asm);
            }
            catch { }
        }

        private List<Type> CollectExportedTypes()
        {
            List<Type> types = new List<Type>();
            foreach (var asm in _assemblies)
            {
                types.AddRange(asm.GetExportedTypes());
            }
            return types;
        }

        private object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);
            return null;
        }

        internal void CallMain()
        {
            foreach (var type in CollectExportedTypes())
            {
                MethodInfo main = type.GetMethod("Main");
                if (main != null)
                {
                    ParameterInfo[] paramsInfo = main.GetParameters();
                    object[] parameters = new object[paramsInfo.Length];
                    for (int i = 0; i < paramsInfo.Length; i++)
                    {
                        parameters[i] = GetDefaultValue(paramsInfo[i].ParameterType);
                    }
                    main.Invoke(null, parameters);
                }
            }
        }
    }
}
