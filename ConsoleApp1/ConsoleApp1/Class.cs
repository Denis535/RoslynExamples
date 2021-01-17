#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS0067 // An event was declared but never used in the class in which it was declared.
#pragma warning disable CS0219 // Variable is assigned but its value is never used

// Symbols:
// ConsoleApp1,
// Class
// SubClass
// Class<T>, Method, Method2, argument,
// Class<T>, Field, Field2, Prop, Prop2, Event, Event2, Event3, Func, Func2, Method, Method2, argument,
namespace ConsoleApp1 {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    internal partial class Class {
        private partial class SubClass {
        }
    }

    public partial class Class<T> {
        public static partial void Method();
        public static partial void Method2<T1>(T1 argument) where T1 : class, new();
    }
    public partial class Class<T> where T : class, new() {
        public object? Field, Field2;
        public object? Prop { get; set; }
        public object? Prop2 => null;
        public event Action? Event, Event2;
        public event Action? Event3 {
            add => Event += value;
            remove => Event -= value;
        }

        public object? Func() {
            return null;
        }
        public object? Func2()
            => null;

        public static partial void Method() {
            LocalMethod();
            static void LocalMethod() { }
        }
        public static partial void Method2<T1>(T1 argument) where T1 : class, new() {
            var variable = default( object );
        }
    }
}