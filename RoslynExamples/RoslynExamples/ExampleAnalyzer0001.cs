#pragma warning disable RS1013
#pragma warning disable RS1026
#pragma warning disable RS2008 // Enable analyzer release tracking

namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public class ExampleAnalyzer0001 : DiagnosticAnalyzer {

        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "ExampleAnalyzer0001",
            "ExampleAnalyzer0001",
            "Symbol '{0}' must end with underscore",
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
            //Trace.WriteLine( "OnCompilation: " + context.Compilation.AssemblyName );
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
        private static void OnSymbol_Start(SymbolStartAnalysisContext context) { // for specific SymbolKind
            context.RegisterSymbolEndAction( OnSymbol_End );
        }
        private static void OnSymbol(SymbolAnalysisContext context) { // for specific SymbolKind list
            var symbol = context.Symbol;
            //var node = context.Symbol.DeclaringSyntaxReferences.First().GetSyntax();
            //Trace.WriteLine( $"OnSymbol: {symbol.Name} ({symbol.Kind} / {node.Kind()})" );
            if (CanBeRenamed( symbol )) {
                if (!symbol.Name.EndsWith( "_" )) {
                    var diagnostic = Diagnostic.Create( Rule, symbol.Locations.First(), symbol.Locations.Skip( 1 ), symbol.Name );
                    context.ReportDiagnostic( diagnostic );
                }
            }
        }
        private static void OnSymbol_End(SymbolAnalysisContext context) { // for specific SymbolKind
        }


        // Actions/CodeBlock
        private static void OnCodeBlock_Start(CodeBlockStartAnalysisContext<SyntaxKind> context) {
            context.RegisterCodeBlockEndAction( OnCodeBlock_End );
        }
        private static void OnCodeBlock(CodeBlockAnalysisContext context) {
            //Trace.WriteLine( "OnCodeBlock: " + context.CodeBlock.Kind() );
            //Trace.WriteLine( context.CodeBlock.ToString().Indent( 8 ) );
            //Trace.WriteLine( "" );
        }
        private static void OnCodeBlock_End(CodeBlockAnalysisContext context) {
        }


        // Actions/SyntaxNode
        private static void OnSyntaxNode(SyntaxNodeAnalysisContext context) { // for specific SyntaxKind list
            //Trace.WriteLine( "OnSyntaxNode: " + context.Node.Kind() );
            //Trace.WriteLine( context.Node.ToString().Indent( 8 ) );
            //Trace.WriteLine( "" );
        }


        // Helpers/Symbol
        private static bool CanBeRenamed(ISymbol symbol) {
            return symbol.CanBeReferencedByName && !symbol.IsImplicitlyDeclared && symbol.Locations.First().IsInSource;
        }


    }
}