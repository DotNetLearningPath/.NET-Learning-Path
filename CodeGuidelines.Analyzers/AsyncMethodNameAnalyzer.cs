using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeGuidelines.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncMethodNameAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CG002";

    private static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticId,
        title: "Async method should end with Async",
        messageFormat: "Async method '{0}' should end with 'Async'. Why: the suffix makes asynchronous methods easy to identify. Fix: rename it to '{0}Async' or choose a clear Async name.",
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

        context.RegisterSymbolAction(
            AnalyzeMethod,
            SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (!method.IsAsync)
        {
            return;
        }

        if (method.ReturnsVoid)
        {
            return;
        }

        if (method.Name.EndsWith("Async", StringComparison.Ordinal))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            _rule,
            method.Locations[0],
            method.Name);

        context.ReportDiagnostic(diagnostic);
    }
}