using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeGuidelines.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncVoidAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CG001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Avoid async void",
        messageFormat: "Method '{0}' uses async void. Return Task instead.",
        category: "CodeGuidelines",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
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

        if (!method.ReturnsVoid)
        {
            return;
        }

        // Allow event handlers
        if (IsEventHandler(method))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            Rule,
            method.Locations[0],
            method.Name);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsEventHandler(IMethodSymbol method)
    {
        if (method.Parameters.Length != 2)
        {
            return false;
        }

        var firstParameter = method.Parameters[0].Type;
        var secondParameter = method.Parameters[1].Type;

        return firstParameter.SpecialType == SpecialType.System_Object
               && secondParameter.Name.EndsWith("EventArgs");
    }
}