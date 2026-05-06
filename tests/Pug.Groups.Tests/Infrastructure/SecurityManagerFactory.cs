using Moq;
using Pug.Application.Security;

namespace Pug.Groups.Tests.Infrastructure;

/// <summary>
/// Creates <see cref="ISecurityManager"/> instances that unconditionally grant every
/// authorization request, so tests are not blocked by the security layer.
/// </summary>
internal static class SecurityManagerFactory
{
	/// <summary>
	/// Returns an always-authorized <see cref="ISecurityManager"/>.
	/// </summary>
	/// <param name="userId">
	/// The identity reported for the current user (used when recording who added/removed members).
	/// </param>
	internal static ISecurityManager CreateAlwaysAuthorized(string userId = "test-user")
	{
		BasicPrincipalIdentity identity = new BasicPrincipalIdentity(
			userId, "password", true, userId,
			new Dictionary<string, string>()
		);

		Mock<IUser> mockUser = new Mock<IUser>();
		mockUser.Setup(x => x.Identity).Returns(identity);

		mockUser
			.Setup(x => x.IsAuthorized(
				It.IsAny<IDictionary<string, string>>(),
				It.IsAny<string>(), It.IsAny<NounQualifier>(),
				It.IsAny<string>()))
			.Returns(true);

		mockUser
			.Setup(x => x.IsAuthorizedAsync(
				It.IsAny<IDictionary<string, string>>(),
				It.IsAny<string>(), It.IsAny<NounQualifier>(),
				It.IsAny<string>()))
			.ReturnsAsync(true);

		Mock<ISecurityManager> mockManager = new Mock<ISecurityManager>();
		mockManager.Setup(x => x.CurrentUser).Returns(mockUser.Object);

		return mockManager.Object;
	}
}
