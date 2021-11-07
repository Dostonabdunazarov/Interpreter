using System.Collections.Generic;
namespace Lab3.Ast.Statements {
	sealed class ExpressionStatement : BaseNode, IStatement {
		public readonly IExpression Expr;
		public Token Semicolon => (Token)Children[1];
		public ExpressionStatement(IReadOnlyList<INode> children, IExpression expr) : base(children) {
			Expr = expr;
		}
		public override string FormattedString => $"{Expr.FormattedString};\n";
		public void Accept(IStatementVisitor visitor) => visitor.VisitExpressionStatement(this);
		public T Accept<T>(IStatementVisitor<T> visitor) => visitor.VisitExpressionStatement(this);
	}
}
