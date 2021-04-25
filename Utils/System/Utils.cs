namespace System {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal static class Utils {


        // StringBuilder
        public static StringBuilder AppendIndent(this StringBuilder builder, string indent, int depth) {
            if (depth == 0) return builder;
            return builder.Append( ' ', 4 * (depth - 1) ).Append( indent );
        }
        public static StringBuilder AppendLineFormat(this StringBuilder builder, string format, params object?[] args) {
            return builder.AppendFormat( format, args ).AppendLine();
        }


        // String
        public static string Format(this string value, params object?[] args) {
            return string.Format( value, args: args );
        }
        public static string Join<T>(this IEnumerable<T> values) {
            return string.Join( ", ", values );
        }
        public static string Join<T>(this IEnumerable<T> values, Func<T, object> selector) {
            return string.Join( ", ", values.Select( selector ) );
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


        // Enumerable
        //public static IEnumerable<T> Concat<T>(this T first, T second) {
        //    return Enumerable.Empty<T>().Append( first ).Append( second );
        //}
        //public static IEnumerable<T> Concat<T>(this T first, IEnumerable<T> second) {
        //    return second.Prepend( first );
        //}
        //public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, T second) {
        //    return first.Append( second );
        //}
        public static IEnumerable<T> Concat<T>(params IEnumerable<T>[] items) {
            var result = Enumerable.Empty<T>();
            foreach (var item in items) result = result.Concat( item );
            return result;
        }
        public static IEnumerable<T> AsEnumerable<T>(this T item) {
            return Enumerable.Empty<T>().Append( item );
        }


        // Misc
        public static TResult Map<TSource, TResult>(this TSource source, Func<TSource, TResult> selector) {
            return selector( source );
        }


    }
}
