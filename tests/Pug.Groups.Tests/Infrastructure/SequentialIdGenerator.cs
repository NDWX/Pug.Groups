using Pug.Groups.Common;

namespace Pug.Groups.Tests.Infrastructure;

/// <summary>
/// Thread-safe identifier generator that produces sequential string IDs.
/// Used to give <see cref="Groups"/> predictable, unique identifiers during tests.
/// </summary>
internal sealed class SequentialIdGenerator : IdentifierGenerator
{
	private int _counter;

	public string GetNext() => $"gen-{Interlocked.Increment(ref _counter):D6}";
}
