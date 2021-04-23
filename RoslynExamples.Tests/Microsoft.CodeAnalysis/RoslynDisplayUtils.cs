namespace Microsoft.CodeAnalysis {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.FlowAnalysis;

    public static class RoslynDisplayUtils {


        // SyntaxNode
        public static string GetDisplayString(SyntaxNode syntax) {
            var identifiers = (syntax as MemberDeclarationSyntax)?.GetIdentifiers();
            if (identifiers != null) {
                return string.Format( "SyntaxNode: {0}", identifiers.Join() );
            } else {
                return string.Format( "SyntaxNode: {0}", syntax.Kind() );
            }
        }


        // ISymbol
        public static string GetDisplayString(ISymbol symbol) {
            if (symbol.Kind == SymbolKind.Namespace) {
                return string.Format( "Symbol: {0}, ({1})", symbol.Kind, symbol );
            }
            if (symbol.Kind == SymbolKind.Discard) {
                return string.Format( "Symbol: {0}", symbol.Kind );
            }
            {
                return string.Format( "Symbol: {0}, {1}, ({2})", symbol.Kind, symbol.Name, symbol );
            }
        }
        public static string GetDisplayString(ITypeSymbol symbol) {
            var format = SymbolDisplayFormat.MinimallyQualifiedFormat;
            format = format.WithMiscellaneousOptions( format.MiscellaneousOptions | SymbolDisplayMiscellaneousOptions.ExpandNullable );
            return string.Format( "{0}", symbol.ToDisplayString( format ) );
        }


        // ControlFlowGraph
        public static string GetDisplayString(ControlFlowGraph graph) {
            var builder = new HierarchicalStringBuilder();
            using (builder.AppendTitle( "Control flow graph:" )) {
                builder.AppendProperty( "Original operation", graph.OriginalOperation );
                builder.AppendSeparator();
                builder.AppendProperty( "Root region", graph.Root );
                builder.AppendSeparator();
                foreach (var block in graph.Blocks) {
                    builder.AppendProperty( "Block", block );
                    builder.AppendSeparator();
                }
            }
            return builder.ToString();
        }
        // ControlFlowGraph/Operation
        private static void AppendProperty(this HierarchicalStringBuilder builder, string name, IOperation? operation) {
            if (operation == null) return;
            builder.AppendLine( "{0}: Kind={1}", name, operation.Kind ).AppendText( operation.Syntax );
        }
        // ControlFlowGraph/ControlFlowRegion
        private static void AppendProperty(this HierarchicalStringBuilder builder, string name, ControlFlowRegion region) {
            using (builder.AppendSection( "{0}: Kind={1}", name, region.Kind )) {
                builder.AppendLine( "Capture ids: {0}", region.CaptureIds.Join() );
                builder.AppendLine( "Locals: {0}", region.Locals.Join() );
                builder.AppendLine( "Local functions: {0}", region.LocalFunctions.Join() );
                foreach (var nestedRegion in region.NestedRegions) {
                    builder.AppendProperty( "Nested region", nestedRegion );
                }
            }
        }
        // ControlFlowGraph/BasicBlock
        private static void AppendProperty(this HierarchicalStringBuilder builder, string name, BasicBlock block) {
            using (builder.AppendSection( "{0}: Ordinal={1}, Kind={2}, Condition={3}, IsReachable={4}", name, block.Ordinal, block.Kind, block.ConditionKind, block.IsReachable )) {
                builder.AppendProperty( "Fall through successor", block.FallThroughSuccessor );
                builder.AppendProperty( "Conditional successor", block.ConditionalSuccessor );
                builder.AppendProperty( "Branch operation", block.BranchValue );
                foreach (var operation in block.Operations) {
                    builder.AppendProperty( "Operation", operation );
                }
            }
        }
        // ControlFlowGraph/ControlFlowBranch
        private static void AppendProperty(this HierarchicalStringBuilder builder, string name, ControlFlowBranch? branch) {
            if (branch == null) return;
            builder.AppendLine( "{0}: Semantics={1}, Destination={2}", name, branch.Semantics, branch.Destination?.Ordinal );
        }


        // ControlFlowAnalysis
        public static string GetDisplayString(BlockSyntax syntax, ControlFlowAnalysis analysis) {
            var builder = new HierarchicalStringBuilder();
            using (builder.AppendTitle( "Control flow analysis: {0}", syntax.Parent!.Kind() )) {
                builder.AppendLine( "Start point is reachable: {0}", analysis.StartPointIsReachable );
                builder.AppendLine( "End point is reachable: {0}", analysis.EndPointIsReachable );

                builder.AppendLine( "Entry points: {0}", analysis.EntryPoints.Join() );
                builder.AppendLine( "Exit points: {0}", analysis.ExitPoints.Join() );

                builder.AppendLine( "Return statements: {0}", analysis.ReturnStatements.Join() );
            }
            return builder.ToString();
        }


        // DataFlowAnalysis
        public static string GetDisplayString(BlockSyntax syntax, DataFlowAnalysis analysis) {
            var builder = new HierarchicalStringBuilder();
            using (builder.AppendTitle( "Data flow analysis: {0}", syntax.Parent!.Kind() )) {
                builder.AppendLine( "Definitely assigned (On entry): {0}", analysis.DefinitelyAssignedOnEntry.Join() );
                builder.AppendLine( "Definitely assigned (On exit): {0}", analysis.DefinitelyAssignedOnExit.Join() );

                builder.AppendLine( "Declared (Inside): {0}", analysis.VariablesDeclared.Join() );
                builder.AppendLine( "Always assigned (Inside): {0}", analysis.AlwaysAssigned.Join() );

                builder.AppendLine( "Written (Outside): {0}", analysis.WrittenOutside.Join() );
                builder.AppendLine( "Read (Outside): {0}", analysis.ReadOutside.Join() );

                builder.AppendLine( "Written (Inside): {0}", analysis.WrittenInside.Join() );
                builder.AppendLine( "Read (Inside): {0}", analysis.ReadInside.Join() );

                builder.AppendLine( "Data flows (In): {0}", analysis.DataFlowsIn.Join() );
                builder.AppendLine( "Data flows (Out): {0}", analysis.DataFlowsOut.Join() );

                builder.AppendLine( "Captured: {0}", analysis.Captured.Join() );
                builder.AppendLine( "Captured (Inside): {0}", analysis.CapturedInside.Join() );
                builder.AppendLine( "Captured (Outside): {0}", analysis.CapturedOutside.Join() );

                builder.AppendLine( "Unsafe address taken: {0}", analysis.UnsafeAddressTaken.Join() );
                builder.AppendLine( "Used local functions: {0}", analysis.UsedLocalFunctions.Join() );
            }
            return builder.ToString();
        }


        // DependenciesAnalysis
        public static string GetDisplayString(SyntaxNode syntax, DependenciesAnalysis analysis) {
            var builder = new HierarchicalStringBuilder();
            using (builder.AppendTitle( "Dependencies analysis:" )) {
                builder.AppendHierarchy( syntax, analysis.References );
            }
            return builder.ToString();
        }
        private static void AppendHierarchy(this HierarchicalStringBuilder builder, SyntaxNode scope, ImmutableArray<DependenciesAnalysis.Reference> references) {
            using (builder.AppendSection( scope )) {
                if (scope is LocalDeclarationStatementSyntax or LocalFunctionStatementSyntax or ExpressionStatementSyntax) {
                    builder.AppendText( scope.ToString() );
                }
                foreach (var reference in references.Where( i => i.GetScope() == scope )) {
                    builder.AppendItem( reference );
                }
                foreach (var child in scope.ChildNodes().Where( IsScope )) {
                    builder.AppendHierarchy( child, references );
                }
            }
        }
        private static IDisposable AppendSection(this HierarchicalStringBuilder builder, SyntaxNode scope) {
            var identifiers = (scope as MemberDeclarationSyntax)?.GetIdentifiers();
            if (identifiers != null) {
                return builder.AppendSection( "{0}: {1}", scope.Kind(), identifiers.Join() );
            } else {
                return builder.AppendSection( "{0}", scope.Kind() );
            }
        }
        private static void AppendItem(this HierarchicalStringBuilder builder, DependenciesAnalysis.Reference reference) {
            builder.AppendItem( "{0} ({1})", reference.Syntax, reference.Symbol?.Kind );
            //if (reference.TypeSymbols.Any()) {
            //    builder.AppendItem( "{0} ({1}): {2}", reference.Syntax, reference.Symbol?.Kind, reference.TypeSymbols.Join() );
            //} else {
            //    builder.AppendItem( "{0} ({1})", reference.Syntax, reference.Symbol?.Kind );
            //}
        }


        // Helpers/DependenciesAnalysis
        private static SyntaxNode? GetScope(this DependenciesAnalysis.Reference reference) {
            return reference.Syntax.Ancestors().FirstOrDefault( i => IsScope( i ) );
        }
        private static bool IsScope(this SyntaxNode syntax) {
            return syntax is CompilationUnitSyntax or MemberDeclarationSyntax or StatementSyntax;
        }
        // Helpers/HierarchicalStringBuilder
        private static void AppendText(this HierarchicalStringBuilder builder, SyntaxNode syntax) {
            var lines = syntax.WithoutTrivia().GetText().Lines.Select( i => i.ToString() );
            builder.WithIndent().AppendText( lines );
        }
        // Helpers/SyntaxNode
        private static string[]? GetIdentifiers(this MemberDeclarationSyntax syntax) {
            if (syntax is NamespaceDeclarationSyntax @namespace) {
                return new[] { @namespace.Name.ToString() };
            }
            if (syntax is BaseFieldDeclarationSyntax field) {
                return field.Declaration.Variables.Select( i => i.Identifier ).Select( i => i.ToString() ).ToArray();
            }
            if (syntax is IndexerDeclarationSyntax indexer) {
                return null;
            }
            if (syntax is MemberDeclarationSyntax member) {
                return member.ChildTokens().Where( i => i.Kind() == SyntaxKind.IdentifierToken ).Select( i => i.ToString() ).ToArray();
            }
            return null;
        }
        // Helpers/String
        private static string Join(this IEnumerable<SyntaxNode> values) {
            return values.Join( i => i.Kind() );
        }
        private static string Join(this IEnumerable<ISymbol> values) {
            return values.Join( i => i.Name );
        }


    }
}
