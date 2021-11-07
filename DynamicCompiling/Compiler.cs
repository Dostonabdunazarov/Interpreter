using DynamicRuntime;
using Lab3.Ast;
using Lab3.Ast.Expressions;
using Lab3.Ast.Statements;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
namespace Lab3.DynamicCompiling {
	sealed class Compiler : IStatementVisitor, IExpressionVisitor {
		static readonly VariableDefinition missingVariable = null;
		readonly Document document;
		readonly ProgramNode program;
		readonly ModuleDefinition module;
		readonly MethodDefinition method;
		readonly ILProcessor cil;
		ScopeDebugInformation currentScope;
		List<ScopeDebugInformation> currentBlockScopes = null;
		readonly Dictionary<string, VariableDefinition> variables = new Dictionary<string, VariableDefinition>();
		Dictionary<string, VariableDefinition> currentBlockShadowedVariables = null;
		public Compiler(Document document, ProgramNode program, MethodDefinition method) {
			this.document = document;
			this.program = program;
			module = method.Module;
			this.method = method;
			cil = method.Body.GetILProcessor();
		}
		public void CompileProgram() {
			var start = InsertHiddenSequencePoint();
			currentScope = new ScopeDebugInformation(start, null);
			method.DebugInformation.Scope = currentScope;
			foreach (var field in typeof(BuiltinVariables).GetFields()) {
				if (field.FieldType != typeof(object)) {
					continue;
				}
				var variable = new VariableDefinition(module.TypeSystem.Object);
				method.Body.Variables.Add(variable);
				variables[field.Name] = variable;
				cil.Emit(OpCodes.Ldsfld, module.ImportReference(field));
				cil.Emit(OpCodes.Stloc, variable);
				currentScope.Variables.Add(new VariableDebugInformation(variable, field.Name));
			}
			foreach (var statement in program.Statements) {
				CompileStatement(statement);
			}
			cil.Emit(OpCodes.Ret);
		}
		void EmitRuntimeCall(string methodName) {
			var method = typeof(Op).GetMethod(methodName);
			if (method == null) {
				throw new Exception($"{methodName} не найден");
			}
			var methodReference = module.ImportReference(method);
			cil.Emit(OpCodes.Call, methodReference);
		}
		void CompileBlock(Block block) {
			var oldScope = currentScope;
			var oldBlockScopes = currentBlockScopes;
			currentBlockScopes = new List<ScopeDebugInformation>();
			var oldShadowedVariables = currentBlockShadowedVariables;
			currentBlockShadowedVariables = new Dictionary<string, VariableDefinition>();
			foreach (var statement in block.Statements) {
				CompileStatement(statement);
			}
			foreach (var kv in currentBlockShadowedVariables) {
				var name = kv.Key;
				var shadowedVariable = kv.Value;
				if (shadowedVariable == missingVariable) {
					variables.Remove(name);
				}
				else {
					variables[name] = shadowedVariable;
				}
			}
			currentBlockShadowedVariables = oldShadowedVariables;
			InsertSequencePoint(block.RightBrace.BeginOffset, block.RightBrace.EndOffset);
			var blockEnd = Instruction.Create(OpCodes.Nop);
			cil.Append(blockEnd);
			foreach (var scope in currentBlockScopes) {
				scope.End = new InstructionOffset(blockEnd);
			}
			currentBlockScopes = oldBlockScopes;
			currentScope = oldScope;
		}
		Instruction InsertHiddenSequencePoint() {
			var nop = Instruction.Create(OpCodes.Nop);
			cil.Append(nop);
			method.DebugInformation.SequencePoints.Add(new SequencePoint(nop, document) {
				StartLine = 0xFEEFEE,
				StartColumn = 0,
				EndLine = 0xFEEFEE,
				EndColumn = 0,
			});
			return nop;
		}
		Instruction InsertSequencePoint(int beginOffset, int endOffset) {
			var nop = Instruction.Create(OpCodes.Nop);
			cil.Append(nop);
			var beginLocation = program.SourceFile.GetLocation(beginOffset);
			var endLocation = program.SourceFile.GetLocation(endOffset);
			method.DebugInformation.SequencePoints.Add(new SequencePoint(nop, document) {
				StartLine = beginLocation.LineIndex + 1,
				StartColumn = beginLocation.ColumnIndex + 1,
				EndLine = endLocation.LineIndex + 1,
				EndColumn = endLocation.ColumnIndex + 1,
			});
			return nop;
		}
		#region statements
		void CompileStatement(IStatement statement) {
			statement.Accept(this);
		}
		public void VisitIf(If ifStatement) {
			throw new NotImplementedException();
		}
		public void VisitWhile(While whileStatement) {
			throw new NotImplementedException();
		}
		public void VisitExpressionStatement(ExpressionStatement expressionStatement) {
			throw new NotImplementedException();
		}
		public void VisitVariableDeclaration(VariableDeclaration variableDeclaration) {
			CompileExpression(variableDeclaration.Expr);
			var name = variableDeclaration.VariableName;
			if (currentBlockShadowedVariables != null && !currentBlockShadowedVariables.ContainsKey(name)) {
				if (variables.TryGetValue(name, out var existingVariable)) {
					currentBlockShadowedVariables[name] = existingVariable;
				}
				else {
					currentBlockShadowedVariables[name] = missingVariable;
				}
			}
			var variable = new VariableDefinition(module.TypeSystem.Object);
			method.Body.Variables.Add(variable);
			variables[name] = variable;
			cil.Emit(OpCodes.Stloc, variable);
			var variableScopeStart = Instruction.Create(OpCodes.Nop);
			cil.Append(variableScopeStart);
			var variabeScope = new ScopeDebugInformation(variableScopeStart, null);
			variabeScope.Variables.Add(new VariableDebugInformation(variable, name));
			if (currentBlockScopes != null) {
				currentBlockScopes.Add(variabeScope);
			}
			currentScope.Scopes.Add(variabeScope);
			currentScope = variabeScope;
		}
		public void VisitVariableAssignment(VariableAssignment variableAssignment) {
			throw new NotImplementedException();
		}
		#endregion
		#region expressions
		void CompileExpression(IExpression expression) {
			InsertSequencePoint(expression.BeginOffset, expression.EndOffset);
			expression.Accept(this);
		}
		public void VisitBinary(Binary binary) {
			throw new NotImplementedException();
		}
		public void VisitCall(Call call) {
			throw new NotImplementedException();
		}
		public void VisitParentheses(Parentheses parentheses) {
			throw new NotImplementedException();
		}
		public void VisitNumber(Number number) {
			throw new NotImplementedException();
		}
		public void VisitIdentifier(Identifier identifier) {
			throw new NotImplementedException();
		}
		public void VisitMemberAccess(MemberAccess memberAccess) {
			throw new NotSupportedException();
		}
		#endregion
		Exception MakeError(int position, string message) {
			return new Exception(program.SourceFile.MakeErrorMessage(position, message));
		}
	}
}
