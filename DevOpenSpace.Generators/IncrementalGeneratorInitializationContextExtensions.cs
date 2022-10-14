using Microsoft.CodeAnalysis;

namespace DevOpenSpace.Generators;

public static class IncrementalGeneratorInitializationContextExtensions
{
	public static void AddSource(this IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<GeneratorResult> provider)
	{
		context.RegisterSourceOutput(provider, AddSource);
	}

	private static void AddSource(SourceProductionContext context, GeneratorResult result)
	{
		if (result.SourceText is not null)
		{
			context.AddSource($"{result.TypeName}.g.cs", result.SourceText);
		}
		else if (result.Diagnostic is not null)
		{
			context.ReportDiagnostic(result.Diagnostic);
		}
	}
}