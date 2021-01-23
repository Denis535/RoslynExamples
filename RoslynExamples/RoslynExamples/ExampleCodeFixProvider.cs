namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;

    [ExportCodeFixProvider( LanguageNames.CSharp, Name = nameof( ExampleCodeFixProvider ) ), Shared]
    public class ExampleCodeFixProvider : CodeFixProvider {

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create( ExampleAnalyzer0000.Rule.Id, ExampleAnalyzer0001.Rule.Id );


        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var solution = context.Document.Project.Solution;
            var model = await context.Document.GetSemanticModelAsync( context.CancellationToken ).ConfigureAwait( false ) ?? throw new Exception( "Semantic model is not found" );
            var root = await context.Document.GetSyntaxRootAsync( context.CancellationToken ).ConfigureAwait( false ) ?? throw new Exception( "Syntax root is not found" );
            var symbol = CodeAnalysisUtils.GetSymbol( model, root, context.Span, context.CancellationToken );
            if (symbol == null) return;

            foreach (var diagnostic in context.Diagnostics) {
                if (diagnostic.Id == ExampleAnalyzer0000.Rule.Id) RegisterCodeFixesFor0000( context, diagnostic, solution, symbol );
                if (diagnostic.Id == ExampleAnalyzer0001.Rule.Id) RegisterCodeFixesFor0001( context, diagnostic, solution, symbol );
            }
        }
        private static void RegisterCodeFixesFor0000(CodeFixContext context, Diagnostic diagnostic, Solution solution, ISymbol symbol) {
            context.RegisterCodeFix( $"Make symbol '{symbol.Name}' start with underscore ({diagnostic.Descriptor.Id})", diagnostic, Action );
            context.RegisterCodeFix( $"Make symbol '{symbol.Name}' start with double underscore ({diagnostic.Descriptor.Id})", diagnostic, Action2 );

            async Task<Solution> Action(CancellationToken cancellationToken) {
                return await WorkspacesUtils.WithFormattedSymbol( solution, symbol, "_{0}", cancellationToken ).ConfigureAwait( false );
            }
            async Task<Solution> Action2(CancellationToken cancellationToken) {
                return await WorkspacesUtils.WithFormattedSymbol( solution, symbol, "__{0}", cancellationToken ).ConfigureAwait( false );
            }
        }
        private static void RegisterCodeFixesFor0001(CodeFixContext context, Diagnostic diagnostic, Solution solution, ISymbol symbol) {
            context.RegisterCodeFix( $"Make symbol '{symbol.Name}' end with underscore ({diagnostic.Descriptor.Id})", diagnostic, Action );
            context.RegisterCodeFix( $"Make symbol '{symbol.Name}' end with double underscore ({diagnostic.Descriptor.Id})", diagnostic, Action2 );

            async Task<Solution> Action(CancellationToken cancellationToken) {
                return await WorkspacesUtils.WithFormattedSymbol( solution, symbol, "{0}_", cancellationToken ).ConfigureAwait( false );
            }
            async Task<Solution> Action2(CancellationToken cancellationToken) {
                return await WorkspacesUtils.WithFormattedSymbol( solution, symbol, "{0}__", cancellationToken ).ConfigureAwait( false );
            }
        }


        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }


    }
}
