namespace Microsoft.CodeAnalysis {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    public static class CodeAnalysisTestingUtils {


        // Initialization/Compilation
        public static CSharpCompilation CreateFakeCompilation((SourceText Text, string Path)[] documents) {
            var trees = documents.Select( i => CSharpSyntaxTree.ParseText( i.Text, null, i.Path ) );
            return CreateFakeCompilation().AddSyntaxTrees( trees );
        }
        private static CSharpCompilation CreateFakeCompilation() {
            var mscorlib = MetadataReference.CreateFromFile( typeof( object ).Assembly.Location );
            return CSharpCompilation.Create( "FakeCompilation" )
                .WithOptions( new CSharpCompilationOptions( OutputKind.ConsoleApplication ) )
                .WithReferences( mscorlib );
        }
        // Initialization/Documents
        public static IEnumerable<(SourceText Text, string Path)> LoadDocuments(string directory, params string[] names) {
            foreach (var name in names) {
                var path = Path.GetFullPath( Path.Combine( directory, name ) );
                var text = File.ReadAllText( path );
                yield return (SourceText.From( text ), path);
            }
        }
        // Initialization/Utils
        public static (SyntaxTree, SemanticModel) FindSyntaxTree(this Compilation compilation, string name) {
            var tree = compilation.SyntaxTrees.Where( i => Path.GetFileName( i.FilePath ) == name ).SingleOrDefault() ?? throw new Exception( "Syntax tree is not found: " + name );
            var model = compilation.GetSemanticModel( tree ) ?? throw new Exception( "Semantic model is null" );
            return (tree, model);
        }
        public static MethodDeclarationSyntax FindMethod(this SyntaxTree tree, string name) {
            return tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().SingleOrDefault( i => i.Identifier.Text == name ) ?? throw new Exception( "Method is not found: " + name );
        }


        // Analysis
        public static async Task<Diagnostic[]> AnalyzeAsync(Compilation compilation, DiagnosticAnalyzer[] analyzers, AnalyzerOptions? analyzerOptions, CancellationToken cancellationToken) {
            var compilationWithAnalyzers = compilation.WithAnalyzers( analyzers.ToImmutableArray(), analyzerOptions, cancellationToken );
            var diagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync( cancellationToken ).ConfigureAwait( false );
            return diagnostics.Where( i => !IsCompilerDiagnostic( i ) ).OrderBy( i => i.Id ).ThenBy( i => i.Location.SourceTree?.FilePath ).ThenBy( i => i.Location.SourceSpan ).ToArray();
        }


        // Generation
        public static GeneratorRunResult GenerateAsync(ISourceGenerator generator, Compilation compilation, CancellationToken cancellationToken) {
            var driver = GetGeneratorDriver( generator, null, null, null );
            return driver.RunGenerators( compilation, cancellationToken ).GetRunResult().Results.Single();
        }


        // Helpers/Analysis
        private static bool IsCompilerDiagnostic(Diagnostic diagnostic) {
            return diagnostic.Descriptor.CustomTags.Contains( WellKnownDiagnosticTags.Compiler );
        }
        // Helpers/Generation
        private static CSharpGeneratorDriver GetGeneratorDriver(ISourceGenerator generator, IEnumerable<AdditionalText>? additionalTexts, CSharpParseOptions? parseOptions, AnalyzerConfigOptionsProvider? analyzerConfigOptionsProvider) {
            return CSharpGeneratorDriver.Create( new[] { generator }, additionalTexts, parseOptions, analyzerConfigOptionsProvider );
        }


    }
    //internal sealed class DiagnosticProvider : FixAllContext.DiagnosticProvider {

    //    private Diagnostic[] Diagnostics { get; }

    //    public DiagnosticProvider(Diagnostic[] diagnostics) {
    //        Diagnostics = diagnostics;
    //    }


    //    public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken) {
    //        return Task.FromResult( Diagnostics.AsEnumerable() );
    //    }

    //    public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken) {
    //        // todo: is it ok that project is unused?
    //        return Task.FromResult( Diagnostics.Where( i => !i.Location.IsInSource ) );
    //    }

    //    public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken) {
    //        return Task.FromResult( Diagnostics.Where( i => i.Location.GetLineSpan().Path == document.Name ) );
    //    }

    //}
}
