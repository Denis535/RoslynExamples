namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    [SetCulture( "en-US" )]
    [SetUICulture( "en-US" )]
    public class Tests_01_Workspaces {


        [SetUp]
        public void SetUp() {
            Trace.Listeners.Add( new TextWriterTraceListener( TestContext.Out ) );
            Directory.SetCurrentDirectory( TestContext.CurrentContext.TestDirectory );
            Directory.SetCurrentDirectory( "../../../../RoslynExamples.Tests.Data/RoslynExamples.Tests.Data/" );
        }
        [TearDown]
        public void TearDown() {
        }


        // Fixing
        [Test]
        public async Task Test_00_Fixing() {
            var project = WorkspacesTestingUtils.CreateFakeProject().LoadDocuments( "TestData_DiagnosticAnalysis.cs" );
            var compilation = await project.GetCompilationAsync( default ).ConfigureAwait( false ) ?? throw new Exception( "Compilation is null" );
            var analyzers = new DiagnosticAnalyzer[] { new ExampleAnalyzer0000(), new ExampleAnalyzer0001() };
            var diagnostics = await CodeAnalysisTestingUtils.AnalyzeAsync( compilation, analyzers, null, default ).ConfigureAwait( false );
            var fixer = new ExampleCodeFixProvider();

            var changedProjects = await WorkspacesTestingUtils.FixAsync( fixer, project, diagnostics, default ).ConfigureAwait( false );
            var message = WorkspacesTestingMessages.GetMessage( fixer, project, analyzers, diagnostics, changedProjects );
            TestContext.WriteLine( message );
        }


        // Refactoring
        [Test]
        public async Task Test_01_Refactoring() {
            var project = WorkspacesTestingUtils.CreateFakeProject().LoadDocuments( "TestData_DiagnosticAnalysis.cs" );
            var refactorer = new ExampleCodeRefactoringProvider();

            var changedProjects = await WorkspacesTestingUtils.RefactorAsync( refactorer, project, default ).ConfigureAwait( false );
            var message = WorkspacesTestingMessages.GetMessage( refactorer, project, changedProjects );
            TestContext.WriteLine( message );
        }


    }
}