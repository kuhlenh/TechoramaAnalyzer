using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;

namespace AnalyzerOrama
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AnalyzerOramaCodeFixProvider)), Shared]
    public class AnalyzerOramaCodeFixProvider : CodeFixProvider
    {
        private const string title = "Use Array.Empty<T>";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AnalyzerOramaAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            // get the reported diagnostic
            var diagnostic = context.Diagnostics.First();

            //// Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => MakeArrayEmpty(context.Document, diagnostic, c), 
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> MakeArrayEmpty(Document document, Diagnostic diagnostic, CancellationToken c)
        {
            var x = Array.Empty<string>();
            var root = await document.GetSyntaxRootAsync();
            var arrayCreation = root.FindNode(diagnostic.Location.SourceSpan);

            var generator = SyntaxGenerator.GetGenerator(document);
            var semanticModel = await document.GetSemanticModelAsync();
            //identifier
            var array = semanticModel.Compilation.GetTypeByMetadataName("System.Array");
            var identifier = generator.TypeExpression(array);
            //generic
            var operation = semanticModel.GetOperation(arrayCreation);
            var arrayType = (IArrayTypeSymbol)operation.Type;
            var generic = generator.GenericName("Empty", arrayType.ElementType);
            //memberaccess
            var memberAccessExpression = generator.MemberAccessExpression(identifier, generic);
            //invocation
            var invocationExpression = generator.InvocationExpression(memberAccessExpression);

            var newRoot = root.ReplaceNode(arrayCreation, invocationExpression);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
