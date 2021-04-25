namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Composition;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.CSharp;

    [ExportCodeRefactoringProvider( LanguageNames.CSharp, Name = nameof( ExampleCodeRefactoringProvider ) ), Shared]
    public class ExampleCodeRefactoringProvider : CodeRefactoringProvider {


        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
            var solution = context.Document.Project.Solution;
            var model = await context.Document.GetSemanticModelAsync( context.CancellationToken ).ConfigureAwait( false ) ?? throw new Exception( "Semantic model is not found" );
            var root = await context.Document.GetSyntaxRootAsync( context.CancellationToken ).ConfigureAwait( false ) ?? throw new Exception( "Syntax root is not found" );
            
            var span = context.Span;
            var symbols = CodeAnalysisUtils.FindSymbols( model, root, span, context.CancellationToken ).Where( CodeAnalysisUtils.CanBeRenamed ).Reverse().ToArray();
            if (!symbols.Any()) return;

            context.RegisterRefactoring( $"Make symbols '{symbols.Join( i => i.Name )}' start/end with underscore ({GetType().Name})", Action );
            context.RegisterRefactoring( $"Make symbols '{symbols.Join( i => i.Name )}' start/end with double underscore ({GetType().Name})", Action2 );

            async Task<Solution> Action(CancellationToken cancellationToken) {
                return await WorkspacesUtils.WithFormattedSymbols( solution, symbols, "_{0}_", cancellationToken ).ConfigureAwait( false );
            }
            async Task<Solution> Action2(CancellationToken cancellationToken) {
                return await WorkspacesUtils.WithFormattedSymbols( solution, symbols, "__{0}__", cancellationToken ).ConfigureAwait( false );
            }
        }


    }
}
