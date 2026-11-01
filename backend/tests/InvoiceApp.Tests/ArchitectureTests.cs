using System.Reflection;
using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

/// <summary>
/// Architecture Tests verifying structural integrity, clean domain boundaries,
/// and preventing leakage of infrastructure, API, or external payment SDKs into Domain.
/// </summary>
public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(Invoice).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Api_Or_Infrastructure()
    {
        // Act: Extract all assemblies referenced by InvoiceApp.Domain
        var referencedAssemblies = DomainAssembly.GetReferencedAssemblies();

        // Forbidden assembly prefixes that domain must NEVER depend upon
        var forbiddenPrefixes = new[]
        {
            "InvoiceApp.Api",
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "Azure.Messaging.ServiceBus",
            "System.Data",
            "Microsoft.Data.SqlClient"
        };

        foreach (var reference in referencedAssemblies)
        {
            foreach (var forbidden in forbiddenPrefixes)
            {
                Assert.False(
                    reference.Name?.StartsWith(forbidden, StringComparison.OrdinalIgnoreCase) == true,
                    $"Architectural Boundary Violation: Domain references '{reference.Name}', violating Clean Architecture isolation."
                );
            }
        }
    }

    [Fact]
    public void Domain_Entities_Must_Not_Reference_External_Payment_Gateways()
    {
        // Act: Inspect all types in Domain assembly
        var domainTypes = DomainAssembly.GetTypes();

        foreach (var type in domainTypes)
        {
            // Fields and properties must not reference any external payment client or infrastructure
            var fieldsAndProps = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.MemberType is MemberTypes.Field or MemberTypes.Property);

            foreach (var member in fieldsAndProps)
            {
                var memberTypeStr = member.ToString() ?? string.Empty;
                Assert.DoesNotContain("PayEx", memberTypeStr, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("PaymentGateway", memberTypeStr, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void Domain_Types_Must_Not_Import_Web_Or_Http_Namespaces()
    {
        // Act: Check namespaces used across all types in the domain assembly
        var types = DomainAssembly.GetTypes();

        foreach (var type in types)
        {
            // Verify that no type resides in an Api or Web namespace
            Assert.DoesNotContain(".Api", type.Namespace ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".Controllers", type.Namespace ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
    }
}

