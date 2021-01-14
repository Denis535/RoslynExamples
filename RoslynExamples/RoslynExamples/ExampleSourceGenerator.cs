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
            context.RegisterForSyntaxNotifications( () => new CompilationUnitReceiver() );
        }


        public void Execute(GeneratorExecutionContext context) {
            //Debugger.Launch();
            var receiver = (CompilationUnitReceiver?) context.SyntaxReceiver ?? throw new Exception( "SyntaxReceiver is null" );
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
            var result = (CompilationUnitSyntax) new ExampleSourceProducer( model ).Visit( unit );
            if (result.DescendantNodes().OfType<TypeDeclarationSyntax>().Any()) return result;
            return null;
        }

    }

    // CompilationUnitReceiver
    class CompilationUnitReceiver : ISyntaxReceiver {

        public List<CompilationUnitSyntax> Units { get; } = new List<CompilationUnitSyntax>();

        void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode node) {
            if (node is CompilationUnitSyntax unit) {
                Units.Add( unit );
            }
        }

    }

    // ExampleSourceProducer
    class ExampleSourceProducer : CSharpSyntaxRewriter {

        private SemanticModel Model { get; set; }


        public ExampleSourceProducer(SemanticModel model) {
            Model = model;
        }


        // CompilationUnit
        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node) {
            node = CompilationUnit()
                .WithExterns( node.Externs )
                .WithUsings( node.Usings )
                .AddMembers( node.Members.ToArray() );
            return base.VisitCompilationUnit( node );
        }
        // Namespace
        public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) {
            node = NamespaceDeclaration( node.Name )
                .WithExterns( node.Externs )
                .WithUsings( node.Usings )
                .AddMembers( node.Members.OfType<ClassDeclarationSyntax>().Where( IsPartial ).ToArray() ); // only partial classes
            return base.VisitNamespaceDeclaration( node );
        }

        // Interface
        //public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
        //    return base.VisitInterfaceDeclaration( node );
        //}
        // Class
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) {
            //var symbol = Model.GetDeclaredSymbol( node );
            //Trace.WriteLine( symbol?.Name ?? "Null" );
            node = ClassDeclaration( node.Identifier )
                .WithModifiers( node.Modifiers )
                .WithTypeParameterList( node.TypeParameterList );
            //.AddMembers( node.Members.OfType<MethodDeclarationSyntax>().Where( IsPartial ).ToArray() ) // only partial methods
            //.AddMembers( ParseMemberDeclaration( "public static string HelloWorld() => \"Hello World !!!\";" )! );
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
        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node) {
            //node = MethodDeclaration( node.ReturnType, node.Identifier )
            //    .WithModifiers( node.Modifiers )
            //    .WithTypeParameterList( node.TypeParameterList )
            //    .WithParameterList( node.ParameterList )
            //    .WithConstraintClauses( node.ConstraintClauses )
            //    .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ); // only method declaration
            return base.VisitMethodDeclaration( node );
        }


        // Helpers
        private static bool IsPartial(ClassDeclarationSyntax node) {
            return node.Modifiers.Any( i => i.Kind() == SyntaxKind.PartialKeyword );
        }
        private static bool IsPartial(MethodDeclarationSyntax node) {
            return node.Modifiers.Any( i => i.Kind() == SyntaxKind.PartialKeyword );
        }


    }

}
