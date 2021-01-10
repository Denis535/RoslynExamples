namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Rename;
    using Microsoft.CodeAnalysis.Text;

    [ExportCodeFixProvider( LanguageNames.CSharp, Name = nameof( ExampleCodeFixProvider ) ), Shared]
    public class ExampleCodeFixProvider : CodeFixProvider {

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create( ExampleAnalyzer0000.Rule.Id, ExampleAnalyzer0001.Rule.Id );


        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var solution = context.Document.Project.Solution;
            var root = await context.Document.GetSyntaxRootAsync( context.CancellationToken ).ConfigureAwait( false ) ?? throw new Exception( "Syntax root not found" );
            var model = await context.Document.GetSemanticModelAsync( context.CancellationToken ).ConfigureAwait( false ) ?? throw new Exception( "Semantic model not found" );
            var symbol = GetSymbol( root, model, context.Span, context.CancellationToken );

            if (symbol != null) {
                foreach (var diagnostic in context.Diagnostics) {
                    if (diagnostic.Id == ExampleAnalyzer0000.Rule.Id) RegisterCodeFixesFor0000( context, diagnostic, solution, symbol );
                    if (diagnostic.Id == ExampleAnalyzer0001.Rule.Id) RegisterCodeFixesFor0001( context, diagnostic, solution, symbol );
                }
            }
        }
        private static void RegisterCodeFixesFor0000(CodeFixContext context, Diagnostic diagnostic, Solution solution, ISymbol symbol) {
            RegisterCodeFix( context, $"Make symbol '{symbol.Name}' start with underscore ({diagnostic.Descriptor.Id})", diagnostic, Action );
            RegisterCodeFix( context, $"Make symbol '{symbol.Name}' start with double underscore ({diagnostic.Descriptor.Id})", diagnostic, Action2 );

            async Task<Solution> Action(CancellationToken cancellationToken) {
                return await WithFormattedSymbol( solution, symbol, "_{0}", cancellationToken ).ConfigureAwait( false );
            }
            async Task<Solution> Action2(CancellationToken cancellationToken) {
                return await WithFormattedSymbol( solution, symbol, "__{0}", cancellationToken ).ConfigureAwait( false );
            }
        }
        private static void RegisterCodeFixesFor0001(CodeFixContext context, Diagnostic diagnostic, Solution solution, ISymbol symbol) {
            RegisterCodeFix( context, $"Make symbol '{symbol.Name}' end with underscore ({diagnostic.Descriptor.Id})", diagnostic, Action );
            RegisterCodeFix( context, $"Make symbol '{symbol.Name}' end with double underscore ({diagnostic.Descriptor.Id})", diagnostic, Action2 );

            async Task<Solution> Action(CancellationToken cancellationToken) {
                return await WithFormattedSymbol( solution, symbol, "{0}_", cancellationToken ).ConfigureAwait( false );
            }
            async Task<Solution> Action2(CancellationToken cancellationToken) {
                return await WithFormattedSymbol( solution, symbol, "{0}__", cancellationToken ).ConfigureAwait( false );
            }
        }


        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }


        // Helpers/CodeFixContext
        private static void RegisterCodeFix(CodeFixContext context, string title, Diagnostic diagnostic, Func<CancellationToken, Task<Solution>> action) {
            context.RegisterCodeFix( CodeAction.Create( title, action, title ), diagnostic );
        }
        private static void RegisterCodeFix(CodeFixContext context, string title, Diagnostic[] diagnostics, Func<CancellationToken, Task<Solution>> action) {
            context.RegisterCodeFix( CodeAction.Create( title, action, title ), diagnostics );
        }
        // Helpers/Solution
        private static async Task<Solution> WithFormattedSymbol(Solution solution, ISymbol symbol, string format, CancellationToken cancellationToken) {
            var newName = string.Format( format, symbol.Name );
            return await Renamer.RenameSymbolAsync( solution, symbol, newName, solution.Options, cancellationToken ).ConfigureAwait( false );
        }
        // Helpers/Document
        private static ISymbol? GetSymbol(SyntaxNode root, SemanticModel model, TextSpan span, CancellationToken cancellationToken) {
            // Note: GetDeclaredSymbol() returns symbol for MemberDeclarationSyntax nodes
            // Note: GetSymbolInfo() returns symbol for other nodes (IdentifierNameSyntax for example)
            var node = root.FindNode( span );
            return model.GetDeclaredSymbol( node, cancellationToken ) ?? model.GetSymbolInfo( node, cancellationToken ).Symbol;
        }


    }
}
