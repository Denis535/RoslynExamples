namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Tests {

        private const string ClassSourceCode = @"
    namespace Namespace {
        using System;
        using System.Collections.Generic;
        using System.Text;

        public partial class Class<T> {
            public static partial void Method();
            public static partial void Method2<T1>(T1 argument) where T1 : class, new();
        }

        public partial class Class<T> where T : class, new() {
            public object? Field, Field2;
            public object? Prop { get; set; }
            public object? Prop2 => null;
            public event Action? Event, Event2;
            public event Action? Event3 {
                add => Event += value;
                remove => Event -= value;
            }

            public object? Func() {
                return null;
            }
            public object? Func2()
                => null;

            public static partial void Method() {
                LocalMethod();
                static void LocalMethod() { }
            }
            public static partial void Method2<T1>(T1 argument) where T1 : class, new() {
                var variable = default( object );
            }
        }
    }";

        private Project Project { get; set; } = default!;
        private DiagnosticAnalyzer[] Analyzers { get; set; } = default!;


        [SetUp]
        public void SetUp() {
            Trace.Listeners.Add( new TextWriterTraceListener( TestContext.Out ) );
            Project = RoslynTestingUtils.CreateFakeProject( "Class.cs", ClassSourceCode );
            Analyzers = new DiagnosticAnalyzer[] { new ExampleAnalyzer0000(), new ExampleAnalyzer0001(), new ExampleAnalyzer0002() };
        }
        [TearDown]
        public void TearDown() {
        }


        // Analysis
        [Test]
        public async Task Test_00_Analysis() {
            // Symbols:
            // Namespace,
            // Class, Method, Method2, argument, 
            // Field, Field2, Prop, Prop2, Event, Event2, Event3, Func, Func2, Method, Method2, argument
            var diagnostics = await RoslynTestingUtils.AnalyzeAsync( Project, Analyzers, default ).ConfigureAwait( false );

            foreach (var diagnostic in diagnostics) {
                TestContext.WriteLine( "Diagnostic: " + diagnostic );
            }
            Assert.That( diagnostics, Has.Length.EqualTo( 17 + 17 + 1 ) );
        }


        // Analysis && fixing
        [Test]
        public async Task Test_01_Analysis_And_Fixing() {
            var diagnostics = await RoslynTestingUtils.AnalyzeAsync( Project, Analyzers, default ).ConfigureAwait( false );
            diagnostics = diagnostics.Where( i => i.Location != Location.None ).ToArray();

            foreach (var diagnostic in diagnostics) {
                TestContext.WriteLine( "Diagnostic: " + diagnostic );
                var fixer = new ExampleCodeFixProvider();
                var changedProjects = await RoslynTestingUtils.Fix( Project, fixer, diagnostic, default ).ConfigureAwait( false );
                foreach (var newProject in changedProjects) {
                    var originalDocument = Project.Documents.First();
                    var changedDocument = newProject.Documents.First();
                    //TestContext.WriteLine( "Document: " + changedDocument.Name );
                    //TestContext.WriteLine( changedDocument.GetTextAsync().Result );
                }
            }
        }


        // Analysis && fixing
        //[Test]
        //public async Task Test_02_Analysis_And_Fixing() {
        //    var analyzers = new DiagnosticAnalyzer[] { new ExampleAnalyzer0001(), new ExampleAnalyzer0002() };
        //    var diagnostics = await Analyze( Project, analyzers, default ).ConfigureAwait( false );
        //    diagnostics = diagnostics.Distinct().Where( i => i.Location != Location.None ).ToArray();

        //    var fixer = new ExampleCodeFixProvider();
        //    foreach (var diagnostic in diagnostics) {
        //        TestContext.WriteLine( "Diagnostic: " + diagnostic );
        //        var document = Project.GetDocument( diagnostic.Location.SourceTree );

        //        //var context = new FixAllContext( document, fixer, FixAllScope.Solution, null, diagnostics, new FixAllContext.DiagnosticProvider(), default );
        //    }
        //}


        // Refactoring
        [Test]
        public async Task Test_03_Refactoring() {
            var refactorer = new ExampleCodeRefactoringProvider();
            var changedProjects = await RoslynTestingUtils.Refactor( Project, refactorer, default ).ConfigureAwait( false );
            foreach (var newProject in changedProjects) {
                var originalDocument = Project.Documents.First();
                var changedDocument = newProject.Documents.First();
                //TestContext.WriteLine( "Document: " + changedDocument.Name );
                //TestContext.WriteLine( changedDocument.GetTextAsync().Result );
            }
        }


    }
}