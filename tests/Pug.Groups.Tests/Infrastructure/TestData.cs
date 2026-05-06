namespace Pug.Groups.Tests.Infrastructure;

/// <summary>
/// Identifiers and constants for seeded test data.
/// </summary>
internal static class TestData
{
	public const string Domain = "test";
	public const string OtherDomain = "other";
	public const string RegistrationUser = "system";

	// ── Group identifiers ──────────────────────────────────────────────────
	/// <summary>Empty group in the test domain.</summary>
	public const string AdminGroupId = "grp-admin";
	/// <summary>Contains Alice and Bob directly.</summary>
	public const string UsersGroupId = "grp-users";
	/// <summary>Contains Alice directly and SubEditors as a nested group.</summary>
	public const string EditorsGroupId = "grp-editors";
	/// <summary>Contains Charlie directly; is a nested member of Editors.</summary>
	public const string SubEditorsGroupId = "grp-subeditors";
	/// <summary>Sole group in the other domain.</summary>
	public const string OtherDomainGroupId = "grp-other";
	/// <summary>Mutually nested with CircleB to exercise circular-membership prevention.</summary>
	public const string CircleAGroupId = "grp-circle-a";
	/// <summary>Mutually nested with CircleA to exercise circular-membership prevention.</summary>
	public const string CircleBGroupId = "grp-circle-b";

	// ── Group names ────────────────────────────────────────────────────────
	public const string AdminGroupName = "Administrators";
	public const string UsersGroupName = "Users";
	public const string EditorsGroupName = "Editors";
	public const string SubEditorsGroupName = "Sub-Editors";
	public const string OtherDomainGroupName = "OtherGroup";
	public const string CircleAGroupName = "Circle-A";
	public const string CircleBGroupName = "Circle-B";

	// ── User identifiers ───────────────────────────────────────────────────
	/// <summary>Direct member of UsersGroup and EditorsGroup.</summary>
	public const string AliceUserId = "user-alice";
	/// <summary>Direct member of UsersGroup only.</summary>
	public const string BobUserId = "user-bob";
	/// <summary>Direct member of SubEditorsGroup → inherits EditorsGroup transitively.</summary>
	public const string CharlieUserId = "user-charlie";
	/// <summary>Direct member of CircleAGroup; used for circular-membership tests.</summary>
	public const string DaveUserId = "user-dave";
}
