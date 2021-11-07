using System.Collections.Generic;
using System.Linq;
namespace Lab3.Ast {
	sealed class Block : BaseNode, INode {
		public Token LeftBrace => (Token)Children[0];
		public readonly IReadOnlyList<IStatement> Statements;
		public Token RightBrace => (Token)Children[Children.Count - 1];
		public Block(IReadOnlyList<INode> children, IReadOnlyList<IStatement> statements) : base(children) {
			Statements = statements;
		}
		public override string FormattedString => "{\n" + string.Join("", Statements.Select(x => x.FormattedString)) + "}\n";
	}
}
