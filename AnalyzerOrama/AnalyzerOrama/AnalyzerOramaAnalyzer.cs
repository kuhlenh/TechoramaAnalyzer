using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace AnalyzerOrama
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AnalyzerOramaAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AnalyzerOrama";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Best Practices";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // check if Array.Empty is even available to use

            // register callback when analysis sees our node/symbol/operation
            context.RegisterOperationAction(AnalyzeOperation, OperationKind.ArrayCreation);
        }

        private void AnalyzeOperation(OperationAnalysisContext context)
        {
            var arrayCreation = (IArrayCreationOperation)context.Operation;

            if (arrayCreation.DimensionSizes.Length == 1 && 
                arrayCreation.DimensionSizes[0].ConstantValue.HasValue)
            {
                object dimensions = arrayCreation.DimensionSizes[0].ConstantValue.Value;
                if (dimensions is 0)
                {
                    var diagnostic = Diagnostic.Create(Rule, arrayCreation.Syntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
