namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.FlowAnalysis;
    using NUnit.Framework;

    [SetCulture( "en-US" )]
    [SetUICulture( "en-US" )]
    public class Tests_00_CodeAnalysis {

        private Project Project { get; set; } = default!;


        [SetUp]
        public void SetUp() {
            Trace.Listeners.Add( new TextWriterTraceListener( TestContext.Out ) );
            Project = RoslynTestingUtils.CreateFakeProject( RoslynTestingUtils.GetDocuments( "../../../../ConsoleApp1/", "ConsoleApp1/Program.cs", "ConsoleApp1/Class.cs" ).ToArray() );
        }
        [TearDown]
        public void TearDown() {
        }


        // Generation
        [Test]
        public async Task Test_00_Generation() {
            var generator = new ExampleSourceGenerator();
            var result = await RoslynTestingUtils.GenerateAsync( Project, generator, default ).ConfigureAwait( false );
            var message = RoslynTestingMessages.GetMessage( result.Generator, Project, result.GeneratedSources.ToArray(), result.Diagnostics.ToArray(), result.Exception );
            TestContext.WriteLine( message );
            foreach (var diagnostic in result.Diagnostics) {
                Assert.Warn( diagnostic.ToString() );
            }
            if (result.Exception != null) {
                Assert.Fail( result.Exception.ToString() );
            }
        }


        // ControlFlowGraph
        [Test]
        public async Task Test_01_ControlFlowGraph() {
            var document = Project.Documents.Where( i => i.Name == "ConsoleApp1/Program.cs" ).Single();
            var model = await document.GetSemanticModelAsync().ConfigureAwait( false );
            var root = await document.GetSyntaxRootAsync().ConfigureAwait( false );
            var method = root!.DescendantNodes().OfType<MethodDeclarationSyntax>().Single( i => i.Identifier.Text == "ControlFlowGraphExample" );
            var graph = ControlFlowGraph.Create( method, model );
            var message = RoslynTestingMessages.GetMessage( graph );
            TestContext.WriteLine( message );
        }


        // ControlFlowAnalysis
        [Test]
        public async Task Test_02_ControlFlowAnalysis() {
            var document = Project.Documents.Where( i => i.Name == "ConsoleApp1/Program.cs" ).Single();
            var model = await document.GetSemanticModelAsync().ConfigureAwait( false );
            var root = await document.GetSyntaxRootAsync().ConfigureAwait( false );
            var method = root!.DescendantNodes().OfType<MethodDeclarationSyntax>().Single( i => i.Identifier.Text == "ControlFlowAnalysisExample" );
            var analysis = model!.AnalyzeControlFlow( method.Body! )!;
            var message = RoslynTestingMessages.GetMessage( analysis, method.Body! );
            TestContext.WriteLine( message );
        }


        // DataFlowAnalysis
        [Test]
        public async Task Test_03_DataFlowAnalysis() {
            var document = Project.Documents.Where( i => i.Name == "ConsoleApp1/Program.cs" ).Single();
            var model = await document.GetSemanticModelAsync().ConfigureAwait( false );
            var root = await document.GetSyntaxRootAsync().ConfigureAwait( false );
            var method = root!.DescendantNodes().OfType<MethodDeclarationSyntax>().Single( i => i.Identifier.Text == "DataFlowAnalysisExample" );
            var analysis = model!.AnalyzeDataFlow( method.Body! )!;
            var message = RoslynTestingMessages.GetMessage( analysis, method.Body! );
            TestContext.WriteLine( message );
        }


    }
}
