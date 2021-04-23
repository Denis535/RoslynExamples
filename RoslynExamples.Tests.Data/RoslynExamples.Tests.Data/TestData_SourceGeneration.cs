#pragma warning disable CA1821 // Remove empty Finalizers

namespace RoslynExamples.Tests.Data.SourceGeneration {
    using System;
    using System.Collections.Generic;

    public class Class {
        public object? Value { get; set; }
        public object? Func(object? argument) {
            return default;
        }
    }
    public partial class PartialClass {
        public object? Value { get; set; }
        public object? Func(object? argument) {
            return default;
        }
    }
    public partial class PartialClass<T> where T : class {
        public object? Value { get; set; }
        public object? Func(object? argument) {
            return default;
        }
    }
    public static partial class StaticPartialClass {
        public static object? Value { get; set; }
        public static object? Func(object? argument) {
            return default;
        }
    }
}
