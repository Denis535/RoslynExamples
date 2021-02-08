#pragma warning disable CS0162 // Unreachable code detected
#pragma warning disable CS8321 // Local function is declared but never used

// Symbols:
// ConsoleApp1,
// Program, Main, args,
// Class,
// SubClass
namespace ConsoleApp1 {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    public static partial class Program {


        public static void Main(string[] args) {
            Console.WriteLine( "Press Any Key To Exit..." );
            Console.Read();
        }


        //| + Block: Ordinal=0, Kind=Entry, Condition=None, IsReachable=True
        //    | - Fall through successor: Semantics=Regular, Destination=1
        //————————————————————————————————————————————————————————————————————————————————————————————————————
        //| + Block: Ordinal=1, Kind=Block, Condition=WhenFalse, IsReachable=True
        //    | - Fall through successor: Semantics=Regular, Destination=2
        //    | - Conditional successor: Semantics=Regular, Destination=3
        //    | + BranchValue: Kind=Literal
        //        ##  true
        //    | + Operation: Kind=ExpressionStatement
        //        ##  Trace.WriteLine( "Block Entry" );
        //————————————————————————————————————————————————————————————————————————————————————————————————————
        //| + Block: Ordinal=2, Kind=Block, Condition=None, IsReachable=True
        //    | - Fall through successor: Semantics=Regular, Destination=5
        //    | + Operation: Kind=ExpressionStatement
        //        ##  Trace.WriteLine( "Block (true)" );
        //————————————————————————————————————————————————————————————————————————————————————————————————————
        //| + Block: Ordinal=3, Kind=Block, Condition=None, IsReachable=False
        //    | - Fall through successor: Semantics=Throw, Destination=Null
        //    | + BranchValue: Kind=ObjectCreation
        //        ##  new Exception()
        //    | + Operation: Kind=ExpressionStatement
        //        ##  Trace.WriteLine( "Block (else)" );
        //————————————————————————————————————————————————————————————————————————————————————————————————————
        //| + Block: Ordinal=4, Kind=Block, Condition=None, IsReachable=False
        //    | - Fall through successor: Semantics=Regular, Destination=5
        //    | + Operation: Kind=ExpressionStatement
        //        ##  Trace.WriteLine( "Block Exit" );
        //————————————————————————————————————————————————————————————————————————————————————————————————————
        //| + Block: Ordinal=5, Kind=Exit, Condition=None, IsReachable=True
        //————————————————————————————————————————————————————————————————————————————————————————————————————
        public static void ControlFlowGraphExample() {
            Trace.WriteLine( "Block Entry" );
            if (true) {
                Trace.WriteLine( "Block (true)" );
                return;
            } else {
                Trace.WriteLine( "Block (else)" );
                throw new Exception();
            }
            Trace.WriteLine( "Block Exit" );

            static void LocalMethod() { }
        }


        // Start Point Is Reachable: True
        // End Point Is Reachable:   False
        // Entry Points: 
        // Exit Points: ReturnStatement
        // Return Statements: ReturnStatement
        public static void ControlFlowAnalysisExample() {
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
        public static ref int DataFlowAnalysisExample(int arg1, ref int arg2) {
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