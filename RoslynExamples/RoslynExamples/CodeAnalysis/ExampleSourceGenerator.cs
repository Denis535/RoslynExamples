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
        private class SyntaxReceiver : ISyntaxReceiver {
            public IList<CompilationUnitSyntax> Units { get; } = new List<CompilationUnitSyntax>();
            public void OnVisitSyntaxNode(SyntaxNode syntax) {
                if (syntax is CompilationUnitSyntax unit) Units.Add( unit );
            }
        }

        private static readonly DiagnosticDescriptor ErrorDiagnosticDescriptor = new DiagnosticDescriptor(
            "SourceGenerator",
            "SourceGenerator",
            "Error: {0}",
            "Error",
            DiagnosticSeverity.Error,
            true );


        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications( () => new SyntaxReceiver() );
        }


        public void Execute(GeneratorExecutionContext context) {
#if DEBUG
            //Debugger.Launch();
#endif
            var compilation = context.Compilation;
            var receiver = (SyntaxReceiver) context.SyntaxReceiver!;
            foreach (var unit in receiver.Units) {
                if (unit.SyntaxTree.FilePath.Contains( ".nuget" )) continue;
                if (unit.SyntaxTree.FilePath.Contains( "\\obj\\Debug\\" )) continue;
                if (unit.SyntaxTree.FilePath.Contains( "\\obj\\Release\\" )) continue;

                try {
                    var model = compilation.GetSemanticModel( unit.SyntaxTree );
                    Execute( context, unit, model );
                } catch (Exception ex) {
                    context.ReportDiagnostic( Diagnostic.Create( ErrorDiagnosticDescriptor, null, ex.Message ) );
                }
            }
        }
        private static void Execute(GeneratorExecutionContext context, CompilationUnitSyntax unit, SemanticModel model) {
            var name = GetGeneratedSourceName( unit );
            var source = GetGeneratedSource( unit, model );
            if (source != null) {
                Debug.WriteLine( "Generated source: " + name );
                Debug.WriteLine( source );
                context.AddSource( name, source );
            }
        }


        // Helpers
        private static string GetGeneratedSourceName(CompilationUnitSyntax unit) {
            return Path.GetFileNameWithoutExtension( unit.SyntaxTree.FilePath ) + $".Generated.{Guid.NewGuid()}.cs";
        }
        private static string? GetGeneratedSource(CompilationUnitSyntax unit, SemanticModel model) {
            unit = (CompilationUnitSyntax) new ExampleSyntaxProducer0( model ).Visit( unit );
            unit = (CompilationUnitSyntax) new ExampleSyntaxProducer1().Visit( unit );
            unit = (CompilationUnitSyntax) new ExampleSyntaxProducer2().Visit( unit );
            if (unit.DescendantNodes().OfType<TypeDeclarationSyntax>().Any()) return unit?.NormalizeWhitespace().ToString();
            return null;
        }


    }


    // Initializes ClassDeclarationSyntax nodes with annotations
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
            if (IsSupported( node )) {
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
        internal static bool IsSupported(ClassDeclarationSyntax node) {
            return !CodeAnalysisUtils.IsStatic( node ) && CodeAnalysisUtils.IsPartial( node );
        }
        private static IEnumerable<SyntaxAnnotation> GetSyntaxAnnotations(ClassDeclarationSyntax node, SemanticModel model) {
            var type = model.GetDeclaredSymbol( node )!;
            var members = type.GetMembers().Where( i => i.Kind != SymbolKind.NamedType ).Where( i => i.CanBeReferencedByName ).Where( i => !i.IsImplicitlyDeclared );
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
                .AddMembers( node.Members.OfType<ClassDeclarationSyntax>().Where( ExampleSyntaxProducer0.IsSupported ).ToArray() )
                .AddMembers( node.Members.OfType<NamespaceDeclarationSyntax>().ToArray() );
            return base.VisitCompilationUnit( node );
        }
        // Namespace
        public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) {
            node = NamespaceDeclaration( node.Name )
                .WithExterns( node.Externs )
                .WithUsings( node.Usings )
                .AddMembers( node.Members.OfType<ClassDeclarationSyntax>().Where( ExampleSyntaxProducer0.IsSupported ).ToArray() );
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
                .AddMembers( node.Members.OfType<ClassDeclarationSyntax>().Where( ExampleSyntaxProducer0.IsSupported ).ToArray() )
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


    }

    // Produces partial classes with additional methods
    class ExampleSyntaxProducer2 : CSharpSyntaxRewriter {


        // Interface
        //public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
        //    return base.VisitInterfaceDeclaration( node );
        //}
        // Class
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) {
            node = node.AddMembers( CreateMethodDeclarationSyntax_ToString( node ) );
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
        private static MethodDeclarationSyntax CreateMethodDeclarationSyntax_ToString(ClassDeclarationSyntax node) {
            var type = node.GetAnnotations( "Type" ).Single()!.Data!;
            var members = node.GetAnnotations( "Type.Member" ).Select( i => i.Data! ).ToArray();
            return CreateMethodDeclarationSyntax_ToString( GetStringValue( type, members ) );
        }
        private static MethodDeclarationSyntax CreateMethodDeclarationSyntax_ToString(string @string) {
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
