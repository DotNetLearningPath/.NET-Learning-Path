using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeGuidelines.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThreadSleepInTestsAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CG004";

    private static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticId,
        title: "Thread.Sleep is not allowed in unit tests",
        messageFormat: "Thread.Sleep is used in a test. Why: it makes tests slow and flaky. Fix: use async waiting, mocks, fake time, or proper synchronization instead.",
        category: "CodeGuidelines",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(_rule);

    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(
            AnalyzeInvocation,
            SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (!IsInsideTestProject(context))
        {
            return;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (memberAccess.Name.Identifier.Text != "Sleep")
        {
            return;
        }

        var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;

        if (symbol is null)
        {
            return;
        }

        if (symbol.Name != "Sleep")
        {
            return;
        }

        if (symbol.ContainingType.ToDisplayString() != "System.Threading.Thread")
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            _rule,
            invocation.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsInsideTestProject(SyntaxNodeAnalysisContext context)
    {
        var treePath = context.Node.SyntaxTree.FilePath;

        if (string.IsNullOrWhiteSpace(treePath))
        {
            return false;
        }

        return treePath.Contains("Test", StringComparison.OrdinalIgnoreCase)
               || treePath.Contains("Tests", StringComparison.OrdinalIgnoreCase);
    }
}