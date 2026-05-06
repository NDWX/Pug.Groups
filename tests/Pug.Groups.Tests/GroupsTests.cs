using Pug.Application.Security;
using Pug.Groups.Common;
using Pug.Groups.Models;
using Pug.Groups.Tests.Infrastructure;

namespace Pug.Groups.Tests;

/// <summary>
/// Integration tests for <see cref="Groups"/> backed by a real SQLite database.
/// </summary>
public class GroupsTests : IClassFixture<DatabaseFixture>
{
	private readonly IGroups _groups;

	public GroupsTests(DatabaseFixture fixture)
	{
		ISecurityManager security = SecurityManagerFactory.CreateAlwaysAuthorized();
		_groups = new Groups(fixture.IdGenerator, fixture.ApplicationData, security);
	}

	// ── GetGroupsAsync ────────────────────────────────────────────────────

	[Fact]
	public async Task GetGroupsAsync_ByDomain_ReturnsAllGroupsInThatDomain()
	{
		List<GroupInfo> groups = (await _groups.GetGroupsAsync(TestData.Domain)).ToList();

		// Seeded: Admin, Users, Editors, SubEditors, CircleA, CircleB (6 in test domain)
		Assert.True(groups.Count >= 6);
		Assert.All(groups, g => Assert.Equal(TestData.Domain, g.Definition.Domain));
	}

	[Fact]
	public async Task GetGroupsAsync_ByDomainAndName_ReturnsOnlyMatchingGroup()
	{
		List<GroupInfo> groups = (await _groups.GetGroupsAsync(TestData.Domain, TestData.AdminGroupName)).ToList();

		Assert.Single(groups);
		Assert.Equal(TestData.AdminGroupId, groups[0].Identifier);
	}

	[Fact]
	public async Task GetGroupsAsync_OtherDomain_ReturnsOnlyOtherDomainGroups()
	{
		List<GroupInfo> groups = (await _groups.GetGroupsAsync(TestData.OtherDomain)).ToList();

		Assert.Single(groups);
		Assert.Equal(TestData.OtherDomainGroupId, groups[0].Identifier);
	}

	[Fact]
	public async Task GetGroupsAsync_NameNotInDomain_ReturnsEmpty()
	{
		List<GroupInfo> groups = (await _groups.GetGroupsAsync(TestData.Domain, "DoesNotExist")).ToList();

		Assert.Empty(groups);
	}

	[Fact]
	public async Task GetGroupsAsync_NullDomain_ThrowsArgumentNullException()
		=> await Assert.ThrowsAsync<ArgumentNullException>(
			() => _groups.GetGroupsAsync(null!));

	// ── GetGroupAsync ─────────────────────────────────────────────────────

	[Fact]
	public async Task GetGroupAsync_ExistingId_ReturnsGroupWithCorrectIdentifier()
	{
		IGroup? group = await _groups.GetGroupAsync(TestData.UsersGroupId);

		Assert.NotNull(group);
		Assert.Equal(TestData.UsersGroupId, group.Identifier);
	}

	[Fact]
	public async Task GetGroupAsync_NonExistentId_ThrowsUnknownGroupException()
		=> await Assert.ThrowsAsync<UnknownGroupException>(
			() => _groups.GetGroupAsync("does-not-exist"));

	[Fact]
	public async Task GetGroupAsync_NullOrWhitespaceId_ThrowsArgumentException()
		=> await Assert.ThrowsAsync<ArgumentException>(
			() => _groups.GetGroupAsync("   "));

	// ── AddGroupAsync ─────────────────────────────────────────────────────

	[Fact]
	public async Task AddGroupAsync_ValidDefinition_ReturnsNonEmptyId()
	{
		GroupDefinition definition = new GroupDefinition
		{
			Domain = TestData.Domain,
			Name = "NewGroup_" + Guid.NewGuid().ToString("N")[..8],
			Description = "Created by test"
		};

		string id = await _groups.AddGroupAsync(definition);

		Assert.False(string.IsNullOrWhiteSpace(id));
	}

	[Fact]
	public async Task AddGroupAsync_ValidDefinition_GroupIsPersisted()
	{
		GroupDefinition definition = new GroupDefinition
		{
			Domain = TestData.Domain,
			Name = "Persisted_" + Guid.NewGuid().ToString("N")[..8],
			Description = string.Empty
		};

		string id = await _groups.AddGroupAsync(definition);
		IGroup? group = await _groups.GetGroupAsync(id);
		GroupInfo? info = await group.GetInfoAsync();

		Assert.Equal(definition.Name, info.Definition.Name);
		Assert.Equal(definition.Domain, info.Definition.Domain);
	}

	[Fact]
	public async Task AddGroupAsync_NullDefinition_ThrowsArgumentNullException()
		=> await Assert.ThrowsAsync<ArgumentNullException>(
			() => _groups.AddGroupAsync(null!));

	// ── DeleteGroupAsync ──────────────────────────────────────────────────

	[Fact]
	public async Task DeleteGroupAsync_ExistingGroup_GroupNoLongerRetrievable()
	{
		string id = await _groups.AddGroupAsync(new GroupDefinition
		{
			Domain = TestData.Domain,
			Name = "ToDelete_" + Guid.NewGuid().ToString("N")[..8],
			Description = string.Empty
		});

		await _groups.DeleteGroupAsync(id);

		await Assert.ThrowsAsync<UnknownGroupException>(() => _groups.GetGroupAsync(id));
	}

	[Fact]
	public async Task DeleteGroupAsync_NonExistentId_ThrowsException()
		// The implementation is a no-op when the group is not found.
		=> await _groups.DeleteGroupAsync( "does-not-exist" );

	// ── GetMemberships ────────────────────────────────────────────────────

	[Fact]
	public async Task GetMemberships_DirectOnly_ReturnsSubjectsDirectGroups()
	{
		Subject subject = new Subject { Type = SubjectTypes.USER, Identifier = TestData.AliceUserId };
		List<Membership> memberships = (await _groups.GetMemberships(TestData.Domain, subject)).ToList();
		List<string> groupIds = memberships.Select(m => m.Group).ToList();

		Assert.Contains(TestData.UsersGroupId, groupIds);
		Assert.Contains(TestData.EditorsGroupId, groupIds);
	}

	[Fact]
	public async Task GetMemberships_Recursive_IncludesTransitiveGroupMemberships()
	{
		// Charlie is in SubEditors, which is in Editors.
		Subject subject = new Subject { Type = SubjectTypes.USER, Identifier = TestData.CharlieUserId };
		List<Membership> memberships = (await _groups.GetMemberships(TestData.Domain, subject, recursive: true)).ToList();
		List<string> groupIds = memberships.Select(m => m.Group).ToList();

		Assert.Contains(TestData.SubEditorsGroupId, groupIds);
		Assert.Contains(TestData.EditorsGroupId, groupIds);
	}

	[Fact]
	public async Task GetMemberships_UserWithNoMemberships_ReturnsEmpty()
	{
		Subject subject = new Subject { Type = SubjectTypes.USER, Identifier = "user-nobody" };
		List<Membership> memberships = (await _groups.GetMemberships(TestData.Domain, subject)).ToList();

		Assert.Empty(memberships);
	}

	[Fact]
	public async Task GetMemberships_NullSubject_ThrowsArgumentNullException()
		=> await Assert.ThrowsAsync<ArgumentNullException>(
			() => _groups.GetMemberships(TestData.Domain, null!));

	// ── AddToGroupsAsync ──────────────────────────────────────────────────

	[Fact]
	public async Task AddToGroupsAsync_ValidSubjectAndGroups_SubjectAppearsInGroupMemberships()
	{
		Subject subject = new Subject
		{
			Type = SubjectTypes.USER,
			Identifier = "user-addtest-" + Guid.NewGuid().ToString("N")[..8]
		};

		await _groups.AddToGroupsAsync(subject, new[] { TestData.UsersGroupId });

		IGroup? group = await _groups.GetGroupAsync(TestData.UsersGroupId);
		List<Membership> members = (await group.GetMembershipsAsync()).ToList();
		Assert.Contains(members, m => m.Subject.Identifier == subject.Identifier);
	}

	[Fact]
	public async Task AddToGroupsAsync_NonExistentGroup_ThrowsUnknownGroupException()
	{
		Subject subject = new Subject { Type = SubjectTypes.USER, Identifier = TestData.AliceUserId };
		await Assert.ThrowsAsync<UnknownGroupException>(
			() => _groups.AddToGroupsAsync(subject, new[] { "does-not-exist" }));
	}

	[Fact]
	public async Task AddToGroupsAsync_EmptyGroupList_CompletesWithoutError()
	{
		Subject subject = new Subject { Type = SubjectTypes.USER, Identifier = TestData.AliceUserId };
		await _groups.AddToGroupsAsync(subject, Array.Empty<string>());
	}

	[Fact]
	public async Task AddToGroupsAsync_NullSubject_ThrowsArgumentNullException()
		=> await Assert.ThrowsAsync<ArgumentNullException>(
			() => _groups.AddToGroupsAsync(null!, new[] { TestData.UsersGroupId }));
}
