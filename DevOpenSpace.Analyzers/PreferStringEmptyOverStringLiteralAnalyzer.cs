using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DevOpenSpace.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferStringEmptyOverStringLiteralAnalyzer : DiagnosticAnalyzer
{
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(OnStringLiteralAnalysis, SyntaxKind.StringLiteralExpression);
	}

	public string S => "";

	private static void OnStringLiteralAnalysis(SyntaxNodeAnalysisContext context)
	{
		context.CancellationToken.ThrowIfCancellationRequested();
		var stringLiteral = (LiteralExpressionSyntax)context.Node;
		if (stringLiteral.Parent is EqualsValueClauseSyntax { Parent: ParameterSyntax })
		{
			return;
		}

		var token = stringLiteral.Token.Text;
		if (token == "\"\"")
		{
			var diagnostic = Diagnostic.Create(DiagnosticDescriptors.PreferStringEmptyOverStringLiteral, stringLiteral.GetLocation());
			context.ReportDiagnostic(diagnostic);
		}
	}

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
		= ImmutableArray.Create(DiagnosticDescriptors.PreferStringEmptyOverStringLiteral);
}