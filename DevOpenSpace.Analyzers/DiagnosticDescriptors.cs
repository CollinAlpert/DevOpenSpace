using Microsoft.CodeAnalysis;

namespace DevOpenSpace.Analyzers;

public static class DiagnosticDescriptors
{
	public static readonly DiagnosticDescriptor PreferStringEmptyOverStringLiteral = new(
		"DEV002",
		"Prefer 'string.Empty' over \"\"",
		"Prefer 'string.Empty' over \"\"",
		"Style",
		DiagnosticSeverity.Warning,
		true
	);
}