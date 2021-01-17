namespace Microsoft.CodeAnalysis {
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class SyntaxNodeExtensions {


        public static T CopyAnnotationsFrom<T>(this T node, SyntaxNode other) where T : SyntaxNode {
            return other.CopyAnnotationsTo( node )!;
        }


    }
}
