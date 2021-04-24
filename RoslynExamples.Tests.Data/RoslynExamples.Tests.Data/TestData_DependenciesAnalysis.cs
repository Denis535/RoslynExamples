#pragma warning disable CS0067 // An event was declared but never used in the class in which it was declared.
#pragma warning disable CS0219 // Variable is assigned but its value is never used
#pragma warning disable CS8321 // Local function is declared but never used

namespace RoslynExamples.Tests.Data.DependenciesAnalysis {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Alias = System.String;

    public static class Examples {

        public static void Example_00_Expressions_Literals() {
            // Note: There is no symbol for: null
            _ = (object?) null;
            _ = (object?) default;
            _ = "Hello World !!!";
            _ = $"Hello {"World"} {"!!!"}";
        }
        public static void Example_01_Expressions_TypeOf<T>() {
            // Note: Unbound and original definition type are different things
            _ = typeof( T );
            _ = typeof( Alias );
            _ = typeof( Class );
            _ = typeof( Class<> ); // Unbound type
            _ = typeof( Class<Class<object>[][]>[][] ); // Constructed type
            _ = typeof( int?*[][] ); // PointerType
            _ = typeof( delegate* managed<int?, int?>[][] ); // FunctionPointerType
        }
        public static void Example_02_Expressions_NameOf<T>() {
            // Note: There is no symbol for: IdentifierName of nameof
            // Note: There is no symbol for: IdentifierName of method within nameof
            _ = nameof( T );
            _ = nameof( Alias );
            _ = nameof( Class );
            _ = nameof( Class.Const );
            _ = nameof( Class.Field );
            _ = nameof( Class.Property );
            _ = nameof( Class.Event );
            _ = nameof( Class.Function );
            _ = nameof( Class.GenericFunction );
        }
        public static void Example_03_Expressions_MemberAccess(Class argument) {
            _ = Class.Const;
            _ = argument.Field;
            _ = argument.Property;
            //_ = argument.Event;
            //_ = argument.Event( null );
        }
        public static void Example_03_Expressions_ElementAccess(Class argument) {
            // Note: There is no way to get symbol for: indexer (in case of ConditionalAccessExpressionSyntax)
            _ = argument![ null ];
            _ = argument?[ null ];
            _ = argument!.Array![ 0 ];
            _ = argument?.Array?[ 0 ];
        }
        public static void Example_03_Expressions_Invocation(Class argument) {
            // Note: Methods can have implicit generics and arguments
            // Note: Lambda's parameters have implicit types
            _ = argument.Function( null );
            _ = argument.GenericFunction<object>( null );
            _ = argument.OnCallback( i => null );
        }
        public static void Example_03_Expressions_Invocation_Operators(Class argument) {
            // Note: There is no way to get symbol for: true, false, implicit operators
            // Unary
            if (argument) { } // true operator
            //if (!argument) { } // false operator does't work
            // Binary
            _ = argument == null;
            _ = argument != null;
            // Conversions
            _ = (string?) argument;
            _ = (int?) argument;
            Class<object>? tmp = argument;
        }
        public static unsafe void Example_04_Statements_Locals(object? argument) {
            // Note: There is no symbol for: RefTypeExpressionSyntax
            var variable = argument;
            ref var ref_variable = ref variable;
            static object? LocalFunction(object? argument) => null;
        }

    }

    // Annotation
    [AttributeUsage( AttributeTargets.All, AllowMultiple = false )]
    public class AnnotationAttribute : Attribute {
    }

    // Class
    [Annotation]
    public class Class {
        public const object? Const = null;

        public object? Field, Field2;
        public object? Property { get; set; }
        public object? this[ object? index ] => null;
        public object[]? Array { get; set; }

        public event Action<object?>? Event, Event2;
        public event Action<object?>? Event3 {
            add => new object();
            remove => new object();
        }


        public object? Function(object? argument) {
            return null;
        }
        public TValue? GenericFunction<TValue>(TValue? argument) where TValue : class {
            return null;
        }
        public object? OnCallback(Func<object?, object?> lambda) {
            return null;
        }

        // Utils
        public override bool Equals(object? obj) {
            return base.Equals( obj );
        }
        public override int GetHashCode() {
            return base.GetHashCode();
        }

        // Operators/Unary
        public static bool operator true(Class obj) {
            return obj is not null;
        }
        public static bool operator false(Class obj) {
            return obj is null;
        }
        // Operators/Binary
        public static bool operator ==(Class? left, Class? right) {
            return ReferenceEquals( left, right );
        }
        public static bool operator !=(Class? left, Class? right) {
            return !ReferenceEquals( left, right );
        }
        // Operators/Conversion
        public static explicit operator string?(Class? obj) {
            return obj?.ToString();
        }
        public static explicit operator int?(Class? obj) {
            return obj?.GetHashCode();
        }
        public static implicit operator Class<object>?(Class? obj) {
            return null;
        }

    }

    [Annotation]
    public class Class<T> where T : class {
    }


    // Simple/Interface
    public interface ISimpleInterface {
        public object? Value { get; }
    }
    // Simple/Class
    public class SimpleClass {
        public object? Value => null;
    }
    // Simple/Struct
    public struct SimpleStruct {
        public object? Value => null;
    }
    // Simple/Record
    public record SimpleRecord {
        public object? Value => null;
    }
    // Simple/Enum
    public enum SimpleEnum {
        Value,
    }
    // Simple/Delegate
    public delegate object? SimpleDelegate(object? argument);

}