using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Perf.SourceGenerator;
using Perf.Core;

namespace Perf.SourceGenerator.Tests;

public class PerfGeneratorTests
{
    [Fact]
    public void Generator_CreatesWrapperMethod()
    {
        string source = @"
using System.Threading.Tasks;
using Perf.Core;

namespace MyApp.Services
{
    public partial class PlayerService
    {
        [Perf]
        private async Task<string> GetPlayerNameAsync_Impl(int id)
        {
            await Task.Delay(10);
            return ""Player"" + id;
        }
    }
}
";

        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        
        // Add references required for compilation
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();
        
        references.Add(MetadataReference.CreateFromFile(typeof(PerfAttribute).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "Tests",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new PerfGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var runResult = driver.GetRunResult();
        Assert.Single(runResult.GeneratedTrees);

        var generatedSource = runResult.GeneratedTrees[0].GetText().ToString();
        
        // Verify expected parts of the generated code
        Assert.Contains("public partial class PlayerService", generatedSource);
        Assert.Contains("public async System.Threading.Tasks.Task<string> GetPlayerNameAsync(int id)", generatedSource);
        Assert.Contains("using var scope = (PerfScope)Perf.Measure(__opId_GetPlayerNameAsync);", generatedSource);
        Assert.Contains("return await GetPlayerNameAsync_Impl(id);", generatedSource);
    }
}
