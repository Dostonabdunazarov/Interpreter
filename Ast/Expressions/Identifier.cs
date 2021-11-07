using System.Collections.Generic;
namespace Lab3.Ast.Expressions {
	sealed class Identifier : BaseNode, IExpression {
		public Token Token => (Token)Children[0];
		public readonly string Name;
		public Identifier(IReadOnlyList<INode> children, string name) : base(children) {
			Name = name;
		}
		public override string FormattedString => Name;
		public void Accept(IExpressionVisitor visitor) => visitor.VisitIdentifier(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitIdentifier(this);
	}
}
