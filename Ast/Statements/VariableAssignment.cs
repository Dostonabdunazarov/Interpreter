using System.Collections.Generic;
namespace Lab3.Ast.Statements {
	sealed class VariableAssignment : BaseNode, IStatement {
		public readonly string VariableName;
		public Token OperatorToken => (Token)Children[1];
		public readonly IExpression Expr;
		public Token Semicolon => (Token)Children[3];
		public VariableAssignment(IReadOnlyList<INode> children, string variableName, IExpression expr) : base(children) {
			VariableName = variableName;
			Expr = expr;
		}
		public override string FormattedString => $"{VariableName} = {Expr.FormattedString};\n";
		public void Accept(IStatementVisitor visitor) => visitor.VisitVariableAssignment(this);
		public T Accept<T>(IStatementVisitor<T> visitor) => visitor.VisitVariableAssignment(this);
	}
}
