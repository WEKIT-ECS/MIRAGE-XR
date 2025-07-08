using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace ReadyPlayerMe.Core.Tests
{
    public class TemplateAvatarTests
    {
        [Test]
        public Task Check_Template_Avatar_XR()
        {
            var avatar = TestAvatarData.GetTemplateAvatarXR();
            Assert.IsNotNull(avatar, $"Failed to load '{TestAvatarData.GetTemplateAvatarXRPath()}' from from Assets folder.");

            var renderers = avatar.GetComponentsInChildren<SkinnedMeshRenderer>();
            Assert.IsNotEmpty(renderers, "No SkinnedMeshRenderer components found on the avatar.");

            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                Assert.IsNotNull(renderer.bones, $"Bones array in SkinnedMeshRenderer on {renderer.gameObject.name} is null.");
                Assert.IsNotEmpty(renderer.bones, $"Bones array in SkinnedMeshRenderer on {renderer.gameObject.name} is empty.");
                foreach (var bone in renderer.bones)
                {
                    Assert.IsNotNull(bone, $"A bone in SkinnedMeshRenderer on {renderer.gameObject.name} is null.");
                }
            }
            return Task.CompletedTask;
        }

        [Test]
        public Task Check_Template_Avatar()
        {
            var avatar = TestAvatarData.GetTemplateAvatar();
            Assert.IsNotNull(avatar, $"Failed to load '{TestAvatarData.GetTemplateAvatarPath()}' from Assets folder.");

            var renderers = avatar.GetComponentsInChildren<SkinnedMeshRenderer>();
            Assert.IsNotEmpty(renderers, "No SkinnedMeshRenderer components found on the avatar.");

            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                Assert.IsNotNull(renderer.bones, $"Bones array in SkinnedMeshRenderer on {renderer.gameObject.name} is null.");
                Assert.IsNotEmpty(renderer.bones, $"Bones array in SkinnedMeshRenderer on {renderer.gameObject.name} is empty.");
                foreach (var bone in renderer.bones)
                {
                    Assert.IsNotNull(bone, $"A bone in SkinnedMeshRenderer on {renderer.gameObject.name} is null.");
                }
            }
            return Task.CompletedTask;
        }
    }
}
