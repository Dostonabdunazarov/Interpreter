using Lab3.Ast;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using System;
using System.IO;
namespace Lab3.DynamicCompiling {
	sealed class CompilingInterpreter {
		public string ClassName = "Program";
		public string MethodName = "Main";
		public string BinPath = "out.exe";
		public bool WritePdb = true;
		public static void Run(ProgramNode program) {
			new CompilingInterpreter().CompileAndRun(program);
		}
		public void CompileAndRun(ProgramNode program) {
			Compile(program);
			RunCompiled();
		}
		public void Compile(ProgramNode program) {
			var document = new Document(Path.GetFullPath(program.SourceFile.Path)) {
				Type = DocumentType.Text,
			};
			var module = ModuleDefinition.CreateModule(
				Path.GetFileName(BinPath),
				ModuleKind.Console
			);
			var mainClass = new TypeDefinition(
				"", ClassName,
				TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
				module.TypeSystem.Object
			);
			module.Types.Add(mainClass);
			var method = new MethodDefinition(
				MethodName,
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
				module.TypeSystem.Void
			);
			mainClass.Methods.Add(method);
			var compiler = new Compiler(document, program, method);
			compiler.CompileProgram();
			module.EntryPoint = method;
			module.Write(BinPath, new WriterParameters {
				SymbolWriterProvider = WritePdb ? new PdbWriterProvider() : null,
			});
		}
		public void RunCompiled() {
			var method = System.Reflection.Assembly.LoadFrom(BinPath).GetType(ClassName).GetMethod(MethodName);
			method.Invoke(null, Array.Empty<object>());
		}
	}
}
