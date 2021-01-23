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
    using Microsoft.CodeAnalysis.Text;

    public static class RoslynTestingMessages {


        public static string GetMessage_AnalysisResult(Project project, DiagnosticAnalyzer[] analyzers, Diagnostic[] diagnostics) {
            var builder = new StringBuilder();
            builder.AppendLineFormat( "Project: {0}", project.Name );
            builder.AppendLineFormat( "Documents: {0}", project.Documents.Select( i => i.Name ).Join() );
            builder.AppendLineFormat( "Analyzers: {0}", analyzers.Select( i => i.GetType().Name ).Join() );
            foreach (var diagnostic in diagnostics) {
                builder.AppendObject( diagnostic );
            }
            return builder.ToString();
        }
        public static string GetMessage_FixingResult(Project project, CodeFixProvider fixer, DiagnosticAnalyzer[] analyzers, Diagnostic[] diagnostics, (Project, CodeAction)[] newProjects) {
            var builder = new StringBuilder();
            builder.AppendLineFormat( "Project: {0}", project.Name );
            builder.AppendLineFormat( "Documents: {0}", project.Documents.Select( i => i.Name ).Join() );
            builder.AppendLineFormat( "Fixer: {0}", fixer.GetType().Name );
            builder.AppendLineFormat( "Analyzers: {0}", analyzers.Select( i => i.GetType().Name ).Join() );
            foreach (var diagnostic in diagnostics) {
                builder.AppendObject( diagnostic );
            }
            foreach (var (newProject, action) in newProjects) {
                builder.AppendObject( newProject, project, action );
            }
            return builder.ToString();
        }
        public static string GetMessage_RefactoringResult(Project project, CodeRefactoringProvider refactorer, (Project, CodeAction)[] newProjects) {
            var builder = new StringBuilder();
            builder.AppendLineFormat( "Project: {0}", project.Name );
            builder.AppendLineFormat( "Documents: {0}", project.Documents.Select( i => i.Name ).Join() );
            builder.AppendLineFormat( "Refactorer: {0}", refactorer.GetType().Name );
            foreach (var (newProject, action) in newProjects) {
                builder.AppendObject( newProject, project, action );
            }
            return builder.ToString();
        }
        public static string GetMessage_GenerationResult(Project project, ISourceGenerator generator, GeneratedSourceResult[] sources, Diagnostic[] diagnostics, Exception? exception) {
            var builder = new StringBuilder();
            builder.AppendLineFormat( "Project: {0}", project.Name );
            builder.AppendLineFormat( "Documents: {0}", project.Documents.Select( i => i.Name ).Join() );
            builder.AppendLineFormat( "Generator: {0}", generator.GetType().Name );
            foreach (var source in sources) {
                builder.AppendObject( source );
            }
            foreach (var diagnostic in diagnostics) {
                builder.AppendObject( diagnostic );
            }
            if (exception != null) {
                builder.AppendObject( exception );
            }
            return builder.ToString();
        }


        public static string GetMessage_DataFlowAnalysis(DataFlowAnalysis analysis, BlockSyntax syntax) {
            var builder = new StringBuilder();
            builder.AppendLineFormat( "DataFlowAnalysis: {0}", syntax.Parent!.Kind().ToString() );

            builder.AppendLineFormat( "Definitely Assigned (On Entry): {0}", analysis.DefinitelyAssignedOnEntry.GetDisplayString() );
            builder.AppendLineFormat( "Definitely Assigned (On Exit): {0}", analysis.DefinitelyAssignedOnExit.GetDisplayString() );

            builder.AppendLineFormat( "Declared (Inside): {0}", analysis.VariablesDeclared.GetDisplayString() );
            builder.AppendLineFormat( "Always Assigned (Inside): {0}", analysis.AlwaysAssigned.GetDisplayString() );

            builder.AppendLineFormat( "Written (Outside): {0}", analysis.WrittenOutside.GetDisplayString() );
            builder.AppendLineFormat( "Read (Outside): {0}", analysis.ReadOutside.GetDisplayString() );

            builder.AppendLineFormat( "Written (Inside): {0}", analysis.WrittenInside.GetDisplayString() );
            builder.AppendLineFormat( "Read (Inside): {0}", analysis.ReadInside.GetDisplayString() );

            builder.AppendLineFormat( "Data Flows (In): {0}", analysis.DataFlowsIn.GetDisplayString() );
            builder.AppendLineFormat( "Data Flows (Out): {0}", analysis.DataFlowsOut.GetDisplayString() );

            builder.AppendLineFormat( "Captured: {0}", analysis.Captured.GetDisplayString() );
            builder.AppendLineFormat( "Captured (Inside): {0}", analysis.CapturedInside.GetDisplayString() );
            builder.AppendLineFormat( "Captured (Outside): {0}", analysis.CapturedOutside.GetDisplayString() );

            builder.AppendLineFormat( "Unsafe Address Taken: {0}", analysis.UnsafeAddressTaken.GetDisplayString() );
            builder.AppendLineFormat( "Used Local Functions: {0}", analysis.UsedLocalFunctions.GetDisplayString() );
            return builder.ToString();
        }


        // Helpers/StringBuilder
        private static void AppendObject(this StringBuilder builder, Project project, Project oldProject, CodeAction action) {
            builder.AppendObject( project.GetChanges( oldProject ), action );
        }
        private static void AppendObject(this StringBuilder builder, ProjectChanges changes, CodeAction action) {
            builder.AppendLineFormat( "Project: {0} ({1})", changes.NewProject.Name, action.Title );
            foreach (var id in changes.GetAddedDocuments()) {
                var document = changes.NewProject.GetDocument( id );
                builder.AppendLineFormat( "Added document: {0}", document!.Name ).AppendLine( document.GetDisplayString() );
            }
            foreach (var id in changes.GetRemovedDocuments()) {
                var document = changes.NewProject.GetDocument( id );
                builder.AppendLineFormat( "Removed document: {0}", document!.Name );
            }
            foreach (var id in changes.GetChangedDocuments()) {
                var document = changes.NewProject.GetDocument( id );
                builder.AppendLineFormat( "Changed document: {0}", document!.Name ).AppendLine( document.GetDisplayString() );
            }
        }
        private static void AppendObject(this StringBuilder builder, Diagnostic diagnostic) {
            if (diagnostic.Location.IsInSource) {
                var location = diagnostic.Location;
                builder.AppendLineFormat( "Diagnostic ({0}): {1} ({2}{3})", diagnostic.Id, diagnostic.GetMessage(), location.SourceTree.FilePath, location.SourceSpan.ToString() );
            } else {
                builder.AppendLineFormat( "Diagnostic ({0}): {1}", diagnostic.Id, diagnostic.GetMessage() );
            }
        }
        private static void AppendObject(this StringBuilder builder, GeneratedSourceResult source) {
            builder.AppendLineFormat( "Source: {0}", source.HintName ).AppendLine( source.SourceText.GetDisplayString() );
        }
        private static void AppendObject(this StringBuilder builder, Exception exception) {
            builder.AppendLineFormat( "Exception: {0}", exception.ToString() );
        }
        // Helpers/String
        private static string GetDisplayString(this Document document) {
            return document.GetTextAsync().Result.ToString().Indent( "|  " );
        }
        private static string GetDisplayString(this SourceText document) {
            return document.ToString().Indent( "|  " );
        }
        private static string GetDisplayString(this IImmutableList<ISymbol> values) {
            return values.Select( i => i.Name ).Join();
        }
        private static string GetDisplayString(this IImmutableList<IMethodSymbol> values) {
            return values.Select( i => i.Name ).Join();
        }


    }
}
