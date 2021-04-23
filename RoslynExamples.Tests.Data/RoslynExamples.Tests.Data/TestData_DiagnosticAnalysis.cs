#pragma warning disable CA1821 // Remove empty Finalizers
#pragma warning disable CS0067

namespace RoslynExamples.Tests.Data.DiagnosticAnalysis {
    using System;
    using System.Collections.Generic;

    public class Class<T> {

        public const object? Const = null;

        public object? Field, Field2;
        public object? Prop { get; set; }

        public event Action? Event, Event2;
        public event Action? Event3 {
            add => new object();
            remove => new object();
        }

        public Class() {
        }
        ~Class() {
        }

        public object? Func(object? argument) {
            return default;
        }

    }
}
