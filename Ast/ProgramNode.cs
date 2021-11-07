using System.Collections.Generic;
using System.Linq;
namespace Lab3.Ast {
	sealed class ProgramNode : BaseNode, INode {
		public readonly SourceFile SourceFile;
		public readonly IReadOnlyList<IStatement> Statements;
		public ProgramNode(SourceFile sourceFile, IReadOnlyList<IStatement> statements) : base(statements) {
			SourceFile = sourceFile;
			Statements = statements;
		}
		public override string FormattedString => string.Join("", Statements.Select(x => x.FormattedString));
	}
}
