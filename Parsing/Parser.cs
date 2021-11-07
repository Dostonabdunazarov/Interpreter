using Lab3.Ast;
using Lab3.Ast.Expressions;
using Lab3.Ast.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Lab3.Parsing {
	sealed class Parser {
		readonly SourceFile sourceFile;
		readonly IReadOnlyList<Token> tokens;
		int tokenIndex = 0;
		Token CurrentToken => tokens[tokenIndex];
		int CurrentPosition => CurrentToken.BeginOffset;
		Parser(SourceFile sourceFile, IReadOnlyList<Token> tokens) {
			this.sourceFile = sourceFile;
			this.tokens = tokens;
		}
		#region stuff
		string[] DebugCurrentPosition => sourceFile.FormatLines(CurrentPosition,
			inlinePointer: true,
			pointer: " <|> "
			).ToArray();
		string DebugCurrentLine => string.Join("", sourceFile.FormatLines(CurrentPosition,
			linesAround: 0,
			inlinePointer: true,
			pointer: " <|> "
			).ToArray());
		T CheckNode<T>(T node) where T : INode {
			node.Check(sourceFile);
			return node;
		}
		Exception MakeError(string message) {
			return new Exception(sourceFile.MakeErrorMessage(CurrentPosition, message));
		}
		void Reset() {
			tokenIndex = 0;
		}
		void ReadNextToken() {
			tokenIndex += 1;
		}
		static T AddChild<T>(T child, List<INode> children) where T : INode {
			children.Add(child);
			return child;
		}
		bool SkipIf(string s, List<INode> children) {
			var token = CurrentToken;
			if (token.Lexeme == s) {
				children.Add(token);
				ReadNextToken();
				return true;
			}
			return false;
		}
		void Expect(string s, List<INode> children) {
			if (!SkipIf(s, children)) {
				throw MakeError($"Ожидали \"{s}\", получили {CurrentToken}");
			}
		}
		void ExpectEof() {
			if (CurrentToken.Type != TokenType.EndOfFile) {
				throw MakeError($"Недопарсили до конца, остался {CurrentToken}");
			}
		}
		#endregion
		public static ProgramNode Parse(SourceFile sourceFile) {
			var eof = new Token(sourceFile.Text.Length, TokenType.EndOfFile, "");
			var tokens = Lexer.GetTokens(sourceFile).Concat(new[] { eof }).Where(Lexer.IsNotWhitespace).ToList();
			var parser = new Parser(sourceFile, tokens);
			return parser.ParseProgram();
		}
		ProgramNode ParseProgram() {
			Reset();
			var statements = new List<IStatement>();
			while (CurrentToken.Type != TokenType.EndOfFile) {
				statements.Add(ParseStatement());
			}
			var result = CheckNode(new ProgramNode(sourceFile, statements));
			ExpectEof();
			return result;
		}
		Block ParseBlock() {
			var children = new List<INode>();
			Expect("{", children);
			var statements = new List<IStatement>();
			while (!SkipIf("}", children)) {
				var statement = AddChild(ParseStatement(), children);
				statements.Add(statement);
			}
			return CheckNode(new Block(children, statements));
		}
		IStatement ParseStatement() {
			var children = new List<INode>();
			if (SkipIf("if", children)) {
				Expect("(", children);
				var condition = AddChild(ParseExpression(), children);
				Expect(")", children);
				var block = AddChild(ParseBlock(), children);
				return CheckNode(new If(children, condition, block));
			}
			if (SkipIf("while", children)) {
				Expect("(", children);
				var condition = AddChild(ParseExpression(), children);
				Expect(")", children);
				var block = AddChild(ParseBlock(), children);
				return CheckNode(new While(children, condition, block));
			}
			if (SkipIf("var", children)) {
				var variable = ParseIdentifier(children);
				Expect("=", children);
				var expression = AddChild(ParseExpression(), children);
				Expect(";", children);
				return CheckNode(new VariableDeclaration(children, variable, expression));
			}
			var leftExpression = AddChild(ParseExpression(), children);
			if (SkipIf("=", children)) {
				if (!(leftExpression is Identifier identifier)) {
					throw MakeError("Присваивание не в переменную");
				}
				var rightExpression = AddChild(ParseExpression(), children);
				Expect(";", children);
				return CheckNode(new VariableAssignment(children, identifier.Name, rightExpression));
			}
			else {
				Expect(";", children);
				return CheckNode(new ExpressionStatement(children, leftExpression));
			}
		}
		string ParseIdentifier(List<INode> children) {
			if (CurrentToken.Type != TokenType.Identifier) {
				throw MakeError($"Ожидали идентификатор, получили {CurrentToken}");
			}
			var currentToken = AddChild(CurrentToken, children);
			ReadNextToken();
			return currentToken.Lexeme;
		}
		#region expressions
		IExpression ParseExpression() {
			return ParseEqualityExpression();
		}
		IExpression ParseEqualityExpression() {
			var left = ParseRelationalExpression();
			while (true) {
				var children = new List<INode> { left };
				if (SkipIf("==", children)) {
					var right = AddChild(ParseRelationalExpression(), children);
					left = CheckNode(new Binary(children, left, BinaryOperator.Equal, right));
				}
				else {
					break;
				}
			}
			return left;
		}
		IExpression ParseRelationalExpression() {
			var left = ParseAdditiveExpression();
			while (true) {
				var children = new List<INode> { left };
				if (SkipIf("<", children)) {
					var right = AddChild(ParseAdditiveExpression(), children);
					left = CheckNode(new Binary(children, left, BinaryOperator.Less, right));
				}
				else {
					break;
				}
			}
			return left;
		}
		IExpression ParseAdditiveExpression() {
			var left = ParseMultiplicativeExpression();
			while (true) {
				var children = new List<INode> { left };
				if (SkipIf("+", children)) {
					var right = AddChild(ParseMultiplicativeExpression(), children);
					left = CheckNode(new Binary(children, left, BinaryOperator.Addition, right));
				}
				else if (SkipIf("-", children)) {
					var right = AddChild(ParseMultiplicativeExpression(), children);
					left = CheckNode(new Binary(children, left, BinaryOperator.Subtraction, right));
				}
				else {
					break;
				}
			}
			return left;
		}
		IExpression ParseMultiplicativeExpression() {
			var left = ParsePrimary();
			while (true) {
				var children = new List<INode> { left };
				if (SkipIf("*", children)) {
					var right = AddChild(ParsePrimary(), children);
					left = CheckNode(new Binary(children, left, BinaryOperator.Multiplication, right));
				}
				else if (SkipIf("/", children)) {
					var right = AddChild(ParsePrimary(), children);
					left = CheckNode(new Binary(children, left, BinaryOperator.Division, right));
				}
				else if (SkipIf("%", children)) {
					var right = AddChild(ParsePrimary(), children);
					left = CheckNode(new Binary(children, left, BinaryOperator.Remainder, right));
				}
				else {
					break;
				}
			}
			return left;
		}
		IExpression ParsePrimary() {
			var expression = ParsePrimitive();
			while (true) {
				var children = new List<INode> { expression };
				if (SkipIf("(", children)) {
					var arguments = new List<IExpression>();
					if (!SkipIf(")", children)) {
						arguments.Add(AddChild(ParseExpression(), children));
						while (SkipIf(",", children)) {
							arguments.Add(AddChild(ParseExpression(), children));
						}
						Expect(")", children);
					}
					expression = CheckNode(new Call(children, expression, arguments));
				}
				else if (SkipIf(".", children)) {
					var member = ParseIdentifier(children);
					expression = CheckNode(new MemberAccess(children, expression, member));
				}
				else {
					break;
				}
			}
			return expression;
		}
		IExpression ParsePrimitive() {
			var children = new List<INode>();
			if (CurrentToken.Type == TokenType.NumberLiteral) {
				var token = AddChild(CurrentToken, children);
				ReadNextToken();
				return CheckNode(new Number(children, token.Lexeme));
			}
			if (CurrentToken.Type == TokenType.Identifier) {
				var token = AddChild(CurrentToken, children);
				ReadNextToken();
				return CheckNode(new Identifier(children, token.Lexeme));
			}
			if (SkipIf("(", children)) {
				var expression = AddChild(ParseExpression(), children);
				Expect(")", children);
				return CheckNode(new Parentheses(children, expression));
			}
			throw MakeError($"Ожидали идентификатор, число или открывающую скобку, получили {CurrentToken}");
		}
		#endregion
	}
}
