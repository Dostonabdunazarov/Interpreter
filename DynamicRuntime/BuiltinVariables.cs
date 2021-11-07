using DynamicRuntime.Functions;
using System;
using System.IO;
namespace DynamicRuntime {
	public static class BuiltinVariables {
		public static object @true = true;
		public static object @false = false;
		public static object @null = null;
		public static object dump = new DumpFunction(Console.Out);
		public static object trace = new TraceFunction(Console.Out);
		public static void SetOutput(TextWriter output) {
			dump = new DumpFunction(output);
			trace = new TraceFunction(output);
		}
	}
}
