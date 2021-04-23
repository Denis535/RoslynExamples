namespace Microsoft.CodeAnalysis {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    // ExpressionSyntax
    // - (Identifiers)
    // - TypeSyntax:                                       Identifier
    // - RefTypeExpressionSyntax:                          ref Identifier
    // - InstanceExpressionSyntax:                         this, base

    // - (Literals)
    // - LiteralExpressionSyntax:                          "Hello World !!!"
    // - InterpolatedStringExpressionSyntax:               "Value: {value}"
    // - TupleExpressionSyntax:                            (0, 1, 2)
    // - RangeExpressionSyntax:                            0..10
    // - DefaultExpressionSyntax:                          default(Identifier)
    // - TypeOfExpressionSyntax:                           typeof(Identifier)
    // - SizeOfExpressionSyntax:                           sizeof(Identifier)

    // - (Creators)
    // - BaseObjectCreationExpressionSyntax
    // - ArrayCreationExpressionSyntax
    // - ImplicitArrayCreationExpressionSyntax
    // - StackAllocArrayCreationExpressionSyntax
    // - ImplicitStackAllocArrayCreationExpressionSyntax
    // - AnonymousObjectCreationExpressionSyntax:          new {...}
    // - AnonymousFunctionExpressionSyntax:                delegate() {}, () => { }

    // - (Creators/Initializers)
    // - InitializerExpressionSyntax:                      new Class() {...}
    // - WithExpressionSyntax:                             obj with {...}

    // - (ControlFlow)
    // - InvocationExpressionSyntax:                       obj.Func()
    // - ThrowExpressionSyntax:                            throw exception

    // - (ControlFlow/Selectors)
    // - ConditionalExpressionSyntax:                      flag ? a : b
    // - SwitchExpressionSyntax:                           value switch {}

    // - (DataFlow)
    // - AssignmentExpressionSyntax:                       obj = null

    // - (DataFlow/Accessors)
    // - MemberAccessExpressionSyntax:                     obj.Value
    // - MemberBindingExpressionSyntax:                    obj?.Value
    // - ElementAccessExpressionSyntax:                    array[0]
    // - ElementBindingExpressionSyntax:                   array?[0]
    // - ImplicitElementAccessSyntax:                      [0]
    // - ConditionalAccessExpressionSyntax:                obj?.Value

    // - (Operators)
    // - PostfixUnaryExpressionSyntax:                     a++
    // - PrefixUnaryExpressionSyntax:                      ++a
    // - BinaryExpressionSyntax:                           a+b

    // - (Patterns)
    // - IsPatternExpressionSyntax:                        obj is Object

    // - (Declarations)
    // - DeclarationExpressionSyntax:                      var (a, b), var value

    // - (Query)
    // - QueryExpressionSyntax:                            from i in array where i => 0 select i

    // - (Utils)
    // - ParenthesizedExpressionSyntax:                    (...)
    // - CheckedExpressionSyntax:                          checked(0+1)
    // - MakeRefExpressionSyntax:                          __makeref(value)
    // - RefValueExpressionSyntax:                         __refvalue(reference, object)
    // - CastExpressionSyntax:                             (object) obj
    // - RefExpressionSyntax:                              ref value
    // - AwaitExpressionSyntax:                            await task

    // - (Misc)
    // - OmittedArraySizeExpressionSyntax:                 []


    // TypeSyntax:
    // - NameSyntax
    // - PredefinedTypeSyntax
    // - NullableTypeSyntax
    // - TupleTypeSyntax
    // - RefTypeSyntax - no symbol
    // - OmittedTypeArgumentSyntax - no sense
    // - ArrayTypeSyntax

    // LiteralExpressionSyntax:
    // - NullLiteralExpression
    // - DefaultLiteralExpression
    // - FalseLiteralExpression
    // - TrueLiteralExpression
    // - NumericLiteralExpression
    // - CharacterLiteralExpression
    // - StringLiteralExpression
    // - ArgListExpression - can have child references


    // ISymbol:
    // - IAssemblySymbol
    // - ISourceAssemblySymbol
    // - IModuleSymbol

    // - IPreprocessingSymbol
    // - INamespaceOrTypeSymbol
    // - INamespaceSymbol
    // - IAliasSymbol

    // - ITypeSymbol
    // - INamedTypeSymbol
    // - ITypeParameterSymbol
    // - IDynamicTypeSymbol
    // - IPointerTypeSymbol
    // - IFunctionPointerTypeSymbol
    // - IArrayTypeSymbol
    // - IErrorTypeSymbol

    // - IFieldSymbol
    // - IPropertySymbol
    // - IEventSymbol
    // - IMethodSymbol

    // - IParameterSymbol
    // - ILabelSymbol
    // - ILocalSymbol
    // - IDiscardSymbol

    // - IRangeVariableSymbol

    public static class DependenciesAnalyzer {


        // Analyze
        public static DependenciesAnalysis Analyze(SyntaxNode syntax, SemanticModel model) {
            // Note: Analyzer doesn't support references for: indexer and unary, binary, conversion operators.
            // Note: Because there is no way to get symbol for indexer (in case of ConditionalAccessExpressionSyntax), true, false, implicit operators.
            if (syntax is null) throw new ArgumentNullException( nameof( syntax ) );
            if (model is null) throw new ArgumentNullException( nameof( model ) );

            var references = syntax
                .FindReferences()
                .Select( i => CreateReference( i, model ) )
                .ToImmutableArray();
            return new DependenciesAnalysis( references );
        }

        // GetTypeSymbols
        public static IEnumerable<ITypeSymbol> GetTypeSymbols(DependenciesAnalysis.Reference reference) {
            return GetTypeSymbols( reference.Symbol );
        }

        // Deconstruct
        public static IEnumerable<ITypeSymbol> Deconstruct(ITypeSymbol symbol) {
            return GetSimpleTypeSymbols( symbol );
        }


        // Helpers/Reference
        private static DependenciesAnalysis.Reference CreateReference(SyntaxNode syntax, SemanticModel model) {
            var symbol = GetSymbol( syntax, model );
            return new DependenciesAnalysis.Reference( syntax, symbol );
        }
        // Helpers/SyntaxNode
        private static IEnumerable<SyntaxNode> FindReferences(this SyntaxNode syntax) {
            return syntax
                .DescendantNodes( i => !i.IsReference() || i == syntax )
                .Where( IsReference )
                .SelectMany( GetDeconstructedReferences );
        }
        private static bool IsReference(this SyntaxNode syntax) {
            return
                (syntax is TypeSyntax and not OmittedTypeArgumentSyntax) ||
                syntax is LiteralExpressionSyntax ||
                syntax is InterpolatedStringExpressionSyntax;
        }
        private static IEnumerable<SyntaxNode> GetDeconstructedReferences(SyntaxNode syntax) {
            if (syntax is RefTypeSyntax refType) {
                return refType.Type.AsEnumerable();
            }
            if (syntax is LiteralExpressionSyntax literal && literal.Kind() == SyntaxKind.ArgListExpression) {
                return Utils.Concat( literal.AsEnumerable(), literal.FindReferences() );
            }
            if (syntax is InterpolatedStringExpressionSyntax @string) {
                return Utils.Concat( @string.AsEnumerable(), @string.FindReferences() );
            }
            return syntax.AsEnumerable();
        }
        private static ISymbol? GetSymbol(SyntaxNode syntax, SemanticModel model) {
            // Note: There is no symbol for: Alias, nameof, nameof( Method ), null
            if (syntax is TypeSyntax) {
                return model.GetSymbolInfo( syntax ).Symbol;
            }
            if (syntax is LiteralExpressionSyntax) {
                return model.GetTypeInfo( syntax ).Type;
            }
            if (syntax is InterpolatedStringExpressionSyntax) {
                return model.Compilation.GetSpecialType( SpecialType.System_String );
            }
            throw new ArgumentException( "SyntaxNode is invalid: " + syntax.Kind() );
        }
        // Helpers/ISymbol
        private static IEnumerable<ITypeSymbol> GetTypeSymbols(ISymbol? symbol) {
            if (symbol is null) {
                return Enumerable.Empty<ITypeSymbol>();
            }

            if (symbol is INamespaceSymbol @namespace) {
                return Enumerable.Empty<ITypeSymbol>();
            }
            if (symbol is ITypeSymbol type) {
                return type.AsEnumerable();
            }

            if (symbol is IFieldSymbol field) {
                return field.Type.AsEnumerable();
            }
            if (symbol is IPropertySymbol property) {
                return Utils.Concat(
                    property.Type.AsEnumerable(),
                    property.Parameters.Select( i => i.Type )
                    );
            }
            if (symbol is IEventSymbol @event) {
                return @event.Type.AsEnumerable();
            }
            if (symbol is IMethodSymbol method) {
                // Should we return generic constraint types???
                return Utils.Concat(
                    method.TypeArguments,
                    method.Parameters.Select( i => i.Type ),
                    method.ReturnType.AsEnumerable()
                    );
            }

            if (symbol is IParameterSymbol parameter) {
                return parameter.Type.AsEnumerable();
            }
            if (symbol is ILocalSymbol local) {
                return local.Type.AsEnumerable();
            }
            if (symbol is IDiscardSymbol discard) {
                return discard.Type.AsEnumerable();
            }
            throw new ArgumentException( "Symbol is invalid: " + symbol );
        }
        private static IEnumerable<ITypeSymbol> GetSimpleTypeSymbols(this ITypeSymbol symbol) {
            if (symbol is INamedTypeSymbol type) {
                if (type.IsGenericType) {
                    // Should we return generic constraint types???
                    return Utils.Concat(
                        type.ConstructUnboundGenericType().AsEnumerable(),
                        type.OriginalDefinition.AsEnumerable(),
                        type.TypeArguments.SelectMany( GetSimpleTypeSymbols )
                        );
                } else {
                    return type.AsEnumerable();
                }
            }
            if (symbol is ITypeParameterSymbol typeParameter) {
                return typeParameter.AsEnumerable();
            }
            if (symbol is IDynamicTypeSymbol dynamicType) {
                return dynamicType.AsEnumerable();
            }
            if (symbol is IPointerTypeSymbol pointerType) {
                return pointerType.PointedAtType.GetSimpleTypeSymbols();
            }
            if (symbol is IFunctionPointerTypeSymbol functionPointerType) {
                var signature = functionPointerType.Signature;
                return Utils.Concat(
                    signature.Parameters.Select( i => i.Type ).SelectMany( GetSimpleTypeSymbols ),
                    signature.ReturnType.GetSimpleTypeSymbols()
                    );
            }
            if (symbol is IArrayTypeSymbol array) {
                return array.ElementType.GetSimpleTypeSymbols();
            }
            return symbol.AsEnumerable();
        }

    }
    public class DependenciesAnalysis {
        public class Reference {
            public SyntaxNode Syntax { get; }
            public ISymbol? Symbol { get; }

            internal Reference(SyntaxNode syntax, ISymbol? symbol) {
                (Syntax, Symbol) = (syntax, symbol);
            }

            public override string ToString() {
                if (Symbol != null) {
                    return string.Format( "Reference: {0} ({1})", Syntax, Symbol.Kind );
                } else {
                    return string.Format( "Reference: {0}", Syntax );
                }
            }
        }

        public ImmutableArray<Reference> References { get; } = default!;


        internal DependenciesAnalysis(ImmutableArray<Reference> references) {
            References = references;
        }


    }
}
