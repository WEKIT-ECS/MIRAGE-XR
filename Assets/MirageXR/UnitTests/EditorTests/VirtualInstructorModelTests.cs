using LearningExperienceEngine.DataModel;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests
{
    public class VirtualInstructorModelTests
    {
        [Test]
        public void InstructorContentData_DefaultValues_AreCorrect()
        {
            var data = new InstructorContentData();

            Assert.IsTrue(data.StandOnFloorPlane);
            Assert.IsNull(data.PersistentThread);
            Assert.IsNull(data.StartConversation);
            Assert.IsFalse(data.FollowUser);
            Assert.IsFalse(data.EyesFollowUser);
            Assert.IsNull(data.ThreadId);
            Assert.IsNull(data.AssistantId);
        }

        [Test]
        public void InstructorContentData_SnakeCase_SerializationAndDeserialization()
        {
            var original = new InstructorContentData
            {
                CharacterName = "Edison",
                PersistentThread = true,
                ThreadId = "thread_12345",
                AssistantId = "asst_67890",
                StartConversation = true,
                FollowUser = true,
                EyesFollowUser = true,
                StandOnFloorPlane = false
            };

            var json = JsonConvert.SerializeObject(original);

            StringAssert.Contains("\"persistent_thread\":true", json);
            StringAssert.Contains("\"thread_id\":\"thread_12345\"", json);
            StringAssert.Contains("\"assistant_id\":\"asst_67890\"", json);
            StringAssert.Contains("\"start_conversation\":true", json);
            StringAssert.Contains("\"follow_user\":true", json);
            StringAssert.Contains("\"eyes_follow_user\":true", json);
            StringAssert.Contains("\"stand_on_floor_plane\":false", json);

            var deserialized = JsonConvert.DeserializeObject<InstructorContentData>(json);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual("Edison", deserialized.CharacterName);
            Assert.AreEqual(true, deserialized.PersistentThread);
            Assert.AreEqual("thread_12345", deserialized.ThreadId);
            Assert.AreEqual("asst_67890", deserialized.AssistantId);
            Assert.AreEqual(true, deserialized.StartConversation);
            Assert.IsTrue(deserialized.FollowUser);
            Assert.IsTrue(deserialized.EyesFollowUser);
            Assert.IsFalse(deserialized.StandOnFloorPlane);
        }

        [Test]
        public void InstructorContentData_PascalAndCamelCase_AliasesAreHandled()
        {
            var json = @"{
                ""PersistentThread"": true,
                ""threadId"": ""thread_alias_abc"",
                ""assistantId"": ""asst_alias_xyz"",
                ""StartConversation"": true,
                ""FollowUser"": true,
                ""EyesFollowUser"": true,
                ""StandOnFloorPlane"": false
            }";

            var deserialized = JsonConvert.DeserializeObject<InstructorContentData>(json);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(true, deserialized.PersistentThread);
            Assert.AreEqual("thread_alias_abc", deserialized.ThreadId);
            Assert.AreEqual("asst_alias_xyz", deserialized.AssistantId);
            Assert.AreEqual(true, deserialized.StartConversation);
            Assert.IsTrue(deserialized.FollowUser);
            Assert.IsTrue(deserialized.EyesFollowUser);
            Assert.IsFalse(deserialized.StandOnFloorPlane);
        }
    }
}
