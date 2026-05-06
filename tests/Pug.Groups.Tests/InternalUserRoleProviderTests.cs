using Pug.Groups.Common;
using Pug.Groups.Tests.Infrastructure;

namespace Pug.Groups.Tests;

/// <summary>
/// Integration tests for <see cref="InternalUserRoleProvider"/> backed by a real SQLite database.
/// Each test class receives its own isolated database via <see cref="DatabaseFixture"/>.
/// </summary>
public class InternalUserRoleProviderTests : IClassFixture<DatabaseFixture>
{
	private readonly InternalUserRoleProvider _provider;

	public InternalUserRoleProviderTests(DatabaseFixture fixture)
	{
		_provider = new InternalUserRoleProvider(fixture.ApplicationData);
	}

	// ── UserIsInRole ──────────────────────────────────────────────────────

	[Fact]
	public void UserIsInRole_DirectMember_ReturnsTrue()
		=> Assert.True(_provider.UserIsInRole(TestData.AliceUserId, TestData.UsersGroupId));

	[Fact]
	public void UserIsInRole_DirectMemberOfSecondGroup_ReturnsTrue()
		=> Assert.True(_provider.UserIsInRole(TestData.AliceUserId, TestData.EditorsGroupId));

	[Fact]
	public void UserIsInRole_UserNotInGroup_ReturnsFalse()
		=> Assert.False(_provider.UserIsInRole(TestData.BobUserId, TestData.EditorsGroupId));

	[Fact]
	public void UserIsInRole_UserNotInAnyGroup_ReturnsFalse()
		=> Assert.False(_provider.UserIsInRole("user-nobody", TestData.UsersGroupId));

	/// <summary>
	/// Charlie → SubEditors → Editors: recursive lookup must resolve to true.
	/// </summary>
	[Fact]
	public void UserIsInRole_IndirectMemberViaNestedGroup_ReturnsTrue()
		=> Assert.True(_provider.UserIsInRole(TestData.CharlieUserId, TestData.EditorsGroupId));

	[Fact]
	public void UserIsInRole_DirectMemberOfNestedGroup_ButNotOfParent_WithoutRecursion_ReturnsFalse()
		=> Assert.False(_provider.UserIsInRole(TestData.CharlieUserId, TestData.UsersGroupId));

	/// <summary>
	/// Alice is not a member of CircleA, and there must be no infinite loop from the
	/// mutually-nested CircleA ↔ CircleB groups.
	/// </summary>
	[Fact]
	public void UserIsInRole_CircularGroups_NonMember_NoInfiniteLoop_ReturnsFalse()
		=> Assert.False(_provider.UserIsInRole(TestData.AliceUserId, TestData.CircleAGroupId));

	/// <summary>
	/// Dave is a direct member of CircleA.
	/// </summary>
	[Fact]
	public void UserIsInRole_DirectMemberOfCircularGroup_ReturnsTrue()
		=> Assert.True(_provider.UserIsInRole(TestData.DaveUserId, TestData.CircleAGroupId));

	// ── UserIsInRoleAsync ─────────────────────────────────────────────────

	[Fact]
	public async Task UserIsInRoleAsync_DirectMember_ReturnsTrue()
		=> Assert.True(await _provider.UserIsInRoleAsync(TestData.AliceUserId, TestData.UsersGroupId));

	[Fact]
	public async Task UserIsInRoleAsync_IndirectMemberViaNestedGroup_ReturnsTrue()
		=> Assert.True(await _provider.UserIsInRoleAsync(TestData.CharlieUserId, TestData.EditorsGroupId));

	[Fact]
	public async Task UserIsInRoleAsync_NotMember_ReturnsFalse()
		=> Assert.False(await _provider.UserIsInRoleAsync(TestData.BobUserId, TestData.EditorsGroupId));

	[Fact]
	public async Task UserIsInRoleAsync_CircularGroups_NonMember_NoInfiniteLoop_ReturnsFalse()
		=> Assert.False(await _provider.UserIsInRoleAsync(TestData.AliceUserId, TestData.CircleAGroupId));

	// ── UserIsInRoles ─────────────────────────────────────────────────────

	[Fact]
	public void UserIsInRoles_MemberOfAllSpecifiedRoles_ReturnsTrue()
		=> Assert.True(_provider.UserIsInRoles(
			TestData.AliceUserId, new[] { TestData.UsersGroupId, TestData.EditorsGroupId }));

	[Fact]
	public void UserIsInRoles_NotMemberOfOneRole_ReturnsFalse()
		=> Assert.False(_provider.UserIsInRoles(
			TestData.AliceUserId, new[] { TestData.UsersGroupId, TestData.AdminGroupId }));

	[Fact]
	public void UserIsInRoles_SingleRole_DirectMember_ReturnsTrue()
		=> Assert.True(_provider.UserIsInRoles(TestData.BobUserId, new[] { TestData.UsersGroupId }));

	[Fact]
	public void UserIsInRoles_IndirectMemberCoveredByRecursion_ReturnsTrue()
		=> Assert.True(_provider.UserIsInRoles(
			TestData.CharlieUserId, new[] { TestData.SubEditorsGroupId, TestData.EditorsGroupId }));

	// ── UserIsInRolesAsync ────────────────────────────────────────────────

	[Fact]
	public async Task UserIsInRolesAsync_MemberOfAllRoles_ReturnsTrue()
		=> Assert.True(await _provider.UserIsInRolesAsync(
			TestData.AliceUserId, new[] { TestData.UsersGroupId, TestData.EditorsGroupId }));

	[Fact]
	public async Task UserIsInRolesAsync_NotMemberOfOneRole_ReturnsFalse()
		=> Assert.False(await _provider.UserIsInRolesAsync(
			TestData.AliceUserId, new[] { TestData.UsersGroupId, TestData.AdminGroupId }));

	// ── GetUserRoles ──────────────────────────────────────────────────────

	[Fact]
	public void GetUserRoles_SingleDirectGroup_ReturnsThatGroup()
	{
		List<string> roles = _provider.GetUserRoles(TestData.BobUserId).ToList();

		Assert.Single(roles);
		Assert.Contains(TestData.UsersGroupId, roles);
	}

	[Fact]
	public void GetUserRoles_MultipleDirectGroups_ReturnsAll()
	{
		List<string> roles = _provider.GetUserRoles(TestData.AliceUserId).ToList();

		Assert.Contains(TestData.UsersGroupId, roles);
		Assert.Contains(TestData.EditorsGroupId, roles);
	}

	/// <summary>
	/// Charlie → SubEditors → Editors: both groups must appear via recursive resolution.
	/// </summary>
	[Fact]
	public void GetUserRoles_TransitiveGroupsViaNestedMembership_ReturnsAll()
	{
		List<string> roles = _provider.GetUserRoles(TestData.CharlieUserId).ToList();

		Assert.Contains(TestData.SubEditorsGroupId, roles);
		Assert.Contains(TestData.EditorsGroupId, roles);
	}

	/// <summary>
	/// Dave is in CircleA, which is mutually nested with CircleB.
	/// Both groups must appear exactly once (no duplicates, no infinite loop).
	/// </summary>
	[Fact]
	public void GetUserRoles_CircularMembership_BothGroupsReturnedExactlyOnce()
	{
		List<string> roles = _provider.GetUserRoles(TestData.DaveUserId).ToList();

		Assert.Contains(TestData.CircleAGroupId, roles);
		Assert.Contains(TestData.CircleBGroupId, roles);
		Assert.Equal(roles.Count, roles.Distinct().Count());
	}

	// ── GetUserRolesAsync ─────────────────────────────────────────────────

	[Fact]
	public async Task GetUserRolesAsync_SingleDirectGroup_ReturnsThatGroup()
	{
		List<string> roles = (await _provider.GetUserRolesAsync(TestData.BobUserId)).ToList();

		Assert.Single(roles);
		Assert.Contains(TestData.UsersGroupId, roles);
	}

	[Fact]
	public async Task GetUserRolesAsync_TransitiveGroupsViaNestedMembership_ReturnsAll()
	{
		List<string> roles = (await _provider.GetUserRolesAsync(TestData.CharlieUserId)).ToList();

		Assert.Contains(TestData.SubEditorsGroupId, roles);
		Assert.Contains(TestData.EditorsGroupId, roles);
	}
	
	[Fact]
	public async Task GetUserRolesAsync_CircularMembership_NoDuplicates()
	{
		List<string> roles = (await _provider.GetUserRolesAsync(TestData.DaveUserId)).ToList();

		Assert.Contains(TestData.CircleAGroupId, roles);
		Assert.Contains(TestData.CircleBGroupId, roles);
		Assert.Equal(roles.Count, roles.Distinct().Count());
	}
}
