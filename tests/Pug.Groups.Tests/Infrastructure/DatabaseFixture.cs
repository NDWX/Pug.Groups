using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using Microsoft.Data.Sqlite;
using Pug.Application.Data;
using Pug.Effable;
using Pug.Groups.Common;
using Pug.Groups.Models;
using Pug.Groups.SQLiteData;

namespace Pug.Groups.Tests.Infrastructure;

/// <summary>
/// xUnit class fixture that provisions a fresh SQLite database for each test class,
/// seeds it with a known set of groups and memberships, and tears it down afterwards.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
	private string? _dbPath;
	private bool _disposed;

	public IApplicationData<IDataSession> ApplicationData { get; private set; } = null!;

	internal SequentialIdGenerator IdGenerator { get; } = new SequentialIdGenerator();

	// ── IAsyncLifetime ─────────────────────────────────────────────────────

	public async Task InitializeAsync()
	{
		// Use a unique temp file so each fixture instance has an isolated database.
		_dbPath = Path.Combine(Path.GetTempPath(), $"pug_groups_test_{Guid.NewGuid():N}.db");

		// Pooling=False ensures each connection is physically closed when disposed,
		// preventing "database is locked" errors between DbUp migration and seed writes.
		string connectionString = $"Data Source={_dbPath};Pooling=False;";

		DapperData data = new DapperData(connectionString, SqliteFactory.Instance);

		UpgradeDatabase(connectionString, data);

		ApplicationData = data;

		await SeedAsync();
	}

	public Task DisposeAsync()
	{
		Cleanup();
		return Task.CompletedTask;
	}

	// ── Seeding ────────────────────────────────────────────────────────────

	private async Task SeedAsync()
	{
		List<GroupInfo> groups = new List<GroupInfo>
		{
			MakeGroup(TestData.AdminGroupId,       TestData.Domain,       TestData.AdminGroupName),
			MakeGroup(TestData.UsersGroupId,       TestData.Domain,       TestData.UsersGroupName),
			MakeGroup(TestData.EditorsGroupId,     TestData.Domain,       TestData.EditorsGroupName),
			MakeGroup(TestData.SubEditorsGroupId,  TestData.Domain,       TestData.SubEditorsGroupName),
			MakeGroup(TestData.OtherDomainGroupId, TestData.OtherDomain,  TestData.OtherDomainGroupName),
			MakeGroup(TestData.CircleAGroupId,     TestData.Domain,       TestData.CircleAGroupName),
			MakeGroup(TestData.CircleBGroupId,     TestData.Domain,       TestData.CircleBGroupName),
		};

		List<Membership> memberships = new List<Membership>
		{
			// ── Direct user memberships ────────────────────────────────────
			// Alice is directly in Users and Editors
			MakeMembership(SubjectTypes.USER,  TestData.AliceUserId,       TestData.UsersGroupId),
			MakeMembership(SubjectTypes.USER,  TestData.AliceUserId,       TestData.EditorsGroupId),
			// Bob is directly in Users only
			MakeMembership(SubjectTypes.USER,  TestData.BobUserId,         TestData.UsersGroupId),
			// Charlie is directly in SubEditors (which is nested inside Editors)
			MakeMembership(SubjectTypes.USER,  TestData.CharlieUserId,     TestData.SubEditorsGroupId),
			// Dave is directly in CircleA
			MakeMembership(SubjectTypes.USER,  TestData.DaveUserId,        TestData.CircleAGroupId),

			// ── Nested group memberships ───────────────────────────────────
			// SubEditors is a member of Editors (creates a one-level nesting chain)
			MakeMembership(SubjectTypes.GROUP, TestData.SubEditorsGroupId, TestData.EditorsGroupId),

			// ── Circular group memberships ─────────────────────────────────
			// CircleB is a member of CircleA, and CircleA is a member of CircleB
			MakeMembership(SubjectTypes.GROUP, TestData.CircleBGroupId,    TestData.CircleAGroupId),
			MakeMembership(SubjectTypes.GROUP, TestData.CircleAGroupId,    TestData.CircleBGroupId),
		};

		await ApplicationData.PerformAsync(
			async (session, ctx) =>
			{
				foreach (GroupInfo g in ctx.groups)
					await session.InsertAsync(g);
				foreach (Membership m in ctx.memberships)
					await session.InsertAsync(m);
			},
			new { groups, memberships }
		);
	}

	// ── Schema ─────────────────────────────────────────────────────────────

	private static void UpgradeDatabase(string connectionString, DapperData data)
	{
		UpgradeEngineBuilder builder = DeployChanges.To.SQLiteDatabase(connectionString);

		foreach (SchemaUpgradeScript schemaScript in data.GetSchemaUpgradeScripts())
			builder = builder.WithScript(schemaScript.UpgradeScript.Name, schemaScript.UpgradeScript.Script);

		DatabaseUpgradeResult result = builder.LogToNowhere().Build().PerformUpgrade();

		if (!result.Successful)
			throw new InvalidOperationException("DbUp schema migration failed.", result.Error);

		// Switch to WAL journal mode so concurrent connections use append-only writes
		// instead of exclusive file locks, and set a retry timeout to avoid spurious
		// "database is locked" failures caused by Windows file-lock release latency.
		using SqliteConnection connection = new SqliteConnection(connectionString);
		connection.Open();
		using SqliteCommand cmd = connection.CreateCommand();
		cmd.CommandText = "PRAGMA journal_mode=WAL;";
		cmd.ExecuteNonQuery();
		cmd.CommandText = "PRAGMA busy_timeout=5000;";
		cmd.ExecuteNonQuery();
	}

	// ── Helpers ────────────────────────────────────────────────────────────

	private static GroupInfo MakeGroup(string id, string domain, string name) =>
		new()
		{
			Identifier = id,
			Definition = new GroupDefinition
			{
				Domain = domain,
				Name = name,
				Description = string.Empty
			},
			RegistrationInfo = new ActionContext<string>
			{
				Actor = TestData.RegistrationUser,
				Timestamp = DateTime.UtcNow
			}
		};

	private static Membership MakeMembership(string subjectType, string subjectId, string group) =>
		new()
		{
			Subject = new Subject { Type = subjectType, Identifier = subjectId },
			Group = group,
			RegistrationInfo = new ActionContext<string>
			{
				Actor = TestData.RegistrationUser,
				Timestamp = DateTime.UtcNow
			}
		};

	private void Cleanup()
	{
		if (_disposed) return;
		_disposed = true;

		if (_dbPath != null && File.Exists(_dbPath))
			File.Delete(_dbPath);
	}
}
