using System.Linq;
using FluentAssertions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.ArchitectureTests;

public class ArchitectureTests
{
    private const string DomainProject = "InventoryManagement.Domain";
    private const string ApplicationProject = "InventoryManagement.Application";
    private const string InfrastructureProject = "InventoryManagement.Infrastructure";
    private const string ApiProject = "InventoryManagement.API";

    private Assembly DomainAssembly => Assembly.Load(DomainProject);
    private Assembly ApplicationAssembly => Assembly.Load(ApplicationProject);
    private Assembly InfrastructureAssembly => Assembly.Load(InfrastructureProject);
    private Assembly ApiAssembly => Assembly.Load(ApiProject);

    [Fact]
    public void Domain_Should_Not_HaveDependencyOnOtherProjects()
    {
        var forbiddenDependencies = new[]
        {
            ApplicationProject,
            InfrastructureProject,
            ApiProject
        };

        var references = DomainAssembly.GetReferencedAssemblies();

        references.Should().NotContain(reference => 
            forbiddenDependencies.Contains(reference.Name),
            "Domain layer should not depend on other layers");
    }

    [Fact]
    public void Application_Should_OnlyDependOnDomain()
    {
        var forbiddenDependencies = new[]
        {
            InfrastructureProject,
            ApiProject
        };

        var references = ApplicationAssembly.GetReferencedAssemblies();

        references.Should().Contain(reference => reference.Name == DomainProject,
            "Application layer should depend on Domain layer");
        
        references.Should().NotContain(reference => 
            forbiddenDependencies.Contains(reference.Name),
            "Application layer should not depend on Infrastructure or WebApi layers");
    }

    [Fact]
    public void Commands_Should_FollowNamingConvention()
    {
        var commands = ApplicationAssembly.GetTypes()
            .Where(type => type.Name.EndsWith("Command"))
            .ToList();

        commands.Should().NotBeEmpty("there should be at least one command");
        foreach (var command in commands)
        {
            command.GetProperties()
                .Where(p => p.CanWrite && p.SetMethod?.IsPrivate == false)
                .Should().BeEmpty($"command {command.Name} should have only read-only properties");

            var handlerName = $"{command.Name}Handler";
            var handler = ApplicationAssembly.GetTypes()
                .FirstOrDefault(t => t.Name == handlerName);

            handler.Should().NotBeNull($"command {command.Name} should have a corresponding handler named {handlerName}");
        }
    }

    [Fact]
    public void Queries_Should_FollowNamingConvention()
    {
        var queries = ApplicationAssembly.GetTypes()
            .Where(type => type.Name.EndsWith("Query"))
            .ToList();

        queries.Should().NotBeEmpty("there should be at least one query");
        foreach (var query in queries)
        {
            query.GetProperties()
                .Where(p => p.CanWrite && p.SetMethod?.IsPrivate == false)
                .Should().BeEmpty($"query {query.Name} should have only read-only properties");

            var handlerName = $"{query.Name}Handler";
            var handler = ApplicationAssembly.GetTypes()
                .FirstOrDefault(t => t.Name == handlerName);

            handler.Should().NotBeNull($"query {query.Name} should have a corresponding handler named {handlerName}");
        }
    }

    [Fact]
    public void Entities_Should_FollowConventions()
    {
        var entities = DomainAssembly.GetTypes()
            .Where(type => !type.Name.EndsWith("Event") &&
                         !type.Name.EndsWith("Exception") &&
                         !type.IsAbstract &&
                         type.Namespace?.Contains(".Entities") == true)
            .ToList();

        entities.Should().NotBeEmpty("there should be at least one entity");
        foreach (var entity in entities)
        {
            var idProperty = entity.GetProperty("Id");
            idProperty.Should().NotBeNull($"entity {entity.Name} should have an Id property");

            entity.IsSealed.Should().BeTrue($"entity {entity.Name} should be sealed");

            idProperty?.SetMethod?.IsPrivate.Should().BeTrue(
                $"entity {entity.Name} should have a private setter for Id");
        }
    }

    [Fact]
    public void ValueObjects_Should_FollowConventions()
    {
        var valueObjects = DomainAssembly.GetTypes()
            .Where(type => !type.IsAbstract &&
                         type.Namespace?.Contains(".ValueObjects") == true)
            .ToList();

        valueObjects.Should().NotBeEmpty("there should be at least one value object");
        foreach (var vo in valueObjects)
        {
            vo.GetProperties()
                .Where(p => p.CanWrite && p.SetMethod?.IsPrivate == false)
                .Should().BeEmpty($"value object {vo.Name} should have only read-only properties");

            vo.GetMethod("Equals").Should().NotBeNull(
                $"value object {vo.Name} should override Equals");
            vo.GetMethod("GetHashCode").Should().NotBeNull(
                $"value object {vo.Name} should override GetHashCode");
        }
    }

    [Fact]
    public void DomainEvents_Should_FollowConventions()
    {
        var events = DomainAssembly.GetTypes()
            .Where(type => type.Name.EndsWith("Event") &&
                         !type.IsAbstract &&
                         type.Namespace?.Contains(".Events") == true)
            .ToList();

        events.Should().NotBeEmpty("there should be at least one domain event");
        foreach (var @event in events)
        {
            @event.GetProperties()
                .Where(p => p.CanWrite && p.SetMethod?.IsPrivate == false)
                .Should().BeEmpty($"domain event {@event.Name} should have only read-only properties");

            var timestampProperty = @event.GetProperty("OccurredOn");
            timestampProperty.Should().NotBeNull(
                $"domain event {@event.Name} should have an OccurredOn timestamp");
        }
    }

    [Fact]
    public void Repositories_Should_FollowNamingConvention()
    {
        var repositories = InfrastructureAssembly.GetTypes()
            .Where(type => type.Name.EndsWith("Repository") &&
                         !type.IsInterface &&
                         type.Namespace?.Contains(".Repositories") == true)
            .ToList();

        repositories.Should().NotBeEmpty("there should be at least one repository");
        foreach (var repo in repositories)
        {
            var interfaceName = $"I{repo.Name}";
            var hasInterface = repo.GetInterfaces()
                .Any(i => i.Name == interfaceName);

            hasInterface.Should().BeTrue(
                $"repository {repo.Name} should implement interface {interfaceName}");
        }
    }

    [Fact]
    public void Controllers_Should_FollowConventions()
    {
        var controllers = ApiAssembly.GetTypes()
            .Where(type => type.Name.EndsWith("Controller") &&
                         !type.IsAbstract)
            .ToList();

        controllers.Should().NotBeEmpty("there should be at least one controller");
        foreach (var controller in controllers)
        {
            controller.Should().BeAssignableTo<ControllerBase>(
                $"controller {controller.Name} should inherit from ControllerBase");

            var constructor = controller.GetConstructors().First();
            constructor.GetParameters()
                .Should()
                .Contain(param => param.ParameterType.Name.Contains("IMediator"),
                    $"controller {controller.Name} should have IMediator dependency");
            
            controller.Namespace.Should().Contain(".Controllers",
                $"controller {controller.Name} should be in a Controllers namespace");
        }
    }

    [Fact]
    public void Services_Should_FollowConventions()
    {
        var services = ApplicationAssembly.GetTypes()
            .Where(type => type.Name.EndsWith("Service") &&
                         !type.IsInterface &&
                         type.Namespace?.Contains(".Services") == true)
            .ToList();

        services.Should().NotBeEmpty("there should be at least one service");
        foreach (var service in services)
        {
            var interfaceName = $"I{service.Name}";
            var hasInterface = service.GetInterfaces()
                .Any(i => i.Name == interfaceName);

            hasInterface.Should().BeTrue(
                $"service {service.Name} should implement interface {interfaceName}");
        }
    }
}
