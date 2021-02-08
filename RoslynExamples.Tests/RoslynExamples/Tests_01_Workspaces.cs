namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    [SetCulture( "en-US" )]
    [SetUICulture( "en-US" )]
    public class Tests_01_Workspaces {

        private Project Project { get; set; } = default!;


        [SetUp]
        public void SetUp() {
            Trace.Listeners.Add( new TextWriterTraceListener( TestContext.Out ) );
            Project = RoslynTestingUtils.CreateFakeProject( RoslynTestingUtils.GetDocuments( "../../../../ConsoleApp1/", "ConsoleApp1/Program.cs", "ConsoleApp1/Class.cs" ).ToArray() );
        }
        [TearDown]
        public void TearDown() {
        }


        // Analysis
        [Test]
        public async Task Test_00_Analysis() {
            var analyzers = new DiagnosticAnalyzer[] { new ExampleAnalyzer0000(), new ExampleAnalyzer0001(), new ExampleAnalyzer0002() };
            var diagnostics = await RoslynTestingUtils.AnalyzeAsync( Project, analyzers, default ).ConfigureAwait( false );
            var message = RoslynTestingMessages.GetMessage( Project, analyzers, diagnostics );
            TestContext.WriteLine( message );
        }


        // Analysis && fixing
        [Test]
        public async Task Test_01_Analysis_Fixing() {
            var analyzers = new DiagnosticAnalyzer[] { new ExampleAnalyzer0000(), new ExampleAnalyzer0001(), new ExampleAnalyzer0002() };
            var diagnostics = await RoslynTestingUtils.AnalyzeAsync( Project, analyzers, default ).ConfigureAwait( false );
            diagnostics = diagnostics.Where( i => i.Location.IsInSource ).ToArray();

            var fixer = new ExampleCodeFixProvider();
            var newProjects = await RoslynTestingUtils.FixAsync( fixer, Project, diagnostics, default ).ConfigureAwait( false );
            var message = RoslynTestingMessages.GetMessage( fixer, Project, analyzers, diagnostics, newProjects );
            TestContext.WriteLine( message );
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
            var newProjects = await RoslynTestingUtils.RefactorAsync( refactorer, Project, default ).ConfigureAwait( false );
            var message = RoslynTestingMessages.GetMessage( refactorer, Project, newProjects );
            TestContext.WriteLine( message );
        }


    }
}