using System.Collections.Generic;
namespace Lab3.Ast.Expressions {
	sealed class MemberAccess : BaseNode, IExpression {
		public readonly IExpression Obj;
		public Token OperatorToken => (Token)Children[1];
		public readonly string Member;
		public MemberAccess(IReadOnlyList<INode> children, IExpression obj, string member) : base(children) {
			Obj = obj;
			Member = member;
		}
		public override string FormattedString => $"{Obj.FormattedString}.{Member}";
		public void Accept(IExpressionVisitor visitor) => visitor.VisitMemberAccess(this);
		public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.VisitMemberAccess(this);
	}
}
