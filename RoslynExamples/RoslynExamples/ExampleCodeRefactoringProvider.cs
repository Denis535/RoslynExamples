namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Composition;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Rename;
    using Microsoft.CodeAnalysis.Text;

    [ExportCodeRefactoringProvider( LanguageNames.CSharp, Name = nameof( ExampleCodeRefactoringProvider ) ), Shared]
    public class ExampleCodeRefactoringProvider : CodeRefactoringProvider {


        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
            var solution = context.Document.Project.Solution;
            var root = await context.Document.GetSyntaxRootAsync( context.CancellationToken ).ConfigureAwait( false ) ?? throw new Exception( "Syntax root not found" );
            var model = await context.Document.GetSemanticModelAsync( context.CancellationToken ).ConfigureAwait( false ) ?? throw new Exception( "Semantic model not found" );
            var symbols = GetSymbols( root, model, context.Span, context.CancellationToken ).Where( CanBeRenamed ).Reverse().ToArray();
            if (!symbols.Any()) return;

            RegisterRefactoring( context, $"Make symbols '{symbols.Select( i => i.Name ).Join()}' start/end with underscore ({GetType().Name})", Action );
            RegisterRefactoring( context, $"Make symbols '{symbols.Select( i => i.Name ).Join()}' start/end with double underscore ({GetType().Name})", Action2 );

            async Task<Solution> Action(CancellationToken cancellationToken) {
                return await WithFormattedSymbol( solution, symbols, "_{0}_", cancellationToken ).ConfigureAwait( false );
            }
            async Task<Solution> Action2(CancellationToken cancellationToken) {
                return await WithFormattedSymbol( solution, symbols, "__{0}__", cancellationToken ).ConfigureAwait( false );
            }
        }


        // Helpers/CodeRefactoringContext
        private static void RegisterRefactoring(CodeRefactoringContext context, string title, Func<CancellationToken, Task<Solution>> action) {
            context.RegisterRefactoring( CodeAction.Create( title, action, title ) );
        }
        // Helpers/Solution
        private static async Task<Solution> WithFormattedSymbol(Solution solution, IEnumerable<ISymbol> symbols, string format, CancellationToken cancellationToken) {
            // Note: Renamer can rename related symbols, so those related symbols will become broken
            // Note: This may stop working in future versions because old symbols and new solutions should be not compatible
            // todo: use SymbolFinder.FindSimilarSymbols to check is symbol valid
            foreach (var symbol in symbols) {
                try {
                    solution = await WithFormattedSymbol( solution, symbol, format, cancellationToken ).ConfigureAwait( false );
                } catch (InvalidOperationException) {
                }
            }
            return solution;
        }
        private static async Task<Solution> WithFormattedSymbol(Solution solution, ISymbol symbol, string format, CancellationToken cancellationToken) {
            var newName = string.Format( format, symbol.Name );
            return await Renamer.RenameSymbolAsync( solution, symbol, newName, solution.Options, cancellationToken ).ConfigureAwait( false );
        }
        // Helpers/Document
        private static IEnumerable<ISymbol> GetSymbols(SyntaxNode root, SemanticModel model, TextSpan span, CancellationToken cancellationToken) {
            // Note: GetDeclaredSymbol() returns symbol for MemberDeclarationSyntax nodes
            // Note: GetSymbolInfo() returns symbol for other nodes (IdentifierNameSyntax for example)
#pragma warning disable RS1024 // Compare symbols correctly
            var symbols = new HashSet<ISymbol>( SymbolEqualityComparer.Default );
#pragma warning restore RS1024 // Compare symbols correctly
            foreach (var node in root.DescendantTokens( span ).Select( i => i.Parent ).OfType<SyntaxNode>()) {
                var symbol = model.GetDeclaredSymbol( node, cancellationToken ) ?? model.GetSymbolInfo( node, cancellationToken ).Symbol;
                if (symbol == null) continue;

                if (!symbols.Contains( symbol )) {
                    symbols.Add( symbol );
                    yield return symbol;
                }
            }
        }
        // Helpers/Symbol
        private static bool CanBeRenamed(ISymbol symbol) {
            return symbol.CanBeReferencedByName && !symbol.IsImplicitlyDeclared && symbol.Locations.First().IsInSource;
        }


    }
}
