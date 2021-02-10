namespace Microsoft.CodeAnalysis {
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
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    public static class WorkspacesTestingUtils {


        // Initialization/Project
        public static Project CreateFakeProject((SourceText Text, string Name, string Path)[] documents) {
            var project = CreateFakeProject();
            foreach (var (text, name, path) in documents) {
                project = project.AddDocument( name, text, null, path ).Project;
            }
            return project;
        }
        private static Project CreateFakeProject() {
            var mscorlib = MetadataReference.CreateFromFile( typeof( object ).Assembly.Location );
            return new AdhocWorkspace()
                .AddSolution( SolutionInfo.Create( SolutionId.CreateNewId(), VersionStamp.Create(), null, null, null ) )
                .AddProject( "FakeProject", "FakeProject", LanguageNames.CSharp )
                .WithCompilationOptions( new CSharpCompilationOptions( OutputKind.ConsoleApplication ) )
                .AddMetadataReference( mscorlib );
        }
        // Initialization/Documents
        public static IEnumerable<(SourceText Text, string Name, string Path)> LoadDocuments(string directory, params string[] names) {
            foreach (var name in names) {
                var path = Path.GetFullPath( Path.Combine( directory, name ) );
                var text = File.ReadAllText( path );
                yield return (SourceText.From( text ), name, path);
            }
        }
        // Initialization/Utils
        public static (Document, SemanticModel) FindDocument(this Project project, string name) {
            var document = project.Documents.Where( i => i.Name == name ).SingleOrDefault() ?? throw new Exception( "Document is not found: " + name );
            var model = document.GetSemanticModelAsync().Result ?? throw new Exception( "Semantic model is null" );
            return (document, model);
        }
        public static MethodDeclarationSyntax FindMethod(this Document document, string name) {
            var root = document.GetSyntaxRootAsync().Result ?? throw new Exception( "Syntax root is null" );
            return root.DescendantNodes().OfType<MethodDeclarationSyntax>().SingleOrDefault( i => i.Identifier.Text == name ) ?? throw new Exception( "Method is not found: " + name );
        }


        // Fixing
        public static async Task<(Project, CodeAction)[]> FixAsync(CodeFixProvider fixer, Project project, Diagnostic diagnostic, CancellationToken cancellationToken) {
            var actions = new List<CodeAction>();
            await GetCodeFixActionsAsync( fixer, project, diagnostic, actions, cancellationToken ).ConfigureAwait( false );
            return await ApplyCodeActionsAsync( actions, cancellationToken ).ConfigureAwait( false );
        }
        public static async Task<(Project, CodeAction)[]> FixAsync(CodeFixProvider fixer, Project project, Diagnostic[] diagnostics, CancellationToken cancellationToken) {
            var actions = new List<CodeAction>();
            foreach (var diagnostics_ in diagnostics.GroupBy( i => (i.Location.SourceTree, i.Location.SourceSpan) )) {
                await GetCodeFixActionsAsync( fixer, project, diagnostics_.ToArray(), actions, cancellationToken ).ConfigureAwait( false );
            }
            return await ApplyCodeActionsAsync( actions, cancellationToken ).ConfigureAwait( false );
        }


        // Refactoring
        public static async Task<(Project, CodeAction)[]> RefactorAsync(CodeRefactoringProvider refactorer, Project project, CancellationToken cancellationToken) {
            var actions = new List<CodeAction>();
            foreach (var document in project.Documents) {
                await GetRefactoringActionsAsync( refactorer, document, actions, cancellationToken ).ConfigureAwait( false );
            }
            return await ApplyCodeActionsAsync( actions, cancellationToken ).ConfigureAwait( false );
        }
        public static async Task<(Project, CodeAction)[]> RefactorAsync(CodeRefactoringProvider refactorer, Document document, CancellationToken cancellationToken) {
            var actions = new List<CodeAction>();
            await GetRefactoringActionsAsync( refactorer, document, actions, cancellationToken ).ConfigureAwait( false );
            return await ApplyCodeActionsAsync( actions, cancellationToken ).ConfigureAwait( false );
        }


        // Helpers/Fixing
        private static async Task GetCodeFixActionsAsync(CodeFixProvider fixer, Project project, Diagnostic diagnostic, List<CodeAction> actions, CancellationToken cancellationToken) {
            if (!fixer.FixableDiagnosticIds.Contains( diagnostic.Id )) throw new ArgumentException( $"Diagnostic is not supported by CodeFixProvider: Diagnostic={diagnostic.Id}, CodeFixProvider={fixer.GetType().Name}" );

            var tree = diagnostic.Location.SourceTree ?? throw new Exception( "Syntax tree is null" );
            var document = project.GetDocument( tree ) ?? throw new Exception( "Document is not found" );
            var context = new CodeFixContext( document, diagnostic, (action, _) => actions.Add( action ), cancellationToken );
            await fixer.RegisterCodeFixesAsync( context ).ConfigureAwait( false );
        }
        private static async Task GetCodeFixActionsAsync(CodeFixProvider fixer, Project project, Diagnostic[] diagnostics, List<CodeAction> actions, CancellationToken cancellationToken) {
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
        private static async Task GetRefactoringActionsAsync(CodeRefactoringProvider refactorer, Document document, List<CodeAction> actions, CancellationToken cancellationToken) {
            var root = await document.GetSyntaxRootAsync( cancellationToken ).ConfigureAwait( false ) ?? throw new Exception( "Document is not found" ); ;
            var context = new CodeRefactoringContext( document, root.FullSpan, action => actions.Add( action ), cancellationToken );
            await refactorer.ComputeRefactoringsAsync( context ).ConfigureAwait( false );
        }
        // Helpers/CodeAction
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
}