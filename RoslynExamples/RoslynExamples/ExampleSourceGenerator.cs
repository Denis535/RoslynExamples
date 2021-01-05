#pragma warning disable RS2008 // Enable analyzer release tracking
namespace RoslynExamples {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    [Generator]
    public class ExampleSourceGenerator : ISourceGenerator {

        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "ExampleSourceGenerator",
            "ExampleSourceGenerator",
            "Message: {0}",
            "Example",
            DiagnosticSeverity.Error,
            true );


        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications( () => new ExampleSourceGenerator_SyntaxReceiver() );
        }


        public void Execute(GeneratorExecutionContext context) {
            //Debugger.Launch();
            try {
                var syntaxReceiver = (ExampleSourceGenerator_SyntaxReceiver?) context.SyntaxReceiver ?? throw new Exception( "SyntaxReceiver is null" );
                foreach (var unit in syntaxReceiver.Units) {
                    var content = GetContent( unit );
                    var name = GetName( content );
                    if (name != null) context.AddSource( $"{name}.Generated.cs", content.NormalizeWhitespace().ToString() );
                }
            } catch (Exception ex) {
                context.ReportDiagnostic( Diagnostic.Create( Rule, null, ex ) );
                throw;
            }
        }


        // Helpers
        private static CompilationUnitSyntax GetContent(CompilationUnitSyntax unit) {
            return (CompilationUnitSyntax) new ExampleSourceGenerator_Rewriter().Visit( unit );
        }
        private static string? GetName(CompilationUnitSyntax unit) {
            return unit.DescendantNodes().OfType<TypeDeclarationSyntax>().FirstOrDefault()?.Identifier.Text;
        }


    }

    class ExampleSourceGenerator_SyntaxReceiver : ISyntaxReceiver {

        public List<CompilationUnitSyntax> Units { get; } = new List<CompilationUnitSyntax>();

        void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode node) {
            if (node is CompilationUnitSyntax unit && unit.Members.Any()) {
                Units.Add( unit );
            }
        }

    }

    class ExampleSourceGenerator_Rewriter : CSharpSyntaxRewriter {

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
                .AddMembers( node.Members.OfType<ClassDeclarationSyntax>().Where( IsPartial ).ToArray() );
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
                .AddMembers( node.Members.OfType<MethodDeclarationSyntax>().Where( IsPartial ).ToArray() );
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
            if (IsPartial( node )) {
                node = MethodDeclaration( node.ReturnType, node.Identifier )
                    .WithModifiers( node.Modifiers )
                    .WithTypeParameterList( node.TypeParameterList )
                    .WithParameterList( node.ParameterList )
                    .WithConstraintClauses( node.ConstraintClauses )
                    .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );
            }
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
