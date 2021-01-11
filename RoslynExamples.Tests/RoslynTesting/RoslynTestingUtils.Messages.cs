namespace RoslynTesting {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    public static partial class RoslynTestingUtils {
        public static class Messages {


            public static string GetMessage_AnalysisResult(Project project, DiagnosticAnalyzer[] analyzers, Diagnostic[] diagnostics) {
                var builder = new StringBuilder();
                builder.AppendLineFormat( "Project: {0}", project.Name );
                builder.AppendLineFormat( "Documents: {0}", project.Documents.Select( i => i.Name ).Join() );
                builder.AppendLineFormat( "Analyzers: {0}", analyzers.Select( i => i.GetType().Name ).Join() );
                foreach (var diagnostic in diagnostics) {
                    builder.AppendLineFormat( GetDisplayString( diagnostic ) );
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
                    builder.AppendLineFormat( GetDisplayString( diagnostic ) );
                }
                foreach (var (newProject, action) in newProjects) {
                    builder.AppendLineFormat( "New project: {0} ({1})", newProject.Name, action.Title );
                    foreach (var newDocument in newProject.Documents) {
                        builder.AppendLineFormat( "New document: {0}", newDocument.Name ).AppendLine( newDocument.GetTextAsync().Result.ToString().Indent( 4 ) );
                    }
                }
                return builder.ToString();
            }
            public static string GetMessage_RefactoringResult(Project project, CodeRefactoringProvider refactorer, (Project, CodeAction)[] newProjects) {
                var builder = new StringBuilder();
                builder.AppendLineFormat( "Project: {0}", project.Name );
                builder.AppendLineFormat( "Documents: {0}", project.Documents.Select( i => i.Name ).Join() );
                builder.AppendLineFormat( "Refactorer: {0}", refactorer.GetType().Name );
                foreach (var (newProject, action) in newProjects) {
                    builder.AppendLineFormat( "New project: {0} ({1})", newProject.Name, action.Title );
                    foreach (var newDocument in newProject.Documents) {
                        builder.AppendLineFormat( "New document: {0}", newDocument.Name ).AppendLine( newDocument.GetTextAsync().Result.ToString().Indent( 4 ) );
                    }
                }
                return builder.ToString();
            }
            public static string GetMessage_GenerationResult(Project project, ISourceGenerator generator, GeneratedSourceResult[] sources, Diagnostic[] diagnostics, Exception? exception) {
                var builder = new StringBuilder();
                builder.AppendLineFormat( "Project: {0}", project.Name );
                builder.AppendLineFormat( "Documents: {0}", project.Documents.Select( i => i.Name ).Join() );
                builder.AppendLineFormat( "Generator: {0}", generator.GetType().Name );
                foreach (var source in sources) {
                    builder.AppendLineFormat( "Source: {0}", source.HintName ).AppendLine( source.SourceText.ToString().Indent( 4 ) );
                }
                foreach (var diagnostic in diagnostics) {
                    builder.AppendLineFormat( GetDisplayString( diagnostic ) );
                }
                if (exception != null) {
                    builder.AppendLineFormat( "Exception: {0}", exception.ToString() );
                }
                return builder.ToString();
            }


            // Helpers
            private static string GetDisplayString(Diagnostic diagnostic) {
                if (diagnostic.Location.IsInSource) {
                    var location = diagnostic.Location;
                    return string.Format( "Diagnostic ({0}): {1} ({2}{3})", diagnostic.Id, diagnostic.GetMessage(), location.SourceTree.FilePath, location.SourceSpan );
                } else {
                    return string.Format( "Diagnostic ({0}): {1}", diagnostic.Id, diagnostic.GetMessage() );
                }
            }


        }
    }
}
