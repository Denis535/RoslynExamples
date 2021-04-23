namespace Microsoft.CodeAnalysis {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    public static class CodeAnalysisTestingMessages {


        // Analysis
        public static string GetMessage(Compilation compilation, DiagnosticAnalyzer[] analyzers, Diagnostic[] diagnostics) {
            var builder = new HierarchicalStringBuilder();
            using (builder.AppendTitle( "Analysis result:" )) {
                builder.AppendObject( compilation );
                builder.AppendObject( analyzers );
                builder.AppendObject( diagnostics );
            }
            return builder.ToString();
        }


        // Generation
        public static string GetMessage(ISourceGenerator generator, Compilation compilation, GeneratedSourceResult[] sources, Diagnostic[] diagnostics, Exception? exception) {
            var builder = new HierarchicalStringBuilder();
            using (builder.AppendTitle( "Generation result:" )) {
                builder.AppendLine( "Generator: {0}", generator.GetType().Name );
                builder.AppendObject( compilation );
                builder.AppendObject( diagnostics );
                builder.AppendObject( exception );
                builder.AppendObject( sources );
            }
            return builder.ToString();
        }


        // Helpers/AppendObject
        private static void AppendObject(this HierarchicalStringBuilder builder, Compilation compilation) {
            builder.AppendLine( "Compilation: {0} ({1})", compilation.AssemblyName, compilation.SyntaxTrees.Join( i => Path.GetFileName( i.FilePath ) ) );
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
        private static void AppendObject(this HierarchicalStringBuilder builder, Exception? exception) {
            if (exception != null) {
                builder.AppendLine( "Exception: {0}", exception );
            }
        }
        private static void AppendObject(this HierarchicalStringBuilder builder, GeneratedSourceResult[] sources) {
            foreach (var source in sources) {
                builder.AppendLine( "Source: {0}", source.HintName ).AppendText( source.SourceText );
            }
        }
        // Helpers/AppendText
        private static void AppendText(this HierarchicalStringBuilder builder, SourceText text) {
            var lines = text.Lines.Select( i => i.ToString() );
            builder.WithIndent().AppendText( lines );
        }


    }
}
