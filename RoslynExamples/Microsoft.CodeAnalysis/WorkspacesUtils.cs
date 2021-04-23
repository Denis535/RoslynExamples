namespace Microsoft.CodeAnalysis {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.Rename;

    public static class WorkspacesUtils {


        // CodeFixContext
        public static void RegisterCodeFix(this CodeFixContext context, string title, Diagnostic diagnostic, Func<CancellationToken, Task<Solution>> action) {
            context.RegisterCodeFix( CodeAction.Create( title, action, title ), diagnostic );
        }
        public static void RegisterCodeFix(this CodeFixContext context, string title, Diagnostic[] diagnostics, Func<CancellationToken, Task<Solution>> action) {
            context.RegisterCodeFix( CodeAction.Create( title, action, title ), diagnostics );
        }


        // CodeRefactoringContext
        public static void RegisterRefactoring(this CodeRefactoringContext context, string title, Func<CancellationToken, Task<Solution>> action) {
            context.RegisterRefactoring( CodeAction.Create( title, action, title ) );
        }


        // Solution
        public static async Task<Solution> WithFormattedSymbols(Solution solution, IEnumerable<ISymbol> symbols, string format, CancellationToken cancellationToken) {
            // Note: Renamer can rename related symbols (particle methods), so those related symbols will become broken
            // Note: This may stop working in future versions because old symbols and new solutions should be not compatible
            // todo: Use SymbolFinder.FindSimilarSymbols to check is symbol valid
            foreach (var symbol in symbols) {
                try {
                    solution = await WithFormattedSymbol( solution, symbol, format, cancellationToken ).ConfigureAwait( false );
                } catch (InvalidOperationException) {
                }
            }
            return solution;
        }
        public static async Task<Solution> WithFormattedSymbol(Solution solution, ISymbol symbol, string format, CancellationToken cancellationToken) {
            var newName = string.Format( format, symbol.Name );
            return await Renamer.RenameSymbolAsync( solution, symbol, newName, solution.Options, cancellationToken ).ConfigureAwait( false );
        }


    }
}
