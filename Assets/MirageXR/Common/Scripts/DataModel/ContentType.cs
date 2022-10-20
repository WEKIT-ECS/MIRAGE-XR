using System;
using UnityEngine;

public enum ContentType
{
    UNKNOWN,
    IMAGE,
    VIDEO,
    AUDIO,
    GHOST,
    LABEL,
    ACT,
    VFX,
    MODEL,
    CHARACTER,
    PICKANDPLACE,
    IMAGEMARKER,
    PLUGIN,
    DRAWING
}

public static class ContentTypeExtenstion
{
    private const string UNKNOWN = "Unknown";
    private const string IMAGE = "Image";
    private const string VIDEO = "Video";
    private const string AUDIO = "Audio";
    private const string GHOST = "Ghost";
    private const string LABEL = "Label";
    private const string ACT = "Action";
    private const string VFX = "Vfx";
    private const string MODEL = "Model";
    private const string CHARACTER = "Character";
    private const string PICKANDPLACE = "Pick and place";
    private const string IMAGEMARKER = "Image marker";
    private const string PLUGIN = "Plugin";
    private const string DRAWING = "Drawing";

    private const string PREDICATE_UNKNOWN = "unknown";
    private const string PREDICATE_LABEL = "label";
    private const string PREDICATE_IMAGE = "image";
    private const string PREDICATE_AUDIO = "audio";
    private const string PREDICATE_AUDIO_V2 = "sound"; // TODO: we should leave only one option, but we have several different references in the code. ideally, we should use the index enum
    private const string PREDICATE_VIDEO = "video";
    private const string PREDICATE_GHOST = "ghosttracks";
    private const string PREDICATE_GHOST_V2 = "ghost"; // TODO: ^^^
    private const string PREDICATE_ACT = "act";
    private const string PREDICATE_VFX = "vfx";
    private const string PREDICATE_MODEL = "model";
    private const string PREDICATE_MODEL_V2 = "3d"; // TODO: ^^^
    private const string PREDICATE_CHARACTER = "char";
    private const string PREDICATE_PICKANDPLACE = "pickandplace";
    private const string PREDICATE_PICKANDPLACE_V2 = "pick&place"; // TODO: ^^^
    private const string PREDICATE_IMAGEMARKER = "imagemarker";
    private const string PREDICATE_PLUGIN = "plugin";
    private const string PREDICATE_DRAWING = "drawing";

    private const string UNKNOWN_HINT = "Unknown";
    private const string IMAGE_HINT = "Take a photo and add it as an augmentation to this action step.";
    private const string VIDEO_HINT = "Record a video and add it as an augmentation to this action step.";
    private const string AUDIO_HINT = "Record an audio and add it to this action step.";
    private const string GHOST_HINT = "Ghost track lets you record your movement in the real world and adds it to the this action as a virtual avatar.";
    private const string LABEL_HINT = "Label augmentation lets you add a text.";
    private const string ACT_HINT = "Pre-define models which represents verbs.";
    private const string VFX_HINT = "Visual Effects lets you add effects like fire, explosion, etc.";
    private const string MODEL_HINT = "You can import 3d models from Sketchfab.com to this action step. Sketchfab is a library with more than 3 million 3d models which half milions of them are free.";
    private const string CHARACTER_HINT = "Add an AI character to this action. You can choose between different characters that each of them can do different tasks. ";
    private const string PICKANDPLACE_HINT = "Add flags on an objects.";
    private const string IMAGEMARKER_HINT = "Image marker allows to take a photo of an object (or select a pretrained image target) and thus allow to move task stations with the marker around.";
    private const string PLUGIN_HINT = "Augmentations that are created for specific activities";
    private const string DRAWING_HINT = "Draw in 3d space";

    private const string IMAGE_IMAGE_PATH = "Icons/Editors/image";
    private const string VIDEO_IMAGE_PATH = "Icons/Editors/video";
    private const string AUDIO_IMAGE_PATH = "Icons/Editors/audio";
    private const string GHOST_IMAGE_PATH = "Icons/Editors/ghost";
    private const string LABEL_IMAGE_PATH = "Icons/Editors/label";
    private const string ACT_IMAGE_PATH = "Icons/Editors/action";
    private const string VFX_IMAGE_PATH = "Icons/Editors/visualeffect";
    private const string MODEL_IMAGE_PATH = "Icons/Editors/model";
    private const string CHARACTER_IMAGE_PATH = "Icons/Editors/character";
    private const string PICKANDPLACE_IMAGE_PATH = "Icons/Editors/pickandplace";
    private const string IMAGEMARKER_IMAGE_PATH = "Icons/Editors/imagemarker";
    private const string PLUGIN_IMAGE_PATH = "Materials/Textures/plugineditor";
    private const string DRAWING_IMAGE_PATH = "Materials/Textures/drawingeditor";

    public static string GetName(this ContentType type)
    {
        switch (type)
        {
            case ContentType.UNKNOWN: return UNKNOWN;
            case ContentType.IMAGE: return IMAGE;
            case ContentType.VIDEO: return VIDEO;
            case ContentType.AUDIO: return AUDIO;
            case ContentType.GHOST: return GHOST;
            case ContentType.LABEL: return LABEL;
            case ContentType.ACT: return ACT;
            case ContentType.VFX: return VFX;
            case ContentType.MODEL: return MODEL;
            case ContentType.CHARACTER: return CHARACTER;
            case ContentType.PICKANDPLACE: return PICKANDPLACE;
            case ContentType.IMAGEMARKER: return IMAGEMARKER;
            case ContentType.PLUGIN: return PLUGIN;
            case ContentType.DRAWING: return DRAWING;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static Sprite GetIcon(this ContentType type)
    {
        return Resources.Load<Sprite>(type.GetImagePath());
    }

    private static string GetImagePath(this ContentType type)
    {
        switch (type)
        {
            case ContentType.IMAGE: return IMAGE_IMAGE_PATH;
            case ContentType.VIDEO: return VIDEO_IMAGE_PATH;
            case ContentType.AUDIO: return AUDIO_IMAGE_PATH;
            case ContentType.GHOST: return GHOST_IMAGE_PATH;
            case ContentType.LABEL: return LABEL_IMAGE_PATH;
            case ContentType.ACT: return ACT_IMAGE_PATH;
            case ContentType.VFX: return VFX_IMAGE_PATH;
            case ContentType.MODEL: return MODEL_IMAGE_PATH;
            case ContentType.CHARACTER: return CHARACTER_IMAGE_PATH;
            case ContentType.PICKANDPLACE: return PICKANDPLACE_IMAGE_PATH;
            case ContentType.IMAGEMARKER: return IMAGEMARKER_IMAGE_PATH;
            case ContentType.PLUGIN: return PLUGIN_IMAGE_PATH;
            case ContentType.UNKNOWN: return MODEL_IMAGE_PATH; // TODO: add icon for unknown content type
            case ContentType.DRAWING: return DRAWING_IMAGE_PATH;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static string GetHint(this ContentType type)
    {
        switch (type)
        {
            case ContentType.UNKNOWN: return UNKNOWN_HINT;
            case ContentType.IMAGE: return IMAGE_HINT;
            case ContentType.VIDEO: return VIDEO_HINT;
            case ContentType.AUDIO: return AUDIO_HINT;
            case ContentType.GHOST: return GHOST_HINT;
            case ContentType.LABEL: return LABEL_HINT;
            case ContentType.ACT: return ACT_HINT;
            case ContentType.VFX: return VFX_HINT;
            case ContentType.MODEL: return MODEL_HINT;
            case ContentType.CHARACTER: return CHARACTER_HINT;
            case ContentType.PICKANDPLACE: return PICKANDPLACE_HINT;
            case ContentType.IMAGEMARKER: return IMAGEMARKER_HINT;
            case ContentType.PLUGIN: return PLUGIN_HINT;
            case ContentType.DRAWING: return DRAWING_HINT;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static string GetPredicate(this ContentType type)
    {
        switch (type)
        {
            case ContentType.UNKNOWN: return PREDICATE_UNKNOWN;
            case ContentType.IMAGE: return PREDICATE_IMAGE;
            case ContentType.VIDEO: return PREDICATE_VIDEO;
            case ContentType.AUDIO: return PREDICATE_AUDIO;
            case ContentType.GHOST: return PREDICATE_GHOST;
            case ContentType.LABEL: return PREDICATE_LABEL;
            case ContentType.ACT: return PREDICATE_ACT;
            case ContentType.VFX: return PREDICATE_VFX;
            case ContentType.MODEL: return PREDICATE_MODEL;
            case ContentType.CHARACTER: return PREDICATE_CHARACTER;
            case ContentType.PICKANDPLACE: return PREDICATE_PICKANDPLACE;
            case ContentType.IMAGEMARKER: return PREDICATE_IMAGEMARKER;
            case ContentType.PLUGIN: return PREDICATE_PLUGIN;
            case ContentType.DRAWING: return PREDICATE_DRAWING;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static ContentType ParsePredicate(string predicate)
    {
        var predicateLowCase = predicate.ToLower();
        if (predicateLowCase.Contains(PREDICATE_IMAGE))
            return ContentType.IMAGE;
        if (predicateLowCase.Contains(PREDICATE_VIDEO))
            return ContentType.VIDEO;
        if (predicateLowCase.Contains(PREDICATE_AUDIO) || predicateLowCase.Contains(PREDICATE_AUDIO_V2))
            return ContentType.AUDIO;
        if (predicateLowCase.Contains(PREDICATE_GHOST) || predicateLowCase.Contains(PREDICATE_GHOST_V2))
            return ContentType.GHOST;
        if (predicateLowCase.Contains(PREDICATE_LABEL))
            return ContentType.LABEL;
        if (predicateLowCase.Contains(PREDICATE_ACT))
            return ContentType.ACT;
        if (predicateLowCase.Contains(PREDICATE_VFX))
            return ContentType.VFX;
        if (predicateLowCase.Contains(PREDICATE_MODEL) || predicateLowCase.Contains(PREDICATE_MODEL_V2))
            return ContentType.MODEL;
        if (predicateLowCase.Contains(PREDICATE_CHARACTER))
            return ContentType.CHARACTER;
        if (predicateLowCase.Contains(PREDICATE_PICKANDPLACE) || predicateLowCase.Contains(PREDICATE_PICKANDPLACE_V2))
            return ContentType.PICKANDPLACE;
        if (predicateLowCase.Contains(PREDICATE_IMAGEMARKER))
            return ContentType.IMAGEMARKER;
        if (predicateLowCase.Contains(PREDICATE_PLUGIN))
            return ContentType.PLUGIN;
        if (predicateLowCase.Contains(PREDICATE_DRAWING))
            return ContentType.DRAWING;

        return ContentType.UNKNOWN;
    }
}