﻿using Lab3.Ast.Expressions;
namespace Lab3.Ast {
	interface IExpressionVisitor {
		void VisitBinary(Binary binary);
		void VisitUnary(Unary unary);
		void VisitCall(Call call);
		void VisitParentheses(Parentheses parentheses);
		void VisitNumber(Number number);
		void VisitIdentifier(Identifier identifier);
		void VisitMemberAccess(MemberAccess memberAccess);
	}
	interface IExpressionVisitor<T> {
		T VisitBinary(Binary binary);
		T VisitUnary(Unary unary);
		T VisitCall(Call call);
		T VisitParentheses(Parentheses parentheses);
		T VisitNumber(Number number);
		T VisitIdentifier(Identifier identifier);
		T VisitMemberAccess(MemberAccess memberAccess);
	}
}
