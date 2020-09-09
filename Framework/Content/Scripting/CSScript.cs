using Microsoft.CSharp;
using Spectrum.Framework.Entities;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content.Scripting
{
    public class CSScript
    {
        Assembly assembly;
        ConstructorInfo constructor;
        public CSScript(params string[] filenames)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters()
            {
                GenerateInMemory = true,
                GenerateExecutable = false,
            };
            parameters.ReferencedAssemblies.Add("Spectrum.dll");
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Linq.dll");
            parameters.ReferencedAssemblies.Add("MonoGame.Framework.dll");
            parameters.IncludeDebugInformation = true;
            parameters.CompilerOptions = "/d:DEBUG";
            var result = provider.CompileAssemblyFromFile(parameters, filenames);
            if (result.Errors.HasErrors)
            {
                var errors = result.Errors;
                var text = Enumerable.Range(0, errors.Count).Select(i => errors[i].ErrorText).ToList();
                throw new Exception(string.Join("\n", text));
            }
            assembly = result.CompiledAssembly;
            var candidateTypes = assembly.DefinedTypes
                .Where(typeof(Component).IsAssignableFrom).ToList();
            var d = AppDomain.CurrentDomain;
            if (candidateTypes.Count != 1)
                throw new InvalidOperationException(
                    $"C# scripts should contain exactly 1 Component type, found {candidateTypes.Count}");
            constructor = candidateTypes.First().GetConstructor(new Type[] { });
        }

        public Component GetComponent()
        {
            return constructor.Invoke(new object[] { }) as Component;
        }
    }
}
