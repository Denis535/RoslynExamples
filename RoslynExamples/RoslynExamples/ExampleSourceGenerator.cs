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
        private static string GetSourceName(CompilationUnitSyntax unit) {
            return Path.GetFileNameWithoutExtension( unit.SyntaxTree.FilePath ) + $".Generated.{Guid.NewGuid()}.cs";
        }
        private static CompilationUnitSyntax? GetSourceContent(CompilationUnitSyntax unit, SemanticModel model) {
            unit = (CompilationUnitSyntax) new ExampleSyntaxProducer0( model ).Visit( unit );
            unit = (CompilationUnitSyntax) new ExampleSyntaxProducer1().Visit( unit );
            unit = (CompilationUnitSyntax) new ExampleSyntaxProducer2().Visit( unit );
            if (unit.DescendantNodes().OfType<TypeDeclarationSyntax>().Any()) return unit;
            return null;
        }


    }

    // Collect syntax nodes
    class ExampleSyntaxReceiver : ISyntaxReceiver {

        public List<CompilationUnitSyntax> Units { get; } = new List<CompilationUnitSyntax>();

        void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode node) {
            if (node is CompilationUnitSyntax unit) Units.Add( unit );
        }

    }

    // Initialize syntax tree with annotations
    class ExampleSyntaxProducer0 : CSharpSyntaxRewriter {

        private SemanticModel Model { get; set; } = default!;

        public ExampleSyntaxProducer0(SemanticModel model) {
            Model = model;
        }


        // Interface
        //public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
        //    return base.VisitInterfaceDeclaration( node );
        //}
        // Class
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) {
            var type = Model.GetDeclaredSymbol( node )!; // Get symbol of ORIGINAL node
            var members = type.GetMembers().Where( i => i.Kind != SymbolKind.NamedType ).Where( i => i.CanBeReferencedByName ).Where( i => !i.IsImplicitlyDeclared ).ToArray();
            node = (ClassDeclarationSyntax) base.VisitClassDeclaration( node )!; // Pass ORIGINAL node
            return node.WithAdditionalAnnotations( GetAnnotations( type, members ) );
        }
        // Record
        //public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node) {
        //    return base.VisitRecordDeclaration( node );
        //}
        // Struct
        //public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) {
        //    return base.VisitStructDeclaration( node );
        //}


        // Helpers
        private static IEnumerable<SyntaxAnnotation> GetAnnotations(INamedTypeSymbol type, ISymbol[] members) {
            yield return new SyntaxAnnotation( "Type", type.Name );
            foreach (var member in members) {
                yield return new SyntaxAnnotation( "Type.Member", member.Name );
            }
        }


    }

    // Produce empty partial classes (skeletons) for future processing
    class ExampleSyntaxProducer1 : CSharpSyntaxRewriter {


        // CompilationUnit
        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node) {
            node = CompilationUnit()
                .WithExterns( node.Externs )
                .WithUsings( node.Usings )
                .AddMembers( node.Members.OfType<ClassDeclarationSyntax>().Where( CodeAnalysisUtils.IsPartial ).ToArray() )
                .AddMembers( node.Members.OfType<NamespaceDeclarationSyntax>().ToArray() );
            return base.VisitCompilationUnit( node );
        }
        // Namespace
        public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) {
            node = NamespaceDeclaration( node.Name )
                .WithExterns( node.Externs )
                .WithUsings( node.Usings )
                .AddMembers( node.Members.OfType<ClassDeclarationSyntax>().Where( CodeAnalysisUtils.IsPartial ).ToArray() );
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
                .AddMembers( node.Members.OfType<ClassDeclarationSyntax>().Where( CodeAnalysisUtils.IsPartial ).ToArray() )
                .CopyAnnotationsFrom( node );
            return base.VisitClassDeclaration( node );
        }
        // Record
        //public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node) {
        //    return base.VisitRecordDeclaration( node );
        //}
        // Struct
        //public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) {
        //    return base.VisitStructDeclaration( node );
        //}


        // Method
        //public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node) {
        //    node = MethodDeclaration( node.ReturnType, node.Identifier )
        //        .WithModifiers( node.Modifiers )
        //        .WithTypeParameterList( node.TypeParameterList )
        //        .WithParameterList( node.ParameterList )
        //        .WithConstraintClauses( node.ConstraintClauses )
        //        .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );
        //    return base.VisitMethodDeclaration( node );
        //}


        // Helpers
        //private static bool IsPartial(ClassDeclarationSyntax node) {
        //    return node.Modifiers.Any( i => i.Kind() == SyntaxKind.PartialKeyword );
        //}
        //private static bool IsPartial(MethodDeclarationSyntax node) {
        //    return node.Modifiers.Any( i => i.Kind() == SyntaxKind.PartialKeyword );
        //}


    }

    // Produce partial classes with additional methods
    class ExampleSyntaxProducer2 : CSharpSyntaxRewriter {


        // Interface
        //public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
        //    return base.VisitInterfaceDeclaration( node );
        //}
        // Class
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) {
            var type = node.GetAnnotations( "Type" ).Single()!.Data!;
            var members = node.GetAnnotations( "Type.Member" ).Select( i => i.Data! ).ToArray();
            node = node.AddMembers( GetSyntax_ToString( type, members ) );
            return base.VisitClassDeclaration( node );
        }
        // Record
        //public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node) {
        //    return base.VisitRecordDeclaration( node );
        //}
        // Struct
        //public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) {
        //    return base.VisitStructDeclaration( node );
        //}


        // Helpers
        private static MethodDeclarationSyntax GetSyntax_ToString(string type, string[] members) {
            var builder = new StringBuilder();
            builder.AppendLine( "public override string ToString() {" );
            {
                builder.AppendLineFormat( "return \"{0}\";", GetDisplayString( type, members ) );
            }
            builder.AppendLine( "}" );
            return (MethodDeclarationSyntax) ParseMemberDeclaration( builder.ToString() )!;
        }
        private static string GetDisplayString(string type, string[] members) {
            var text = new StringBuilder();
            text.AppendFormat( "Type: {0}", type );
            if (members.Any()) {
                text.Append( ", " );
                text.AppendFormat( "Members: {0}", members.Join() );
            }
            return text.ToString();
        }


    }

}
