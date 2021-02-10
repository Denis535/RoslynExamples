namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.FlowAnalysis;
    using NUnit.Framework;

    [SetCulture( "en-US" )]
    [SetUICulture( "en-US" )]
    public class Tests_00_CodeAnalysis {

        private Compilation Compilation { get; set; } = default!;


        [SetUp]
        public void SetUp() {
            Trace.Listeners.Add( new TextWriterTraceListener( TestContext.Out ) );
            Compilation = CodeAnalysisTestingUtils.CreateFakeCompilation( CodeAnalysisTestingUtils.LoadDocuments( "../../../../ConsoleApp1/", "ConsoleApp1/Program.cs", "ConsoleApp1/Class.cs" ).ToArray() );
        }
        [TearDown]
        public void TearDown() {
        }


        // Analysis
        [Test]
        public async Task Test_00_Analysis() {
            var analyzers = new DiagnosticAnalyzer[] { new ExampleAnalyzer0000(), new ExampleAnalyzer0001(), new ExampleAnalyzer0002() };
            var diagnostics = await CodeAnalysisTestingUtils.AnalyzeAsync( Compilation, analyzers, null, default ).ConfigureAwait( false );
            var message = Messages.GetMessage( Compilation, analyzers, diagnostics );
            TestContext.WriteLine( message );
        }


        // Generation
        [Test]
        public void Test_01_Generation() {
            var generator = new ExampleSourceGenerator();
            var generation = CodeAnalysisTestingUtils.GenerateAsync( generator, Compilation, default );
            var message = Messages.GetMessage( generation.Generator, Compilation, generation.GeneratedSources.ToArray(), generation.Diagnostics.ToArray(), generation.Exception );
            TestContext.WriteLine( message );
            foreach (var diagnostic in generation.Diagnostics) {
                Assert.Warn( diagnostic.ToString() );
            }
            if (generation.Exception != null) {
                Assert.Fail( generation.Exception.ToString() );
            }
        }


        // ControlFlowGraph
        [Test]
        public void Test_02_ControlFlowGraph() {
            var (tree, model) = Compilation.FindSyntaxTree( "Program.cs" );
            var method = tree.FindMethod( "ControlFlowGraphExample" );
            var graph = ControlFlowGraph.Create( method, model ) ?? throw new Exception( "Control flow graph is null" );
            var message = Messages.GetMessage( graph );
            TestContext.WriteLine( message );
        }


        // ControlFlowAnalysis
        [Test]
        public void Test_03_ControlFlowAnalysis() {
            var (tree, model) = Compilation.FindSyntaxTree( "Program.cs" );
            var method = tree.FindMethod( "ControlFlowAnalysisExample" );
            var analysis = model!.AnalyzeControlFlow( method.Body! ) ?? throw new Exception( "Control flow analysis is null" );
            var message = Messages.GetMessage( analysis, method.Body! );
            TestContext.WriteLine( message );
        }


        // DataFlowAnalysis
        [Test]
        public void Test_04_DataFlowAnalysis() {
            var (tree, model) = Compilation.FindSyntaxTree( "Program.cs" );
            var method = tree.FindMethod( "DataFlowAnalysisExample" );
            var analysis = model!.AnalyzeDataFlow( method.Body! ) ?? throw new Exception( "Data flow analysis is null" );
            var message = Messages.GetMessage( analysis, method.Body! );
            TestContext.WriteLine( message );
        }


    }
}
