using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace InsuranceApp.WebApi.Security;

public class BearerSecurityOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            return;
        }

        operation.Security = [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", context.Document)] = []
            }
        ];
    }
}
