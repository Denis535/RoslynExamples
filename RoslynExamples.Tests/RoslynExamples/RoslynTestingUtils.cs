namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
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

    public static class RoslynTestingUtils {


        // Initialization
        public static Project CreateFakeProject(string name, string content) {
            return new AdhocWorkspace()
                .AddSolution( SolutionInfo.Create( SolutionId.CreateNewId(), VersionStamp.Create(), null, null, null ) )
                .AddProject( "FakeProject", "FakeProject", LanguageNames.CSharp )
                .AddMetadataReference( MetadataReference.CreateFromFile( typeof( object ).Assembly.Location ) )
                .AddDocument( name, content ).Project;
        }


        // Analysis
        public static async Task<Diagnostic[]> AnalyzeAsync(Project project, DiagnosticAnalyzer[] analyzers, CancellationToken cancellationToken) {
            var compilation = await project.GetCompilationAsync( cancellationToken ).ConfigureAwait( false ) ?? throw new Exception( "Compilation not found" );
            var compilationWithAnalyzers = compilation.WithAnalyzers( analyzers.ToImmutableArray(), project.AnalyzerOptions );
            var diagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync( cancellationToken ).ConfigureAwait( false );
            return diagnostics.Where( i => !IsCompilerDiagnostic( i ) ).ToArray();
        }
        private static bool IsCompilerDiagnostic(Diagnostic diagnostic) {
            return diagnostic.Descriptor.CustomTags.Contains( WellKnownDiagnosticTags.Compiler );
        }


        // Fixing
        public static async Task<Project[]> Fix(Project project, CodeFixProvider fixer, Diagnostic[] diagnostics, CancellationToken cancellationToken) {
            var actions = await GetCodeFixActionsAsync( project, fixer, diagnostics, cancellationToken ).ConfigureAwait( false );
            return await ApplyCodeActionsAsync( actions, cancellationToken ).ConfigureAwait( false );
        }
        public static async Task<Project[]> Fix(Project project, CodeFixProvider fixer, Diagnostic diagnostic, CancellationToken cancellationToken) {
            var actions = await GetCodeFixActionsAsync( project, fixer, diagnostic, cancellationToken ).ConfigureAwait( false );
            return await ApplyCodeActionsAsync( actions, cancellationToken ).ConfigureAwait( false );
        }
        private static async Task<CodeAction[]> GetCodeFixActionsAsync(Project project, CodeFixProvider fixer, Diagnostic[] diagnostics, CancellationToken cancellationToken) {
            // Note: diagnostics must point to the same document and location
            foreach (var diagnostic in diagnostics) {
                if (!fixer.FixableDiagnosticIds.Contains( diagnostic.Id )) throw new ArgumentException( $"Diagnostic is not supported by CodeFixProvider: Diagnostic={diagnostic.Id}, CodeFixProvider={fixer.GetType().Name}" );
            }
            if (diagnostics.Select( i => i.Location.SourceTree ).Distinct().Count() > 1) throw new ArgumentException( $"Diagnostics are invalid: {diagnostics.Select( i => i.Id ).Join()}" );
            if (diagnostics.Select( i => i.Location.SourceSpan ).Distinct().Count() > 1) throw new ArgumentException( $"Diagnostics are invalid: {diagnostics.Select( i => i.Id ).Join()}" );

            var actions = new List<CodeAction>();
            var document = project.GetDocument( diagnostics.First().Location.SourceTree ) ?? throw new Exception( "Document not found" ); ;
            var span = diagnostics.First().Location.SourceSpan;
            var context = new CodeFixContext( document, span, diagnostics.ToImmutableArray(), (action, _) => actions.Add( action ), cancellationToken );
            await fixer.RegisterCodeFixesAsync( context ).ConfigureAwait( false );
            return actions.ToArray();
        }
        private static async Task<CodeAction[]> GetCodeFixActionsAsync(Project project, CodeFixProvider fixer, Diagnostic diagnostic, CancellationToken cancellationToken) {
            if (!fixer.FixableDiagnosticIds.Contains( diagnostic.Id )) throw new ArgumentException( $"Diagnostic is not supported by CodeFixProvider: Diagnostic={diagnostic.Id}, CodeFixProvider={fixer.GetType().Name}" );

            var actions = new List<CodeAction>();
            var document = project.GetDocument( diagnostic.Location.SourceTree ) ?? throw new Exception( "Document not found" );
            var context = new CodeFixContext( document, diagnostic, (action, _) => actions.Add( action ), cancellationToken );
            await fixer.RegisterCodeFixesAsync( context ).ConfigureAwait( false );
            return actions.ToArray();
        }


        // Refactoring
        public static async Task<Project[]> Refactor(Project project, CodeRefactoringProvider refactorer, CancellationToken cancellationToken) {
            var actions = await GetRefactoringActionsAsync( project, refactorer, cancellationToken ).ConfigureAwait( false );
            return await ApplyCodeActionsAsync( actions, cancellationToken ).ConfigureAwait( false );
        }
        public static async Task<Project[]> Refactor(Document document, CodeRefactoringProvider refactorer, CancellationToken cancellationToken) {
            var actions = await GetRefactoringActionsAsync( document, refactorer, cancellationToken ).ConfigureAwait( false );
            return await ApplyCodeActionsAsync( actions, cancellationToken ).ConfigureAwait( false );
        }
        private static async Task<CodeAction[]> GetRefactoringActionsAsync(Project project, CodeRefactoringProvider refactorer, CancellationToken cancellationToken) {
            var actions = new List<CodeAction>();
            foreach (var document in project.Documents) {
                var root = await document.GetSyntaxRootAsync().ConfigureAwait( false ) ?? throw new Exception( "Document not found" ); ;
                var context = new CodeRefactoringContext( document, root.FullSpan, action => actions.Add( action ), cancellationToken );
                await refactorer.ComputeRefactoringsAsync( context ).ConfigureAwait( false );
            }
            return actions.ToArray();
        }
        private static async Task<CodeAction[]> GetRefactoringActionsAsync(Document document, CodeRefactoringProvider refactorer, CancellationToken cancellationToken) {
            var actions = new List<CodeAction>();
            var root = await document.GetSyntaxRootAsync().ConfigureAwait( false ) ?? throw new Exception( "Document not found" ); ;
            var context = new CodeRefactoringContext( document, root.FullSpan, action => actions.Add( action ), cancellationToken );
            await refactorer.ComputeRefactoringsAsync( context ).ConfigureAwait( false );
            return actions.ToArray();
        }


        // Generation
        public static async Task<GeneratorRunResult> GenerateAsync(Project project, ISourceGenerator generator, CancellationToken cancellationToken) {
            var compilation = await project.GetCompilationAsync().ConfigureAwait( false ) ?? throw new Exception( "Compilation not found" );
            var driver = CSharpGeneratorDriver.Create( new[] { generator }, project.AnalyzerOptions.AdditionalFiles, (CSharpParseOptions?) project.ParseOptions, project.AnalyzerOptions.AnalyzerConfigOptionsProvider );
            driver = (CSharpGeneratorDriver) driver.RunGenerators( compilation, cancellationToken );
            return driver.GetRunResult().Results.Single();
        }


        // Misc
        private static async Task<Project[]> ApplyCodeActionsAsync(CodeAction[] actions, CancellationToken cancellationToken) {
            var result = new List<Project>();
            foreach (var action in actions) {
                var project = await ApplyCodeActionAsync( action, cancellationToken ).ConfigureAwait( false );
                result.Add( project );
            }
            return result.ToArray();
        }
        private static async Task<Project> ApplyCodeActionAsync(CodeAction action, CancellationToken cancellationToken) {
            var operations = await action.GetOperationsAsync( cancellationToken ).ConfigureAwait( false );
            var operation = operations.Cast<ApplyChangesOperation>().Single();
            return operation.ChangedSolution.Projects.First();
        }


    }

    //internal sealed class TesterDiagnosticProvider : FixAllContext.DiagnosticProvider {

    //    private ImmutableDictionary<ProjectId, ImmutableDictionary<string, ImmutableArray<Diagnostic>>> DocumentDiagnostics { get; set; }
    //    private ImmutableDictionary<ProjectId, ImmutableArray<Diagnostic>> ProjectDiagnostics { get; set; }


    //    public TesterDiagnosticProvider(ImmutableDictionary<ProjectId, ImmutableDictionary<string, ImmutableArray<Diagnostic>>> documentDiagnostics, ImmutableDictionary<ProjectId, ImmutableArray<Diagnostic>> projectDiagnostics) {
    //        DocumentDiagnostics = documentDiagnostics;
    //        ProjectDiagnostics = projectDiagnostics;
    //    }


    //    public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken) {
    //        if (!ProjectDiagnostics.TryGetValue( project.Id, out var filteredProjectDiagnostics )) {
    //            filteredProjectDiagnostics = ImmutableArray<Diagnostic>.Empty;
    //        }
    //        if (!DocumentDiagnostics.TryGetValue( project.Id, out var filteredDocumentDiagnostics )) {
    //            filteredDocumentDiagnostics = ImmutableDictionary<string, ImmutableArray<Diagnostic>>.Empty;
    //        }
    //        return Task.FromResult( filteredProjectDiagnostics.Concat( filteredDocumentDiagnostics.Values.SelectMany( i => i ) ) );
    //    }

    //    public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken) {
    //        if (ProjectDiagnostics.TryGetValue( project.Id, out var diagnostics )) {
    //            return Task.FromResult( diagnostics.AsEnumerable() );
    //        }
    //        return Task.FromResult( Enumerable.Empty<Diagnostic>() );
    //    }

    //    public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken) {
    //        if (DocumentDiagnostics.TryGetValue( document.Project.Id, out var diagnostics )) {
    //            if (diagnostics.TryGetValue( document.FilePath, out var diagnostics2 )) {
    //                return Task.FromResult( diagnostics2.AsEnumerable() );
    //            }
    //        }
    //        return Task.FromResult( Enumerable.Empty<Diagnostic>() );
    //    }


    //}

}
