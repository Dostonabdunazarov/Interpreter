using Lab3.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Lab3.Ast {
	abstract class BaseNode : INode {
		protected IReadOnlyList<INode> Children { get; }
		public int BeginOffset { get; }
		public int EndOffset { get; }
		protected BaseNode(IReadOnlyList<INode> children) {
			Children = children;
			BeginOffset = children[0].BeginOffset;
			EndOffset = children[children.Count - 1].EndOffset;
		}
		public abstract string FormattedString { get; }
		public void Check(SourceFile sourceFile) {
			Check(this, sourceFile);
		}
		static void Check(BaseNode node, SourceFile sourceFile) {
			var children = new HashSet<INode>();
			foreach (var child in node.Children) {
				if (!children.Add(child)) {
					throw new Exception($"Повторяющийся {child} в {nameof(Children)}");
				}
			}
			INode prevChild = null;
			foreach (var child in node.Children) {
				if (!(child.BeginOffset < child.EndOffset)) {
					throw new Exception($"У {child} неправильные смещения");
				}
				if (prevChild != null) {
					if (!(prevChild.EndOffset <= child.BeginOffset)) {
						throw new Exception($"Смещения {prevChild} и {child} пересекаются");
					}
				}
				prevChild = child;
			}
			var tokensFromChildren = node.GetAllTokensFromChildren().ToList();
			{
				var tokensFromSourceFile = Lexer
					.GetTokens(SourceFile.FromString(sourceFile.Text.Substring(node.BeginOffset, node.EndOffset - node.BeginOffset)))
					.Where(Lexer.IsNotWhitespace)
					.Select(token => new Token(token.BeginOffset + node.BeginOffset, token.Type, token.Lexeme))
					.ToList();
				var comparableTokensFromChildren = tokensFromChildren.Select(token => new { token.BeginOffset, token.Type, token.Lexeme });
				var comparableTokensFromSourceFile = tokensFromSourceFile.Select(token => new { token.BeginOffset, token.Type, token.Lexeme });
				if (!comparableTokensFromChildren.SequenceEqual(comparableTokensFromSourceFile)) {
					throw new Exception(string.Join("\n", new[] {
						$"Токены из {nameof(Children)} не соответствуют токенам из лексера, скорее всего в {nameof(Children)} пропущен или добавлен лишний элемент",
						$"\t{string.Join(", ", tokensFromChildren.Select(x => $"({x})"))}",
						$"\t{string.Join(", ", tokensFromSourceFile.Select(x => $"({x})"))}",
					}));
				}
			}
			{
				var lexemesFromChildren = tokensFromChildren.Select(token => token.Lexeme);
				var lexemesFromFormattedString = Lexer
					.GetTokens(SourceFile.FromString(node.FormattedString))
					.Where(Lexer.IsNotWhitespace)
					.Select(token => token.Lexeme)
					.ToList();
				if (!lexemesFromChildren.SequenceEqual(lexemesFromFormattedString)) {
					throw new Exception(string.Join("\n", new[] {
						$"Лексемы из {nameof(Children)} не соответствуют лексемам из {nameof(FormattedString)}, скорее всего {nameof(FormattedString)} кривой",
						$"\t{string.Join(", ", lexemesFromChildren.Select(x=> $"\"{x}\""))}",
						$"\t{string.Join(", ", lexemesFromFormattedString.Select(x=> $"\"{x}\""))}",
					}));
				}
			}
			{
				var childrenFromFieldsAndProperties = node.GetChildrenFromFieldsAndProperties();
				foreach (var child in childrenFromFieldsAndProperties) {
					if (!children.Contains(child.Value)) {
						throw new Exception(string.Join("\n", new[] {
							$"Узел из поля {child.Key} отсутствует в {nameof(Children)}",
						}));
					}
				}
			}
		}
		IEnumerable<Token> GetAllTokensFromChildren() {
			foreach (var child in Children) {
				if (child is Token token) {
					yield return token;
				}
				else if (child is BaseNode baseNode) {
					foreach (var grandChild in baseNode.GetAllTokensFromChildren()) {
						yield return grandChild;
					}
				}
				else {
					throw new NotSupportedException($"{child}");
				}
			}
		}
		IEnumerable<KeyValuePair<string, INode>> GetChildrenFromFieldsAndProperties() {
			IEnumerable<INode> empty = Array.Empty<INode>();
			IEnumerable<INode> normalize(object value) {
				if (value is INode node) {
					return new[] { node };
				}
				if (value is IEnumerable<INode> values) {
					return values;
				};
				return Array.Empty<INode>();
			}
			return GetType().GetMembers()
				.Select(member => {
					if (member.Name != nameof(Children)) {
						if (member is FieldInfo field) {
							if (!field.IsStatic) {
								return new { field.Name, Values = normalize(field.GetValue(this)) };
							}
						}
						else if (member is PropertyInfo property) {
							if (!property.GetGetMethod(true).IsStatic) {
								return new { property.Name, Values = normalize(property.GetValue(this)) };
							}
						}
					}
					return new { Name = "", Values = empty };
				})
				.SelectMany(member => member.Values.Select(value => new KeyValuePair<string, INode>(member.Name, value)))
				.OrderBy(member => member.Value.BeginOffset);
		}
	}
}
