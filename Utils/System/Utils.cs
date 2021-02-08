namespace System {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    internal static class Utils {


        // StringBuilder/AppendIndent
        public static StringBuilder AppendIndent(this StringBuilder builder, string indent, int depth) {
            if (depth == 0) return builder;
            for (var i = 0; i < depth - 1; i++) builder.Append( "    " );
            return builder.Append( indent );
        }
        // StringBuilder/AppendLine
        public static StringBuilder AppendLineFormat(this StringBuilder builder, string format, params object?[] args) {
            return builder.AppendFormat( format, args ).AppendLine();
        }


        // String
        public static string Format(this string value, params object?[] args) {
            return string.Format( value, args: args );
        }
        public static string Join(this IEnumerable<string> values) {
            return string.Join( ", ", values );
        }
        public static string Concat(this IEnumerable<string> values) {
            return string.Concat( values );
        }
        //public static IEnumerable<string> GetLines(this string value) {
        //    var start = 0;
        //    while (start < value.Length) {
        //        var end = value.IndexOf( '\n', start ); // \n or \r\n
        //        if (end != -1) {
        //            yield return value.Substring( start, (end - start + 1) );
        //            start = end + 1;
        //        } else {
        //            if (start < value.Length) yield return value.Substring( start );
        //            yield break;
        //        }
        //    }
        //}


        // Enum
        public static T[] GetEnumValues<T>() where T : Enum {
            return (T[]) Enum.GetValues( typeof( T ) );
        }


    }
}
