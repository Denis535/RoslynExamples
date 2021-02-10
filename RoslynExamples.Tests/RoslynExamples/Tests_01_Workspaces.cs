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
            Project = WorkspacesTestingUtils.CreateFakeProject( WorkspacesTestingUtils.LoadDocuments( "../../../../ConsoleApp1/", "ConsoleApp1/Program.cs", "ConsoleApp1/Class.cs" ).ToArray() );
        }
        [TearDown]
        public void TearDown() {
        }


        // Fixing
        [Test]
        public async Task Test_00_Fixing() {
            var compilation = await Project.GetCompilationAsync( default ).ConfigureAwait( false ) ?? throw new Exception( "Compilation is null" );
            var analyzers = new DiagnosticAnalyzer[] { new ExampleAnalyzer0000(), new ExampleAnalyzer0001() };
            var diagnostics = await CodeAnalysisTestingUtils.AnalyzeAsync( compilation, analyzers, null, default ).ConfigureAwait( false );

            var fixer = new ExampleCodeFixProvider();
            var newProjects = await WorkspacesTestingUtils.FixAsync( fixer, Project, diagnostics, default ).ConfigureAwait( false );
            var message = Messages.GetMessage( fixer, Project, analyzers, diagnostics, newProjects );
            TestContext.WriteLine( message );
        }


        // Refactoring
        [Test]
        public async Task Test_01_Refactoring() {
            var refactorer = new ExampleCodeRefactoringProvider();
            var newProjects = await WorkspacesTestingUtils.RefactorAsync( refactorer, Project, default ).ConfigureAwait( false );
            var message = Messages.GetMessage( refactorer, Project, newProjects );
            TestContext.WriteLine( message );
        }


    }
}