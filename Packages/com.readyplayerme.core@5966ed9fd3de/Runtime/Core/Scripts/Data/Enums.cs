using System.ComponentModel;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    ///     This enumeration describes the body type of the avatar.
    /// </summary>
    public enum BodyType
    {
        None,
        [Description("fullbody")] FullBody,
        [Description("halfbody")] HalfBody,
        [Description("fullbody-xr")] FullBodyXR
    }

    public enum BodyShape
    {
        None,
        [Description("average")] Average,
        [Description("athletic")] Athletic,
        [Description("heavyset")] HeavySet,
        [Description("plussize")] PlusSize
    }

    /// <summary>
    ///     This enumeration describes the avatars OutfitGender which is used for setting the correct skeleton.
    /// </summary>
    public enum OutfitGender
    {
        None,
        [Description("masculine")] Masculine,
        [Description("feminine")] Feminine,
        [Description("neutral")] Neutral
    }

    /// <summary>
    ///     This enumeration describes the type of mesh.
    /// </summary>
    public enum MeshType
    {
        HeadMesh,
        BeardMesh,
        TeethMesh
    }

    /// <summary>
    ///     This enumeration describes the pose options for the avatar skeleton.
    /// </summary>
    public enum Pose
    {
        APose,
        TPose
    }

    /// <summary>
    ///     This enumeration describes the avatar mesh LOD (Level of Detail) options.
    /// </summary>
    public enum Lod
    {
        [InspectorName("High (LOD0)")]
        High,
        [InspectorName("Medium (LOD1)")]
        Medium,
        [InspectorName("Low (LOD2)")]
        Low
    }

    /// <summary>
    ///     This enumeration describes the TextureAtlas setting options.
    /// </summary>
    /// <remarks>If set to <c>None</c> the avatar meshes, materials and textures will NOT be combined.</remarks>
    public enum TextureAtlas
    {
        None,
        [InspectorName("High (1024)")]
        High,
        [InspectorName("Medium (512)")]
        Medium,
        [InspectorName("Low (256)")]
        Low
    }

    public enum TextureChannel
    {
        BaseColor,
        Normal,
        MetallicRoughness,
        Emissive,
        Occlusion
    }

    /// <summary>
    ///     This enumeration describes the different types of failures.
    /// </summary>
    public enum FailureType
    {
        None,
        NoInternetConnection,
        UrlProcessError,
        ShortCodeError,
        DownloadError,
        MetadataDownloadError,
        MetadataParseError,
        ModelDownloadError,
        ModelImportError,
        DirectoryAccessError,
        AvatarProcessError,
        AvatarRenderError,
        OperationCancelled,
        Unknown
    }

    public enum Expression
    {
        None,
        Happy,
        Lol,
        Sad,
        Scared,
        Rage
    }

    /// <summary>
    ///     This enumeration describes the different types of render poses.
    ///     These poses are only supported for fullbody avatars, for halfbody avatars this should be set to None.
    /// </summary>
    public enum RenderPose
    {
        None = -1,
        Relaxed,
        PowerStance,
        Standing,
        ThumbsUp
    }

    public enum RenderCamera
    {
        Portrait,
        FullBody
    }

}
