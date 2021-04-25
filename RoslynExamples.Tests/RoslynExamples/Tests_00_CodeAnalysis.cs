namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.IO;
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


        [SetUp]
        public void SetUp() {
            Trace.Listeners.Add( new TextWriterTraceListener( TestContext.Out ) );
            Directory.SetCurrentDirectory( TestContext.CurrentContext.TestDirectory );
            Directory.SetCurrentDirectory( "../../../../RoslynExamples.Tests.Data/RoslynExamples.Tests.Data/" );
        }
        [TearDown]
        public void TearDown() {
        }


        // DiagnosticAnalysis
        [Test]
        public async Task Test_00_DiagnosticAnalysis() {
            var compilation = CodeAnalysisTestingUtils.CreateFakeCompilation().LoadDocuments( "TestData_DiagnosticAnalysis.cs" );
            var analyzers = new DiagnosticAnalyzer[] { new ExampleAnalyzer0000(), new ExampleAnalyzer0001(), new ExampleAnalyzer0002() };

            var diagnostics = await CodeAnalysisTestingUtils.AnalyzeAsync( compilation, analyzers, null, default ).ConfigureAwait( false );
            var message = CodeAnalysisTestingMessages.GetMessage( compilation, analyzers, diagnostics );
            TestContext.WriteLine( message );
        }


        // SourceGeneration
        [Test]
        public async Task Test_01_SourceGeneration() {
            var compilation = CodeAnalysisTestingUtils.CreateFakeCompilation().LoadDocuments( "TestData_SourceGeneration.cs" );
            var generator = new ExampleSourceGenerator();

            var generation = await CodeAnalysisTestingUtils.GenerateAsync( generator, compilation, default ).ConfigureAwait( false );
            var message = CodeAnalysisTestingMessages.GetMessage( generation.Generator, compilation, generation.GeneratedSources.ToArray(), generation.Diagnostics.ToArray(), generation.Exception );
            TestContext.WriteLine( message );
            foreach (var diagnostic in generation.Diagnostics) {
                Assert.Fail( diagnostic.ToString() );
            }
            if (generation.Exception != null) {
                Assert.Fail( generation.Exception.ToString() );
            }
        }


        // ControlFlowGraph
        [Test]
        public void Test_02_ControlFlowGraph() {
            var compilation = CodeAnalysisTestingUtils.CreateFakeCompilation().LoadDocuments( "TestData_ControlFlowGraph.cs" );
            var (method, model) = compilation.FindDocument().FindMethod( "ControlFlowGraphExample" );

            var graph = ControlFlowGraph.Create( method, model ) ?? throw new Exception( "Control flow graph is null" );
            var message = RoslynDisplayUtils.GetDisplayString( graph );
            TestContext.WriteLine( message );
        }


        // ControlFlowAnalysis
        [Test]
        public void Test_03_ControlFlowAnalysis() {
            var compilation = CodeAnalysisTestingUtils.CreateFakeCompilation().LoadDocuments( "TestData_ControlFlowGraph.cs" );
            var (method, model) = compilation.FindDocument().FindMethod( "ControlFlowAnalysisExample" );

            var analysis = model!.AnalyzeControlFlow( method.Body! ) ?? throw new Exception( "Control flow analysis is null" );
            var message = RoslynDisplayUtils.GetDisplayString( method.Body!, analysis );
            TestContext.WriteLine( message );
        }


        // DataFlowAnalysis
        [Test]
        public void Test_04_DataFlowAnalysis() {
            var compilation = CodeAnalysisTestingUtils.CreateFakeCompilation().LoadDocuments( "TestData_ControlFlowGraph.cs" );
            var (method, model) = compilation.FindDocument().FindMethod( "DataFlowAnalysisExample" );

            var analysis = model!.AnalyzeDataFlow( method.Body! ) ?? throw new Exception( "Data flow analysis is null" );
            var message = RoslynDisplayUtils.GetDisplayString( method.Body!, analysis );
            TestContext.WriteLine( message );
        }


        // DependenciesAnalysis
        [Test]
        public void Test_05_DependenciesAnalysis_Analyze() {
            var compilation = CodeAnalysisTestingUtils.CreateFakeCompilation().LoadDocuments( "TestData_DependenciesAnalysis.cs" );
            var (root, model) = compilation.FindDocument();

            var analysis = DependenciesAnalyzer.Analyze( root, model );
            TestContext.WriteLine( RoslynDisplayUtils.GetDisplayString( root, analysis ) );
        }
        [Test]
        public void Test_05_DependenciesAnalysis_Deconstruct() {
            var compilation = CodeAnalysisTestingUtils.CreateFakeCompilation();

            var simpleType = compilation.GetSpecialType( SpecialType.System_Object );
            var arrayType = compilation.CreateArrayTypeSymbol( compilation.GetSpecialType( SpecialType.System_Object ) );

            var genericType_Unbound = compilation.GetSpecialType( SpecialType.System_Collections_Generic_IList_T ).ConstructUnboundGenericType();
            var genericType_Original = compilation.GetSpecialType( SpecialType.System_Collections_Generic_IList_T );
            var genericType_Constructed = compilation.GetSpecialType( SpecialType.System_Collections_Generic_IList_T ).Construct(
                compilation.CreateArrayTypeSymbol( compilation.GetSpecialType( SpecialType.System_Nullable_T ).Construct( compilation.GetSpecialType( SpecialType.System_Int32 ) ) )
                );

            var pointerType = compilation.CreatePointerTypeSymbol(
                compilation.GetSpecialType( SpecialType.System_Int32 )
                );
            var funcPointerType = compilation.CreateFunctionPointerTypeSymbol(
                compilation.GetSpecialType( SpecialType.System_Int32 ),
                RefKind.None,
                ImmutableArray.Create<ITypeSymbol>( compilation.GetSpecialType( SpecialType.System_Int32 ) ),
                ImmutableArray.Create( RefKind.None )
                );


            TestContext.WriteLine( GetDisplayString( simpleType, DependenciesAnalysis.Reference.Deconstruct( simpleType ) ) );
            TestContext.WriteLine( GetDisplayString( arrayType, DependenciesAnalysis.Reference.Deconstruct( arrayType ) ) );

            TestContext.WriteLine( GetDisplayString( genericType_Unbound, DependenciesAnalysis.Reference.Deconstruct( genericType_Unbound ) ) );
            TestContext.WriteLine( GetDisplayString( genericType_Original, DependenciesAnalysis.Reference.Deconstruct( genericType_Original ) ) );
            TestContext.WriteLine( GetDisplayString( genericType_Constructed, DependenciesAnalysis.Reference.Deconstruct( genericType_Constructed ) ) );

            TestContext.WriteLine( GetDisplayString( pointerType, DependenciesAnalysis.Reference.Deconstruct( pointerType ) ) );
            TestContext.WriteLine( GetDisplayString( funcPointerType, DependenciesAnalysis.Reference.Deconstruct( funcPointerType ) ) );
        }


        // Helpers
        private static string GetDisplayString(ITypeSymbol type, IEnumerable<ITypeSymbol> types) {
            return string.Format(
                "{0}: {1}",
                RoslynDisplayUtils.GetDisplayString( type ),
                types.Join( RoslynDisplayUtils.GetDisplayString )
                );
        }


    }
}
