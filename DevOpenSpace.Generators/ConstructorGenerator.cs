using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DevOpenSpace.Generators;

[Generator]
public class ConstructorGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var provider = context.SyntaxProvider.CreateSyntaxProvider(IsCandidate, GetClassDeclaration)
			.Where(static classDeclaration => classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
			.Where(static classDeclaration => classDeclaration.Members.Any(SyntaxKind.ConstructorDeclaration))
			.Select(GetFieldDeclarations)
			.Where(static item => item.PrivateReadonlyFields.Any())
			.Select(GenerateCode);

		context.AddSource(provider);
	}
	
	private static (SyntaxToken ClassIdentifier, List<FieldDeclarationSyntax> PrivateReadonlyFields) GetFieldDeclarations(ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
	{
		return (classDeclaration.Identifier,
			classDeclaration.Members
			.OfType<FieldDeclarationSyntax>()
			.Where(field => field.Modifiers.Any(SyntaxKind.PrivateKeyword) && field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
			.ToList());
	}

	private static ClassDeclarationSyntax GetClassDeclaration(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		return (ClassDeclarationSyntax)context.Node;
	}

	private static GeneratorResult GenerateCode((SyntaxToken ClassIdentifier, List<FieldDeclarationSyntax> Fields) item, CancellationToken cancellationToken)
	{
		IEnumerable<ParameterSyntax> parameters = item.Fields
			.SelectMany(field => field.Declaration.Variables.Select(v => (field.Declaration.Type, v.Identifier)))
			.Select(tuple => Parameter(
					Identifier(tuple.Identifier.Text.TrimStart('_'))
				).WithType(tuple.Type)
			);

		ParameterListSyntax parameterList = ParameterList(
			SeparatedList(parameters)
		);

		IEnumerable<ExpressionStatementSyntax> expressions = item.Fields
			.SelectMany(field => field.Declaration.Variables.Select(v => (field.Declaration.Type, v.Identifier)))
			.Select(tuple => ExpressionStatement(
					AssignmentExpression(
						SyntaxKind.SimpleAssignmentExpression,
						IdentifierName(tuple.Identifier.Text),
						IdentifierName(tuple.Identifier.Text.TrimStart('_'))
					)
				)
			);

		ConstructorDeclarationSyntax constructor = ConstructorDeclaration(
				item.ClassIdentifier
			).WithModifiers(
				TokenList(
					Token(SyntaxKind.PublicKeyword)
				)
			).WithParameterList(parameterList)
			.WithBody(
				Block(
					List(expressions)
				)
			);

		var sourceText = CompilationUnit()
			.WithMembers(
				SingletonList<MemberDeclarationSyntax>(
					FileScopedNamespaceDeclaration(
						IdentifierName("Test")
					).WithMembers(
						SingletonList<MemberDeclarationSyntax>(
							ClassDeclaration(item.ClassIdentifier)
								.WithModifiers(
									TokenList(
										Token(SyntaxKind.PublicKeyword),
										Token(SyntaxKind.PartialKeyword)
									)
								).WithMembers(
									SingletonList<MemberDeclarationSyntax>(
										constructor
									)
								)
						)
					)
				)
			).NormalizeWhitespace()
			.GetText(Encoding.UTF8);

		return new GeneratorResult(item.ClassIdentifier.Text, sourceText);
	}

	private static bool IsCandidate(SyntaxNode node, CancellationToken cancellationToken)
	{
		return node.IsKind(SyntaxKind.ClassDeclaration);
	}
}