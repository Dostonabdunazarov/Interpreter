using System.Collections.Generic;
namespace Lab3.Ast.Expressions {
	sealed class Unary : BaseNode, IExpression {
		public Token OperatorToken => (Token)Children[0];
		public readonly UnaryOperator Operator;
		public readonly IExpression Value;
		public Unary(IReadOnlyList<INode> children, UnaryOperator op, IExpression value) : base(children) {
			Operator = op;
			Value = value;
		}
		static readonly IReadOnlyDictionary<UnaryOperator, string> operators = new Dictionary<UnaryOperator, string>{
			{ UnaryOperator.UnaryPlus, "+" },
			{ UnaryOperator.UnaryMinus, "-" },
			{ UnaryOperator.BitwiseNegation, "~" },
			{ UnaryOperator.LogicalNegation, "!" },
		};
		public string OperatorString => operators[Operator];
		public override string FormattedString => $"{OperatorString}{Value.FormattedString}";
		public void Accept(IExpressionVisitor visitor) => visitor.VisitUnary(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitUnary(this);
	}
}
