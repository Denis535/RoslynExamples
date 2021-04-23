#pragma warning disable CS0162 // Unreachable code detected
#pragma warning disable CS0219 // Variable is assigned but its value is never used
#pragma warning disable CS8321 // Local function is declared but never used

namespace RoslynExamples.Tests.Data.ControlFlowGraph {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class Examples {


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


        public static void ControlFlowAnalysisExample() {
            if (false) {
                return;
            }
            return;
        }


        public static ref int DataFlowAnalysisExample(int arg1, ref int arg2) {
            var outer = 0;
            if (false) {
                var inner = 0;
            }
            return ref arg2;
        }


    }
}
