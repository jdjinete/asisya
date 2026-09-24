using System.Reflection;
using Asisya.WebApi.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace Asisya.Application.Tests.Controllers;

/// <summary>
/// Architecture compliance tests ensuring critical endpoints are strictly secured with JWT [Authorize].
/// </summary>
public class ControllerSecurityTests
{
    [Theory]
    [InlineData(typeof(CategoryController))]
    [InlineData(typeof(ProductsController))]
    public void MutationEndpoints_MustBeDecoratedWithAuthorizeAttribute(Type controllerType)
    {
        var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (var method in methods)
        {
            var isMutation = method.GetCustomAttributes().Any(attr =>
                attr is HttpPostAttribute ||
                attr is HttpPutAttribute ||
                attr is HttpDeleteAttribute ||
                attr is HttpPatchAttribute);

            if (isMutation)
            {
                var hasAuthorize = method.GetCustomAttribute<AuthorizeAttribute>() != null ||
                                   controllerType.GetCustomAttribute<AuthorizeAttribute>() != null;

                hasAuthorize.Should().BeTrue(
                    $"Endpoint '{controllerType.Name}.{method.Name}' performs data mutation and MUST be secured with [Authorize].");
            }
        }
    }

    [Fact]
    public void AuditLogsController_MustBeDecoratedWithClassLevelAuthorize()
    {
        var hasAuthorize = typeof(AuditLogsController).GetCustomAttribute<AuthorizeAttribute>() != null;
        hasAuthorize.Should().BeTrue("AuditLogsController contains sensitive compliance logs and must be secured at class level.");
    }

    [Theory]
    [InlineData("GetProducts")]
    [InlineData("GetProductById")]
    public void ProductsController_ReadEndpoints_AreIntentionallyPublicWithAllowAnonymous(string methodName)
    {
        var method = typeof(ProductsController).GetMethod(methodName);
        method.Should().NotBeNull($"Method {methodName} must exist on ProductsController");

        var hasAllowAnonymous = method!.GetCustomAttribute<AllowAnonymousAttribute>() != null;
        hasAllowAnonymous.Should().BeTrue(
            $"Read endpoint '{methodName}' is intentionally public for catalog discovery and must be decorated with [AllowAnonymous].");
    }
}
