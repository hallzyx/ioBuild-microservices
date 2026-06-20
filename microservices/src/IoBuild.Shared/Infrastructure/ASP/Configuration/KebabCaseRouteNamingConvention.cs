using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using IoBuild.Shared.Infrastructure.ASP.Configuration.Extensions;

namespace IoBuild.Shared.Infrastructure.ASP.Configuration;

/// <summary>
/// Custom routing convention that transforms controller routes to kebab-case.
/// Applied globally in Program.cs.
/// </summary>
public partial class KebabCaseRouteNamingConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
        {
            if (selector.AttributeRouteModel?.Template != null)
            {
                selector.AttributeRouteModel.Template =
                    KebabCaseTemplate(selector.AttributeRouteModel.Template);
            }
        }

        foreach (var action in controller.Actions)
        {
            foreach (var selector in action.Selectors)
            {
                if (selector.AttributeRouteModel?.Template != null)
                {
                    selector.AttributeRouteModel.Template =
                        KebabCaseTemplate(selector.AttributeRouteModel.Template);
                }
            }
        }
    }

    /// <summary>
    /// Kebab-cases the literal parts of a route template while leaving route
    /// parameter tokens (e.g. <c>{projectId}</c>) untouched. Transforming the
    /// tokens would rename them (<c>{projectId}</c> → <c>{project-id}</c>),
    /// breaking model binding to the action parameter so it silently defaults
    /// to 0 — which is exactly what made every <c>by-project/{projectId}</c>
    /// endpoint return empty results.
    /// </summary>
    private static string KebabCaseTemplate(string template) =>
        TokenOrLiteralRegex().Replace(
            template,
            match => match.Value.StartsWith('{')
                ? match.Value                  // preserve {param} tokens verbatim
                : match.Value.ToKebabCase());  // kebab-case literal segments only

    // Matches either a whole {...} route token or a run of non-brace literal text.
    [GeneratedRegex(@"\{[^}]*\}|[^{}]+")]
    private static partial Regex TokenOrLiteralRegex();
}
