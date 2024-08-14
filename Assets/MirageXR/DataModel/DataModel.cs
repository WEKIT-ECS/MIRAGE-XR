#nullable enable
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DataModel
{
    [Serializable]
    public class DataModel
    {
        [JsonProperty] public Guid Id { get; set; }
        [JsonProperty] public DateTime CreationDate { get; set; }
        [JsonProperty] public string Version { get; set; }
    }

    [Serializable]
    public class Activity : DataModel
    {
        [JsonProperty] public User Creator { get; set; }
        [JsonProperty] public List<User> Contributors { get; set; }
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string Description { get; set; }
        [JsonProperty] public string Language { get; set; } //is it needed?
        [JsonProperty] public File Thumbnail { get; set; }
        [JsonProperty] public StepHierarchy Hierarchy { get; set; }
        [JsonProperty] public List<Content> Content { get; set; }
        [JsonProperty] public List<ActivityStep> Steps { get; set; }
    }

    [Serializable]
    public class StepHierarchy
    {
        [JsonProperty] public List<HierarchyItem> Item { get; set; }
    }

    [Serializable]
    public class HierarchyItem
    {
        [JsonProperty] public List<Guid>? StepIds { get; set; }
        [JsonProperty] public List<HierarchyItem>? Item { get; set; }
        [JsonProperty] public string Title { get; set; }
        [JsonProperty] public string Description { get; set; }
    }

    [Serializable]
    public class ActivityStep : DataModel
    {
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string Description { get; set; }
        [JsonProperty] public List<Guid> Contents { get; set; } //Content.Id
        [JsonProperty] public Location Location { get; set; }
        [JsonProperty] public List<Trigger>? Triggers { get; set; } // cannot be null?
        [JsonProperty] public List<Comment> Comments { get; set; }
        [JsonProperty] public List<RequiredToolsPartsMaterials> RequiredToolsPartsMaterials { get; set; }
        [JsonProperty] public List<File>? Attachment { get; set; }
        [JsonProperty] public List<Comment> PrivateNotes { get; set; }
    }

    [Serializable]
    public class Comment : DataModel
    {
        [JsonProperty] public User User { get; set; }
        [JsonProperty] public string Text { get; set; }
        [JsonProperty] public List<File>? Attachment { get; set; }
    }


    [Serializable]
    public class RequiredToolsPartsMaterials
    {
        [JsonProperty] public string ToolPartMaterial { get; set; }
    }

    [Serializable]
    public class User : DataModel
    {
        [JsonProperty] public string Name { get; set; }
    }

    [Serializable]
    public class Content : DataModel
    {
        [JsonProperty] public ContentType Type { get; set; }
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string Description { get; set; }
        [JsonProperty] public bool IsVisible { get; set; }
        [JsonProperty] public Location Location { get; set; }
        [JsonProperty] public JToken ContentData { get; set; }

        public Content<T> ToSpecifiedContent<T>() where T : ContentData
        {
            return new Content<T>
            {
                Id = Id,
                Name = Name,
                Version = Version,
                Type = Type,
                ContentData = ContentData.ToObject<T>(),
                Location = Location
            };
        }
    }

    [Serializable]
    public class File : DataModel
    {
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string FileHash { get; set; }
    }

    public enum ContentType
    {
        Unknown,
        Image,
        Video,
        Audio,
        Ghost,
        Label,
        Act,
        Effects,
        Model,
        Character,
        Interaction, //pick and place
        ImageMarker,
        Plugin,
        Drawing,
        Instructor //virtual instructor
    }

    [Serializable]
    public class Content<T> : DataModel where T : ContentData
    {
        [JsonProperty] public ContentType Type { get; set; }
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public Location Location { get; set; }
        [JsonProperty] public T ContentData { get; set; }
    }

    [Serializable]
    public class Trigger    //todo
    {
        [JsonProperty] public TriggerType Type { get; set; }
        [JsonProperty] public Guid? JumpToStepId { get; set; } // step.id
        [JsonProperty] public Guid ContentId { get; set; } // step.id
        [JsonProperty] public string InputEvent { get; set; } // string needs to be eval’d
        [JsonProperty] public string OutputEvent { get; set; }
        [JsonProperty] public string OutputEventParameter { get; set; }
        [JsonProperty] public string OutputEventParameterValue { get; set; }
        [JsonProperty] public string PromptInjectionText { get; set; } // What text to drop to the AI conversation when the trigger is triggered (e.g. “I pick the Banana”, “I have done that”)
        [JsonProperty] public string SensorChannel { get; set; }
        [JsonProperty] public string SensorVariable { get; set; }
        [JsonProperty] public string SensorVariableType { get; set; }
        [JsonProperty] public string SensorExpectedValue { get; set; }
        [JsonProperty] public string SensorEvalOperator { get; set; } // "exceed", "below", "equal", "between" - or ">", "", "=", "[x;y]"
    }

    [Serializable]
    public enum TriggerType     //todo
    {
        Unknown = -1,
        Click = 1,
        Sensor = 2,
        Detect = 3,
        Event = 4, // maybe? From improved pick&place, that uses events to advance?
        Proximity = 5
        //todo
    }

    [Serializable]
    public class ContentData
    {
        [JsonProperty] public List<Trigger> Triggers { get; set; }
        [JsonProperty] public List<TriggerType> AvailableTriggers { get; set; }
    }

    [Serializable]
    public class Location
    {
        [JsonProperty] public Vector3 Position { get; set; }
        [JsonProperty] public Vector3 Rotation { get; set; }
        [JsonProperty] public Vector3 Scale { get; set; }
        [JsonProperty] public TargetMarker? TargetMarker { get; set; } //can be null
    }

    [Serializable]
    public class TargetMarker
    {
        [JsonProperty] public File Image { get; set; }
    }

    [Serializable]
    public enum Gender
    {
        Unknown = -1,
        Female = 0,
        Male = 1,
    }

    [Serializable]
    public class GhostDataFrame
    {
        [JsonProperty] public Pose Head { get; set; }
        [JsonProperty] public Pose LeftHand { get; set; }
        [JsonProperty] public Pose RightHand { get; set; }
        [JsonProperty] public Pose UpperSpine { get; set; }
        [JsonProperty] public Pose LowerSpine { get; set; }

        [JsonProperty] public Pose LeftThumbTip { get; set; }
        [JsonProperty] public Pose LeftIndexTip { get; set; }
        [JsonProperty] public Pose LeftMiddleTip { get; set; }
        [JsonProperty] public Pose LeftRingTip { get; set; }
        [JsonProperty] public Pose LeftPinkyTip { get; set; }

        [JsonProperty] public Pose RightThumbTip { get; set; }
        [JsonProperty] public Pose RightIndexTip { get; set; }
        [JsonProperty] public Pose RightMiddleTip { get; set; }
        [JsonProperty] public Pose RightRingTip { get; set; }
        [JsonProperty] public Pose RightPinkyTip { get; set; }
    }

    [Serializable]
    public class DrawDataFrame
    {
        [JsonProperty] public Pose Pose { get; set; }
    }

    [Serializable]
    public class ActionContentData : ContentData
    {
        [JsonProperty] public string FileName { get; set; }
    }

    [Serializable]
    public class AudioContentData : ContentData
    {
        [JsonProperty] public File Audio { get; set; }
        [JsonProperty] public bool IsLooped { get; set; }
        [JsonProperty] public bool Is3dSound { get; set; }
        [JsonProperty] public float SoundRange { get; set; }
    }

    [Serializable]
    public class CharacterContentData : ContentData
    {
        [JsonProperty] public string CharacterName { get; set; }
        //todo
    }

    [Serializable]
    public class DrawingContentData : ContentData
    {
        [JsonProperty] public List<DrawDataFrame> Frames { get; set; }
    }

    [Serializable]
    public class GhostContentData : ContentData
    {
        [JsonProperty] public File Audio { get; set; }
        [JsonProperty] public List<GhostDataFrame> Frames { get; set; }
        [JsonProperty] public Gender Gender { get; set; }
        [JsonProperty] public bool IsLooped { get; set; }
    }

    [Serializable]
    public class GlyphContentData : ContentData
    {
        [JsonProperty] public string FileName { get; set; }
    }

    [Serializable]
    public class ImageContentData : ContentData
    {
        [JsonProperty] public string Text { get; set; }
        [JsonProperty] public File Image { get; set; }
    }

    [Serializable]
    public class ModelContentData : ContentData //or 3dModelContentData 
    {
        [JsonProperty] public bool isLibraryModel { get; set; }
        [JsonProperty] public File? Model { get; set; }
        [JsonProperty] public LibraryModel? LibraryModel { get; set; }
    }

    [Serializable]
    public class LibraryModel 
    {
        [JsonProperty] public string Catalog { get; set; }
        [JsonProperty] public string? ModelName { get; set; }
    }

    [Serializable]
    public class LabelContentData : ContentData
    {
        [JsonProperty] public string Text { get; set; }
        [JsonProperty] public float FrontSize { get; set; }
        [JsonProperty] public Color FrontColor { get; set; }
        [JsonProperty] public Color BackgroundColor { get; set; }
        [JsonProperty] public bool IsBillboarded { get; set; }
    }

    [Serializable]
    public class InteractionContentData : ContentData
    {
        [JsonProperty] public string Text { get; set; }
    }

    [Serializable]
    public class VfxContentData : ContentData
    {
        [JsonProperty] public string FileName { get; set; }
    }

    [Serializable]
    public class VideoContentData : ContentData
    {
        [JsonProperty] public File Video { get; set; }
        [JsonProperty] public bool IsLooped { get; set; }
        [JsonProperty] public bool Is3dSound { get; set; }
        [JsonProperty] public float SoundRange { get; set; }
    }
}