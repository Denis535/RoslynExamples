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
    // - IErrorTypeSymbol
    // - IDynamicTypeSymbol
    // - ITypeParameterSymbol
    // - IPointerTypeSymbol
    // - IFunctionPointerTypeSymbol
    // - IArrayTypeSymbol

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
            // Note: There is no way to get symbol for: indexer (in case of ConditionalAccessExpressionSyntax), true, false, implicit operators.
            // Note: Analyzer doesn't support: indexer, unary, binary, conversion operators.
            // Note: Analyzer doesn't support: lambda parameter's implicit types.
            if (syntax is null) throw new ArgumentNullException( nameof( syntax ) );
            if (model is null) throw new ArgumentNullException( nameof( model ) );

            var references = syntax
                .FindReferences()
                .Select( i => CreateReference( i, model ) )
                .ToImmutableArray();
            return new DependenciesAnalysis( references );
        }


        // Helpers/FindReferences
        private static IEnumerable<SyntaxNode> FindReferences(this SyntaxNode syntax) {
            return syntax
                .DescendantNodes( i => i == syntax || !i.IsReference() )
                .Where( IsReference )
                .SelectMany( GetSimpleReferences );
        }
        private static bool IsReference(this SyntaxNode syntax) {
            return
                (syntax is TypeSyntax and not OmittedTypeArgumentSyntax) ||
                syntax is LiteralExpressionSyntax ||
                syntax is InterpolatedStringExpressionSyntax;
        }
        private static IEnumerable<SyntaxNode> GetSimpleReferences(SyntaxNode syntax) {
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
        // Helpers/CreateReference
        private static DependenciesAnalysis.Reference CreateReference(SyntaxNode syntax, SemanticModel model) {
            var symbol = GetReferenceSymbol( syntax, model );
            return new DependenciesAnalysis.Reference( syntax, symbol );
        }
        private static ISymbol? GetReferenceSymbol(SyntaxNode syntax, SemanticModel model) {
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

    }
    // DependenciesAnalysis
    public partial class DependenciesAnalysis {

        public ImmutableArray<Reference> References { get; } = default!;

        internal DependenciesAnalysis(ImmutableArray<Reference> references) {
            References = references;
        }

    }
    // DependenciesAnalysis.Reference
    public partial class DependenciesAnalysis {
        public class Reference {
            public SyntaxNode Syntax { get; }
            public ISymbol? Symbol { get; }
            public IEnumerable<ITypeSymbol> TypeSymbols => (Symbol != null) ? GetTypeSymbols( Symbol ) : Enumerable.Empty<ITypeSymbol>();

            internal Reference(SyntaxNode syntax, ISymbol? symbol) {
                (Syntax, Symbol) = (syntax, symbol);
            }


            // Utils
            public override string ToString() {
                if (Symbol != null) {
                    return string.Format( "Reference: {0} ({1})", Syntax, Symbol.Kind );
                } else {
                    return string.Format( "Reference: {0}", Syntax );
                }
            }

            // Utils/Deconstruct
            public static IEnumerable<ITypeSymbol> Deconstruct(ITypeSymbol symbol) {
                return GetSimpleTypeSymbols( symbol );
            }


            // Helpers/GetTypeSymbols
            private static IEnumerable<ITypeSymbol> GetTypeSymbols(ISymbol symbol) {
                switch (symbol) {
                    case INamespaceSymbol @namespace: {
                        return Enumerable.Empty<ITypeSymbol>();
                    }
                    case ITypeSymbol type: {
                        return type.AsEnumerable();
                    }

                    case IFieldSymbol field: {
                        return field.Type.AsEnumerable();
                    }
                    case IPropertySymbol property: {
                        return Utils.Concat(
                            property.Type.AsEnumerable(),
                            property.Parameters.Select( i => i.Type )
                            );
                    }
                    case IEventSymbol @event: {
                        return @event.Type.AsEnumerable();
                    }
                    case IMethodSymbol method: {
                        // Should we return generic constraint types???
                        return Utils.Concat(
                            method.TypeArguments,
                            method.Parameters.Select( i => i.Type ),
                            method.ReturnType.AsEnumerable()
                            );
                    }

                    case IParameterSymbol parameter: {
                        return parameter.Type.AsEnumerable();
                    }
                    case ILabelSymbol label: {
                        return Enumerable.Empty<ITypeSymbol>();
                    }
                    case ILocalSymbol local: {
                        return local.Type.AsEnumerable();
                    }
                    case IDiscardSymbol discard: {
                        return discard.Type.AsEnumerable();
                    }

                    default: {
                        throw new ArgumentException( "Symbol is invalid: " + symbol );
                    }
                }
            }
            // Helpers/GetSimpleTypeSymbols
            private static IEnumerable<ITypeSymbol> GetSimpleTypeSymbols(ITypeSymbol symbol) {
                switch (symbol) {
                    case INamedTypeSymbol type when type.IsUnboundGenericType: {
                        return type.AsEnumerable();
                    }
                    case INamedTypeSymbol type when type.IsGenericType: {
                        // Should we return generic constraint types???
                        return Utils.Concat(
                            type.ConstructUnboundGenericType().AsEnumerable(),
                            type.OriginalDefinition.AsEnumerable(),
                            type.TypeArguments.SelectMany( GetSimpleTypeSymbols )
                            );
                    }
                    case INamedTypeSymbol type: {
                        return type.AsEnumerable();
                    }
                    case IDynamicTypeSymbol dynamicType: {
                        return dynamicType.AsEnumerable();
                    }
                    case ITypeParameterSymbol typeParameter: {
                        return typeParameter.AsEnumerable();
                    }

                    case IPointerTypeSymbol pointerType: {
                        return pointerType.PointedAtType.Map( GetSimpleTypeSymbols );
                    }
                    case IFunctionPointerTypeSymbol functionPointerType: {
                        var signature = functionPointerType.Signature;
                        return Utils.Concat(
                            signature.Parameters.Select( i => i.Type ).SelectMany( GetSimpleTypeSymbols ),
                            signature.ReturnType.Map( GetSimpleTypeSymbols )
                            );
                    }

                    case IArrayTypeSymbol array: {
                        return array.ElementType.Map( GetSimpleTypeSymbols );
                    }

                    default: {
                        throw new ArgumentException( "Symbol is invalid: " + symbol );
                    }
                }
            }


        }
    }
}
