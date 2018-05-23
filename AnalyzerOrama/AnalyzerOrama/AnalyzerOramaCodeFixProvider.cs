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
            //context.RegisterCodeFix(
            //    CodeAction.Create(
            //        title: title,
            //        createChangedDocument: c => //put callback for fix here, 
            //        equivalenceKey: title),
            //    diagnostic);
        }
    }
}
