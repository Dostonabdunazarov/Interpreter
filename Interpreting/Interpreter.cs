﻿using DynamicRuntime;
using Lab3.Ast;
using Lab3.Ast.Expressions;
using Lab3.Ast.Statements;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static System.Diagnostics.Debug;
namespace Lab3.Interpreting {
	sealed class Interpreter : IStatementVisitor, IExpressionVisitor<object> {
		static readonly object missingVariable = new object();
		SourceFile sourceFile;
		Dictionary<string, object> currentBlockShadowedVariables = null;
		public readonly IDictionary<string, object> Variables;
		public Interpreter() {
			Variables = new Dictionary<string, object>();
			foreach (var field in typeof(BuiltinVariables).GetFields()) {
				if (field.IsStatic && field.FieldType == typeof(object)) {
					Variables[field.Name] = field.GetValue(null);
				}
			}
		}
		public static void Run(ProgramNode program) {
			var interpreter = new Interpreter();
			interpreter.RunProgram(program);
		}
		public void RunProgram(ProgramNode program) {
			sourceFile = program.SourceFile;
			try {
				foreach (var statement in program.Statements) {
					Run(statement);
				}
			}
			finally {
				sourceFile = null;
			}
		}
		void RunBlock(Block block) {
			var oldShadowedVariables = currentBlockShadowedVariables;
			currentBlockShadowedVariables = new Dictionary<string, object>();
			foreach (var statement in block.Statements) {
				Run(statement);
			}
			foreach (var kv in currentBlockShadowedVariables) {
				var name = kv.Key;
				var shadowedVariable = kv.Value;
				if (shadowedVariable == missingVariable) {
					Variables.Remove(name);
				}
				else {
					Variables[name] = shadowedVariable;
				}
			}
			currentBlockShadowedVariables = oldShadowedVariables;
		}
		#region statements
		void Run(IStatement statement) {
			statement.Accept(this);
		}
		public void VisitIf(If ifStatement) {
			if (Op.ToBool(Calc(ifStatement.Condition))) {
				RunBlock(ifStatement.Body);
			}
		}
		public void VisitWhile(While whileStatement) {
			while (Op.ToBool(Calc(whileStatement.Condition))) {
				RunBlock(whileStatement.Body);
			}
		}
		public void VisitExpressionStatement(ExpressionStatement expressionStatement) {
			Calc(expressionStatement.Expr);
		}
		public void VisitVariableDeclaration(VariableDeclaration variableDeclaration) {
			var name = variableDeclaration.VariableName;
			if (currentBlockShadowedVariables != null && !currentBlockShadowedVariables.ContainsKey(name)) {
				if (Variables.TryGetValue(name, out object value)) {
					currentBlockShadowedVariables[name] = value;
				}
				else {
					currentBlockShadowedVariables[name] = missingVariable;
				}
			}
			Variables[name] = Calc(variableDeclaration.Expr);
		}
		public void VisitVariableAssignment(VariableAssignment variableAssignment) {
			if (!Variables.ContainsKey(variableAssignment.VariableName)) {
				throw MakeError(variableAssignment.OperatorToken.BeginOffset, $"Присваивание в неизвестную переменную {variableAssignment.VariableName}");
			}
			Variables[variableAssignment.VariableName] = Calc(variableAssignment.Expr);
		}
		#endregion
		#region expressions
		object Calc(IExpression expression) {
			return expression.Accept(this);
		}
		public object VisitBinary(Binary binary) {
			switch (binary.Operator) {
				case BinaryOperator.Addition:
					return CalcAddition(binary);
				case BinaryOperator.Subtraction:
					return CalcSubtraction(binary);
				case BinaryOperator.Multiplication:
					return CalcMultiplication(binary);
				case BinaryOperator.Division:
					return CalcDivision(binary);
				case BinaryOperator.Remainder:
					return CalcReminder(binary);
				case BinaryOperator.Equal:
					return CalcEqual(binary);
				case BinaryOperator.Less:
					return CalcLess(binary);
				default:
					throw MakeError(binary.OperatorToken.BeginOffset, $"Неизвестная операция {binary.Operator}");
			}
		}
		#region binary operations
		object CalcAddition(Binary binary) {
			Assert(binary.Operator == BinaryOperator.Addition);
			return Op.Add(Calc(binary.Left), Calc(binary.Right));
		}
		object CalcSubtraction(Binary binary) {
			Assert(binary.Operator == BinaryOperator.Subtraction);
			return Op.Sub(Calc(binary.Left), Calc(binary.Right));
		}
		object CalcMultiplication(Binary binary) {
			Assert(binary.Operator == BinaryOperator.Multiplication);
			return Op.Mul(Calc(binary.Left), Calc(binary.Right));
		}
		object CalcDivision(Binary binary) {
			Assert(binary.Operator == BinaryOperator.Division);
			return Op.Div(Calc(binary.Left), Calc(binary.Right));
		}
		object CalcReminder(Binary binary) {
			Assert(binary.Operator == BinaryOperator.Remainder);
			return Op.Rem(Calc(binary.Left), Calc(binary.Right));
		}
		object CalcEqual(Binary binary) {
			Assert(binary.Operator == BinaryOperator.Equal);
			return Op.Eq(Calc(binary.Left), Calc(binary.Right));
		}
		object CalcLess(Binary binary) {
			Assert(binary.Operator == BinaryOperator.Less);
			return Op.Lt(Calc(binary.Left), Calc(binary.Right));
		}
		public object VisitUnary(Unary unary) {
			switch (unary.Operator) {
				case UnaryOperator.UnaryPlus:
					return CalcUnaryPlus(unary);
				case UnaryOperator.UnaryMinus:
					return CalcUnaryMinus(unary);
				case UnaryOperator.BitwiseNegation:
					return CalcBitwiseNegation(unary);
				case UnaryOperator.LogicalNegation:
					return CalcLogicalNegation(unary);
				default:
					throw MakeError(unary.OperatorToken.BeginOffset, $"Неизвестная операция {unary.Operator}");
			}
		}
		object CalcUnaryPlus(Unary unary) {
			Assert(unary.Operator == UnaryOperator.UnaryPlus);
			return Op.UnaryPlus(Calc(unary.Value));
		}
		object CalcUnaryMinus(Unary unary) {
			Assert(unary.Operator == UnaryOperator.UnaryMinus);
			return Op.UnaryMinus(Calc(unary.Value));
		}
		object CalcBitwiseNegation(Unary unary) {
			Assert(unary.Operator == UnaryOperator.BitwiseNegation);
			return Op.BitwiseNegation(Calc(unary.Value));
		}
		object CalcLogicalNegation(Unary unary) {
			Assert(unary.Operator == UnaryOperator.LogicalNegation);
			return Op.LogicalNegation(Calc(unary.Value));
		}
		#endregion
		public object VisitParentheses(Parentheses parentheses) {
			return Calc(parentheses.Expr);
		}
		public object VisitCall(Call call) {
			var value = Calc(call.Function);
			var args = call.Arguments.Select(Calc).ToArray();
			return Op.Call(value, args);
		}
		public object VisitNumber(Number number) {
			if (int.TryParse(number.Lexeme, NumberStyles.None, NumberFormatInfo.InvariantInfo, out int value)) {
				return value;
			}
			throw MakeError(number.BeginOffset, $"Не удалось преобразовать {number.Lexeme} в int");
		}
		public object VisitIdentifier(Identifier identifier) {
			if (Variables.TryGetValue(identifier.Name, out object value)) {
				return value;
			}
			throw MakeError(identifier.BeginOffset, $"Неизвестная переменная {identifier.Name}");
		}
		public object VisitMemberAccess(MemberAccess memberAccess) {
			throw new NotSupportedException();
		}
		#endregion
		Exception MakeError(int position, string message) {
			return new Exception(sourceFile.MakeErrorMessage(position, message));
		}
	}
}
