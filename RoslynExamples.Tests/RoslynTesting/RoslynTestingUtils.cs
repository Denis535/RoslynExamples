namespace RoslynTesting {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    public static partial class RoslynTestingUtils {


        // Initialization
        public static Project CreateFakeProject(string name, string content) {
            return new AdhocWorkspace()
                .AddSolution( SolutionInfo.Create( SolutionId.CreateNewId(), VersionStamp.Create(), null, null, null ) )
                .AddProject( "FakeProject", "FakeProject", LanguageNames.CSharp )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( object ).Assembly.Location ) )
                .AddDocument( name, content ).Project;
        }
        public static Project CreateFakeProject((string name, string content)[] documents) {
            var project = new AdhocWorkspace()
                .AddSolution( SolutionInfo.Create( SolutionId.CreateNewId(), VersionStamp.Create(), null, null, null ) )
                .AddProject( "FakeProject", "FakeProject", LanguageNames.CSharp )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( object ).Assembly.Location ) );
            foreach (var (name, content) in documents) {
                project = project.AddDocument( name, content ).Project;
            }
            return project;
        }
        public static (string name, string content)[] GetDocuments(string directory, params string[] names) {
            return names
                .Select( i => (Name: i, Path: Path.Combine( directory, i )) )
                .Select( i => (i.Name, File.ReadAllText( i.Path )) )
                .ToArray();
        }
        public static (string name, string content)[] GetDocuments(string directory, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly) {
            return new DirectoryInfo( directory )
                .EnumerateFiles( searchPattern, searchOption )
                .Select( i => (i.Name, File.ReadAllText( i.FullName )) ).ToArray();
        }


        // Analysis
        public static async Task<Diagnostic[]> AnalyzeAsync(Project project, DiagnosticAnalyzer[] analyzers, CancellationToken cancellationToken) {
            var compilation = await project.GetCompilationAsync( cancellationToken ).ConfigureAwait( false ) ?? throw new Exception( "Compilation is not found" );
            var compilationWithAnalyzers = compilation.WithAnalyzers( analyzers.ToImmutableArray(), project.AnalyzerOptions );
            var diagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync( cancellationToken ).ConfigureAwait( false );
            return diagnostics.Where( i => !IsCompilerDiagnostic( i ) ).OrderBy( i => i.Id ).ThenBy( i => i.Location.SourceTree?.FilePath ).ThenBy( i => i.Location.SourceSpan ).ToArray();
        }


        // Fixing
        public static async Task<(Project, CodeAction)[]> FixAsync(Project project, CodeFixProvider fixer, Diagnostic diagnostic, CancellationToken cancellationToken) {
            var actions = new List<CodeAction>();
            await GetCodeFixActionsAsync( project, fixer, diagnostic, actions, cancellationToken ).ConfigureAwait( false );
            return await ApplyCodeActionsAsync( actions, cancellationToken ).ConfigureAwait( false );
        }
        public static async Task<(Project, CodeAction)[]> FixAsync(Project project, CodeFixProvider fixer, Diagnostic[] diagnostics, CancellationToken cancellationToken) {
            var actions = new List<CodeAction>();
            foreach (var diagnostics_ in diagnostics.GroupBy( i => (i.Location.SourceTree, i.Location.SourceSpan) )) {
                await GetCodeFixActionsAsync( project, fixer, diagnostics_.ToArray(), actions, cancellationToken ).ConfigureAwait( false );
            }
            return await ApplyCodeActionsAsync( actions, cancellationToken ).ConfigureAwait( false );
        }


        // Refactoring
        public static async Task<(Project, CodeAction)[]> RefactorAsync(Project project, CodeRefactoringProvider refactorer, CancellationToken cancellationToken) {
            var actions = new List<CodeAction>();
            foreach (var document in project.Documents) {
                await GetRefactoringActionsAsync( document, refactorer, actions, cancellationToken ).ConfigureAwait( false );
            }
            return await ApplyCodeActionsAsync( actions, cancellationToken ).ConfigureAwait( false );
        }
        public static async Task<(Project, CodeAction)[]> RefactorAsync(Document document, CodeRefactoringProvider refactorer, CancellationToken cancellationToken) {
            var actions = new List<CodeAction>();
            await GetRefactoringActionsAsync( document, refactorer, actions, cancellationToken ).ConfigureAwait( false );
            return await ApplyCodeActionsAsync( actions, cancellationToken ).ConfigureAwait( false );
        }


        // Generation
        public static async Task<GeneratorRunResult> GenerateAsync(Project project, ISourceGenerator generator, CancellationToken cancellationToken) {
            var driver = GetGeneratorDriver( project, generator );
            var compilation = await project.GetCompilationAsync( cancellationToken ).ConfigureAwait( false ) ?? throw new Exception( "Compilation is not found" );
            driver = (CSharpGeneratorDriver) driver.RunGenerators( compilation, cancellationToken );
            return driver.GetRunResult().Results.Single();
        }


        // Helpers/Analysis
        private static bool IsCompilerDiagnostic(Diagnostic diagnostic) {
            return diagnostic.Descriptor.CustomTags.Contains( WellKnownDiagnosticTags.Compiler );
        }
        // Helpers/Fixing
        private static async Task GetCodeFixActionsAsync(Project project, CodeFixProvider fixer, Diagnostic diagnostic, List<CodeAction> actions, CancellationToken cancellationToken) {
            if (!fixer.FixableDiagnosticIds.Contains( diagnostic.Id )) throw new ArgumentException( $"Diagnostic is not supported by CodeFixProvider: Diagnostic={diagnostic.Id}, CodeFixProvider={fixer.GetType().Name}" );

            var tree = diagnostic.Location.SourceTree ?? throw new Exception( "Syntax tree is null" );
            var document = project.GetDocument( tree ) ?? throw new Exception( "Document is not found" );
            var context = new CodeFixContext( document, diagnostic, (action, _) => actions.Add( action ), cancellationToken );
            await fixer.RegisterCodeFixesAsync( context ).ConfigureAwait( false );
        }
        private static async Task GetCodeFixActionsAsync(Project project, CodeFixProvider fixer, Diagnostic[] diagnostics, List<CodeAction> actions, CancellationToken cancellationToken) {
            // Note: diagnostics must point to the same document and location
            foreach (var diagnostic in diagnostics) {
                if (!fixer.FixableDiagnosticIds.Contains( diagnostic.Id )) throw new ArgumentException( $"Diagnostic is not supported by CodeFixProvider: Diagnostic={diagnostic.Id}, CodeFixProvider={fixer.GetType().Name}" );
            }
            if (diagnostics.Select( i => i.Location.SourceTree ).Distinct().Count() > 1) throw new ArgumentException( $"Diagnostics are invalid: {diagnostics.Select( i => i.Id ).Join()}" );
            if (diagnostics.Select( i => i.Location.SourceSpan ).Distinct().Count() > 1) throw new ArgumentException( $"Diagnostics are invalid: {diagnostics.Select( i => i.Id ).Join()}" );

            var tree = diagnostics.First().Location.SourceTree ?? throw new Exception( "Syntax tree is null" );
            var document = project.GetDocument( tree ) ?? throw new Exception( "Document is not found" );
            var span = diagnostics.First().Location.SourceSpan;
            var context = new CodeFixContext( document, span, diagnostics.ToImmutableArray(), (action, _) => actions.Add( action ), cancellationToken );
            await fixer.RegisterCodeFixesAsync( context ).ConfigureAwait( false );
        }
        // Helpers/Refactoring
        private static async Task GetRefactoringActionsAsync(Document document, CodeRefactoringProvider refactorer, List<CodeAction> actions, CancellationToken cancellationToken) {
            var root = await document.GetSyntaxRootAsync( cancellationToken ).ConfigureAwait( false ) ?? throw new Exception( "Document is not found" ); ;
            var context = new CodeRefactoringContext( document, root.FullSpan, action => actions.Add( action ), cancellationToken );
            await refactorer.ComputeRefactoringsAsync( context ).ConfigureAwait( false );
        }
        // Helpers/Generation
        private static CSharpGeneratorDriver GetGeneratorDriver(Project project, ISourceGenerator generator) {
            return CSharpGeneratorDriver.Create( new[] { generator }, project.AnalyzerOptions.AdditionalFiles, (CSharpParseOptions?) project.ParseOptions, project.AnalyzerOptions.AnalyzerConfigOptionsProvider );
        }
        // Helpers/Misc
        private static async Task<(Project, CodeAction)[]> ApplyCodeActionsAsync(IList<CodeAction> actions, CancellationToken cancellationToken) {
            var result = new List<(Project, CodeAction)>();
            foreach (var action in actions) {
                var operations = await action.GetOperationsAsync( cancellationToken ).ConfigureAwait( false );
                var operation = operations.Cast<ApplyChangesOperation>().Single();
                var project = operation.ChangedSolution.Projects.First();
                result.Add( (project, action) );
            }
            return result.ToArray();
        }


    }

    internal sealed class DiagnosticProvider : FixAllContext.DiagnosticProvider {

        private Diagnostic[] Diagnostics { get; }

        public DiagnosticProvider(Diagnostic[] diagnostics) {
            Diagnostics = diagnostics;
        }


        public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken) {
            return Task.FromResult( Diagnostics.AsEnumerable() );
        }

        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken) {
            // todo: is it ok that project is unused?
            return Task.FromResult( Diagnostics.Where( i => !i.Location.IsInSource ) );
        }

        public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken) {
            return Task.FromResult( Diagnostics.Where( i => i.Location.GetLineSpan().Path == document.Name ) );
        }

    }

}
