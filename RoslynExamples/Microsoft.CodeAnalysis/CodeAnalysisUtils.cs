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


        // SemanticModel
        public static IEnumerable<ISymbol> FindSymbols(SemanticModel model, SyntaxNode root, TextSpan span, CancellationToken cancellationToken) {
            var symbols = new HashSet<ISymbol>( SymbolEqualityComparer.Default );
            foreach (var node in root.DescendantTokens( span ).Select( i => i.Parent ).OfType<SyntaxNode>()) {
                var symbol = model.GetDeclaredSymbol( node, cancellationToken ) ?? model.GetSymbolInfo( node, cancellationToken ).Symbol;
                if (symbol == null) continue;

                if (!symbols.Contains( symbol )) {
                    symbols.Add( symbol );
                    yield return symbol;
                }
            }
        }
        public static ISymbol? FindSymbol(SemanticModel model, SyntaxNode root, TextSpan span, CancellationToken cancellationToken) {
            // Note: GetDeclaredSymbol() returns symbol for MemberDeclarationSyntax nodes
            // Note: GetSymbolInfo() returns symbol for other nodes (for example: TypeSyntax)
            var node = root.FindNode( span );
            return model.GetDeclaredSymbol( node, cancellationToken ) ?? model.GetSymbolInfo( node, cancellationToken ).Symbol;
        }


        // SyntaxNode
        public static bool IsStatic(ClassDeclarationSyntax node) {
            return node.Modifiers.Any( i => i.Kind() == SyntaxKind.StaticKeyword );
        }
        public static bool IsStatic(MethodDeclarationSyntax node) {
            return node.Modifiers.Any( i => i.Kind() == SyntaxKind.StaticKeyword );
        }
        public static bool IsPartial(ClassDeclarationSyntax node) {
            return node.Modifiers.Any( i => i.Kind() == SyntaxKind.PartialKeyword );
        }
        public static bool IsPartial(MethodDeclarationSyntax node) {
            return node.Modifiers.Any( i => i.Kind() == SyntaxKind.PartialKeyword );
        }
        public static T CopyAnnotationsFrom<T>(this T node, SyntaxNode other) where T : SyntaxNode {
            return other.CopyAnnotationsTo( node )!;
        }


        // ISymbol
        public static bool CanBeRenamed(this ISymbol symbol) {
            return symbol.CanBeReferencedByName && !symbol.IsImplicitlyDeclared && symbol.Locations.First().IsInSource;
        }


    }
}
