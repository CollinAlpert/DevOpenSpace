using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DevOpenSpace.Generators;

[Generator]
public class OverrideGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
#if DEBUG
		SpinWait.SpinUntil(() => Debugger.IsAttached);
#endif
		var provider = context.SyntaxProvider.CreateSyntaxProvider(IsCandidate, GenerateCode);

		context.AddSource(provider);
	}

	private static GeneratorResult GenerateCode(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		var method = (MethodDeclarationSyntax)context.Node;
		if (method.Parent is not ClassDeclarationSyntax classDeclaration)
		{
			return GeneratorResult.Empty;
		}

		var arguments = method.ParameterList.Parameters.Select(p => Argument(IdentifierName(p.Identifier)));

		var sourceText = CompilationUnit()
			.WithMembers(
				SingletonList<MemberDeclarationSyntax>(
					FileScopedNamespaceDeclaration(
						IdentifierName("Test")
					).WithMembers(
						SingletonList<MemberDeclarationSyntax>(
							ClassDeclaration(
								classDeclaration.Identifier.Text + "Implemented"
							).WithModifiers(
								TokenList(
									Token(SyntaxKind.PublicKeyword),
									Token(SyntaxKind.PartialKeyword)
								)
							).WithBaseList(
								BaseList(
									SingletonSeparatedList<BaseTypeSyntax>(
										SimpleBaseType(
											IdentifierName(classDeclaration.Identifier)
										)
									)
								)
							).WithMembers(
								SingletonList<MemberDeclarationSyntax>(
									MethodDeclaration(method.ReturnType, method.Identifier)
										.WithModifiers(
											TokenList(
												Token(SyntaxKind.PublicKeyword),
												Token(SyntaxKind.OverrideKeyword)
											)
										).WithParameterList(method.ParameterList)
										.WithBody(
											Block(
												LocalDeclarationStatement(
													VariableDeclaration(
														IdentifierName(
															Identifier(
																TriviaList(),
																SyntaxKind.VarKeyword,
																"var",
																"var",
																TriviaList()
															)
														)
													).WithVariables(
														SingletonSeparatedList(
															VariableDeclarator(
																Identifier("stopwatch")
															).WithInitializer(
																EqualsValueClause(
																	InvocationExpression(
																		MemberAccessExpression(
																			SyntaxKind.SimpleMemberAccessExpression,
																			IdentifierName("global::System.Diagnostics.Stopwatch"),
																			IdentifierName("StartNew")
																		)
																	)
																)
															)
														)
													)
												),
												ExpressionStatement(
													InvocationExpression(
														MemberAccessExpression(
															SyntaxKind.SimpleMemberAccessExpression,
															BaseExpression(),
															IdentifierName(method.Identifier.Text)
														)
													).WithArgumentList(
														ArgumentList(
															SeparatedList(arguments)
														)
													)
												),
												ExpressionStatement(
													InvocationExpression(
														MemberAccessExpression(
															SyntaxKind.SimpleMemberAccessExpression,
															IdentifierName("stopwatch"),
															IdentifierName("Stop")
														)
													)
												),
												ExpressionStatement(
													InvocationExpression(
														MemberAccessExpression(
															SyntaxKind.SimpleMemberAccessExpression,
															IdentifierName("global::System.Console"),
															IdentifierName("WriteLine")
														)
													).WithArgumentList(
														ArgumentList(
															SingletonSeparatedList(
																Argument(
																	MemberAccessExpression(
																		SyntaxKind.SimpleMemberAccessExpression,
																		IdentifierName("stopwatch"),
																		IdentifierName("ElapsedMilliseconds")
																	)
																)
															)
														)
													)
												)
											)
										)
								)
							)
						)
					)
				)
			).NormalizeWhitespace()
			.GetText(Encoding.UTF8);

		return new GeneratorResult($"{classDeclaration.Identifier.Text}_{method.Identifier.Text}", sourceText);
	}

	private static bool IsCandidate(SyntaxNode node, CancellationToken cancellationToken)
	{
		return node is MethodDeclarationSyntax method && method.Modifiers.Any(SyntaxKind.VirtualKeyword);
	}
}