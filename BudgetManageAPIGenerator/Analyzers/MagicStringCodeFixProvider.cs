using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Microsoft.CodeAnalysis.Editing;

namespace BudgetManageAPIGenerator.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MagicStringCodeFixProvider)), Shared]
    public class MagicStringCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MagicStringAnalyzer.DiagnosticId);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            context.RegisterCodeFix(
                Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
                    "Replace with constant",
                    c => ReplaceWithConstantAsync(context.Document, diagnostic.Location, c),
                    nameof(MagicStringCodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private async Task<Document> ReplaceWithConstantAsync(Document document, Location diagnosticLocation, CancellationToken cancellationToken)
        {
            // Get the syntax root and the literal expression where the magic string was found
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var stringLiteral = root.FindNode(diagnosticLocation.SourceSpan) as LiteralExpressionSyntax;

            if (stringLiteral == null)
                return document;

            // Extract the magic string value (e.g., "Admin")
            var stringValue = stringLiteral.Token.ValueText;

            // Generate the constant name (e.g., MagicString_Admin)
            var constantName = $"MagicString_{stringValue.Replace(" ", "_")}";

            // Find the containing class declaration
            var classDeclaration = stringLiteral.FirstAncestorOrSelf<ClassDeclarationSyntax>();

            // Check if the constant already exists within the class
            var constantExists = classDeclaration.Members
                .OfType<FieldDeclarationSyntax>()
                .Any(field => field.Declaration.Variables.Any(v => v.Identifier.Text == constantName));

            // Use a SyntaxEditor to apply multiple changes at once
            var editor = new SyntaxEditor(root, document.Project.Solution.Workspace);

            // If the constant doesn't exist, add it to the class
            if (!constantExists)
            {
                // Create the constant declaration
                var constantDeclaration = SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("string"))
                        .WithVariables(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(constantName))
                                    .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(stringValue)
                                    )))
                            )
                        )
                ).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ConstKeyword)))
                .NormalizeWhitespace();

                // Create leading and trailing trivia for proper formatting
                var leadingNewLine = SyntaxFactory.TriviaList(SyntaxFactory.ElasticCarriageReturnLineFeed);
                var trailingNewLine = SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed);

                // Insert the constant declaration at the start of the class
                constantDeclaration = constantDeclaration.WithLeadingTrivia(leadingNewLine).WithTrailingTrivia(trailingNewLine);
                editor.InsertBefore(classDeclaration.Members.First(), constantDeclaration);
            }

            // Replace the string literal with the constant reference
            editor.ReplaceNode(stringLiteral, SyntaxFactory.IdentifierName(constantName));

            // Apply both changes in a single update
            var newRoot = editor.GetChangedRoot();
            return document.WithSyntaxRoot(newRoot);
        }






    }
}
