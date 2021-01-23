#pragma warning disable CS0162 // Unreachable code detected

// Symbols:
// ConsoleApp1,
// Program, Main, args,
// Class,
// SubClass
namespace ConsoleApp1 {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using System.Text;

    public static partial class Program {


        public static void Main(string[] args) {
            Console.WriteLine( "Press Any Key To Exit..." );
            Console.Read();
        }


        // Start Point Is Reachable: True
        // End Point Is Reachable:   False
        // Entry Points: 
        // Exit Points: ReturnStatement
        // Return Statements: ReturnStatement
        public static void ControlFlowExample() {
            return;
        }


        // Definitely Assigned (On Entry): arg1, arg2
        // Definitely Assigned (On Exit):  arg1, arg2, outer
        // Declared (Inside):        outer, inner
        // Always Assigned (Inside): outer
        // Written (Outside): arg1, arg2 (Written by method caller)
        // Read (Outside):    arg2       (Read by method caller)
        // Written (Inside):  arg2, outer, inner
        // Read (Inside):     arg2
        // Data Flows (In):  arg2
        // Data Flows (Out): arg2
        public static ref int DataFlowExample(int arg1, ref int arg2) {
            var outer = 0;
            if (false) {
                var inner = 0;
            }
            return ref arg2;
        }


    }
    internal partial class Class {
        private partial class SubClass {
        }
    }
}