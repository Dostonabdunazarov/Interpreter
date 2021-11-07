namespace Lab3.Ast {
	interface INode {
		int BeginOffset { get; }
		int EndOffset { get; }
		string FormattedString { get; }
		void Check(SourceFile sourceFile);
	}
}
