namespace Microsoft.CodeAnalysis {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
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

    public static class Messages {


        // Analysis
        public static string GetMessage(Compilation compilation, DiagnosticAnalyzer[] analyzers, Diagnostic[] diagnostics) {
            using var builder = new MessageBuilder();
            using (builder.AppendTitle( "Analysis result:" )) {
                builder.AppendObject( compilation );
                builder.AppendObject( analyzers );
                builder.AppendObject( diagnostics );
            }
            return builder.ToString();
        }
        // Fixing
        public static string GetMessage(CodeFixProvider fixer, Project project, DiagnosticAnalyzer[] analyzers, Diagnostic[] diagnostics, (Project, CodeAction)[] newProjects) {
            using var builder = new MessageBuilder();
            using (builder.AppendTitle( "Fixing result:" )) {
                builder.AppendLine( "Fixer: {0}", fixer.GetType().Name );
                builder.AppendObject( project );
                builder.AppendObject( analyzers );
                builder.AppendObject( diagnostics );
                foreach (var (newProject, action) in newProjects) {
                    builder.AppendObject( action );
                    builder.AppendObject( newProject, project );
                }
            }
            return builder.ToString();
        }
        // Refactoring
        public static string GetMessage(CodeRefactoringProvider refactorer, Project project, (Project, CodeAction)[] newProjects) {
            using var builder = new MessageBuilder();
            using (builder.AppendTitle( "Refactoring result:" )) {
                builder.AppendLine( "Refactorer: {0}", refactorer.GetType().Name );
                builder.AppendObject( project );
                foreach (var (newProject, action) in newProjects) {
                    builder.AppendObject( action );
                    builder.AppendObject( newProject, project );
                }
            }
            return builder.ToString();
        }
        // Generation
        public static string GetMessage(ISourceGenerator generator, Project project, GeneratedSourceResult[] sources, Diagnostic[] diagnostics, Exception? exception) {
            using var builder = new MessageBuilder();
            using (builder.AppendTitle( "Generation result:" )) {
                builder.AppendLine( "Generator: {0}", generator.GetType().Name );
                builder.AppendObject( project );
                builder.AppendObject( sources );
                builder.AppendObject( diagnostics );
                builder.AppendObject( exception );
            }
            return builder.ToString();
        }
        public static string GetMessage(ISourceGenerator generator, Compilation compilation, GeneratedSourceResult[] sources, Diagnostic[] diagnostics, Exception? exception) {
            using var builder = new MessageBuilder();
            using (builder.AppendTitle( "Generation result:" )) {
                builder.AppendLine( "Generator: {0}", generator.GetType().Name );
                builder.AppendObject( compilation );
                builder.AppendObject( sources );
                builder.AppendObject( diagnostics );
                builder.AppendObject( exception );
            }
            return builder.ToString();
        }


        // ControlFlowGraph
        public static string GetMessage(ControlFlowGraph graph) {
            using var builder = new MessageBuilder();
            using (builder.AppendTitle( "Control flow graph:" )) {
                builder.AppendProperty( "Original operation", graph.OriginalOperation ).Separate();
                builder.AppendProperty( "Root region", graph.Root ).Separate();
                foreach (var block in graph.Blocks) {
                    builder.AppendProperty( "Block", block ).Separate();
                }
            }
            return builder.ToString();
        }
        // BasicBlock
        private static MessageBuilder AppendProperty(this MessageBuilder builder, string name, BasicBlock block) {
            using (builder.AppendSection( "{0}: Ordinal={1}, Kind={2}, Condition={3}, IsReachable={4}", name, block.Ordinal, block.Kind, block.ConditionKind, block.IsReachable )) {
                builder.AppendProperty( "Fall through successor", block.FallThroughSuccessor );
                builder.AppendProperty( "Conditional successor", block.ConditionalSuccessor );
                builder.AppendProperty( "BranchValue", block.BranchValue );
                foreach (var operation in block.Operations) {
                    builder.AppendProperty( "Operation", operation );
                }
            }
            return builder;
        }
        // ControlFlowRegion
        private static MessageBuilder AppendProperty(this MessageBuilder builder, string name, ControlFlowRegion region) {
            using (builder.AppendSection( "{0}: Kind={1}", name, region.Kind )) {
                builder.AppendLine( "Capture ids: {0}", region.CaptureIds.Select( i => i.ToString()! ).Join() );
                builder.AppendLine( "Locals: {0}", region.Locals.Select( i => i.Name ).Join() );
                builder.AppendLine( "Local functions: {0}", region.LocalFunctions.Select( i => i.Name ).Join() );
                foreach (var nestedRegion in region.NestedRegions) {
                    builder.AppendProperty( "Nested region", nestedRegion );
                }
            }
            return builder;
        }
        // ControlFlowBranch
        private static MessageBuilder AppendProperty(this MessageBuilder builder, string name, ControlFlowBranch? branch) {
            if (branch == null) return builder;
            builder.AppendLine( "{0}: Semantics={1}, Destination={2}", name, branch.Semantics, branch.Destination?.Ordinal );
            return builder;
        }
        // IOperation
        private static MessageBuilder AppendProperty(this MessageBuilder builder, string name, IOperation? operation) {
            if (operation == null) return builder;
            using (builder.AppendSection( "{0}: Kind={1}", name, operation.Kind )) {
                builder.AppendText( operation.Syntax.WithoutTrivia().GetDisplayString() );
            }
            return builder;
        }


        // ControlFlowAnalysis
        public static string GetMessage(ControlFlowAnalysis analysis, BlockSyntax syntax) {
            using var builder = new MessageBuilder();
            using (builder.AppendTitle( "Control flow analysis: {0}", syntax.Parent!.Kind() )) {
                builder.AppendLine( "Start point is reachable: {0}", analysis.StartPointIsReachable );
                builder.AppendLine( "End point is reachable: {0}", analysis.EndPointIsReachable );

                builder.AppendLine( "Entry points: {0}", analysis.EntryPoints.GetDisplayString() );
                builder.AppendLine( "Exit points: {0}", analysis.ExitPoints.GetDisplayString() );

                builder.AppendLine( "Return statements: {0}", analysis.ReturnStatements.GetDisplayString() );
            }
            return builder.ToString();
        }


        // DataFlowAnalysis
        public static string GetMessage(DataFlowAnalysis analysis, BlockSyntax syntax) {
            using var builder = new MessageBuilder();
            using (builder.AppendTitle( "Data flow analysis: {0}", syntax.Parent!.Kind() )) {
                builder.AppendLine( "Definitely assigned (On entry): {0}", analysis.DefinitelyAssignedOnEntry.GetDisplayString() );
                builder.AppendLine( "Definitely assigned (On exit): {0}", analysis.DefinitelyAssignedOnExit.GetDisplayString() );

                builder.AppendLine( "Declared (Inside): {0}", analysis.VariablesDeclared.GetDisplayString() );
                builder.AppendLine( "Always assigned (Inside): {0}", analysis.AlwaysAssigned.GetDisplayString() );

                builder.AppendLine( "Written (Outside): {0}", analysis.WrittenOutside.GetDisplayString() );
                builder.AppendLine( "Read (Outside): {0}", analysis.ReadOutside.GetDisplayString() );

                builder.AppendLine( "Written (Inside): {0}", analysis.WrittenInside.GetDisplayString() );
                builder.AppendLine( "Read (Inside): {0}", analysis.ReadInside.GetDisplayString() );

                builder.AppendLine( "Data flows (In): {0}", analysis.DataFlowsIn.GetDisplayString() );
                builder.AppendLine( "Data flows (Out): {0}", analysis.DataFlowsOut.GetDisplayString() );

                builder.AppendLine( "Captured: {0}", analysis.Captured.GetDisplayString() );
                builder.AppendLine( "Captured (Inside): {0}", analysis.CapturedInside.GetDisplayString() );
                builder.AppendLine( "Captured (Outside): {0}", analysis.CapturedOutside.GetDisplayString() );

                builder.AppendLine( "Unsafe address taken: {0}", analysis.UnsafeAddressTaken.GetDisplayString() );
                builder.AppendLine( "Used local functions: {0}", analysis.UsedLocalFunctions.GetDisplayString() );
            }
            return builder.ToString();
        }


        // Helpers/AppendObject
        private static void AppendObject(this MessageBuilder builder, Project project) {
            builder.AppendLine( "Project: {0} ({1})", project.Name, project.Documents.Select( i => i.Name ).Join() );
        }
        private static void AppendObject(this MessageBuilder builder, Compilation compilation) {
            builder.AppendLine( "Compilation: {0} ({1})", compilation.AssemblyName, compilation.SyntaxTrees.Select( i => Path.GetFileName( i.FilePath ) ).Join() );
        }
        private static void AppendObject(this MessageBuilder builder, DiagnosticAnalyzer[] analyzers) {
            foreach (var analyzer in analyzers) {
                builder.AppendLine( "Analyzer: {0}", analyzer.GetType().Name );
            }
        }
        private static void AppendObject(this MessageBuilder builder, Diagnostic[] diagnostics) {
            foreach (var diagnostic in diagnostics) {
                if (diagnostic.Location.IsInSource) {
                    var location = diagnostic.Location;
                    builder.AppendLine( "Diagnostic: {0}, {1} ({2} {3})", diagnostic.Id, diagnostic.GetMessage(), location.SourceTree.FilePath, location.SourceSpan );
                } else {
                    builder.AppendLine( "Diagnostic: {0}, {1}", diagnostic.Id, diagnostic.GetMessage() );
                }
            }
        }
        private static void AppendObject(this MessageBuilder builder, CodeAction action) {
            builder.AppendLine( "Code action: {0}", action.Title );
        }
        private static void AppendObject(this MessageBuilder builder, Project newProject, Project oldProject) {
            using (builder.AppendSection( "Project changes:" )) {
                var changes = newProject.GetChanges( oldProject );
                foreach (var id in changes.GetAddedDocuments()) {
                    var document = changes.NewProject.GetDocument( id );
                    using (builder.AppendSection( "Added document: {0}", document!.Name )) {
                        builder.AppendText( document.GetDisplayString() );
                    }
                }
                foreach (var id in changes.GetRemovedDocuments()) {
                    var document = changes.OldProject.GetDocument( id );
                    builder.AppendLine( "Removed document: {0}", document!.Name );
                }
                foreach (var id in changes.GetChangedDocuments()) {
                    var document = changes.NewProject.GetDocument( id );
                    using (builder.AppendSection( "Changed document: {0}", document!.Name )) {
                        builder.AppendText( document.GetDisplayString() );
                    }
                }
            }
        }
        private static void AppendObject(this MessageBuilder builder, GeneratedSourceResult[] sources) {
            foreach (var source in sources) {
                using (builder.AppendSection( "Source: {0}", source.HintName )) {
                    builder.AppendText( source.SourceText.GetDisplayString() );
                }
            }
        }
        private static void AppendObject(this MessageBuilder builder, Exception? exception) {
            if (exception != null) {
                builder.AppendLine( "Exception: {0}", exception );
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
