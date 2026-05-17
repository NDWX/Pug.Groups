using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Pug.Application.Security;
using Pug.Groups.Common;
using Pug.Groups.Models;

namespace Pug.Groups.WebApi;

public static class GroupsEndpoints
{
    /// <summary>
    /// Maps Groups API endpoints onto the given route builder.
    /// The caller controls the base path — pass a <see cref="RouteGroupBuilder"/> from
    /// <c>app.MapGroup("/your/prefix")</c> to mount under a prefix, or pass <c>app</c>
    /// directly to mount at the application root.
    /// All endpoints require an authenticated caller. Swagger documentation is surfaced
    /// when the host configures <c>AddEndpointsApiExplorer()</c> and <c>AddSwaggerGen()</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapGroupsEndpoints(this IEndpointRouteBuilder routes)
    {
        // Literal-segment routes are registered before parameterised routes of the same
        // HTTP method to ensure the routing system resolves them correctly.

        routes.MapGet("", GetGroups)
            .RequireAuthorization()
            .WithName("GetGroups")
#if NET7_0_OR_GREATER
            .WithTags("Groups")
            .WithSummary("Get groups in a domain")
#endif
            .Produces<IEnumerable<GroupInfo>>(StatusCodes.Status200OK)
            .Produces<BadRequestResultInfo>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        routes.MapGet("memberships", GetMemberships)
            .RequireAuthorization()
            .WithName("GetMemberships")
#if NET7_0_OR_GREATER
            .WithTags("Groups")
            .WithSummary("Get group memberships for a subject")
#endif
            .Produces<IEnumerable<Membership>>(StatusCodes.Status200OK)
            .Produces<BadRequestResultInfo>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        routes.MapGet("{identifier}", GetGroup)
            .RequireAuthorization()
            .WithName("GetGroup")
#if NET7_0_OR_GREATER
            .WithTags("Groups")
            .WithSummary("Get a group by identifier")
#endif
            .Produces<GroupDefinition>(StatusCodes.Status200OK)
            .Produces<BadRequestResultInfo>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        routes.MapGet("{identifier}/memberships", GetGroupMemberships)
            .RequireAuthorization()
            .WithName("GetGroupMemberships")
#if NET7_0_OR_GREATER
            .WithTags("Groups")
            .WithSummary("Get members of a group")
#endif
            .Produces<IEnumerable<Membership>>(StatusCodes.Status200OK)
            .Produces<BadRequestResultInfo>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        routes.MapPut("", AddGroup)
            .RequireAuthorization()
            .WithName("AddGroup")
#if NET7_0_OR_GREATER
            .WithTags("Groups")
            .WithSummary("Add a new group")
#endif
            // NOTE: The original GroupsController.AddGroup returns HTTP 200 (Ok) at runtime
            // despite its Swagger annotation advertising 201. This implementation honours
            // the swagger contract and returns 201 Created — a deliberate behavioural change.
            .Produces(StatusCodes.Status201Created)
            .Produces<BadRequestResultInfo>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        routes.MapPut("memberships", AddToGroups)
            .RequireAuthorization()
            .WithName("AddToGroups")
#if NET7_0_OR_GREATER
            .WithTags("Groups")
            .WithSummary("Add a subject to multiple groups")
#endif
            .Produces(StatusCodes.Status200OK)
            .Produces<BadRequestResultInfo>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        routes.MapPut("{identifier}/memberships", AddMembers)
            .RequireAuthorization()
            .WithName("AddMembers")
#if NET7_0_OR_GREATER
            .WithTags("Groups")
            .WithSummary("Add members to a group")
#endif
            .Produces(StatusCodes.Status200OK)
            .Produces<BadRequestResultInfo>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        routes.MapDelete("{identifier}", DeleteGroup)
            .RequireAuthorization()
            .WithName("DeleteGroup")
#if NET7_0_OR_GREATER
            .WithTags("Groups")
            .WithSummary("Delete a group")
#endif
            .Produces(StatusCodes.Status200OK)
            .Produces<BadRequestResultInfo>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        routes.MapDelete("{identifier}/memberships", DeleteGroupMembers)
            .RequireAuthorization()
            .WithName("DeleteGroupMembers")
#if NET7_0_OR_GREATER
            .WithTags("Groups")
            .WithSummary("Remove a member from a group")
#endif
            .Produces(StatusCodes.Status200OK)
            .Produces<BadRequestResultInfo>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return routes;
    }

    private static async Task<IResult> GetGroups(
        [FromQuery] string domain,
        [FromQuery] string? name,
        [FromServices] IGroups groups)
    {
        if (string.IsNullOrEmpty(domain))
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(domain)));

        try
        {
            IEnumerable<GroupInfo> groupInfos = await groups.GetGroupsAsync(domain, name).ConfigureAwait(false);
            return Results.Ok(groupInfos);
        }
        catch (NotAuthorized)
        {
            return Results.Forbid();
        }
    }

    private static async Task<IResult> GetMemberships(
        [FromQuery] string domain,
        [FromQuery] string subjectIdentifier,
        [FromQuery] string subjectType,
        [FromQuery] bool recursive,
        [FromServices] IGroups groups)
    {
        if (string.IsNullOrEmpty(domain))
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(domain)));

        if (string.IsNullOrEmpty(subjectIdentifier))
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(subjectIdentifier)));

        if (string.IsNullOrEmpty(subjectType))
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(subjectType)));

        try
        {
            Subject subject = new Subject
            {
                Identifier = subjectIdentifier,
                Type = subjectType
            };

            IEnumerable<Membership> memberships = await groups.GetMemberships(domain, subject, recursive).ConfigureAwait(false);
            return Results.Ok(memberships);
        }
        catch (NotAuthorized)
        {
            return Results.Forbid();
        }
    }

    private static async Task<IResult> GetGroup(
        [FromRoute] string identifier,
        [FromServices] IGroups groups)
    {
        if (string.IsNullOrEmpty(identifier))
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(identifier)));

        IGroup? group = await groups.GetGroupAsync(identifier).ConfigureAwait(false);

        if (group is null)
            return Results.NotFound();

        try
        {
            return Results.Ok(await group.GetDefinitionAsync().ConfigureAwait(false));
        }
        catch (NotAuthorized)
        {
            return Results.Forbid();
        }
    }

    private static async Task<IResult> GetGroupMemberships(
        [FromRoute] string identifier,
        [FromServices] IGroups groups)
    {
        if (string.IsNullOrEmpty(identifier))
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(identifier)));

        try
        {
            IGroup group = await groups.GetGroupAsync(identifier).ConfigureAwait(false);
            IEnumerable<Membership> memberships = await group.GetMembershipsAsync().ConfigureAwait(false);
            return Results.Ok(memberships);
        }
        catch (NotAuthorized)
        {
            return Results.Forbid();
        }
    }

    private static async Task<IResult> AddGroup(
        [FromBody] GroupDefinition? definition,
        [FromServices] IGroups groups)
    {
        if (definition is null)
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(definition)));

        try
        {
            var result = await groups.AddGroupAsync(definition).ConfigureAwait(false);
            return Results.Created(string.Empty, result);
        }
        catch (NotAuthorized)
        {
            return Results.Forbid();
        }
    }

    private static async Task<IResult> AddToGroups(
        [FromQuery] string subjectIdentifier,
        [FromQuery] string subjectType,
        [FromBody] IEnumerable<string> groups,
        [FromServices] IGroups groupsService)
    {
        if (subjectIdentifier == null)
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(subjectIdentifier)));

        if (subjectType == null)
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(subjectType)));

        if (groups == null || !groups.Any())
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(groups)));

        try
        {
            Subject subject = new Subject
            {
                Type = subjectType,
                Identifier = subjectIdentifier
            };

            await groupsService.AddToGroupsAsync(subject, groups).ConfigureAwait(false);
            return Results.Ok();
        }
        catch (NotAuthorized)
        {
            return Results.Forbid();
        }
        catch (UnknownGroupException)
        {
            return Results.NotFound();
        }
        catch (ArgumentException e)
        {
            return Results.BadRequest(new BadRequestResultInfo
            {
                Type = "MISSING_ARGUMENT",
                RequestArgument = e.ParamName,
                Message = e.Message
            });
        }
    }

    private static async Task<IResult> AddMembers(
        [FromRoute] string identifier,
        [FromBody] IEnumerable<Subject> subjects,
        [FromServices] IGroups groups)
    {
        if (identifier == null)
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(identifier)));

        if (subjects == null || !subjects.Any())
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(subjects)));

        try
        {
            IGroup group = await groups.GetGroupAsync(identifier).ConfigureAwait(false);
            await group.AddMembersAsync(subjects).ConfigureAwait(false);
            return Results.Ok();
        }
        catch (NotAuthorized)
        {
            return Results.Forbid();
        }
    }

    private static async Task<IResult> DeleteGroup(
        [FromRoute] string identifier,
        [FromServices] IGroups groups)
    {
        if (string.IsNullOrEmpty(identifier))
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(identifier)));

        try
        {
            await groups.DeleteGroupAsync(identifier).ConfigureAwait(false);
            return Results.Ok();
        }
        catch (NotAuthorized)
        {
            return Results.Forbid();
        }
    }

    private static async Task<IResult> DeleteGroupMembers(
        [FromRoute] string identifier,
        [FromQuery] string subjectType,
        [FromQuery] string subjectIdentifier,
        [FromServices] IGroups groups)
    {
        if (string.IsNullOrEmpty(identifier))
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(identifier)));

        if (string.IsNullOrEmpty(subjectType))
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(subjectType)));

        if (string.IsNullOrEmpty(subjectIdentifier))
            return Results.BadRequest(new BadRequestResultInfo("MISSING_ARGUMENT", nameof(subjectIdentifier)));

        try
        {
            IGroup group = await groups.GetGroupAsync(identifier).ConfigureAwait(false);

            Subject subject = new Subject
            {
                Type = subjectType,
                Identifier = subjectIdentifier
            };

            await group.RemoveMemberAsync(subject).ConfigureAwait(false);
            return Results.Ok();
        }
        catch (NotAuthorized)
        {
            return Results.Forbid();
        }
        catch (ArgumentException e)
        {
            return Results.BadRequest(new BadRequestResultInfo
            {
                Type = "MISSING_ARGUMENT",
                RequestArgument = e.ParamName,
                Message = e.Message
            });
        }
    }
}
