using Lab3.Parsing;
namespace Lab3.Ast {
	sealed class Token : INode {
		public int BeginOffset { get; }
		public int EndOffset => BeginOffset + Lexeme.Length;
		public readonly TokenType Type;
		public readonly string Lexeme;
		public Token(int beginOffset, TokenType type, string lexeme) {
			Regexes.Instance.CheckToken(type, lexeme);
			BeginOffset = beginOffset;
			Type = type;
			Lexeme = lexeme;
		}
		public string FormattedString => Lexeme;
		public override string ToString() => $"{BeginOffset}: {Type} \"{Lexeme}\"";
		public void Check(SourceFile sourceFile) {
		}
	}
}
