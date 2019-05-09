using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace B站专栏CSharp代码高亮 {
	public class BilibiliCSharpHighlight {
		public static class Styles {
			public const string Token = "token";
			public const string Bold = "bold";
			public const string Italic = "italic";
			public const string Opacity = "namespace";
			public const string Help = "entity";

			/// <summary>
			/// 注释颜色
			/// </summary>
			public const string Slategray = "comment";
			/// <summary>
			/// 操作符颜色
			/// </summary>
			public const string Xf8f8f2 = "operator";
			/// <summary>
			/// 常量颜色
			/// </summary>
			public const string Xf92672 = "constant";
			/// <summary>
			/// 数字颜色
			/// </summary>
			public const string Xae81ff = "number";
			/// <summary>
			/// 字符串颜色
			/// </summary>
			public const string Xa6e22e = "string";
			/// <summary>
			/// 函数颜色
			/// </summary>
			public const string Xe6db74 = "function";
			/// <summary>
			/// 关键字颜色
			/// </summary>
			public const string X66d9ef = "keyword";
			/// <summary>
			/// 正则表达式颜色
			/// </summary>
			public const string Xfd971f = "regex";


			public const string ColorDefault = "color-default";
			public const string ColorBlue1 = "color-blue-01";
			public const string ColorLightBlue1 = "color-lblue-01";
			public const string ColorGreen1 = "color-green-01";
			public const string ColorYellow1 = "color-yellow-01";
			public const string ColorPink1 = "color-pink-01";
			public const string ColorPurple1 = "color-purple-01";
			public const string ColorBlue2 = "color-blue-02";
			public const string ColorLightBlue2 = "color-lblue-02";
			public const string ColorGreen2 = "color-green-02";
			public const string ColorYellow2 = "color-yellow-02";
			public const string ColorPink2 = "color-pink-02";
			public const string ColorPurple2 = "color-purple-02";
			public const string ColorBlue3 = "color-blue-03";
			public const string ColorLightBlue3 = "color-lblue-03";
			public const string ColorGreen3 = "color-green-03";
			public const string ColorYellow3 = "color-yellow-03";
			public const string ColorPink3 = "color-pink-03";
			public const string ColorPurple3 = "color-purple-03";
			public const string ColorBlue4 = "color-blue-04";
			public const string ColorLightBlue4 = "color-lblue-04";
			public const string ColorGreen4 = "color-green-04";
			public const string ColorYellow4 = "color-yellow-04";
			public const string ColorPink4 = "color-pink-04";
			public const string ColorPurple4 = "color-purple-04";
			public const string ColorGray1 = "color-gray-01";
			public const string ColorGray2 = "color-gray-02";
			public const string ColorGray3 = "color-gray-03";
		}

		private enum StyleIndex : byte {
			/// <summary>
			/// 默认样式
			/// </summary>
			None,
			Comment,
			XmlComment,
			XmlAttr,
			String,
			StringBrace,
			Number,
			Keyword,
			TypeKeyword,
			ControlKeyword,
			Namespace,
			Class,
			Struct,
			Enum,
			EnumField,
			Interface,
			Method,
			TypeParam,
			Field,
			StaticField,
			ControlBrace,
			Local,
			Param,
			Pointer,
			Error,
		}

		static string MakeStyle(params string[] classes)
			=> string.Join(" ", classes);

		static string MakeTokenStyle(params string[] classes)
			=> string.Join(" ", Enumerable.Repeat(Styles.Token, 1).Concat(classes));

		static readonly IEnumerable<PortableExecutableReference> defaultReferences
			= AppDomain.CurrentDomain.GetAssemblies()
			.Where(x => !x.Location.StartsWith(Environment.CurrentDirectory, StringComparison.CurrentCultureIgnoreCase))
			.Select(x => MetadataReference.CreateFromFile(x.Location));
		static readonly string[] styleTable = new string[Enum.GetValues(typeof(StyleIndex)).Length];

		static BilibiliCSharpHighlight() {
			styleTable[(int)StyleIndex.None] = MakeTokenStyle();
			styleTable[(int)StyleIndex.Comment] = MakeTokenStyle(Styles.ColorGreen2);
			styleTable[(int)StyleIndex.XmlComment] = MakeTokenStyle(Styles.ColorGreen2);
			styleTable[(int)StyleIndex.XmlAttr] = MakeTokenStyle(Styles.ColorGray1);
			styleTable[(int)StyleIndex.String] = MakeTokenStyle(Styles.ColorYellow3, Styles.Opacity);
			styleTable[(int)StyleIndex.StringBrace] = MakeTokenStyle(Styles.ColorYellow4);
			styleTable[(int)StyleIndex.Number] = MakeTokenStyle(Styles.ColorYellow1);
			styleTable[(int)StyleIndex.Keyword] = MakeTokenStyle(Styles.ColorPurple1, Styles.Opacity);
			styleTable[(int)StyleIndex.TypeKeyword] = MakeTokenStyle(Styles.ColorBlue2, Styles.Opacity);
			styleTable[(int)StyleIndex.ControlKeyword] = MakeTokenStyle(Styles.ColorPink1);
			styleTable[(int)StyleIndex.Namespace] = MakeTokenStyle(Styles.ColorGray1, Styles.Opacity);
			styleTable[(int)StyleIndex.Class] = MakeTokenStyle(Styles.ColorLightBlue2, Styles.Opacity);
			styleTable[(int)StyleIndex.Struct] = MakeTokenStyle(Styles.ColorGreen1, Styles.Opacity);
			styleTable[(int)StyleIndex.Enum] = MakeTokenStyle(Styles.ColorYellow1, Styles.Opacity);
			styleTable[(int)StyleIndex.EnumField] = MakeTokenStyle(Styles.ColorYellow3);
			styleTable[(int)StyleIndex.Interface] = MakeTokenStyle(Styles.ColorYellow1, Styles.Opacity);
			styleTable[(int)StyleIndex.Method] = MakeTokenStyle(Styles.Xe6db74);
			styleTable[(int)StyleIndex.TypeParam] = MakeTokenStyle(Styles.ColorPink2);
			styleTable[(int)StyleIndex.Field] = MakeTokenStyle();
			styleTable[(int)StyleIndex.StaticField] = MakeTokenStyle();
			styleTable[(int)StyleIndex.ControlBrace] = MakeTokenStyle(Styles.ColorPink1);
			styleTable[(int)StyleIndex.Local] = MakeTokenStyle(Styles.ColorLightBlue1);
			styleTable[(int)StyleIndex.Param] = MakeTokenStyle(Styles.ColorLightBlue1);
			styleTable[(int)StyleIndex.Pointer] = MakeTokenStyle(Styles.ColorPink1);
			styleTable[(int)StyleIndex.Error] = MakeTokenStyle(Styles.ColorPink3);
		}

		private readonly string code;
		private readonly StyleIndex[] styles;
		private readonly SemanticModel model;
		private readonly bool showError;

		public BilibiliCSharpHighlight(string code, bool showError = false, IEnumerable<string> withReferences = null) {
			this.code = code ?? throw new ArgumentNullException(nameof(code));
			styles = new StyleIndex[code.Length];
			this.showError = showError;
			var references = defaultReferences;
			if (withReferences != null) {
				references = references.Concat(withReferences.Select(f => MetadataReference.CreateFromFile(f)));
			}

			var syntaxTree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(LanguageVersion.CSharp8, DocumentationMode.Diagnose, SourceCodeKind.Regular));
			var compilation = CSharpCompilation.Create("Bilibili.Highlight", Enumerable.Repeat(syntaxTree, 1),
				options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithAllowUnsafe(true)).WithReferences(references);
			model = compilation.GetSemanticModel(syntaxTree, false);
			var diagnostics = model.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
			if (diagnostics.Length > 0) {
				foreach (var d in diagnostics) {
					Console.WriteLine(d);
				}
				Console.WriteLine();
			}

			SetSyntaxStyle(syntaxTree.GetRoot());

			var trivias = syntaxTree.GetRoot().DescendantTrivia().ToArray();
			foreach (var t in trivias) {
				if (GetCommentStyle(t.Kind()) is StyleIndex style) {
					styles.AsSpan(t.FullSpan.Start, t.FullSpan.End - t.FullSpan.Start).Fill(style);
				}
				SetDocumentationComment(t);
			}
		}

		private void SetSyntaxStyle(SyntaxNode syntax) {
			foreach (var n in syntax.DescendantNodesAndTokensAndSelf()) {
				if (GetStyle(n) is var (style, span)) {
					styles.AsSpan(span.Start, span.End - span.Start).Fill(style);
				}
				if (showError) {
					ShowTokenError(n);
				}
			}
		}

		private (StyleIndex Style, TextSpan TextSpan)? GetStyle(SyntaxNodeOrToken syntaxToken) {
			switch (syntaxToken.Kind()) {
				case SyntaxKind.NumericLiteralToken:
					return (StyleIndex.Number, syntaxToken.Span);
				case SyntaxKind.StringLiteralToken:
				case SyntaxKind.CharacterLiteralToken:
				case SyntaxKind.InterpolatedStringStartToken:
				case SyntaxKind.InterpolatedStringEndToken:
				case SyntaxKind.InterpolatedStringText:
				case SyntaxKind.InterpolatedStringTextToken:
				case SyntaxKind.InterpolatedStringToken:
				case SyntaxKind.InterpolatedVerbatimStringStartToken:
					return (StyleIndex.String, syntaxToken.Span);
				case SyntaxKind.BoolKeyword:
				case SyntaxKind.SByteKeyword:
				case SyntaxKind.ShortKeyword:
				case SyntaxKind.UShortKeyword:
				case SyntaxKind.IntKeyword:
				case SyntaxKind.UIntKeyword:
				case SyntaxKind.LongKeyword:
				case SyntaxKind.ULongKeyword:
				case SyntaxKind.DoubleKeyword:
				case SyntaxKind.FloatKeyword:
				case SyntaxKind.DecimalKeyword:
				case SyntaxKind.StringKeyword:
				case SyntaxKind.CharKeyword:
				case SyntaxKind.VoidKeyword:
				case SyntaxKind.ObjectKeyword:
				case SyntaxKind.ByteKeyword:
					return (StyleIndex.TypeKeyword, syntaxToken.Span);
				case SyntaxKind.IfKeyword:
				case SyntaxKind.ElseKeyword:
				case SyntaxKind.WhileKeyword:
				case SyntaxKind.ForKeyword:
				case SyntaxKind.ForEachKeyword:
				case SyntaxKind.DoKeyword:
				case SyntaxKind.SwitchKeyword:
				case SyntaxKind.CaseKeyword:
				case SyntaxKind.DefaultKeyword:
				case SyntaxKind.TryKeyword:
				case SyntaxKind.CatchKeyword:
				case SyntaxKind.FinallyKeyword:
				case SyntaxKind.GotoKeyword:
				case SyntaxKind.BreakKeyword:
				case SyntaxKind.ContinueKeyword:
				case SyntaxKind.ReturnKeyword:
				case SyntaxKind.ThrowKeyword:
				case SyntaxKind.YieldKeyword:
				case SyntaxKind.WhenKeyword:
					return (StyleIndex.ControlKeyword, syntaxToken.Span);
				case SyntaxKind.InKeyword:
					return (syntaxToken.Parent is ForEachStatementSyntax ? StyleIndex.ControlKeyword : StyleIndex.Keyword, syntaxToken.Span);
				case SyntaxKind.TypeKeyword:
				case SyntaxKind.TypeOfKeyword:
				case SyntaxKind.SizeOfKeyword:
				case SyntaxKind.NullKeyword:
				case SyntaxKind.TrueKeyword:
				case SyntaxKind.FalseKeyword:
				case SyntaxKind.LockKeyword:
				case SyntaxKind.PublicKeyword:
				case SyntaxKind.PrivateKeyword:
				case SyntaxKind.InternalKeyword:
				case SyntaxKind.ProtectedKeyword:
				case SyntaxKind.StaticKeyword:
				case SyntaxKind.ReadOnlyKeyword:
				case SyntaxKind.SealedKeyword:
				case SyntaxKind.ConstKeyword:
				case SyntaxKind.FixedKeyword:
				case SyntaxKind.StackAllocKeyword:
				case SyntaxKind.VolatileKeyword:
				case SyntaxKind.NewKeyword:
				case SyntaxKind.OverrideKeyword:
				case SyntaxKind.AbstractKeyword:
				case SyntaxKind.VirtualKeyword:
				case SyntaxKind.EventKeyword:
				case SyntaxKind.ExternKeyword:
				case SyntaxKind.RefKeyword:
				case SyntaxKind.OutKeyword:
				case SyntaxKind.IsKeyword:
				case SyntaxKind.AsKeyword:
				case SyntaxKind.ParamsKeyword:
				case SyntaxKind.ArgListKeyword:
				case SyntaxKind.MakeRefKeyword:
				case SyntaxKind.RefTypeKeyword:
				case SyntaxKind.RefValueKeyword:
				case SyntaxKind.ThisKeyword:
				case SyntaxKind.BaseKeyword:
				case SyntaxKind.NamespaceKeyword:
				case SyntaxKind.UsingKeyword:
				case SyntaxKind.ClassKeyword:
				case SyntaxKind.StructKeyword:
				case SyntaxKind.InterfaceKeyword:
				case SyntaxKind.EnumKeyword:
				case SyntaxKind.DelegateKeyword:
				case SyntaxKind.CheckedKeyword:
				case SyntaxKind.UncheckedKeyword:
				case SyntaxKind.UnsafeKeyword:
				case SyntaxKind.OperatorKeyword:
				case SyntaxKind.ExplicitKeyword:
				case SyntaxKind.ImplicitKeyword:
				case SyntaxKind.PartialKeyword:
				case SyntaxKind.AliasKeyword:
				case SyntaxKind.GlobalKeyword:
				case SyntaxKind.AssemblyKeyword:
				case SyntaxKind.ModuleKeyword:
				case SyntaxKind.FieldKeyword:
				case SyntaxKind.MethodKeyword:
				case SyntaxKind.ParamKeyword:
				case SyntaxKind.PropertyKeyword:
				case SyntaxKind.TypeVarKeyword:
				case SyntaxKind.GetKeyword:
				case SyntaxKind.SetKeyword:
				case SyntaxKind.AddKeyword:
				case SyntaxKind.RemoveKeyword:
				case SyntaxKind.WhereKeyword:
				case SyntaxKind.FromKeyword:
				case SyntaxKind.GroupKeyword:
				case SyntaxKind.JoinKeyword:
				case SyntaxKind.IntoKeyword:
				case SyntaxKind.LetKeyword:
				case SyntaxKind.ByKeyword:
				case SyntaxKind.SelectKeyword:
				case SyntaxKind.OrderByKeyword:
				case SyntaxKind.OnKeyword:
				case SyntaxKind.EqualsKeyword:
				case SyntaxKind.AscendingKeyword:
				case SyntaxKind.DescendingKeyword:
				case SyntaxKind.NameOfKeyword:
				case SyntaxKind.AsyncKeyword:
				case SyntaxKind.AwaitKeyword:
				case SyntaxKind.ElifKeyword:
				case SyntaxKind.EndIfKeyword:
				case SyntaxKind.RegionKeyword:
				case SyntaxKind.EndRegionKeyword:
				case SyntaxKind.DefineKeyword:
				case SyntaxKind.UndefKeyword:
				case SyntaxKind.WarningKeyword:
				case SyntaxKind.ErrorKeyword:
				case SyntaxKind.LineKeyword:
				case SyntaxKind.PragmaKeyword:
				case SyntaxKind.HiddenKeyword:
				case SyntaxKind.ChecksumKeyword:
				case SyntaxKind.DisableKeyword:
				case SyntaxKind.RestoreKeyword:
				case SyntaxKind.ReferenceKeyword:
				case SyntaxKind.LoadKeyword:
				case SyntaxKind.NullableKeyword:
				case SyntaxKind.EnableKeyword:
				case SyntaxKind.SafeOnlyKeyword:
				case SyntaxKind.VarKeyword:
					return (StyleIndex.Keyword, syntaxToken.Span);
				case SyntaxKind.NamespaceDeclaration: {
						var namespaceDeclaration = syntaxToken.AsNode() as NamespaceDeclarationSyntax;
						return (StyleIndex.Namespace, namespaceDeclaration.Name.Span);
					}
				case SyntaxKind.VariableDeclarator: {
						var variableDeclarator = syntaxToken.AsNode() as VariableDeclaratorSyntax;
						switch (variableDeclarator.Parent) {
							case VariableDeclarationSyntax parent:
								switch (parent.Parent) {
									case FieldDeclarationSyntax fieldDeclaration:
										if (fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)) {
											return (StyleIndex.StaticField, variableDeclarator.Identifier.Span);
										} else {
											return (StyleIndex.Field, variableDeclarator.Identifier.Span);
										}
									case LocalDeclarationStatementSyntax _:
									case UsingStatementSyntax _:
										return (StyleIndex.Local, variableDeclarator.Identifier.Span);
								}
								break;
						}
						return null;
					}
				case SyntaxKind.IdentifierName: {
						switch (syntaxToken.Parent) {
							case AttributeSyntax { Name: var name }:
								return GetSymbolStyle(name, name.Span);
							case NameEqualsSyntax { Parent: UsingDirectiveSyntax @using }:
								return GetSymbolStyle(@using.Name, @using.Alias.Name.Span);
							default:
								var identifierName = syntaxToken.AsNode() as IdentifierNameSyntax;
								return GetSymbolStyle(identifierName, identifierName.Identifier.Span);
						}
					}
				case SyntaxKind.EnumMemberDeclaration: {
						var enumMember = syntaxToken.AsNode() as EnumMemberDeclarationSyntax;
						return (StyleIndex.EnumField, enumMember.Identifier.Span);
					}
				case SyntaxKind.Parameter: {
						var param = syntaxToken.AsNode() as ParameterSyntax;
						return (StyleIndex.Param, param.Identifier.Span);
					}
				case SyntaxKind.GenericName: {
						var genericName = syntaxToken.AsNode() as GenericNameSyntax;
						return GetSymbolStyle(genericName, genericName.Identifier.Span);
					}
				case SyntaxKind.EnumDeclaration: {
						var enumDeclaration = syntaxToken.AsNode() as EnumDeclarationSyntax;
						return (StyleIndex.Enum, enumDeclaration.Identifier.Span);
					}
				case SyntaxKind.StructDeclaration: {
						var structDeclaration = syntaxToken.AsNode() as StructDeclarationSyntax;
						return (StyleIndex.Struct, structDeclaration.Identifier.Span);
					}
				case SyntaxKind.ClassDeclaration: {
						var classDeclaration = syntaxToken.AsNode() as ClassDeclarationSyntax;
						return (StyleIndex.Class, classDeclaration.Identifier.Span);
					}
				case SyntaxKind.MethodDeclaration: {
						var methodDeclaration = syntaxToken.AsNode() as MethodDeclarationSyntax;
						return (StyleIndex.Method, methodDeclaration.Identifier.Span);
					}
				case SyntaxKind.TypeParameter:
					return (StyleIndex.TypeParam, syntaxToken.Span);
				case SyntaxKind.OpenParenToken:
				case SyntaxKind.CloseParenToken:
				case SyntaxKind.OpenBraceToken:
				case SyntaxKind.CloseBraceToken:
					switch (syntaxToken.Parent) {
						case WhileStatementSyntax _:
						case DoStatementSyntax _:
						case ForStatementSyntax _:
						case ForEachStatementSyntax _:
						case IfStatementSyntax _:
							return (StyleIndex.ControlBrace, syntaxToken.Span);
						case BlockSyntax block:
							switch (block.Parent) {
								case WhileStatementSyntax _:
								case DoStatementSyntax _:
								case ForStatementSyntax _:
								case ForEachStatementSyntax _:
								case IfStatementSyntax _:
								case ElseClauseSyntax _:
									return (StyleIndex.ControlBrace, syntaxToken.Span);
							}
							break;
						case InterpolatedStringContentSyntax _:
							return (StyleIndex.StringBrace, syntaxToken.Span);
						case TypeOfExpressionSyntax _:
						case SizeOfExpressionSyntax _:
							return (StyleIndex.Keyword, syntaxToken.Span);
						case ArgumentListSyntax args:
							switch (args.Parent) {
								case InvocationExpressionSyntax { Expression: IdentifierNameSyntax methodName }:
									if (methodName.Identifier.Text == "nameof") return (StyleIndex.Keyword, syntaxToken.Span);
									break;
								case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccessExpr }: {
										if (GetSymbolStyle(memberAccessExpr, syntaxToken.Span) is var (style, span)) return (style, span);
									}
									break;
							}
							break;
						default:
							if (syntaxToken.Parent is IdentifierNameSyntax { Identifier: { Text: "nameof" } })
								return (StyleIndex.Keyword, syntaxToken.Span);
							break;
					}
					return null;
				case SyntaxKind.ForEachStatement: {
						var forEachStatement = syntaxToken.AsNode() as ForEachStatementSyntax;
						return (StyleIndex.Local, forEachStatement.Identifier.Span);
					}
				case SyntaxKind.IdentifierToken: {
						var token = syntaxToken.AsToken();
						switch (token.Text) {
							case "nameof": return (StyleIndex.Keyword, token.Span);
						}
						return null;
					}
				case SyntaxKind.PointerIndirectionExpression: {
						var pointerIndirection = syntaxToken.AsNode() as PrefixUnaryExpressionSyntax;
						return (StyleIndex.Pointer, pointerIndirection.OperatorToken.Span);
					}
				case SyntaxKind.PointerType: {
						var pointerType = syntaxToken.AsNode() as PointerTypeSyntax;
						return (StyleIndex.Pointer, pointerType.AsteriskToken.Span);
					}
				default:
					return null;
			}
		}

		private void ShowTokenError(SyntaxNodeOrToken syntaxToken) {
			if (syntaxToken.IsToken) {
				var diagnostics = model.GetDiagnostics(syntaxToken.Span);
				if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error && d.Location.SourceSpan.OverlapsWith(syntaxToken.Span))) {
					styles.AsSpan(syntaxToken.Span.Start, syntaxToken.Span.End - syntaxToken.Span.Start).Fill(StyleIndex.Error);
				}
			}
		}

		private (StyleIndex Style, TextSpan TextSpan)? GetSymbolStyle(SyntaxNode syntaxNode, TextSpan syntaxSpan) {
			if (syntaxNode is TypeSyntax { IsVar: true }) return (StyleIndex.Keyword, syntaxSpan);

			var symbol = model.GetSymbolInfo(syntaxNode).Symbol;
			if (symbol == null) return null;

			Retry:
			switch (symbol.Kind) {
				case SymbolKind.Local:
					return (StyleIndex.Local, syntaxSpan);
				case SymbolKind.Parameter:
					return (StyleIndex.Param, syntaxSpan);
				case SymbolKind.Namespace:
					return (StyleIndex.Namespace, syntaxSpan);
				case SymbolKind.Field:
					if (symbol is IFieldSymbol { Type: { TypeKind: TypeKind.Enum } }) {
						return (StyleIndex.EnumField, syntaxSpan);
					}
					return (symbol.IsStatic ? StyleIndex.StaticField : StyleIndex.Field, syntaxSpan);
				case SymbolKind.Method:
					if (symbol is IMethodSymbol methodSymbol) {
						if (methodSymbol is { MethodKind: MethodKind.Constructor, ReceiverType: var receiverType }) {
							symbol = receiverType;
							goto Retry;
						}
						return (StyleIndex.Method, syntaxSpan);
					}
					break;
				case SymbolKind.TypeParameter:
					return (StyleIndex.TypeParam, syntaxSpan);
				case SymbolKind.NamedType:
					if (symbol is INamedTypeSymbol namedType) {
						switch (namedType.TypeKind) {
							case TypeKind.Class: return (StyleIndex.Class, syntaxSpan);
							case TypeKind.Struct: return (StyleIndex.Struct, syntaxSpan);
							case TypeKind.Interface: return (StyleIndex.Interface, syntaxSpan);
							case TypeKind.Enum: return (StyleIndex.Enum, syntaxSpan);
							case TypeKind.TypeParameter: return (StyleIndex.TypeParam, syntaxSpan);
							case TypeKind.Unknown: return showError ? (StyleIndex.Error, syntaxSpan) : null as (StyleIndex, TextSpan)?;
						}
					}
					break;
				case SymbolKind.ErrorType:
					return showError ? (StyleIndex.Error, syntaxSpan) : null as (StyleIndex, TextSpan)?;
			}
			return null;
		}

		private static StyleIndex? GetCommentStyle(SyntaxKind kind) {
			switch (kind) {
				case SyntaxKind.SingleLineCommentTrivia:
				case SyntaxKind.MultiLineCommentTrivia:
					return StyleIndex.Comment;
				case SyntaxKind.SingleLineDocumentationCommentTrivia:
				case SyntaxKind.MultiLineDocumentationCommentTrivia:
				case SyntaxKind.EndOfDocumentationCommentToken:
				case SyntaxKind.DocumentationCommentExteriorTrivia:
				case SyntaxKind.XmlCommentStartToken:
				case SyntaxKind.XmlCommentEndToken:
				case SyntaxKind.XmlCDataStartToken:
				case SyntaxKind.XmlCDataEndToken:
				case SyntaxKind.XmlProcessingInstructionStartToken:
				case SyntaxKind.XmlProcessingInstructionEndToken:
				case SyntaxKind.XmlEntityLiteralToken:
				case SyntaxKind.XmlTextLiteralToken:
				case SyntaxKind.XmlTextLiteralNewLineToken:
				case SyntaxKind.XmlElement:
				case SyntaxKind.XmlElementStartTag:
				case SyntaxKind.XmlElementEndTag:
				case SyntaxKind.XmlEmptyElement:
				case SyntaxKind.XmlName:
				case SyntaxKind.XmlText:
				case SyntaxKind.XmlCDataSection:
				case SyntaxKind.XmlComment:
				case SyntaxKind.XmlProcessingInstruction:
				case SyntaxKind.XmlPrefix:
				case SyntaxKind.XmlTextAttribute:
				case SyntaxKind.XmlCrefAttribute:
				case SyntaxKind.XmlNameAttribute:
					return StyleIndex.XmlComment;
				default:
					return null;
			}
		}

		private void SetDocumentationComment(SyntaxTrivia syntaxTrivia) {
			void Visit(XmlNodeSyntax xmlNode) {
				switch (xmlNode) {
					case XmlElementSyntax e:
						foreach (var attr in e.StartTag.Attributes) {
							styles.AsSpan(attr.Name.Span.Start, attr.Name.Span.End - attr.Name.Span.Start).Fill(StyleIndex.XmlAttr);
							if (e.StartTag.Name.LocalName.Text == "param" && attr is XmlNameAttributeSyntax { Name: { LocalName: { Text: "name" } } } nameAttr) {
								SetSyntaxStyle(nameAttr.Identifier);
							}
						}
						foreach (var child in e.Content) Visit(child);
						break;
					case XmlEmptyElementSyntax e:
						foreach (var attr in e.Attributes) {
							styles.AsSpan(attr.Name.Span.Start, attr.Name.Span.End - attr.Name.Span.Start).Fill(StyleIndex.XmlAttr);
							switch (attr) {
								case XmlCrefAttributeSyntax a:
									SetSyntaxStyle(a.Cref);
									break;
							}
						}
						break;
				}
			}

			switch (syntaxTrivia.Kind()) {
				case SyntaxKind.SingleLineDocumentationCommentTrivia:
				case SyntaxKind.MultiLineDocumentationCommentTrivia:
					if (syntaxTrivia.HasStructure && syntaxTrivia.GetStructure() is DocumentationCommentTriviaSyntax doc) {
						foreach (var node in doc.Content) {
							Visit(node);
						}
					}
					break;
			}
		}

		public string ToHtml() {
			var spanBuilder = new StringBuilder();
			var htmlBuilder = new StringBuilder();
			int prevIndex = 0;
			StyleIndex? prevStyle = null;

			void AppendSpan(int i) {
				if (prevStyle is StyleIndex style && prevIndex < i) {
					htmlBuilder.Append("<span class=\"")
						.Append(styleTable[(int)style])
						.Append("\">")
						.Append(spanBuilder)
						.Append("</span>");
					prevIndex = i;
					spanBuilder.Clear();
				}
			}

			for (int i = 0; i < code.Length;) {
				if (code[i] == ' ') {
					spanBuilder.Append(' ');
					i++;
				} else if (code[i] == '\n') {
					AppendSpan(i);
					htmlBuilder.Append("<br>");
					prevStyle = null;
					i++;
				} else if (code[i] == '\r' && i + 1 < code.Length && code[i + 1] == '\n') {
					AppendSpan(i);
					htmlBuilder.Append("<br>");
					prevStyle = null;
					i += 2;
				} else if (code[i] == '\t') {
					spanBuilder.Append("  ");
					i++;
				} else {
					if (!(prevStyle is StyleIndex style)) {
						prevStyle = styles[i];
					} else if (style != styles[i]) {
						AppendSpan(i);
						prevStyle = styles[i];
					}
					switch (code[i]) {
						case '<': spanBuilder.Append("&lt;"); break;
						case '>': spanBuilder.Append("&gt;"); break;
						case '&': spanBuilder.Append("&amp;"); break;
						default: spanBuilder.Append(code[i]); break;
					}
					i++;
				}
			}
			AppendSpan(code.Length);
			return htmlBuilder.ToString();
		}
	}
}
