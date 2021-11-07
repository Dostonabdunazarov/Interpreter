using System.Collections.Generic;
using System.Linq;
namespace Lab3.Ast.Expressions {
	sealed class Call : BaseNode, IExpression {
		public readonly IExpression Function;
		public Token LeftParenthesis => (Token)Children[1];
		public readonly IReadOnlyList<IExpression> Arguments;
		public Token RightParenthesis => (Token)Children[Children.Count - 1];
		public Call(IReadOnlyList<INode> children, IExpression function, IReadOnlyList<IExpression> arguments) : base(children) {
			Function = function;
			Arguments = arguments;
		}
		public override string FormattedString => $"{Function.FormattedString}({string.Join(", ", Arguments.Select(x => x.FormattedString))})";
		public void Accept(IExpressionVisitor visitor) => visitor.VisitCall(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitCall(this);
	}
}
