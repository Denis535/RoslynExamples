#pragma warning disable RS2008 // Enable analyzer release tracking
namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    [Generator]
    public class ExampleSourceGenerator : ISourceGenerator {

        //internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        //    "ExampleSourceGenerator",
        //    "ExampleSourceGenerator",
        //    "Message: {0}",
        //    "Example",
        //    DiagnosticSeverity.Error,
        //    true );


        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications( new ExampleSyntaxReceiver() );
        }


        public void Execute(GeneratorExecutionContext context) {
            //Debugger.Launch();
            var receiver = (ExampleSyntaxReceiver?) context.SyntaxReceiver ?? throw new Exception( "SyntaxReceiver is null" );
            foreach (var unit in receiver.Units) {
                var model = context.Compilation.GetSemanticModel( unit.SyntaxTree );
                var name = GetSourceName( unit );
                var content = GetSourceContent( unit, model );
                if (content != null) {
                    context.AddSource( name, content.NormalizeWhitespace().ToString() );
                }
            }
        }


        // Helpers
        private static string GetSourceName(CompilationUnitSyntax node) {
            return Path.GetFileNameWithoutExtension( node.SyntaxTree.FilePath ) + $".Generated.{Guid.NewGuid()}.cs";
        }
        private static CompilationUnitSyntax? GetSourceContent(CompilationUnitSyntax node, SemanticModel model) {
            node = (CompilationUnitSyntax) new ExampleSyntaxProducer0( model ).Visit( node );
            node = (CompilationUnitSyntax) new ExampleSyntaxProducer1().Visit( node );
            node = (CompilationUnitSyntax) new ExampleSyntaxProducer2().Visit( node );
            if (node.DescendantNodes().OfType<TypeDeclarationSyntax>().Any()) return node;
            return null;
        }


    }

    // Collects CompilationUnitSyntax nodes
    class ExampleSyntaxReceiver : ISyntaxReceiver {

        public List<CompilationUnitSyntax> Units { get; } = new List<CompilationUnitSyntax>(); // todo: should I use ConcurrentBag???

        void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode node) {
            if (node is CompilationUnitSyntax unit) Units.Add( unit );
        }

    }

    // Initializes syntax nodes with annotations
    class ExampleSyntaxProducer0 : CSharpSyntaxRewriter {

        private SemanticModel Model { get; } = default!;


        public ExampleSyntaxProducer0(SemanticModel model) {
            Model = model;
        }


        // Interface
        //public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
        //    return base.VisitInterfaceDeclaration( node );
        //}
        // Class
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) {
            if (Filter( node )) {
                var annotation = GetSyntaxAnnotations( node, Model ); // Pass original syntax!!!
                node = (ClassDeclarationSyntax) base.VisitClassDeclaration( node )!; // Pass original syntax!!!
                return node.WithAdditionalAnnotations( annotation );
            } else {
                node = (ClassDeclarationSyntax) base.VisitClassDeclaration( node )!; // Pass original syntax!!!
                return node;
            }
        }
        // Struct
        //public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) {
        //    return base.VisitStructDeclaration( node );
        //}
        // Record
        //public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node) {
        //    return base.VisitRecordDeclaration( node );
        //}


        // Helpers
        private static bool Filter(ClassDeclarationSyntax node) {
            return CodeAnalysisUtils.IsPartial( node ) && !CodeAnalysisUtils.IsStatic( node );
        }
        private static IEnumerable<SyntaxAnnotation> GetSyntaxAnnotations(ClassDeclarationSyntax node, SemanticModel model) {
            var type = model.GetDeclaredSymbol( node )!;
            var members = type.GetMembers().Where( i => i.Kind != SymbolKind.NamedType ).Where( i => i.CanBeReferencedByName ).Where( i => !i.IsImplicitlyDeclared ).ToArray();
            yield return new SyntaxAnnotation( "Type", type.Name );
            foreach (var member in members) {
                yield return new SyntaxAnnotation( "Type.Member", member.Name );
            }
        }


    }

    // Produces empty partial classes for future processing
    class ExampleSyntaxProducer1 : CSharpSyntaxRewriter {


        // CompilationUnit
        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node) {
            node = CompilationUnit()
                .WithExterns( node.Externs )
                .WithUsings( node.Usings )
                .AddMembers( node.Members.OfType<ClassDeclarationSyntax>().Where( Filter ).ToArray() )
                .AddMembers( node.Members.OfType<NamespaceDeclarationSyntax>().ToArray() );
            return base.VisitCompilationUnit( node );
        }
        // Namespace
        public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) {
            node = NamespaceDeclaration( node.Name )
                .WithExterns( node.Externs )
                .WithUsings( node.Usings )
                .AddMembers( node.Members.OfType<ClassDeclarationSyntax>().Where( Filter ).ToArray() );
            return base.VisitNamespaceDeclaration( node );
        }


        // Interface
        //public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
        //    return base.VisitInterfaceDeclaration( node );
        //}
        // Class
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) {
            node = ClassDeclaration( node.Identifier )
                .WithModifiers( node.Modifiers )
                .WithTypeParameterList( node.TypeParameterList )
                .AddMembers( node.Members.OfType<ClassDeclarationSyntax>().Where( Filter ).ToArray() )
                .CopyAnnotationsFrom( node );
            return base.VisitClassDeclaration( node );
        }
        // Struct
        //public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) {
        //    return base.VisitStructDeclaration( node );
        //}
        // Record
        //public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node) {
        //    return base.VisitRecordDeclaration( node );
        //}


        // Method
        //public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node) {
        //    syntax = MethodDeclaration( node.ReturnType, node.Identifier )
        //        .WithModifiers( node.Modifiers )
        //        .WithTypeParameterList( node.TypeParameterList )
        //        .WithParameterList( node.ParameterList )
        //        .WithConstraintClauses( node.ConstraintClauses )
        //        .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );
        //    return base.VisitMethodDeclaration( syntax );
        //}


        // Helpers
        private static bool Filter(ClassDeclarationSyntax node) {
            return CodeAnalysisUtils.IsPartial( node ) && !CodeAnalysisUtils.IsStatic( node );
        }


    }

    // Produces partial classes with additional methods
    class ExampleSyntaxProducer2 : CSharpSyntaxRewriter {


        // Interface
        //public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
        //    return base.VisitInterfaceDeclaration( node );
        //}
        // Class
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) {
            node = node.AddMembers( GetMethodDeclarationSyntax_ToString( node ) );
            return base.VisitClassDeclaration( node );
        }
        // Struct
        //public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) {
        //    return base.VisitStructDeclaration( node );
        //}
        // Record
        //public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node) {
        //    return base.VisitRecordDeclaration( node );
        //}


        // Helpers
        private static MethodDeclarationSyntax GetMethodDeclarationSyntax_ToString(ClassDeclarationSyntax node) {
            var type = node.GetAnnotations( "Type" ).Single()!.Data!;
            var members = node.GetAnnotations( "Type.Member" ).Select( i => i.Data! ).ToArray();
            return GetMethodDeclarationSyntax_ToString( GetStringValue( type, members ) );
        }
        private static MethodDeclarationSyntax GetMethodDeclarationSyntax_ToString(string @string) {
            var builder = new StringBuilder();
            builder.AppendLine( "public override string ToString() {" );
            builder.AppendLineFormat( "return \"{0}\";", @string );
            builder.AppendLine( "}" );
            return (MethodDeclarationSyntax) ParseMemberDeclaration( builder.ToString() )!;
        }
        private static string GetStringValue(string type, string[] members) {
            if (!members.Any()) {
                return string.Format( "Type: {0}", type );
            } else {
                return string.Format( "Type: {0}, Members: {1}", type, members.Join() );
            }
        }


    }
}
