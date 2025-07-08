using System.Threading.Tasks;
using NUnit.Framework;

namespace ReadyPlayerMe.AvatarCreator.Tests
{
    public class AuthTests
    {
        [Test]
        public async Task Login_As_Anonymous()
        {
            await AuthManager.LoginAsAnonymous();
            Assert.False(string.IsNullOrEmpty(AuthManager.UserSession.Id));
        }
    }
}
