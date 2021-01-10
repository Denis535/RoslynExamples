namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Tests {

        private Project Project { get; set; } = default!;


        [SetUp]
        public void SetUp() {
            Trace.Listeners.Add( new TextWriterTraceListener( TestContext.Out ) );
            Project = RoslynTestingUtils.CreateFakeProject( RoslynTestingUtils.GetDocuments( "../../../../ConsoleApp1/", "ConsoleApp1/Program.cs", "ConsoleApp1/Class.cs" ) );
        }
        [TearDown]
        public void TearDown() {
        }


        // Analysis
        [Test]
        public async Task Test_00_Analysis() {
            // Symbols:
            // ConsoleApp1,
            // Class, Method, Method2, argument, 
            // Field, Field2, Prop, Prop2, Event, Event2, Event3, Func, Func2, Method, Method2, argument
            var analyzers = new DiagnosticAnalyzer[] { new ExampleAnalyzer0000(), new ExampleAnalyzer0001(), new ExampleAnalyzer0002() };
            var diagnostics = await RoslynTestingUtils.AnalyzeAsync( Project, analyzers, default ).ConfigureAwait( false );
            PrintAnalysisResult( Project, analyzers, diagnostics );
        }


        // Analysis && fixing
        [Test]
        public async Task Test_01_Analysis_Fixing() {
            var analyzers = new DiagnosticAnalyzer[] { new ExampleAnalyzer0000(), new ExampleAnalyzer0001(), new ExampleAnalyzer0002() };
            var diagnostics = await RoslynTestingUtils.AnalyzeAsync( Project, analyzers, default ).ConfigureAwait( false );
            diagnostics = diagnostics.Where( i => i.Location.IsInSource ).ToArray();

            var fixer = new ExampleCodeFixProvider();
            var newProjects = await RoslynTestingUtils.FixAsync( Project, fixer, diagnostics, default ).ConfigureAwait( false );
            PrintFixingResult( Project, fixer, analyzers, diagnostics, newProjects );
        }


        // Analysis && fixing && batching
        //[Test]
        //public async Task Test_01_Analysis_Fixing_Batching() {
        //    var diagnostics = await RoslynTestingUtils.AnalyzeAsync( Project, Analyzers, default ).ConfigureAwait( false );
        //    //diagnostics = diagnostics.Distinct().Where( i => i.Location != Location.None ).ToArray();

        //    var document = Project.GetDocument( diagnostics.Skip( 1 ).First().Location.SourceTree ) ?? throw new Exception( "Document is not found" );
        //    var fixer = new ExampleCodeFixProvider();
        //    var fixAllProvider = fixer.GetFixAllProvider();
        //    //var supportedDiagnostics = fixAllProvider.GetSupportedFixAllDiagnosticIds( fixer ); // fixer.FixableDiagnosticIds;
        //    //var supportedScopes = fixAllProvider.GetSupportedFixAllScopes();

        //    var context = new FixAllContext( document, fixer, FixAllScope.Solution, fixer.FixableDiagnosticIds.Join(), fixer.FixableDiagnosticIds, new DiagnosticProvider( diagnostics ), default );
        //    var action = await fixAllProvider.GetFixAsync( context ).ConfigureAwait( false ) ?? throw new Exception( "Fix action is null" );
        //    var operations = await action.GetOperationsAsync( default ).ConfigureAwait( false );
        //    var operation = operations.Cast<ApplyChangesOperation>().Single();
        //    var newProject = operation.ChangedSolution.Projects.First();
        //}


        // Refactoring
        [Test]
        public async Task Test_02_Refactoring() {
            var refactorer = new ExampleCodeRefactoringProvider();
            var newProjects = await RoslynTestingUtils.RefactorAsync( Project, refactorer, default ).ConfigureAwait( false );
            PrintRefactoringResult( Project, refactorer, newProjects );
        }


        // Generation
        [Test]
        public async Task Test_04_Generation() {
            var generator = new ExampleSourceGenerator();
            var result = await RoslynTestingUtils.GenerateAsync( Project, generator, default ).ConfigureAwait( false );
            PrintGenerationResult( Project, result.Generator, result.GeneratedSources.ToArray(), result.Diagnostics.ToArray(), result.Exception );
        }


        // Helpers/Print
        private static void PrintAnalysisResult(Project project, DiagnosticAnalyzer[] analyzers, Diagnostic[] diagnostics) {
            TestContext.WriteLine( "Project: {0}", project.Name );
            TestContext.WriteLine( "Documents: {0}", project.Documents.Select( i => i.Name ).Join() );
            TestContext.WriteLine( "Analyzers: {0}", analyzers.Select( i => i.GetType().Name ).Join() );
            foreach (var diagnostic in diagnostics) {
                TestContext.WriteLine( GetDisplayString( diagnostic ) );
            }
        }
        private static void PrintFixingResult(Project project, CodeFixProvider fixer, DiagnosticAnalyzer[] analyzers, Diagnostic[] diagnostics, Project[] newProjects) {
            TestContext.WriteLine( "Project: {0}", project.Name );
            TestContext.WriteLine( "Documents: {0}", project.Documents.Select( i => i.Name ).Join() );
            TestContext.WriteLine( "Fixer: {0}", fixer.GetType().Name );
            TestContext.WriteLine( "Analyzers: {0}", analyzers.Select( i => i.GetType().Name ).Join() );
            foreach (var diagnostic in diagnostics) {
                TestContext.WriteLine( GetDisplayString( diagnostic ) );
            }
            foreach (var newProject in newProjects) {
                TestContext.WriteLine( "New project: {0}", newProject.Name );
                foreach (var newDocument in newProject.Documents) {
                    TestContext.WriteLine( "New document: {0}", newDocument.Name );
                    TestContext.WriteLine( newDocument.GetTextAsync().Result );
                }
            }
        }
        private static void PrintRefactoringResult(Project project, CodeRefactoringProvider refactorer, Project[] newProjects) {
            TestContext.WriteLine( "Project: {0}", project.Name );
            TestContext.WriteLine( "Documents: {0}", project.Documents.Select( i => i.Name ).Join() );
            TestContext.WriteLine( "Refactorer: {0}", refactorer.GetType().Name );
            foreach (var newProject in newProjects) {
                TestContext.WriteLine( "New project: {0}", newProject.Name );
                foreach (var newDocument in newProject.Documents) {
                    TestContext.WriteLine( "New document: {0}", newDocument.Name );
                    TestContext.WriteLine( newDocument.GetTextAsync().Result );
                }
            }
        }
        private static void PrintGenerationResult(Project project, ISourceGenerator generator, GeneratedSourceResult[] sources, Diagnostic[] diagnostics, Exception? exception) {
            TestContext.WriteLine( "Project: {0}", project.Name );
            TestContext.WriteLine( "Documents: {0}", project.Documents.Select( i => i.Name ).Join() );
            TestContext.WriteLine( "Generator: {0}", generator.GetType().Name );
            foreach (var source in sources) {
                TestContext.WriteLine( "Source: {0}", source.HintName );
                TestContext.WriteLine( source.SourceText );
            }
            foreach (var diagnostic in diagnostics) {
                TestContext.WriteLine( GetDisplayString( diagnostic ) );
            }
            if (exception != null) {
                TestContext.WriteLine( "Exception: {0}", exception );
            }
        }
        // Helpers/Misc
        private static string GetDisplayString(Diagnostic diagnostic) {
            if (diagnostic.Location.IsInSource) {
                var location = diagnostic.Location;
                return string.Format( "Diagnostic ({0}): {1} ({2}{3})", diagnostic.Id, diagnostic.GetMessage(), location.SourceTree.FilePath, location.SourceSpan );
            } else {
                return string.Format( "Diagnostic ({0}): {1}", diagnostic.Id, diagnostic.GetMessage() );
            }
        }


    }
}