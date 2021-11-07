using Lab3.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
namespace Lab3.Ast {
	sealed class SourceFile {
		public readonly string Path;
		public readonly string Text;
		public readonly IReadOnlyList<string> Lines;
		SourceFile(string path, string text) {
			Path = path;
			Text = text;
			Lines = newLine.Split(Text);
		}
		public static SourceFile FromString(string sourceCode) {
			return new SourceFile("<memory>", sourceCode);
		}
		public static SourceFile Read(string path) {
			var text = File.ReadAllText(path, Encoding.UTF8);
			return new SourceFile(path, text);
		}
		static readonly Regex newLine = RegexUtils.CreateRegex(@"(?<=\r\n|\n)");
		public Location GetLocation(int offset) {
			var lineOffset = 0;
			var lineIndex = 0;
			var columnIndex = 0;
			foreach (var line in Lines) {
				if (offset < lineOffset + line.Length) {
					columnIndex = offset - lineOffset;
					break;
				}
				lineOffset += line.Length;
				lineIndex += 1;
			}
			return new Location {
				LineIndex = lineIndex,
				ColumnIndex = columnIndex,
			};
		}
		public IEnumerable<string> FormatLines(
			int offset,
			int linesAround = 1,
			bool inlinePointer = false,
			string pointer = "^",
			int maxLineNumberLength = 5,
			string tabReplacement = "    "
		) {
			var location = GetLocation(offset);
			var lineIndex = location.LineIndex;
			var columnIndex = location.ColumnIndex;
			var minI = Math.Max(0, lineIndex - linesAround);
			var maxI = Math.Min(Lines.Count - 1, lineIndex + linesAround);
			for (var i = minI; i <= maxI; i++) {
				var line = Lines[i].TrimEnd();
				var lineWithoutTabs = line.Replace("\t", tabReplacement);
				var lineNumber = $"{i + 1}:".PadRight(maxLineNumberLength).Substring(0, maxLineNumberLength);
				if (i == lineIndex) {
					var leftPart = line.Substring(0, columnIndex);
					if (inlinePointer) {
						yield return lineNumber + leftPart + pointer + line.Substring(columnIndex);
					}
					else {
						yield return lineNumber + lineWithoutTabs;
						var columnIndexWithoutTabs = leftPart.Replace("\t", tabReplacement).Length;
						yield return new string('-', maxLineNumberLength + columnIndexWithoutTabs) + pointer;
					}
				}
				else {
					yield return lineNumber + lineWithoutTabs;
				}
			}
		}
		public string MakeErrorMessage(int offset, string message) {
			return message + "\n" + string.Join("\n", FormatLines(offset));
		}
	}
}
