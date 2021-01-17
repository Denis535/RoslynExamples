namespace Microsoft.CodeAnalysis {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    public static class CodeAnalysisUtils {


        // GeneratorInitializationContext
        public static void RegisterForSyntaxNotifications(this GeneratorInitializationContext context, ISyntaxReceiver receiver) {
            context.RegisterForSyntaxNotifications( () => receiver );
        }


        // Syntax
        public static IEnumerable<ISymbol> GetSymbols(SyntaxNode root, SemanticModel model, TextSpan span, CancellationToken cancellationToken) {
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
        public static ISymbol? GetSymbol(SyntaxNode root, SemanticModel model, TextSpan span, CancellationToken cancellationToken) {
            // Note: GetDeclaredSymbol() returns symbol for MemberDeclarationSyntax nodes
            // Note: GetSymbolInfo() returns symbol for other nodes (IdentifierNameSyntax for example)
            var node = root.FindNode( span );
            return model.GetDeclaredSymbol( node, cancellationToken ) ?? model.GetSymbolInfo( node, cancellationToken ).Symbol;
        }
        public static bool IsPartial(ClassDeclarationSyntax node) {
            return node.Modifiers.Any( i => i.Kind() == SyntaxKind.PartialKeyword );
        }
        public static bool IsPartial(MethodDeclarationSyntax node) {
            return node.Modifiers.Any( i => i.Kind() == SyntaxKind.PartialKeyword );
        }


        // Symbol
        public static bool CanBeRenamed(this ISymbol symbol) {
            return symbol.CanBeReferencedByName && !symbol.IsImplicitlyDeclared && symbol.Locations.First().IsInSource;
        }


    }
}
