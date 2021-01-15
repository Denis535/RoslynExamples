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
            context.RegisterForSyntaxNotifications( () => new ExampleSyntaxReceiver() );
        }


        public void Execute(GeneratorExecutionContext context) {
            //Debugger.Launch();
            var receiver = (ExampleSyntaxReceiver?) context.SyntaxReceiver ?? throw new Exception( "SyntaxReceiver is null" );
            var types = GetSymbols( receiver.Types, context.Compilation );

            foreach (var unit in receiver.Units) {
                var name = GetSourceName( unit );
                var content = GetSourceContent( unit, types );
                if (content != null) {
                    context.AddSource( name, content.NormalizeWhitespace().ToString() );
                }
            }
        }


        // Helpers
        private static INamedTypeSymbol[] GetSymbols(IEnumerable<TypeDeclarationSyntax> types, Compilation compilation) {
            return types.Select( i => GetSymbol( i, compilation ) ).Distinct( SymbolEqualityComparer.Default ).Cast<INamedTypeSymbol>().ToArray();
        }
        private static INamedTypeSymbol GetSymbol(TypeDeclarationSyntax type, Compilation compilation) {
            var model = compilation.GetSemanticModel( type.SyntaxTree );
            return model.GetDeclaredSymbol( type ) ?? throw new Exception( $"Symbol is not found: Node={type.Identifier}" );
        }
        private static string GetSourceName(CompilationUnitSyntax unit) {
            return Path.GetFileNameWithoutExtension( unit.SyntaxTree.FilePath ) + $".Generated.{Guid.NewGuid()}.cs";
        }
        private static CompilationUnitSyntax? GetSourceContent(CompilationUnitSyntax unit, INamedTypeSymbol[] types) {
            unit = (CompilationUnitSyntax) new ExampleSyntaxProducer().Visit( unit );
            unit = (CompilationUnitSyntax) new ExampleSyntaxProducer2( types ).Visit( unit );
            if (unit.DescendantNodes().OfType<TypeDeclarationSyntax>().Any()) return unit;
            return null;
        }

    }

    // ExampleSyntaxReceiver
    // Collects syntax nodes
    class ExampleSyntaxReceiver : ISyntaxReceiver {

        public List<CompilationUnitSyntax> Units { get; } = new List<CompilationUnitSyntax>();
        public List<TypeDeclarationSyntax> Types { get; } = new List<TypeDeclarationSyntax>();

        void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode node) {
            if (node is CompilationUnitSyntax unit) Units.Add( unit );
            if (node is TypeDeclarationSyntax type) Types.Add( type );
        }

    }

    // ExampleSyntaxProducer
    // Produces empty partial classes (skeletons) for each partial class
    class ExampleSyntaxProducer : CSharpSyntaxRewriter {


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
            node = ClassDeclaration( node.Identifier )
                .WithModifiers( node.Modifiers )
                .WithTypeParameterList( node.TypeParameterList );
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
        //        .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ); // only method declaration
        //    return base.VisitMethodDeclaration( node );
        //}


        // Helpers
        private static bool IsPartial(ClassDeclarationSyntax node) {
            return node.Modifiers.Any( i => i.Kind() == SyntaxKind.PartialKeyword );
        }
        private static bool IsPartial(MethodDeclarationSyntax node) {
            return node.Modifiers.Any( i => i.Kind() == SyntaxKind.PartialKeyword );
        }


    }

    // ExampleSyntaxProducer
    // todo: how to get INamedTypeSymbol for ClassDeclarationSyntax?
    class ExampleSyntaxProducer2 : CSharpSyntaxRewriter {

        private INamedTypeSymbol[] Types { get; set; }

        public ExampleSyntaxProducer2(INamedTypeSymbol[] types) {
            Types = types;
        }


        // Interface
        //public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
        //    return base.VisitInterfaceDeclaration( node );
        //}
        // Class
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) {
            //node = node.AddMembers( ParseMemberDeclaration( "public static string HelloWorld() => \"Hello World !!!\";" )! );
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


    }

}
