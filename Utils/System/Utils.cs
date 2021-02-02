namespace System {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal static class Utils {


        // Hierarchy
        public static IList<object> AppendLine(this IList<object> hierarchy, object value) {
            hierarchy.Add( value.ToString() ?? "Null" );
            return hierarchy;
        }
        public static IList<object> AppendLine(this IList<object> hierarchy, string format, object arg0) {
            hierarchy.Add( string.Format( format, arg0 ) );
            return hierarchy;
        }
        public static IList<object> AppendLine(this IList<object> hierarchy, string format, object arg0, object arg1) {
            hierarchy.Add( string.Format( format, arg0, arg1 ) );
            return hierarchy;
        }
        public static IList<object> AppendLine(this IList<object> hierarchy, string format, object arg0, object arg1, object arg2) {
            hierarchy.Add( string.Format( format, arg0, arg1, arg2 ) );
            return hierarchy;
        }
        public static IList<object> AppendLine(this IList<object> hierarchy, string format, object arg0, object arg1, object arg2, object arg3) {
            hierarchy.Add( string.Format( format, arg0, arg1, arg2, arg3 ) );
            return hierarchy;
        }
        public static IList<object> AppendText(this IList<object> hierarchy, string? text) {
            hierarchy.Add( text ?? "Null" );
            return hierarchy;
        }
        public static IList<object> AppendText(this IList<object> hierarchy, IEnumerable<string> text) {
            foreach (var line in text) hierarchy.Add( line );
            return hierarchy;
        }
        public static IList<object> Children(this IList<object> hierarchy) {
            if (hierarchy.Any() && hierarchy.Last() is IList<object> children) {
                return children;
            } else {
                var children_ = new List<object>();
                hierarchy.Add( children_ );
                return children_;
            }
        }
        public static string Build(this IList<object> hierarchy) {
            var builder = new StringBuilder();
            builder.AppendHierarchy( hierarchy );
            return builder.ToString();
        }


        // StringBuilder/AppendLine
        public static StringBuilder AppendLineFormat(this StringBuilder builder, string format) {
            return builder.AppendFormat( format ).AppendLine();
        }
        public static StringBuilder AppendLineFormat(this StringBuilder builder, string format, object arg0) {
            return builder.AppendFormat( format, arg0 ).AppendLine();
        }
        public static StringBuilder AppendLineFormat(this StringBuilder builder, string format, object arg0, object arg1) {
            return builder.AppendFormat( format, arg0, arg1 ).AppendLine();
        }
        public static StringBuilder AppendLineFormat(this StringBuilder builder, string format, object arg0, object arg1, object arg2) {
            return builder.AppendFormat( format, arg0, arg1, arg2 ).AppendLine();
        }
        public static StringBuilder AppendLineFormat(this StringBuilder builder, string format, object arg0, object arg1, object arg2, object arg3) {
            return builder.AppendFormat( format, arg0, arg1, arg2, arg3 ).AppendLine();
        }
        //public static StringBuilder AppendLineFormat(this StringBuilder builder, string format, params object[] args) {
        //    return builder.AppendFormat( format, args ).AppendLine();
        //}
        // StringBuilder/AppendLines
        public static StringBuilder AppendLines(this StringBuilder builder, IEnumerable<string> lines) {
            foreach (var line in lines) {
                builder.AppendLine( line );
            }
            return builder;
        }
        // StringBuilder/AppendHierarchy
        public static StringBuilder AppendHierarchy(this StringBuilder builder, IEnumerable hierarchy, int depth = 0) {
            foreach (var item in hierarchy) {
                if (item is IEnumerable enumerable && item is not string) {
                    builder.AppendHierarchy( enumerable, depth + 1 );
                } else {
                    builder.Indent( depth ).AppendLine( item?.ToString() ?? "Null" );
                }
            }
            return builder;
        }
        // StringBuilder/Indent
        public static StringBuilder Indent(this StringBuilder builder, int depth) {
            if (depth == 0) return builder;
            if (depth == 1) return builder.Append( "| - " );
            return builder.Append( new string( ' ', (depth - 1) * 4 ) ).Append( "| - " );
        }


        // String
        public static string Format(this string value, params object[] args) {
            return string.Format( value, args: args );
        }
        public static string Join(this IEnumerable<string> values) {
            return string.Join( ", ", values );
        }
        public static string Concat(this IEnumerable<string> values) {
            return string.Concat( values );
        }


        // Enum
        public static T[] GetEnumValues<T>() where T : Enum {
            return (T[]) Enum.GetValues( typeof( T ) );
        }


        // Helpers
        //private static IEnumerable<string> GetLines(this string value) {
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
        //private static string GetIndent(int depth) {
        //    if (depth == 0) return "";
        //    if (depth == 1) return "| - ";
        //    return new string( ' ', (depth - 1) * 4 ) + "| - ";
        //}


    }
}
