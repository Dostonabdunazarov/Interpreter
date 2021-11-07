using DynamicRuntime;
using Lab3.Ast;
using Lab3.DynamicCompiling;
using Lab3.Interpreting;
using Lab3.Parsing;
using System;
using System.IO;
using System.Text;
namespace Lab3 {
	public static class Program {
		static void CheckedRun(Action action) {
			var outputWriter = new StringWriter();
			BuiltinVariables.SetOutput(outputWriter);
			action();
			BuiltinVariables.SetOutput(Console.Out);
			var output = outputWriter.ToString();
			string expectedOutputPath = "../../code_output.txt";
			var expectedOutput = File.ReadAllText(expectedOutputPath, Encoding.UTF8);
			if (output != expectedOutput) {
				throw new Exception($"Вывод не совпадает с {expectedOutputPath}");
			}
		}
		public static void Main(string[] args) {
			var program = Parser.Parse(SourceFile.Read("../../code.txt"));
			Interpreter.Run(program);
			CompilingInterpreter.Run(program);
			CheckedRun(() => Interpreter.Run(program));
			CheckedRun(() => new CompilingInterpreter { BinPath = "out2.exe" }.CompileAndRun(program));
			Console.WriteLine("Всё хорошо!");
		}
	}
}
