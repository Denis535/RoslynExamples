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
            var result = await RoslynTestingUtils.GenerateAsync( generator, Project, default ).ConfigureAwait( false );
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
            var (method, model) = await GetMethod( Project, "ConsoleApp1/Program.cs", "ControlFlowGraphExample" ).ConfigureAwait( false );
            var graph = ControlFlowGraph.Create( method, model ) ?? throw new Exception( "Control flow graph is null" );
            var message = RoslynTestingMessages.GetMessage( graph );
            TestContext.WriteLine( message );
        }


        // ControlFlowAnalysis
        [Test]
        public async Task Test_02_ControlFlowAnalysis() {
            var (method, model) = await GetMethod( Project, "ConsoleApp1/Program.cs", "ControlFlowAnalysisExample" ).ConfigureAwait( false );
            var analysis = model!.AnalyzeControlFlow( method.Body! )!;
            var message = RoslynTestingMessages.GetMessage( analysis, method.Body! );
            TestContext.WriteLine( message );
        }


        // DataFlowAnalysis
        [Test]
        public async Task Test_03_DataFlowAnalysis() {
            var (method, model) = await GetMethod( Project, "ConsoleApp1/Program.cs", "DataFlowAnalysisExample" ).ConfigureAwait( false );
            var analysis = model!.AnalyzeDataFlow( method.Body! )!;
            var message = RoslynTestingMessages.GetMessage( analysis, method.Body! );
            TestContext.WriteLine( message );
        }


        // Helpers
        private static async Task<(MethodDeclarationSyntax, SemanticModel)> GetMethod(Project project, string document, string method) {
            var document_ = project.Documents.Where( i => i.Name == document ).SingleOrDefault() ?? throw new Exception( "Document is null: " + document );
            var root = await document_.GetSyntaxRootAsync().ConfigureAwait( false ) ?? throw new Exception( "Syntax root is null" );
            var method_ = root.DescendantNodes().OfType<MethodDeclarationSyntax>().SingleOrDefault( i => i.Identifier.Text == method ) ?? throw new Exception( "Method is null: " + method );
            var model = await document_.GetSemanticModelAsync().ConfigureAwait( false ) ?? throw new Exception( "Semantic model is null" );
            return (method_, model);
        }


    }
}
