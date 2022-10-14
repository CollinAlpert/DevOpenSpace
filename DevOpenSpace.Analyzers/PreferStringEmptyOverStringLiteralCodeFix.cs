using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DevOpenSpace.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class PreferStringEmptyOverStringLiteralCodeFix : CodeFixProvider
{
	private const string Title = "Use 'string.Empty'";

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
		if(root?.FindNode(context.Span) is not LiteralExpressionSyntax literal)
		{
			return;
		}

		var stringEmpty = MemberAccessExpression(
			SyntaxKind.SimpleMemberAccessExpression,
			PredefinedType(
				Token(SyntaxKind.StringKeyword)
			),
			IdentifierName("Empty")
		);
		var newRoot = root.ReplaceNode(literal, stringEmpty);
		var codeAction = CodeAction.Create(Title, _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)), Title);
		context.RegisterCodeFix(codeAction, context.Diagnostics);
	}

	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.PreferStringEmptyOverStringLiteral.Id);
}