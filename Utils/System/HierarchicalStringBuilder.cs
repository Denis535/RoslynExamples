namespace System {
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class HierarchicalStringBuilder {
        private struct Node {
            public const string IndentPrefix = "|   ";
            public const string IndentEmptyPrefix = "    ";
            public const string TitlePrefix = "";
            public const string SectionPrefix = "| - ";
            public const string LinePrefix = "|   ";
            public const string ItemPrefix = "| * ";
            public const string TextPrefix = "| # ";
            public const string SeparatorPrefix = "    ";
            public string Prefix { get; }
            public string Text { get; }
            public int Level { get; }
            public Node(string prefix, string text, int level) {
                (Prefix, Text, Level) = (prefix, text, level);
            }
            public override string ToString() {
                return string.Format( "Node: {0}, {1}", Level, Text );
            }
        }
        private class Scope : IDisposable {
            private readonly HierarchicalStringBuilder builder;
            public Scope(HierarchicalStringBuilder builder) {
                this.builder = builder;
                this.builder.Level++;
            }
            public void Dispose() {
                this.builder.Level--;
            }
        }

        private List<Node> Nodes { get; }
        private int Level { get; set; }


        public HierarchicalStringBuilder() {
            Nodes = new List<Node>();
            Level = 0;
        }
        private HierarchicalStringBuilder(List<Node> nodes, int level) {
            Nodes = nodes;
            Level = level;
        }


        // Append/Scope
        public IDisposable AppendTitle(string text, params object?[] args) {
            EnsureLevelIsZero( Level );
            AppendNode( Node.TitlePrefix, text, args );
            return new Scope( this );
        }
        public IDisposable AppendSection(string text, params object?[] args) {
            EnsureLevelIsGreaterThanZero( Level );
            AppendNode( Node.SectionPrefix, text, args );
            return new Scope( this );
        }
        // Append/Line
        public HierarchicalStringBuilder AppendLine(string text, params object?[] args) {
            EnsureLevelIsGreaterThanZero( Level );
            return AppendNode( Node.LinePrefix, text, args );
        }
        public HierarchicalStringBuilder AppendItem(string text, params object?[] args) {
            EnsureLevelIsGreaterThanZero( Level );
            return AppendNode( Node.ItemPrefix, text, args );
        }
        public HierarchicalStringBuilder AppendText(string text) {
            EnsureLevelIsGreaterThanZero( Level );
            return AppendNode( Node.TextPrefix, text );
        }
        public HierarchicalStringBuilder AppendText(IEnumerable<string> text) {
            EnsureLevelIsGreaterThanZero( Level );
            foreach (var text_ in text) AppendNode( Node.TextPrefix, text_ );
            return this;
        }
        public HierarchicalStringBuilder AppendSeparator() {
            EnsureLevelIsGreaterThanZero( Level );
            return AppendNode( Node.SeparatorPrefix, new string( '—', 100 ) );
        }
        // Append/Node
        private HierarchicalStringBuilder AppendNode(string prefix, string text, object?[]? args = null) {
            if (args != null) text = Format( text, args );
            Nodes.Add( new Node( prefix, text, Level ) );
            return this;
        }


        // WithIndent
        public HierarchicalStringBuilder WithIndent() {
            return new HierarchicalStringBuilder( Nodes, Level + 1 );
        }


        // Utils
        public override string ToString() {
            if (Level != 0) throw new InvalidOperationException( "Level is invalid: " + Level );
            var builder = new StringBuilder();
            AppendHierarchy( builder, ToHierarchy( Nodes ) );
            return builder.ToString();
        }


        // Helpers
        private static void EnsureLevelIsZero(int level) {
            if (level == 0) return;
            throw new InvalidOperationException( "Level must be zero: " + level );
        }
        private static void EnsureLevelIsGreaterThanZero(int level) {
            if (level > 0) return;
            throw new InvalidOperationException( "Level must be not zero: " + level );
        }
        private static string Format(string text, object?[] args) {
            for (var i = 0; i < args.Length; i++) {
                args[ i ] = args[ i ] ?? "Null";
            }
            return string.Format( text, args );
        }
        // Helpers/Hierarchy
        private static IReadOnlyList<object> ToHierarchy(IReadOnlyList<Node> nodes) {
            var index = 0;
            return ToHierarchy( nodes, 0, ref index );
        }
        private static IReadOnlyList<object> ToHierarchy(IReadOnlyList<Node> nodes, int level, ref int index) {
            var result = new List<object>();
            for (; index < nodes.Count;) {
                var node = nodes[ index ];
                if (node.Level == level) {
                    result.Add( node );
                    index++;
                } else
                if (node.Level > level) {
                    result.Add( ToHierarchy( nodes, node.Level, ref index ) );
                } else
                if (node.Level < level) {
                    break;
                }
            }
            return result;
        }
        // Helpers/StringBuilder
        private static void AppendHierarchy(StringBuilder builder, IReadOnlyList<object> hierarchy) {
            foreach (var item in hierarchy) {
                if (item is Node node) {
                    builder.AppendLine( node.Text );
                    continue;
                }
                if (item is IReadOnlyList<object> children) {
                    AppendHierarchy( builder, children, "" );
                    continue;
                }
                throw new Exception( "Hierarchy item is invalid: " + item );
            }
        }
        private static void AppendHierarchy(StringBuilder builder, IReadOnlyList<object> hierarchy, string indent) {
            for (var i = 0; i < hierarchy.Count; i++) {
                var item = hierarchy[ i ];
                var isLast = i == hierarchy.Count - 1;
                if (item is Node node) {
                    builder.Append( indent ).Append( node.Prefix ).AppendLine( node.Text );
                    continue;
                }
                if (item is IReadOnlyList<object> children) {
                    if (!isLast) {
                        AppendHierarchy( builder, children, indent + Node.IndentPrefix );
                    } else {
                        AppendHierarchy( builder, children, indent + Node.IndentEmptyPrefix );
                    }
                    continue;
                }
                throw new Exception( "Hierarchy item is invalid: " + item );
            }
        }


    }
}
