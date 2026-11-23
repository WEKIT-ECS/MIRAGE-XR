using System;
using System.Collections.Generic;
using FakeItEasy;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.NewDataModel;
using NUnit.Framework;

namespace Tests
{
    public class StepDeletionContentTests
    {
        private IActivityManager _activityManager;
        private IAssetsManager _assetsManager;
        private ISketchfabManager _sketchfabManager;
        private StepManager _stepManager;
        private ContentManager _contentManager;

        [SetUp]
        public void SetUp()
        {
            _activityManager = A.Fake<IActivityManager>();
            A.CallTo(() => _activityManager.IsEditorMode).Returns(true);

            _assetsManager = A.Fake<IAssetsManager>();
            _sketchfabManager = A.Fake<ISketchfabManager>();

            _stepManager = new StepManager();
            _contentManager = new ContentManager();

            _stepManager.InitializeAsync(_contentManager, _activityManager);
            _contentManager.InitializeAsync(_assetsManager, _stepManager, _activityManager, _sketchfabManager);
        }

        [Test]
        public void RemoveStep_RemovesExclusiveContent()
        {
            var step1Id = Guid.NewGuid();
            var step2Id = Guid.NewGuid();

            var activity = new Activity
            {
                Id = Guid.NewGuid(),
                Steps = new List<ActivityStep>
                {
                    new ActivityStep { Id = step1Id, Name = "Step 1" },
                    new ActivityStep { Id = step2Id, Name = "Step 2" }
                },
                Hierarchy = new List<HierarchyItem>
                {
                    new HierarchyItem
                    {
                        Id = Guid.NewGuid(),
                        StepIds = new List<Guid> { step1Id, step2Id }
                    }
                },
                Content = new List<Content>()
            };

            _stepManager.LoadSteps(activity);

            var exclusiveContent = new Content
            {
                Id = Guid.NewGuid(),
                Type = ContentType.Instructor,
                Steps = new List<Guid> { step1Id }
            };
            _contentManager.AddContent(exclusiveContent);

            Assert.AreEqual(1, _contentManager.GetContents().Count);

            _stepManager.RemoveStep(step1Id);

            // Exclusive content assigned only to step1 should be removed
            Assert.AreEqual(0, _contentManager.GetContents().Count);
            Assert.AreEqual(1, _stepManager.GetSteps().Count);
            Assert.AreEqual(step2Id, _stepManager.GetSteps()[0].Id);
        }

        [Test]
        public void RemoveStep_PrunesStepFromMultiStepContent_RetainsContent()
        {
            var step1Id = Guid.NewGuid();
            var step2Id = Guid.NewGuid();

            var activity = new Activity
            {
                Id = Guid.NewGuid(),
                Steps = new List<ActivityStep>
                {
                    new ActivityStep { Id = step1Id, Name = "Step 1" },
                    new ActivityStep { Id = step2Id, Name = "Step 2" }
                },
                Hierarchy = new List<HierarchyItem>
                {
                    new HierarchyItem
                    {
                        Id = Guid.NewGuid(),
                        StepIds = new List<Guid> { step1Id, step2Id }
                    }
                },
                Content = new List<Content>()
            };

            _stepManager.LoadSteps(activity);

            var sharedContent = new Content
            {
                Id = Guid.NewGuid(),
                Type = ContentType.Label,
                Steps = new List<Guid> { step1Id, step2Id }
            };
            _contentManager.AddContent(sharedContent);

            Assert.AreEqual(1, _contentManager.GetContents().Count);

            _stepManager.RemoveStep(step1Id);

            // Shared content should remain, but step1Id should be removed from its Steps list
            Assert.AreEqual(1, _contentManager.GetContents().Count);
            Assert.AreEqual(sharedContent.Id, _contentManager.GetContents()[0].Id);
            Assert.AreEqual(1, _contentManager.GetContents()[0].Steps.Count);
            Assert.AreEqual(step2Id, _contentManager.GetContents()[0].Steps[0]);
            Assert.IsFalse(_contentManager.GetContents()[0].Steps.Contains(step1Id));
        }

        [Test]
        public void RemoveStep_UnrelatedContentRemainsUntouched()
        {
            var step1Id = Guid.NewGuid();
            var step2Id = Guid.NewGuid();

            var activity = new Activity
            {
                Id = Guid.NewGuid(),
                Steps = new List<ActivityStep>
                {
                    new ActivityStep { Id = step1Id, Name = "Step 1" },
                    new ActivityStep { Id = step2Id, Name = "Step 2" }
                },
                Hierarchy = new List<HierarchyItem>
                {
                    new HierarchyItem
                    {
                        Id = Guid.NewGuid(),
                        StepIds = new List<Guid> { step1Id, step2Id }
                    }
                },
                Content = new List<Content>()
            };

            _stepManager.LoadSteps(activity);

            var step2Content = new Content
            {
                Id = Guid.NewGuid(),
                Type = ContentType.Image,
                Steps = new List<Guid> { step2Id }
            };
            _contentManager.AddContent(step2Content);

            _stepManager.RemoveStep(step1Id);

            // Step 2's content should remain completely untouched
            Assert.AreEqual(1, _contentManager.GetContents().Count);
            Assert.AreEqual(step2Content.Id, _contentManager.GetContents()[0].Id);
            Assert.AreEqual(1, _contentManager.GetContents()[0].Steps.Count);
            Assert.AreEqual(step2Id, _contentManager.GetContents()[0].Steps[0]);
        }
    }
}
