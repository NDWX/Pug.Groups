using Pug.Application.Security;
using Pug.Groups.Common;
using Pug.Groups.Models;
using Pug.Groups.Tests.Infrastructure;

namespace Pug.Groups.Tests;

/// <summary>
/// Integration tests for <see cref="Group"/> backed by a real SQLite database.
/// Each test class receives its own isolated database; mutation tests use freshly
/// created groups so seed data is never disturbed.
/// </summary>
public class GroupTests : IClassFixture<DatabaseFixture>
{
	private readonly DatabaseFixture _fixture;
	private readonly IGroups _groups;

	public GroupTests(DatabaseFixture fixture)
	{
		_fixture = fixture;
		ISecurityManager security = SecurityManagerFactory.CreateAlwaysAuthorized();
		_groups = new Groups(fixture.IdGenerator, fixture.ApplicationData, security);
	}

	private async Task<IGroup> GetGroupAsync(string id) =>
		await _groups.GetGroupAsync(id);

	/// <summary>Creates a new empty group with a unique name in the test domain.</summary>
	private async Task<IGroup> CreateEmptyGroupAsync()
	{
		string id = await _groups.AddGroupAsync(new GroupDefinition
		{
			Domain = TestData.Domain,
			Name = "Temp_" + Guid.NewGuid().ToString("N")[..12],
			Description = string.Empty
		});
		return await GetGroupAsync(id);
	}

	// ── GetDefinitionAsync ────────────────────────────────────────────────

	[Fact]
	public async Task GetDefinitionAsync_ReturnsCorrectDomainAndName()
	{
		IGroup group = await GetGroupAsync(TestData.UsersGroupId);
		GroupDefinition? definition = await group.GetDefinitionAsync();

		Assert.NotNull(definition);
		Assert.Equal(TestData.Domain, definition.Domain);
		Assert.Equal(TestData.UsersGroupName, definition.Name);
	}

	// ── GetInfoAsync ──────────────────────────────────────────────────────

	[Fact]
	public async Task GetInfoAsync_ReturnsFullGroupInfo()
	{
		IGroup group = await GetGroupAsync(TestData.EditorsGroupId);
		GroupInfo? info = await group.GetInfoAsync();

		Assert.NotNull(info);
		Assert.Equal(TestData.EditorsGroupId, info.Identifier);
		Assert.Equal(TestData.EditorsGroupName, info.Definition.Name);
		Assert.Equal(TestData.Domain, info.Definition.Domain);
	}

	// ── GetInfo (sync) ────────────────────────────────────────────────────

	// ── GetMembershipsAsync ───────────────────────────────────────────────

	[Fact]
	public async Task GetMembershipsAsync_UsersGroup_ContainsExpectedDirectMembers()
	{
		IGroup group = await GetGroupAsync(TestData.UsersGroupId);
		List<Membership> members = (await group.GetMembershipsAsync()).ToList();
		List<string> subjectIds = members.Select(m => m.Subject.Identifier).ToList();

		Assert.Contains(TestData.AliceUserId, subjectIds);
		Assert.Contains(TestData.BobUserId, subjectIds);
	}

	[Fact]
	public async Task GetMembershipsAsync_EditorsGroup_ContainsDirectUserAndNestedGroup()
	{
		IGroup group = await GetGroupAsync(TestData.EditorsGroupId);
		List<Membership> members = (await group.GetMembershipsAsync()).ToList();
		List<string> subjectIds = members.Select(m => m.Subject.Identifier).ToList();

		Assert.Contains(TestData.AliceUserId, subjectIds);
		Assert.Contains(TestData.SubEditorsGroupId, subjectIds);
	}

	[Fact]
	public async Task GetMembershipsAsync_NewEmptyGroup_ReturnsEmpty()
	{
		IGroup group = await CreateEmptyGroupAsync();
		IEnumerable<Membership>? members = await group.GetMembershipsAsync();

		Assert.Empty(members);
	}

	// ── HasMemberAsync ────────────────────────────────────────────────────

	[Fact]
	public async Task HasMemberAsync_DirectUser_NonRecursive_ReturnsTrue()
	{
		IGroup group = await GetGroupAsync(TestData.UsersGroupId);
		Subject alice = new Subject { Type = SubjectTypes.USER, Identifier = TestData.AliceUserId };

		Assert.True(await group.HasMemberAsync(alice, recursive: false));
	}

	[Fact]
	public async Task HasMemberAsync_NonMember_NonRecursive_ReturnsFalse()
	{
		IGroup group = await GetGroupAsync(TestData.EditorsGroupId);
		// Charlie is in SubEditors but is NOT a direct member of Editors
		Subject charlie = new Subject { Type = SubjectTypes.USER, Identifier = TestData.CharlieUserId };

		Assert.False(await group.HasMemberAsync(charlie, recursive: false));
	}

	[Fact]
	public async Task HasMemberAsync_IndirectMemberViaNestedGroup_Recursive_ReturnsTrue()
	{
		IGroup group = await GetGroupAsync(TestData.EditorsGroupId);
		Subject charlie = new Subject { Type = SubjectTypes.USER, Identifier = TestData.CharlieUserId };

		Assert.True(await group.HasMemberAsync(charlie, recursive: true));
	}

	[Fact]
	public async Task HasMemberAsync_DirectGroupMember_NonRecursive_ReturnsTrue()
	{
		IGroup group = await GetGroupAsync(TestData.EditorsGroupId);
		Subject subEditors = new Subject { Type = SubjectTypes.GROUP, Identifier = TestData.SubEditorsGroupId };

		Assert.True(await group.HasMemberAsync(subEditors, recursive: false));
	}

	/// <summary>
	/// Alice is not a member of CircleA; the mutually-nested CircleA ↔ CircleB structure
	/// must not cause an infinite loop.
	/// </summary>
	[Fact]
	public async Task HasMemberAsync_CircularGroups_NonMember_NoInfiniteLoop_ReturnsFalse()
	{
		IGroup group = await GetGroupAsync(TestData.CircleAGroupId);
		Subject alice = new Subject { Type = SubjectTypes.USER, Identifier = TestData.AliceUserId };

		Assert.False(await group.HasMemberAsync(alice, recursive: true));
	}

	/// <summary>
	/// Dave is in CircleA; CircleA is a member of CircleB, so Dave is transitively in CircleB.
	/// </summary>
	[Fact]
	public async Task HasMemberAsync_TransitiveMemberViaCircularGroup_Recursive_ReturnsTrue()
	{
		IGroup group = await GetGroupAsync(TestData.CircleBGroupId);
		Subject dave = new Subject { Type = SubjectTypes.USER, Identifier = TestData.DaveUserId };

		Assert.True(await group.HasMemberAsync(dave, recursive: true));
	}

	// ── AddMembersAsync ───────────────────────────────────────────────────

	[Fact]
	public async Task AddMembersAsync_NewUser_AppearsInMemberships()
	{
		IGroup group = await CreateEmptyGroupAsync();
		Subject newUser = new Subject
		{
			Type = SubjectTypes.USER,
			Identifier = "user-add-" + Guid.NewGuid().ToString("N")[..8]
		};

		await group.AddMembersAsync(new[] { newUser });

		List<Membership> members = (await group.GetMembershipsAsync()).ToList();
		Assert.Contains(members, m => m.Subject.Identifier == newUser.Identifier);
	}

	[Fact]
	public async Task AddMembersAsync_MultipleUsers_AllAppearInMemberships()
	{
		IGroup group = await CreateEmptyGroupAsync();
		Subject[] users = Enumerable.Range(1, 3)
									.Select(i => new Subject
									{
										Type = SubjectTypes.USER,
										Identifier = $"user-bulk-{i}-{Guid.NewGuid():N}"[..20]
									})
									.ToArray();

		await group.AddMembersAsync(users);

		List<string> memberIds = (await group.GetMembershipsAsync()).Select(m => m.Subject.Identifier).ToList();
		Assert.All(users, u => Assert.Contains(u.Identifier, memberIds));
	}

	[Fact]
	public async Task AddMembersAsync_NullList_DoesNotThrow()
	{
		IGroup group = await GetGroupAsync(TestData.UsersGroupId);
		await group.AddMembersAsync(null!);
	}

	[Fact]
	public async Task AddMembersAsync_EmptyList_DoesNotThrow()
	{
		IGroup group = await GetGroupAsync(TestData.UsersGroupId);
		await group.AddMembersAsync(Array.Empty<Subject>());
	}

	// ── RemoveMemberAsync ─────────────────────────────────────────────────

	[Fact]
	public async Task RemoveMemberAsync_ExistingMember_IsNoLongerAMember()
	{
		IGroup group = await CreateEmptyGroupAsync();
		Subject user = new Subject
		{
			Type = SubjectTypes.USER,
			Identifier = "user-rem-" + Guid.NewGuid().ToString("N")[..8]
		};

		await group.AddMembersAsync(new[] { user });
		Assert.True(await group.HasMemberAsync(user, recursive: false));

		await group.RemoveMemberAsync(user);
		Assert.False(await group.HasMemberAsync(user, recursive: false));
	}

	[Fact]
	public async Task RemoveMemberAsync_NonExistentMember_DoesNotThrow()
	{
		IGroup group = await GetGroupAsync(TestData.AdminGroupId);
		Subject nobody = new Subject { Type = SubjectTypes.USER, Identifier = "user-nobody" };

		await group.RemoveMemberAsync(nobody);
	}

	// ── Constructor ───────────────────────────────────────────────────────

	[Fact]
	public async Task Constructor_UnknownGroupId_ThrowsUnknownGroupException()
		=> await Assert.ThrowsAsync<UnknownGroupException>(
			() => _groups.GetGroupAsync("does-not-exist"));
}
