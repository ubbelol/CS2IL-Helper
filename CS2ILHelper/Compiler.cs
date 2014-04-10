using System;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CS2ILHelper
{
	public class Compiler
	{
		private CSharpCodeProvider _provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.5" } });
		
		public bool Compile(string file, string output, string version, out string sanitizedSource, out string errors) {
			_provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v" + version[0] + "." + version[1] } });
			
			var src = System.IO.File.ReadAllText (file);
			var @params = new CompilerParameters() { CompilerOptions = "/target:exe /unsafe", GenerateExecutable = true, OutputAssembly = output };
			
			@params.IncludeDebugInformation = true;
			@params.TreatWarningsAsErrors = false;
			@params.WarningLevel = 3;
			
			string references = src.Split (new[] { "---CODE---" }, StringSplitOptions.None)[0];
			string code = "";
			
			@params.ReferencedAssemblies.Add ("mscorlib.dll");
			
			foreach(var @ref in references.Split(new[] { "\r\n" }, StringSplitOptions.None))
				code += "using " + @ref + ";" + Environment.NewLine;

			code += "namespace CS2ILStub " + Environment.NewLine +
				"{" + Environment.NewLine +
				"public class Stub" + Environment.NewLine +
				"{" + Environment.NewLine +
				"public static void Main(string[] args){}" + Environment.NewLine;
			code += (sanitizedSource = src.Split (new[] { "---CODE---" }, StringSplitOptions.None)[1]);
			code += "}" + Environment.NewLine + "}";
			
			File.WriteAllText ("./files/source", code);
			
			var result = _provider.CompileAssemblyFromSource(@params, code);
			var formattedErrors = new StringBuilder();
			
			foreach(var err in result.Errors)
				formattedErrors.AppendLine(err.ToString ());
			
			errors = formattedErrors.ToString ();
			return !result.Errors.HasErrors;
		}
		
	}
}
