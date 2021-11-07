using System.Collections.Generic;
namespace Lab3.Ast.Expressions {
	sealed class Binary : BaseNode, IExpression {
		public readonly IExpression Left;
		public Token OperatorToken => (Token)Children[1];
		public readonly BinaryOperator Operator;
		public readonly IExpression Right;
		public Binary(IReadOnlyList<INode> children, IExpression left, BinaryOperator op, IExpression right) : base(children) {
			Left = left;
			Operator = op;
			Right = right;
		}
		static readonly IReadOnlyDictionary<BinaryOperator, string> operators = new Dictionary<BinaryOperator, string>{
			{ BinaryOperator.Addition, "+" },
			{ BinaryOperator.Subtraction, "-" },
			{ BinaryOperator.Multiplication, "*" },
			{ BinaryOperator.Division, "/" },
			{ BinaryOperator.Remainder, "%" },
			{ BinaryOperator.Equal, "==" },
			{ BinaryOperator.Less, "<" },
		};
		public string OperatorString => operators[Operator];
		public override string FormattedString => $"{Left.FormattedString} {OperatorString} {Right.FormattedString}";
		public void Accept(IExpressionVisitor visitor) => visitor.VisitBinary(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitBinary(this);
	}
}
