using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DevOpenSpace.Generators;

public class GeneratorResult
{
	public string? TypeName { get; set; }

	public SourceText? SourceText { get; set; }

	public Diagnostic? Diagnostic { get; set; }

	public static GeneratorResult Empty = new();

	public GeneratorResult(string typeName, SourceText sourceText)
	{
		TypeName = typeName;
		SourceText = sourceText;
	}

	public GeneratorResult(Diagnostic diagnostic)
	{
		Diagnostic = diagnostic;
	}

	private GeneratorResult()
	{
	}
}