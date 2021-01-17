// Symbols:
// ConsoleApp1,
// Program, Main, args,
// Class,
// SubClass
namespace ConsoleApp1 {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    public static partial class Program {

        public static void Main(string[] args) {
            Console.WriteLine( "Press Any Key To Exit..." );
            Console.Read();
        }

    }
    internal partial class Class {
        private partial class SubClass {
        }
    }
}