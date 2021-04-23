#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
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


        // Initialization
        public static CSharpCompilation CreateFakeCompilation() {
            return CSharpCompilation.Create( "FakeCompilation" )
                .WithReferences( Basic.Reference.Assemblies.Net50.All )
                .WithOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary ) );
        }
        public static CSharpCompilation LoadDocuments(this CSharpCompilation compilation, params string[] paths) {
            return compilation.AddSyntaxTrees( LoadDocuments( paths.Select( Path.GetFullPath ) ) );
        }
        private static IEnumerable<SyntaxTree> LoadDocuments(IEnumerable<string> paths) {
            foreach (var path in paths) {
                var text = SourceText.From( File.ReadAllText( path ) );
                var tree = CSharpSyntaxTree.ParseText( text, null, path, default );
                yield return tree;
            }
        }


        // Analysis
        public static async Task<Diagnostic[]> AnalyzeAsync(Compilation compilation, DiagnosticAnalyzer[] analyzers, AnalyzerOptions? analyzerOptions, CancellationToken cancellationToken) {
            var compilationWithAnalyzers = compilation.WithAnalyzers( analyzers.ToImmutableArray(), analyzerOptions, cancellationToken );
            var diagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync( cancellationToken ).ConfigureAwait( false );
            return diagnostics.Where( i => !IsCompilerDiagnostic( i ) ).OrderBy( i => i.Id ).ThenBy( i => i.Location.SourceTree?.FilePath ).ThenBy( i => i.Location.SourceSpan ).ToArray();
        }


        // Generation
        public static async Task<GeneratorRunResult> GenerateAsync(ISourceGenerator generator, Compilation compilation, CancellationToken cancellationToken) {
            var driver = GetGeneratorDriver( generator, null, null, null );
            return driver.RunGenerators( compilation, cancellationToken ).GetRunResult().Results.Single();
        }


        // Utils/FindDocument
        public static (SyntaxNode Root, SemanticModel Model) FindDocument(this Compilation compilation) {
            var tree = compilation.SyntaxTrees.SingleOrDefault() ?? throw new Exception( "Document is not found" );
            var model = compilation.GetSemanticModel( tree );
            return (tree.GetRoot(), model);
        }
        public static (SyntaxNode Root, SemanticModel Model) FindDocument(this Compilation compilation, string name) {
            var tree = compilation.SyntaxTrees.SingleOrDefault( i => Path.GetFileName( i.FilePath ) == name ) ?? throw new Exception( "Document is not found: " + name );
            var model = compilation.GetSemanticModel( tree );
            return (tree.GetRoot(), model);
        }
        // Utils/FindType
        public static (BaseTypeDeclarationSyntax Type, SemanticModel Model) FindType(this (SyntaxNode Node, SemanticModel Model) tuple) {
            var type = tuple.Node.GetMemberDeclarations<BaseTypeDeclarationSyntax>().SingleOrDefault() ?? throw new Exception( "Type is not found" );
            return (type, tuple.Model);
        }
        public static (BaseTypeDeclarationSyntax Type, SemanticModel Model) FindType(this (SyntaxNode Node, SemanticModel Model) tuple, string name) {
            var type = tuple.Node.GetMemberDeclarations<BaseTypeDeclarationSyntax>().SingleOrDefault( i => i.Identifier.Text == name ) ?? throw new Exception( "Type is not found: " + name );
            return (type, tuple.Model);
        }
        // Utils/FindMethod
        public static (MethodDeclarationSyntax Method, SemanticModel Model) FindMethod(this (SyntaxNode Node, SemanticModel Model) tuple) {
            var method = tuple.Node.GetMemberDeclarations<MethodDeclarationSyntax>().SingleOrDefault() ?? throw new Exception( "Method is not found" );
            return (method, tuple.Model);
        }
        public static (MethodDeclarationSyntax Method, SemanticModel Model) FindMethod(this (SyntaxNode Node, SemanticModel Model) tuple, string name) {
            var method = tuple.Node.GetMemberDeclarations<MethodDeclarationSyntax>().SingleOrDefault( i => i.Identifier.Text == name ) ?? throw new Exception( "Method is not found: " + name );
            return (method, tuple.Model);
        }


        // Helpers/Analysis
        private static bool IsCompilerDiagnostic(Diagnostic diagnostic) {
            return diagnostic.Descriptor.CustomTags.Contains( WellKnownDiagnosticTags.Compiler );
        }
        // Helpers/Generation
        private static CSharpGeneratorDriver GetGeneratorDriver(ISourceGenerator generator, IEnumerable<AdditionalText>? additionalTexts, CSharpParseOptions? parseOptions, AnalyzerConfigOptionsProvider? analyzerConfigOptionsProvider) {
            return CSharpGeneratorDriver.Create( new[] { generator }, additionalTexts, parseOptions, analyzerConfigOptionsProvider );
        }
        // Helpers/SyntaxNode
        private static IEnumerable<T> GetMemberDeclarations<T>(this SyntaxNode node) where T : MemberDeclarationSyntax {
            return node.DescendantNodes( i => i is CompilationUnitSyntax or MemberDeclarationSyntax ).OfType<T>();
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
