#pragma warning disable RS1013
#pragma warning disable RS1026
#pragma warning disable RS2008 // Enable analyzer release tracking

namespace RoslynExamples {
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public class ExampleAnalyzer : DiagnosticAnalyzer {

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create<DiagnosticDescriptor>();


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
            //// SyntaxTree, SemanticModel
            //context.RegisterSyntaxTreeAction( OnSyntaxTree );
            //context.RegisterSemanticModelAction( OnSemanticModel );
            //// Symbol
            //context.RegisterSymbolStartAction( OnSymbol_Start, SymbolKind.NamedType );
            //context.RegisterSymbolAction( OnSymbol, Utils.GetEnumValues<SymbolKind>() );
            // Compilation
            context.RegisterCompilationEndAction( OnCompilation_End );
        }
        private static void OnCompilation(CompilationAnalysisContext context) {
        }
        private static void OnCompilation_End(CompilationAnalysisContext context) {
        }


        // Actions/SyntaxTree
        private static void OnSyntaxTree(SyntaxTreeAnalysisContext context) {
        }
        // Actions/SemanticModel
        private static void OnSemanticModel(SemanticModelAnalysisContext context) {
        }


        // Actions/Symbol
        private static void OnSymbol_Start(SymbolStartAnalysisContext context) { // for specific SymbolKind
            //// CodeBlock
            //context.RegisterCodeBlockStartAction<SyntaxKind>( OnCodeBlock_Start );
            //context.RegisterCodeBlockAction( OnCodeBlock );
            // Symbol
            context.RegisterSymbolEndAction( OnSymbol_End );
        }
        private static void OnSymbol(SymbolAnalysisContext context) { // for specific SymbolKind list
        }
        private static void OnSymbol_End(SymbolAnalysisContext context) { // for specific SymbolKind
        }


        // Actions/CodeBlock
        private static void OnCodeBlock_Start(CodeBlockStartAnalysisContext<SyntaxKind> context) {
            //// SyntaxNode
            //context.RegisterSyntaxNodeAction( OnSyntaxNode, Utils.GetEnumValues<SyntaxKind>() );
            // CodeBlock
            context.RegisterCodeBlockEndAction( OnCodeBlock_End );
        }
        private static void OnCodeBlock(CodeBlockAnalysisContext context) {
        }
        private static void OnCodeBlock_End(CodeBlockAnalysisContext context) {
        }


        // Actions/SyntaxNode
        private static void OnSyntaxNode(SyntaxNodeAnalysisContext context) { // for specific SyntaxKind list
        }


    }
}