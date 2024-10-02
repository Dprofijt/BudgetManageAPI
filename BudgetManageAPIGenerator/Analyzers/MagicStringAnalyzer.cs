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
        public const string DiagnosticId = "MAGIC_STRING";
        private const string Title = "Magic String Detected";
        private const string MessageFormat = "The string '{0}' is a magic string";
        private const string Description = "Avoid using magic strings. Use constants or enums instead.";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.StringLiteralExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var stringLiteral = (LiteralExpressionSyntax)context.Node;
            var value = stringLiteral.Token.ValueText;

            // Check if the string is a magic string
            if (IsMagicString(value))
            {
                // Check if the magic string is already replaced with a constant
                var parent = stringLiteral.FirstAncestorOrSelf<MemberDeclarationSyntax>();
                var hasConstant = parent.DescendantNodes()
                    .OfType<FieldDeclarationSyntax>()
                    .Any(field => field.Declaration.Variables.Any(v => v.Identifier.Text == $"MagicString_{value.Replace("\"", "").Replace(" ", "_")}"));

                // Report diagnostic if it's not replaced by a constant
                if (!hasConstant)
                {
                    var diagnostic = Diagnostic.Create(Rule, stringLiteral.GetLocation(), value);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }


        private bool IsMagicString(string value)
        {
            // Define your logic to determine if the string is "magic"
            // For example, magic strings might be defined in a list:
            var magicStrings = new[] { "Admin", "User", "Default", "Active" };
            return Array.Exists(magicStrings, magicString => magicString == value);
        }
    }
}
