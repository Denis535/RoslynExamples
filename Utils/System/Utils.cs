namespace System {
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal static class Utils {

        // String
        public static string Format(this string value, params string[] args) {
            return string.Format( value, args: args );
        }
        public static string Indent(this string value, string indent) {
            var builder = new StringBuilder();
            foreach (var line in value.GetLines()) {
                builder.Append( indent ).Append( line );
            }
            return builder.ToString();
        }
        private static IEnumerable<string> GetLines(this string value) {
            var start = 0;
            while (start < value.Length) {
                var end = value.IndexOf( '\n', start );
                if (end != -1) {
                    yield return value.Substring( start, (end - start + 1) );
                    start = end + 1;
                } else {
                    break;
                }
            }
            if (start < value.Length) yield return value.Substring( start );
        }
        public static string Join(this IEnumerable<string> values) {
            return string.Join( ", ", values );
        }

        // StringBuilder
        public static StringBuilder AppendLineFormat(this StringBuilder builder, string format, params string[] args) {
            return builder.AppendFormat( format, args ).AppendLine();
        }

        // Enum
        public static T[] GetEnumValues<T>() where T : Enum {
            return (T[]) Enum.GetValues( typeof( T ) );
        }

    }
}
