using System.Collections.Generic;
namespace Lab3.Ast.Statements {
	sealed class VariableDeclaration : BaseNode, IStatement {
		public Token VarToken => (Token)Children[0];
		public readonly string VariableName;
		public Token OperatorToken => (Token)Children[2];
		public readonly IExpression Expr;
		public Token Semicolon => (Token)Children[4];
		public VariableDeclaration(IReadOnlyList<INode> children, string variableName, IExpression expr) : base(children) {
			VariableName = variableName;
			Expr = expr;
		}
		public override string FormattedString => $"var {VariableName} = {Expr.FormattedString};\n";
		public void Accept(IStatementVisitor visitor) => visitor.VisitVariableDeclaration(this);
		public T Accept<T>(IStatementVisitor<T> visitor) => visitor.VisitVariableDeclaration(this);
	}
}
