namespace System {
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class MessageBuilder : IDisposable {

        private StringBuilder Builder { get; }
        private int Depth { get; set; }


        public MessageBuilder() {
            Builder = new StringBuilder();
            Depth = 0;
        }
        void IDisposable.Dispose() {
            if (Depth > 0) Depth--;
        }


        // AppendTitle
        public MessageBuilder AppendTitle(string title) {
            if (Depth > 0) throw new InvalidOperationException( $"Depth is invalid: {Depth}" );
            Builder.AppendLine( title );
            return Indent( this );
        }
        public MessageBuilder AppendTitle(string title, params object?[] args) {
            if (Depth > 0) throw new InvalidOperationException( $"Depth is invalid: {Depth}" );
            Builder.AppendLineFormat( title, WithNotNullValues( args ) );
            return Indent( this );
        }
        // AppendSection
        public MessageBuilder AppendSection(string title) {
            Builder.AppendIndent( "| + ", Depth ).AppendLine( title );
            return Indent( this );
        }
        public MessageBuilder AppendSection(string title, params object?[] args) {
            Builder.AppendIndent( "| + ", Depth ).AppendLineFormat( title, WithNotNullValues( args ) );
            return Indent( this );
        }
        // AppendLine
        public MessageBuilder AppendLine(string title) {
            Builder.AppendIndent( "| - ", Depth ).AppendLine( title );
            return this;
        }
        public MessageBuilder AppendLine(string title, params object?[] args) {
            Builder.AppendIndent( "| - ", Depth ).AppendLineFormat( title, WithNotNullValues( args ) );
            return this;
        }
        // AppendText
        public MessageBuilder AppendText(IEnumerable<string> text) {
            foreach (var line in text) {
                Builder.AppendIndent( "##  ", Depth ).AppendLine( line );
            }
            return this;
        }


        // Separate
        public MessageBuilder Separate() {
            Builder.AppendIndent( "", Depth ).AppendLine( new string( '—', 100 ) );
            return this;
        }


        // ToString
        public override string ToString() {
            return Builder.ToString();
        }


        // Helpers
        private static MessageBuilder Indent(MessageBuilder builder) {
            builder.Depth++;
            return builder;
        }
        private static object[] WithNotNullValues(object?[] args) {
            for (var i = 0; i < args.Length; i++) {
                args[ i ] ??= args[ i ] ?? "Null";
            }
            return args!;
        }


    }
}
