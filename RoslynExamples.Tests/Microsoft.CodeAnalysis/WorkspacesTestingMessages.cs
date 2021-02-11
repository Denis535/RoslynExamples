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
        public static string GetMessage(CodeFixProvider fixer, Project project, DiagnosticAnalyzer[] analyzers, Diagnostic[] diagnostics, (Project, CodeAction)[] changedProjects) {
            using var builder = new MessageBuilder();
            using (builder.AppendTitle( "Fixing result:" )) {
                builder.AppendLine( "Fixer: {0}", fixer.GetType().Name );
                builder.AppendObject( project );
                builder.AppendObject( analyzers );
                builder.AppendObject( diagnostics );
                foreach (var (changedProject, action) in changedProjects) {
                    builder.AppendObject( action );
                    builder.AppendObject( changedProject, project );
                }
            }
            return builder.ToString();
        }
        // Refactoring
        public static string GetMessage(CodeRefactoringProvider refactorer, Project project, (Project, CodeAction)[] changedProjects) {
            using var builder = new MessageBuilder();
            using (builder.AppendTitle( "Refactoring result:" )) {
                builder.AppendLine( "Refactorer: {0}", refactorer.GetType().Name );
                builder.AppendObject( project );
                foreach (var (changedProject, action) in changedProjects) {
                    builder.AppendObject( action );
                    builder.AppendObject( changedProject, project );
                }
            }
            return builder.ToString();
        }


        // Helpers/AppendObject
        private static void AppendObject(this MessageBuilder builder, Project project) {
            builder.AppendLine( "Project: {0} ({1})", project.Name, project.Documents.Select( i => i.Name ).Join() );
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
        // Helpers/GetDisplayString
        private static IEnumerable<string> GetDisplayString(this Document value) {
            return value.GetTextAsync().Result.Lines.Select( i => i.ToString() );
        }


    }
}
