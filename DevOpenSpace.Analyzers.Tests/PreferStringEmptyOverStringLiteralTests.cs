using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

namespace DevOpenSpace.Analyzers.Tests;

using Verifier = CSharpCodeFixVerifier<PreferStringEmptyOverStringLiteralAnalyzer, PreferStringEmptyOverStringLiteralCodeFix, XUnitVerifier>;
public class PreferStringEmptyOverStringLiteralTests
{
	private const string Scaffold = @"
namespace UnitTests;

public class Test {{
	public void M() {{
		{0}
	}}
}}
";
	[Fact]
	public Task Test()
	{
		var code = string.Format(Scaffold, "string s = [|\"\"|];");
		var fixedCode = string.Format(Scaffold, "string s = string.Empty;");

		return Verifier.VerifyCodeFixAsync(code, fixedCode);
	}

	[Fact]
	public Task ShouldNotRaiseOnDefaultParameter()
	{
		var code = string.Format(Scaffold, "void M(string s = \"\") {}");

		return Verifier.VerifyAnalyzerAsync(code);
	}
}