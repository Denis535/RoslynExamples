namespace Microsoft.CodeAnalysis {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.FlowAnalysis;
    using Microsoft.CodeAnalysis.Text;

    public static class RoslynTestingMessages {


        // Analysis
        public static string GetMessage(Project project, DiagnosticAnalyzer[] analyzers, Diagnostic[] diagnostics) {
            var hierarchy = new List<object>();
            hierarchy.AppendLine( "Analysis result" );
            {
                hierarchy.Children().AppendObject( project );
                hierarchy.Children().AppendObject( analyzers );
                hierarchy.Children().AppendObject( diagnostics );
            }
            return hierarchy.Build();
        }
        // Fixing
        public static string GetMessage(CodeFixProvider fixer, Project project, DiagnosticAnalyzer[] analyzers, Diagnostic[] diagnostics, (Project, CodeAction)[] newProjects) {
            var hierarchy = new List<object>();
            hierarchy.AppendLine( "Fixing result" );
            {
                hierarchy.Children().AppendLine( "Fixer: {0}", fixer.GetType().Name );
                hierarchy.Children().AppendObject( project );
                hierarchy.Children().AppendObject( analyzers );
                hierarchy.Children().AppendObject( diagnostics );
                foreach (var (newProject, action) in newProjects) {
                    hierarchy.Children().AppendObject( newProject, project, action );
                }
            }
            return hierarchy.Build();
        }
        // Refactoring
        public static string GetMessage(CodeRefactoringProvider refactorer, Project project, (Project, CodeAction)[] newProjects) {
            var hierarchy = new List<object>();
            hierarchy.AppendLine( "Refactoring result" );
            {
                hierarchy.Children().AppendLine( "Refactorer: {0}", refactorer.GetType().Name );
                hierarchy.Children().AppendObject( project );
                foreach (var (newProject, action) in newProjects) {
                    hierarchy.Children().AppendObject( newProject, project, action );
                }
            }
            return hierarchy.Build();
        }
        // Generation
        public static string GetMessage(ISourceGenerator generator, Project project, GeneratedSourceResult[] sources, Diagnostic[] diagnostics, Exception? exception) {
            var hierarchy = new List<object>();
            hierarchy.AppendLine( "Generation result" );
            {
                hierarchy.Children().AppendLine( "Generator: {0}", generator.GetType().Name );
                hierarchy.Children().AppendObject( project );
                hierarchy.Children().AppendObject( sources );
                hierarchy.Children().AppendObject( diagnostics );
                hierarchy.Children().AppendObject( exception );
            }
            return hierarchy.Build();
        }


        // ControlFlowGraph
        public static string GetMessage(ControlFlowGraph graph) {
            var hierarchy = new List<object>();
            hierarchy.AppendLine( "ControlFlowGraph:" );
            {
                hierarchy.Children().AppendObject( "Operation:", graph.OriginalOperation );
                hierarchy.Children().AppendObject( "Root:", graph.Root );
                foreach (var item in graph.Blocks) {
                    hierarchy.Children().AppendObject( "Block:", item );
                }
            }
            return hierarchy.Build();
        }
        private static IList<object> AppendObject(this IList<object> hierarchy, string title, IOperation operation) {
            hierarchy.AppendLine( title );
            hierarchy.Children().AppendLine( "Kind: {0}", operation.Kind );
            hierarchy.Children().AppendLine( "Syntax:" ).Children().AppendText( operation.Syntax.GetDisplayString() );
            return hierarchy;
        }
        private static IList<object> AppendObject(this IList<object> hierarchy, string title, ControlFlowRegion region) {
            hierarchy.AppendLine( title );
            hierarchy.Children().AppendLine( "Kind: {0}", region.Kind );
            hierarchy.Children().AppendLine( "Locals: {0}", region.Locals.Select( i => i.Name ).Join() );
            hierarchy.Children().AppendLine( "LocalFunctions: {0}", region.LocalFunctions.Select( i => i.Name ).Join() );
            foreach (var item in region.NestedRegions) {
                hierarchy.Children().AppendObject( "Nested Region:", item );
            }
            return hierarchy;
        }
        private static IList<object> AppendObject(this IList<object> hierarchy, string title, BasicBlock block) {
            hierarchy.AppendLine( title );
            hierarchy.Children().AppendLine( "Kind: {0}, {1}", block.Kind, block.ConditionKind );
            hierarchy.Children().AppendLine( "IsReachable: {0}", block.IsReachable );
            return hierarchy;
        }


        // ControlFlowAnalysis
        public static string GetMessage(ControlFlowAnalysis analysis, BlockSyntax syntax) {
            var hierarchy = new List<object>();
            hierarchy.AppendLine( "ControlFlowAnalysis: {0}:", syntax.Parent!.Kind() );
            {
                hierarchy.Children()
                    .AppendLine( "Start Point Is Reachable: {0}", analysis.StartPointIsReachable )
                    .AppendLine( "End Point Is Reachable: {0}", analysis.EndPointIsReachable )

                    .AppendLine( "Entry Points: {0}", analysis.EntryPoints.GetDisplayString() )
                    .AppendLine( "Exit Points: {0}", analysis.ExitPoints.GetDisplayString() )

                    .AppendLine( "Return Statements: {0}", analysis.ReturnStatements.GetDisplayString() );
            }
            return hierarchy.Build();
        }


        // DataFlowAnalysis
        public static string GetMessage(DataFlowAnalysis analysis, BlockSyntax syntax) {
            var hierarchy = new List<object>();
            hierarchy.AppendLine( "DataFlowAnalysis: {0}:", syntax.Parent!.Kind() );
            {
                hierarchy.Children()
                    .AppendLine( "Definitely Assigned (On Entry): {0}", analysis.DefinitelyAssignedOnEntry.GetDisplayString() )
                    .AppendLine( "Definitely Assigned (On Exit): {0}", analysis.DefinitelyAssignedOnExit.GetDisplayString() )

                    .AppendLine( "Declared (Inside): {0}", analysis.VariablesDeclared.GetDisplayString() )
                    .AppendLine( "Always Assigned (Inside): {0}", analysis.AlwaysAssigned.GetDisplayString() )

                    .AppendLine( "Written (Outside): {0}", analysis.WrittenOutside.GetDisplayString() )
                    .AppendLine( "Read (Outside): {0}", analysis.ReadOutside.GetDisplayString() )

                    .AppendLine( "Written (Inside): {0}", analysis.WrittenInside.GetDisplayString() )
                    .AppendLine( "Read (Inside): {0}", analysis.ReadInside.GetDisplayString() )

                    .AppendLine( "Data Flows (In): {0}", analysis.DataFlowsIn.GetDisplayString() )
                    .AppendLine( "Data Flows (Out): {0}", analysis.DataFlowsOut.GetDisplayString() )

                    .AppendLine( "Captured: {0}", analysis.Captured.GetDisplayString() )
                    .AppendLine( "Captured (Inside): {0}", analysis.CapturedInside.GetDisplayString() )
                    .AppendLine( "Captured (Outside): {0}", analysis.CapturedOutside.GetDisplayString() )

                    .AppendLine( "Unsafe Address Taken: {0}", analysis.UnsafeAddressTaken.GetDisplayString() )
                    .AppendLine( "Used Local Functions: {0}", analysis.UsedLocalFunctions.GetDisplayString() );
            }
            return hierarchy.Build();
        }


        // Helpers/AppendObject
        private static void AppendObject(this IList<object> hierarchy, Project project) {
            hierarchy.AppendLine( "Project: {0} ({1})", project.Name, project.Documents.Select( i => i.Name ).Join() );
        }
        private static void AppendObject(this IList<object> hierarchy, DiagnosticAnalyzer[] analyzers) {
            foreach (var analyzer in analyzers) {
                hierarchy.AppendLine( "Analyzer: {0}", analyzer.GetType().Name );
            }
        }
        private static void AppendObject(this IList<object> hierarchy, Diagnostic[] diagnostics) {
            foreach (var diagnostic in diagnostics) {
                if (diagnostic.Location.IsInSource) {
                    var location = diagnostic.Location;
                    hierarchy.AppendLine( "Diagnostic: {0}, {1} ({2}{3})", diagnostic.Id, diagnostic.GetMessage(), location.SourceTree.FilePath, location.SourceSpan );
                } else {
                    hierarchy.AppendLine( "Diagnostic: {0}, {1}", diagnostic.Id, diagnostic.GetMessage() );
                }
            }
        }
        private static void AppendObject(this IList<object> hierarchy, GeneratedSourceResult[] sources) {
            foreach (var source in sources) {
                hierarchy.AppendLine( "Source: {0}", source.HintName ).Children().AppendText( source.SourceText.GetDisplayString() );
            }
        }
        private static void AppendObject(this IList<object> hierarchy, Exception? exception) {
            if (exception != null) {
                hierarchy.AppendLine( "Exception: {0}", exception );
            }
        }
        private static void AppendObject(this IList<object> hierarchy, Project newProject, Project oldProject, CodeAction action) {
            hierarchy.AppendLine( "New project: {0}", newProject.Name );
            {
                var changes = newProject.GetChanges( oldProject );
                hierarchy.Children().AppendLine( "Code action: {0}", action.Title );
                foreach (var id in changes.GetAddedDocuments()) {
                    var document = changes.NewProject.GetDocument( id );
                    hierarchy.Children().AppendLine( "Added document: {0}", document!.Name ).Children().AppendText( document.GetDisplayString() );
                }
                foreach (var id in changes.GetRemovedDocuments()) {
                    var document = changes.NewProject.GetDocument( id );
                    hierarchy.Children().AppendLine( "Removed document: {0}", document!.Name );
                }
                foreach (var id in changes.GetChangedDocuments()) {
                    var document = changes.NewProject.GetDocument( id );
                    hierarchy.Children().AppendLine( "Changed document: {0}", document!.Name ).Children().AppendText( document.GetDisplayString() );
                }
            }
        }
        // Helpers/GetDisplayString
        private static IEnumerable<string> GetDisplayString(this Document value) {
            return value.GetTextAsync().Result.Lines.Select( i => i.ToString() );
        }
        private static IEnumerable<string> GetDisplayString(this SyntaxNode value) {
            return value.GetText().Lines.Select( i => i.ToString() );
        }
        private static IEnumerable<string> GetDisplayString(this SourceText value) {
            return value.Lines.Select( i => i.ToString() );
        }
        private static string GetDisplayString(this IImmutableList<SyntaxNode> values) {
            return values.Select( i => i.Kind().ToString() ).Join();
        }
        private static string GetDisplayString(this IImmutableList<ISymbol> values) {
            return values.Select( i => i.Name ).Join();
        }
        private static string GetDisplayString(this IImmutableList<IMethodSymbol> values) {
            return values.Select( i => i.Name ).Join();
        }


    }
}
