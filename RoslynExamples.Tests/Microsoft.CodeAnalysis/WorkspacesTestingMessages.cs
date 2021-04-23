namespace Microsoft.CodeAnalysis {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    public static class WorkspacesTestingMessages {


        // Fixing
        public static string GetMessage(CodeFixProvider fixer, Project project, (Project, CodeAction)[] changedProjects) {
            var builder = new HierarchicalStringBuilder();
            using (builder.AppendTitle( "Fixing result:" )) {
                builder.AppendLine( "Fixer: {0}", fixer.GetType().Name );
                foreach (var (changedProject, action) in changedProjects) {
                    builder.AppendObject( action );
                    builder.AppendObject( changedProject.GetChanges( project ) );
                }
            }
            return builder.ToString();
        }
        public static string GetMessage(CodeFixProvider fixer, Project project, DiagnosticAnalyzer[] analyzers, Diagnostic[] diagnostics, (Project, CodeAction)[] changedProjects) {
            var builder = new HierarchicalStringBuilder();
            using (builder.AppendTitle( "Fixing result:" )) {
                builder.AppendLine( "Fixer: {0}", fixer.GetType().Name );
                builder.AppendObject( project );
                builder.AppendObject( analyzers );
                builder.AppendObject( diagnostics );
                foreach (var (changedProject, action) in changedProjects) {
                    builder.AppendObject( action );
                    builder.AppendObject( changedProject.GetChanges( project ) );
                }
            }
            return builder.ToString();
        }


        // Refactoring
        public static string GetMessage(CodeRefactoringProvider refactorer, Project project, (Project, CodeAction)[] changedProjects) {
            var builder = new HierarchicalStringBuilder();
            using (builder.AppendTitle( "Refactoring result:" )) {
                builder.AppendLine( "Refactorer: {0}", refactorer.GetType().Name );
                builder.AppendObject( project );
                foreach (var (changedProject, action) in changedProjects) {
                    builder.AppendObject( action );
                    builder.AppendObject( changedProject.GetChanges( project ) );
                }
            }
            return builder.ToString();
        }


        // Helpers/AppendObject
        private static void AppendObject(this HierarchicalStringBuilder builder, Project project) {
            builder.AppendLine( "Project: {0} ({1})", project.Name, project.Documents.Join( i => i.Name ) );
        }
        private static void AppendObject(this HierarchicalStringBuilder builder, DiagnosticAnalyzer[] analyzers) {
            foreach (var analyzer in analyzers) {
                builder.AppendLine( "Analyzer: {0}", analyzer.GetType().Name );
            }
        }
        private static void AppendObject(this HierarchicalStringBuilder builder, Diagnostic[] diagnostics) {
            foreach (var diagnostic in diagnostics) {
                if (diagnostic.Location.IsInSource) {
                    var location = diagnostic.Location;
                    builder.AppendLine( "Diagnostic: {0}, {1} ({2} {3})", diagnostic.Id, diagnostic.GetMessage(), location.SourceTree.FilePath, location.SourceSpan );
                } else {
                    builder.AppendLine( "Diagnostic: {0}, {1}", diagnostic.Id, diagnostic.GetMessage() );
                }
            }
        }
        private static void AppendObject(this HierarchicalStringBuilder builder, CodeAction action) {
            builder.AppendLine( "Code action: {0}", action.Title );
        }
        private static void AppendObject(this HierarchicalStringBuilder builder, ProjectChanges changes) {
            using (builder.AppendSection( "Project changes:" )) {
                foreach (var id in changes.GetAddedDocuments()) {
                    var document = changes.NewProject.GetDocument( id );
                    builder.AppendLine( "Added document: {0}", document!.Name ).AppendText( document );
                }
                foreach (var id in changes.GetRemovedDocuments()) {
                    var document = changes.OldProject.GetDocument( id );
                    builder.AppendLine( "Removed document: {0}", document!.Name );
                }
                foreach (var id in changes.GetChangedDocuments()) {
                    var document = changes.NewProject.GetDocument( id );
                    builder.AppendLine( "Changed document: {0}", document!.Name ).AppendText( document );
                }
            }
        }
        // Helpers/AppendText
        private static void AppendText(this HierarchicalStringBuilder builder, Document document) {
            var lines = document.GetTextAsync().Result.Lines.Select( i => i.ToString() );
            builder.WithIndent().AppendText( lines );
        }


    }
}
