#pragma warning disable RS1013
#pragma warning disable RS1026
#pragma warning disable RS2008 // Enable analyzer release tracking

namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public class ExampleAnalyzer0002 : DiagnosticAnalyzer {

        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "ExampleAnalyzer0002",
            "ExampleAnalyzer0002",
            "Compilation '{0}' must start/end with underscore",
            "Example",
            DiagnosticSeverity.Warning,
            true );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create( Rule );


        public override void Initialize(AnalysisContext context) {
            //Trace.WriteLine( GetType().Name );
            context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.None );
            context.EnableConcurrentExecution();

            // Compilation
            context.RegisterCompilationStartAction( OnCompilation_Start );
            context.RegisterCompilationAction( OnCompilation );
            // SyntaxTree, SemanticModel
            context.RegisterSyntaxTreeAction( OnSyntaxTree );
            context.RegisterSemanticModelAction( OnSemanticModel );
            // Symbol
            context.RegisterSymbolStartAction( OnSymbol_Start, SymbolKind.NamedType );
            context.RegisterSymbolAction( OnSymbol, Utils.GetEnumValues<SymbolKind>() );
            // CodeBlock
            context.RegisterCodeBlockStartAction<SyntaxKind>( OnCodeBlock_Start );
            context.RegisterCodeBlockAction( OnCodeBlock );
            // SyntaxNode
            context.RegisterSyntaxNodeAction( OnSyntaxNode, Utils.GetEnumValues<SyntaxKind>() );
        }


        // Actions/Compilation
        private static void OnCompilation_Start(CompilationStartAnalysisContext context) {
            context.RegisterCompilationEndAction( OnCompilation_End );
        }
        private static void OnCompilation(CompilationAnalysisContext context) {
            // Note: project-level diagnostic (without location) can't have code fixes
            var compilation = context.Compilation;
            //Trace.WriteLine( "OnCompilation: " + context.Compilation.AssemblyName );
            if (!compilation.AssemblyName!.EndsWith( "_" ) || !compilation.AssemblyName!.StartsWith( "_" )) {
                var diagnostic = Diagnostic.Create( Rule, null, compilation.AssemblyName );
                context.ReportDiagnostic( diagnostic );
            }
        }
        private static void OnCompilation_End(CompilationAnalysisContext context) {
        }


        // Actions/SyntaxTree
        private static void OnSyntaxTree(SyntaxTreeAnalysisContext context) {
            //Trace.WriteLine( "OnSyntaxTree: " + Path.GetFileName( context.Tree.FilePath ) );
        }
        // Actions/SemanticModel
        private static void OnSemanticModel(SemanticModelAnalysisContext context) {
            //Trace.WriteLine( "OnSemanticModel: " + Path.GetFileName( context.SemanticModel.SyntaxTree.FilePath ) );
        }


        // Actions/Symbol
        private static void OnSymbol_Start(SymbolStartAnalysisContext context) {
            context.RegisterSymbolEndAction( OnSymbol_End );
        }
        private static void OnSymbol(SymbolAnalysisContext context) {
        }
        private static void OnSymbol_End(SymbolAnalysisContext context) {
        }


        // Actions/CodeBlock
        private static void OnCodeBlock_Start(CodeBlockStartAnalysisContext<SyntaxKind> context) {
            context.RegisterCodeBlockEndAction( OnCodeBlock_End );
        }
        private static void OnCodeBlock(CodeBlockAnalysisContext context) {
        }
        private static void OnCodeBlock_End(CodeBlockAnalysisContext context) {
        }


        // Actions/SyntaxNode
        private static void OnSyntaxNode(SyntaxNodeAnalysisContext context) {
        }


    }
}