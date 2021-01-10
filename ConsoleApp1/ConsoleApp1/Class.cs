namespace ConsoleApp1 {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

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