using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Linq;

namespace BudgetManageAPIGenerator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MagicStringAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MagicString";
        private static readonly LocalizableString Title = "Magic String";
        private static readonly LocalizableString MessageFormat = "Avoid using magic strings: '{0}'";
        private static readonly LocalizableString Description = "Magic strings should be replaced with constants.";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.StringLiteralExpression);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {

            var document = context.Node.SyntaxTree;
            var filePath = document.FilePath;

            // Skip analysis for files in "Migrations" or "EF-generated" folders
            if (filePath != null && (filePath.Contains("Migrations") || filePath.Contains("EF-generated")))
            {
                return;
            }


            var stringLiteral = (LiteralExpressionSyntax)context.Node;

            // Check if the string is already assigned to a constant
            var parent = stringLiteral.Parent;

            // Look for constant declarations in the same class
            var classDeclaration = parent.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration != null)
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(stringLiteral);
                if (symbolInfo.Symbol != null && symbolInfo.Symbol.Kind == SymbolKind.Field)
                {
                    // If it's a constant that has been previously defined, do not warn
                    return;
                }

                // Check if the string is already defined as a constant in the class
                var constants = classDeclaration.Members
                    .OfType<FieldDeclarationSyntax>()
                    .SelectMany(field => field.Declaration.Variables)
                    .ToList(); // Use List for compatibility

                // Check if the constant name contains "MagicString" and skip warning if it does
                foreach (var constant in constants)
                {
                    if (constant.Identifier.Text.Contains("MagicString"))
                    {
                        if (constant.Initializer?.Value.ToString() == stringLiteral.Token.ValueText)
                        {
                            return; // Skip warning if it's assigned to a MagicString constant
                        }
                    }
                }

                // Check if the string is used in an attribute
                var attribute = parent.FirstAncestorOrSelf<AttributeSyntax>();
                if (attribute != null)
                {
                    return; // Skip warning if it's used in an attribute
                }
            }

            // If it's a magic string that hasn't been replaced by a constant, report a warning
            var diagnostic = Diagnostic.Create(Rule, stringLiteral.GetLocation(), stringLiteral.Token.ValueText);
            context.ReportDiagnostic(diagnostic);
        }



    }

}