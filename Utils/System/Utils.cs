namespace System {
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal static class Utils {

        // String
        public static string Format(this string value, params string[] args) {
            return string.Format( value, args: args );
        }
        public static string Indent(this string value, int size) {
            var items = value.Split( new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None );
            var builder = new StringBuilder();
            foreach (var item in items) builder.Append( new string( ' ', size ) ).Append( item ).AppendLine();
            return builder.ToString();
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
