using Microsoft.AspNetCore.Mvc.ApplicationModels;
using IoBuild.Shared.Infrastructure.ASP.Configuration.Extensions;

namespace IoBuild.Shared.Infrastructure.ASP.Configuration;

/// <summary>
/// Custom routing convention that transforms controller routes to kebab-case.
/// Applied globally in Program.cs.
/// </summary>
public class KebabCaseRouteNamingConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
        {
            if (selector.AttributeRouteModel?.Template != null)
            {
                selector.AttributeRouteModel.Template =
                    selector.AttributeRouteModel.Template.ToKebabCase();
            }
        }

        foreach (var action in controller.Actions)
        {
            foreach (var selector in action.Selectors)
            {
                if (selector.AttributeRouteModel?.Template != null)
                {
                    selector.AttributeRouteModel.Template =
                        selector.AttributeRouteModel.Template.ToKebabCase();
                }
            }
        }
    }
}
