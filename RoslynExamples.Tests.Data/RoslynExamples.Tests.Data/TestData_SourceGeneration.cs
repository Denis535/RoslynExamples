#pragma warning disable CA1821 // Remove empty Finalizers

namespace RoslynExamples.Tests.Data.SourceGeneration {
    using System;
    using System.Collections.Generic;

    public class Class {
        public object? Value { get; set; }
    }
    public partial class PartialClass {
        public partial class NestedPartialClass {
            public object? Value { get; set; }
        }
        public object? Value { get; set; }
    }
    public partial class PartialClass<T> where T : class {
        public object? Value { get; set; }
    }
    public static partial class StaticPartialClass {
        public static object? Value { get; set; }
    }
}
