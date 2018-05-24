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

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => MakeArrayEmpty(context.Document, diagnostic, c), 
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> MakeArrayEmpty(Document document, Diagnostic diagnostic, CancellationToken c)
        {
            var root = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();
            var arrayCreationExpression = root.FindNode(diagnostic.Location.SourceSpan);

            var x = Array.Empty<int>();
            var generator = SyntaxGenerator.GetGenerator(document);

            var operation = semanticModel.GetOperation(arrayCreationExpression);
            var arrayTypeSymbol = (IArrayTypeSymbol)operation.Type;
            var elementType = arrayTypeSymbol.ElementType;
            
            // genericname expression = Empty<int> (typeExpr, element type)
            var genericName = generator.GenericName("Empty", elementType);
            // type expression = Array
            // member access expression = Array.Empty<int>
            // invocation expression = Array.Empty<int>()

            return document;
        }
    }
}
