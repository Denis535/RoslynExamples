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
        private static string GetSourceName(CompilationUnitSyntax syntax) {
            return Path.GetFileNameWithoutExtension( syntax.SyntaxTree.FilePath ) + $".Generated.{Guid.NewGuid()}.cs";
        }
        private static CompilationUnitSyntax? GetSourceContent(CompilationUnitSyntax syntax, SemanticModel model) {
            syntax = (CompilationUnitSyntax) new ExampleSyntaxProducer0( model ).Visit( syntax );
            syntax = (CompilationUnitSyntax) new ExampleSyntaxProducer1().Visit( syntax );
            syntax = (CompilationUnitSyntax) new ExampleSyntaxProducer2().Visit( syntax );
            if (syntax.DescendantNodes().OfType<TypeDeclarationSyntax>().Any()) return syntax;
            return null;
        }


    }

    // Collects CompilationUnitSyntax nodes
    class ExampleSyntaxReceiver : ISyntaxReceiver {

        public List<CompilationUnitSyntax> Units { get; } = new List<CompilationUnitSyntax>(); // todo: should I use ConcurrentBag???

        void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntax) {
            if (syntax is CompilationUnitSyntax unit) Units.Add( unit );
        }

    }

    // Initializes syntax nodes with annotations
    class ExampleSyntaxProducer0 : CSharpSyntaxRewriter {

        private SemanticModel Model { get; } = default!;

        public ExampleSyntaxProducer0(SemanticModel model) {
            Model = model;
        }


        // Interface
        //public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax syntax) {
        //    return base.VisitInterfaceDeclaration( syntax );
        //}
        // Class
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax syntax) {
            var annotation = GetSyntaxAnnotations( syntax, Model ); // Pass original syntax!!!
            syntax = (ClassDeclarationSyntax) base.VisitClassDeclaration( syntax )!; // Pass original syntax!!!
            return syntax.WithAdditionalAnnotations( annotation );
        }
        // Struct
        //public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax syntax) {
        //    return base.VisitStructDeclaration( syntax );
        //}
        // Record
        //public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax syntax) {
        //    return base.VisitRecordDeclaration( syntax );
        //}


        // Helpers
        private static IEnumerable<SyntaxAnnotation> GetSyntaxAnnotations(ClassDeclarationSyntax syntax, SemanticModel model) {
            var type = model.GetDeclaredSymbol( syntax )!;
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
        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax syntax) {
            syntax = CompilationUnit()
                .WithExterns( syntax.Externs )
                .WithUsings( syntax.Usings )
                .AddMembers( syntax.Members.OfType<ClassDeclarationSyntax>().Where( Filter ).ToArray() )
                .AddMembers( syntax.Members.OfType<NamespaceDeclarationSyntax>().ToArray() );
            return base.VisitCompilationUnit( syntax );
        }
        // Namespace
        public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax syntax) {
            syntax = NamespaceDeclaration( syntax.Name )
                .WithExterns( syntax.Externs )
                .WithUsings( syntax.Usings )
                .AddMembers( syntax.Members.OfType<ClassDeclarationSyntax>().Where( Filter ).ToArray() );
            return base.VisitNamespaceDeclaration( syntax );
        }


        // Interface
        //public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax syntax) {
        //    return base.VisitInterfaceDeclaration( syntax );
        //}
        // Class
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax syntax) {
            syntax = ClassDeclaration( syntax.Identifier )
                .WithModifiers( syntax.Modifiers )
                .WithTypeParameterList( syntax.TypeParameterList )
                .AddMembers( syntax.Members.OfType<ClassDeclarationSyntax>().Where( Filter ).ToArray() )
                .CopyAnnotationsFrom( syntax );
            return base.VisitClassDeclaration( syntax );
        }
        // Struct
        //public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax syntax) {
        //    return base.VisitStructDeclaration( syntax );
        //}
        // Record
        //public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax syntax) {
        //    return base.VisitRecordDeclaration( syntax );
        //}


        // Method
        //public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax syntax) {
        //    syntax = MethodDeclaration( node.ReturnType, node.Identifier )
        //        .WithModifiers( node.Modifiers )
        //        .WithTypeParameterList( node.TypeParameterList )
        //        .WithParameterList( node.ParameterList )
        //        .WithConstraintClauses( node.ConstraintClauses )
        //        .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );
        //    return base.VisitMethodDeclaration( syntax );
        //}


        // Helpers
        private static bool Filter(ClassDeclarationSyntax syntax) {
            return CodeAnalysisUtils.IsPartial( syntax ) && !CodeAnalysisUtils.IsStatic( syntax );
        }


    }

    // Produces partial classes with additional methods
    class ExampleSyntaxProducer2 : CSharpSyntaxRewriter {


        // Interface
        //public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax syntax) {
        //    return base.VisitInterfaceDeclaration( syntax );
        //}
        // Class
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax syntax) {
            syntax = syntax.AddMembers( GetMethodDeclarationSyntax_ToString( syntax ) );
            return base.VisitClassDeclaration( syntax );
        }
        // Struct
        //public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax syntax) {
        //    return base.VisitStructDeclaration( syntax );
        //}
        // Record
        //public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax syntax) {
        //    return base.VisitRecordDeclaration( syntax );
        //}


        // Helpers
        private static MethodDeclarationSyntax GetMethodDeclarationSyntax_ToString(ClassDeclarationSyntax syntax) {
            var type = syntax.GetAnnotations( "Type" ).Single()!.Data!;
            var members = syntax.GetAnnotations( "Type.Member" ).Select( i => i.Data! ).ToArray();
            return GetMethodDeclarationSyntax_ToString( type, members );
        }
        private static MethodDeclarationSyntax GetMethodDeclarationSyntax_ToString(string type, string[] members) {
            var builder = new StringBuilder();
            builder.AppendLine( "public override string ToString() {" );
            builder.AppendLineFormat( "return \"{0}\";", GetStringValue( type, members ) );
            builder.AppendLine( "}" );
            return (MethodDeclarationSyntax) ParseMemberDeclaration( builder.ToString() )!;
        }
        private static string GetStringValue(string type, string[] members) {
            if (!members.Any()) {
                return string.Format( "Type: {0}", type );
            } else {
                return string.Format( "Type: {0}, {1}", type, members.Join() );
            }
        }


    }
}
