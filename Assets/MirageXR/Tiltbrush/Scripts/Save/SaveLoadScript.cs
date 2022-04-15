// Copyright 2020 The Tilt Brush Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace TiltBrush
{

  public class SaveLoadScript : MonoBehaviour
  {

    //
    // Static API
    //

    public const string UNTITLED_PREFIX = "Untitled_";
    public const string TILTASAURUS_PREFIX = "Tiltasaurus_";
    public const string TILT_SUFFIX = ".tilt";

    public static SaveLoadScript m_Instance;

    public static IEnumerable<DiskSceneFileInfo> IterScenes(DirectoryInfo di)
    {
      foreach (var sub in di.GetFiles("*" + TILT_SUFFIX))
      {
        yield return new DiskSceneFileInfo(sub.FullName);
      }
      foreach (var sub in di.GetDirectories("*" + TILT_SUFFIX))
      {
        yield return new DiskSceneFileInfo(sub.FullName);
      }
    }

    // e.g. for "Foo (0123-abcd).tilt", capture "0123-abcd".
    static Regex Md5SuffixRegex = new Regex(@" \(([0-9a-f]{4}-[0-9a-f]{4})\)\.tilt$");
    static Regex Md5RemoveSuffixRegex = new Regex(@"(.+) \([0-9a-f]{4}-[0-9a-f]{4}\)(\.tilt)$");

    /// Returns MD5 suffix portion of given .tilt filename if it's present, else null.
    public static string Md5Suffix(string fileName)
    {
      var groups = Md5SuffixRegex.Match(fileName).Groups;
      return (groups.Count == 2) ? groups[1].Value : null;
    }

    /// Returns filename or path with MD5 suffix removed.
    public static string RemoveMd5Suffix(string fileName)
    {
      return Md5RemoveSuffixRegex.Replace(fileName, @"$1$2");
    }

    /// Returns given filename/path with MD5 suffix inserted.  Input should not have a suffix.
    public static string AddMd5Suffix(string filename, byte[] hash)
    {
      string md5 = BitConverter.ToString(hash).Replace("-", null).ToLower();
      return filename.Replace(
        TILT_SUFFIX,
        String.Format(" ({0}-{1}){2}",
          md5.Substring(md5.Length - 8, 4),
          md5.Substring(md5.Length - 4, 4),
          TILT_SUFFIX));
    }

    /// Returns MD5 hash computed via blocking read of entire file.
    public static byte[] GetMd5(string path)
    {
      var hashAlg = new System.Security.Cryptography.MD5CryptoServiceProvider();
      return hashAlg.ComputeHash(new FileStream(path, FileMode.Open, FileAccess.Read));
    }

    // See https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding?view=netframework-4.7.1
    const int kAsciiCodePage = 20127;
    static SaveLoadScript()
    {
#if UNITY_2018_4_OR_NEWER
      // 2018 doesn't include ANSICodePage any more -- or maybe it's only if we use .net 4.6?
      ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = kAsciiCodePage;
#else
    // There's an ancient mono bug (that Unity inherits) that prevents builds
    // from including the proper set of code pages, causing runtime errors when
    // SharpZipLib tries to use one. We only ever write ASCII filenames, so
    // the choice here is arbitrary. It just needs to be something that is
    // guaranteed to be included in the build.
    ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage =
        System.Globalization.CultureInfo.InvariantCulture.TextInfo.ANSICodePage;
#endif
    }

    //
    // Instance API
    //

    private string m_SaveDir;
    private SceneFileInfo m_LastSceneFile;
    private bool m_LastSceneIsLegacy;

    private int m_LastNonexistentFileIndex = 0;

    private JsonSerializer m_JsonSerializer;

    [SerializeField] private SaveIconCaptureScript m_SaveIconCapture;
    private IEnumerator<Timeslice> m_SaveCoroutine;

    [SerializeField] private bool m_CaptureHiResSaveIcon = false;
    [SerializeField] private bool m_CaptureGifSaveIcon = false;

    // Various Save Icon render textures.
    [SerializeField] private int m_SaveIconHiResWidth = 1920;
    [SerializeField] private int m_SaveIconHiResHeight = 1080;
    private RenderTexture m_SaveIconHiResRenderTexture;

    [SerializeField] private int m_SaveGifWidth = 256;
    [SerializeField] private int m_SaveGifHeight = 256;
    private RenderTexture[] m_SaveGifRenderTextures;
    public int m_SaveGifTextureCount = 5;

    private RenderTexture m_SaveIconRenderTexture;

    private byte[] m_LastThumbnailBytes;
    private string m_LastJsonMetadatError;
    private string m_LastWriteSnapshotError;
    private bool m_SuppressNotify;

    public TrTransform? LastThumbnail_SS { get; set; }

    public TrTransform ReasonableThumbnail_SS
    {
      get
      {
        if (LastThumbnail_SS.HasValue)
        {
          return LastThumbnail_SS.Value;
        }
        else
        {
          // If we need to create a default position, we want it to be quick, so we set the
          // number of items to enumerate to 1.
          return SketchControlsScript.m_Instance.GenerateBestGuessSaveIconTransform(
              itemsToEnumerate: 1);
        }
      }
    }

    /// Check .Valid on return value if you want to know
    public SceneFileInfo SceneFile => m_LastSceneFile;

    public RenderTexture GetSaveIconRenderTexture()
    {
      return m_SaveIconRenderTexture;
    }

    public bool SuppressSaveNotifcation { set => m_SuppressNotify = value; }

    public bool CanOverwriteSource { get { return !m_LastSceneFile.ReadOnly; } }

    public string LastMetadataError
    {
      get { return m_LastJsonMetadatError; }
    }

    public string LastWriteSnapshotError { get => m_LastWriteSnapshotError; }

    public JsonSerializer JsonSerializer { get { return m_JsonSerializer; } }

    public bool IsSavingAllowed() { return m_SaveCoroutine == null; }

    public string GetLastFileHumanName()
    {
      return m_LastSceneFile.Valid ? m_LastSceneFile.HumanName : "New Sketch";
    }

    public byte[] GetLastThumbnailBytes()
    {
      return m_LastThumbnailBytes;
    }

    void Awake()
    {
      m_Instance = this;
      m_JsonSerializer = new JsonSerializer();
      m_JsonSerializer.ContractResolver = new CustomJsonContractResolver();
      m_JsonSerializer.Error += HandleDeserializationError;

      ResetLastFilename();

      // Create hi-res save icon render texture.
      m_SaveIconHiResRenderTexture = new RenderTexture(m_SaveIconHiResWidth, m_SaveIconHiResHeight,
                                                       0, RenderTextureFormat.ARGB32);

      // Guarantee we've got an odd, >0 number of gif render textures.
      Debug.Assert((m_SaveGifTextureCount % 2) == 1);

      // Create low-res save gif render textures.
      m_SaveGifRenderTextures = new RenderTexture[m_SaveGifTextureCount];
      for (int i = 0; i < m_SaveGifTextureCount; ++i)
      {
        m_SaveGifRenderTextures[i] = new RenderTexture(m_SaveGifWidth, m_SaveGifHeight, 0,
                                                       RenderTextureFormat.ARGB32);
      }
      m_SaveIconRenderTexture = new RenderTexture(m_SaveGifWidth, m_SaveGifHeight, 0,
                                                  RenderTextureFormat.ARGB32);
    }

    public void ResetLastFilename()
    {
      m_LastSceneFile = new DiskSceneFileInfo();
    }

    // Create a name that is guaranteed not to exist.
    public string GenerateNewUntitledFilename(string directory, string extension)
    {
      int iIndex = m_LastNonexistentFileIndex;
      int iSanity = 9999;
      while (iSanity > 0)
      {
        string attempt = UNTITLED_PREFIX + iIndex.ToString();
        --iSanity;
        ++iIndex;

        attempt = Path.Combine(directory, attempt) + extension;
        if (!File.Exists(attempt) && !Directory.Exists(attempt))
        {
          m_LastNonexistentFileIndex = iIndex;
          return attempt;
        }
      }

      Debug.Assert(false, "Could not generate a name");
      return null;
    }

    public void SaveOverwriteOrNewIfNotAllowed()
    {
      bool saveNew = true;
      if (SceneFile.Valid)
      {
        if (!SceneFile.ReadOnly)
        {
          SketchControlsScript.m_Instance.IssueGlobalCommand(
              SketchControlsScript.GlobalCommands.Save);
          saveNew = false;
        }
      }

      if (saveNew)
      {
        SketchControlsScript.m_Instance.IssueGlobalCommand(
            SketchControlsScript.GlobalCommands.SaveNew);
      }
    }

    /// Used only for monoscopic backwards compatibility
    public IEnumerator<Timeslice> SaveMonoscopic(int slot)
    {
      string path = Path.Combine(m_SaveDir, "Sketch" + slot) + TILT_SUFFIX;
      Debug.LogFormat("Saving to {0}", path);
      return SaveLow(new DiskSceneFileInfo(path));
    }

    /// When a new scene file info or metadata is being created from an existing scene file info,
    /// we either preserve SourceId, or if this was a cloud sketch set it from the original asset.
    public string TransferredSourceIdFrom(SceneFileInfo info)
    {
      return info.SourceId;
    }

    public DiskSceneFileInfo GetNewNameSceneFileInfo(bool tiltasaurusMode = false)
    {
      DiskSceneFileInfo fileInfo = new DiskSceneFileInfo(GenerateNewUntitledFilename(m_SaveDir, TILT_SUFFIX));
      if (m_LastSceneFile.Valid)
      {
        fileInfo.SourceId = TransferredSourceIdFrom(m_LastSceneFile);
      }
      return fileInfo;
    }

    /// Save a snapshot directly to a location.
    /// The snapshot's AssetId is the source of truth
    public IEnumerator<Timeslice> SaveSnapshot(SceneFileInfo fileInfo, SketchSnapshot snapshot)
    {
      return SaveLow(fileInfo, false, snapshot);
    }

    /// Save, overwriting current file name
    /// Used mostly for doing silent upgrades-in-place and so on.
    /// Also used for the emergency-save command
    public IEnumerator<object> SaveOverwrite(bool tiltasaurusMode = false)
    {
      if (!m_LastSceneFile.Valid || tiltasaurusMode)
      {
        yield return SaveNewName(tiltasaurusMode);
      }
      else
      {
        yield return SaveLow(m_LastSceneFile);
      }
    }

    /// Save to a completely new name
    public IEnumerator<Timeslice> SaveNewName(bool tiltasaurusMode = false)
    {
      return SaveLow(GetNewNameSceneFileInfo(tiltasaurusMode));
    }

    /// In order to for this to work properly:
    /// - m_SaveIconRenderTexture must contain data
    /// - SaveIconTool.LastSaveCameraRigState must be good
    /// SaveIconTool.ProgrammaticCaptureSaveIcon() does both of these things
    private IEnumerator<Timeslice> SaveLow(
        SceneFileInfo info, bool bNotify = true, SketchSnapshot snapshot = null)
    {
      Debug.Assert(!SelectionManager.m_Instance.HasSelection);
      if (snapshot != null && info.AssetId != snapshot.AssetId)
      {
        Debug.LogError($"AssetId in FileInfo '{info.AssetId}' != shapshot '{snapshot.AssetId}'");
      }
      if (!info.Valid)
      {
        throw new ArgumentException("null filename");
      }

      if (!FileUtils.CheckDiskSpaceWithError(m_SaveDir))
      {
        return new List<Timeslice>().GetEnumerator();
      }

      m_LastSceneFile = info;
      Debug.LogFormat("Saving {0}", info.HumanName);

      m_SaveCoroutine = ThreadedSave(info, bNotify, snapshot);
      return m_SaveCoroutine;
    }

    private IEnumerator<Timeslice> ThreadedSave(SceneFileInfo fileInfo,
                                                bool bNotify = true, SketchSnapshot snapshot = null)
    {
      bool newFile = !fileInfo.Exists;

      if (snapshot == null)
      {
        IEnumerator<Timeslice> timeslicedConstructor;
        snapshot = CreateSnapshotWithIcons(out timeslicedConstructor);
        if (App.CurrentState != App.AppState.Reset)
        {
          App.Instance.SetDesiredState(App.AppState.Saving);
        }
        while (timeslicedConstructor.MoveNext())
        {
          yield return timeslicedConstructor.Current;
        }
      }
      LastThumbnail_SS = snapshot.LastThumbnail_SS;
      App.Instance.SetDesiredState(App.AppState.Standard);
      m_LastWriteSnapshotError = null;

      string error = null;
      var writeFuture = new Future<string>(
          () => snapshot.WriteSnapshotToFile(fileInfo.FullPath),
          null, true);
      while (!writeFuture.TryGetResult(out error))
      {
        yield return null;
      }

      m_LastWriteSnapshotError = error;
      m_LastThumbnailBytes = snapshot.Thumbnail;
      SketchMemoryScript.m_Instance.SetLastOperationStackCount();
      SketchMemoryScript.m_Instance.InitialSketchTransform = App.Scene.Pose;
      m_SaveCoroutine = null;

      if (bNotify && !m_SuppressNotify)
      {
        NotifySaveFinished(m_LastSceneFile, error, newFile);
      }
      m_SuppressNotify = false;
    }

    /// If success, error should be null
    private void NotifySaveFinished(SceneFileInfo info, string error, bool newFile)
    {
      if (error == null)
      {
        // TODO: More long term something should be done in OutputWindowScript itself to handle such
        // cases, such as a check for Floating Panels Mode or a check for if the controller requested
        // is tracked.
        if (newFile)
        {
          OutputWindowScript.ReportFileSaved("Added to Sketchbook!", info.FullPath,
              OutputWindowScript.InfoCardSpawnPos.Brush);
        }
        else
        {
          OutputWindowScript.ReportFileSaved("Saved!", info.FullPath,
              OutputWindowScript.InfoCardSpawnPos.UIReticle);
        }
        
      }
      else
      {
        OutputWindowScript.Error(
            InputManager.ControllerName.Wand,
            "Failed to save sketch", error);
      }
    }

    static public Stream GetMetadataReadStream(SceneFileInfo fileInfo)
    {
      var stream = fileInfo.GetReadStream(TiltFile.FN_METADATA);
      if (stream != null)
      {
        return stream;
      }
      else
      {
        return fileInfo.GetReadStream(TiltFile.FN_METADATA_LEGACY);
      }
    }

    // Loads the head and scene trandsforms into the secondary ODS
    public bool LoadTransformsForOds(SceneFileInfo fileInfo,
                                     ref TrTransform head,
                                     ref TrTransform scene)
    {
      if (!fileInfo.IsHeaderValid())
      {
        OutputWindowScript.m_Instance.AddNewLine(
            "Could not load transform: {0}", fileInfo.HumanName);
        return false;
      }

      m_LastSceneIsLegacy = false;
      Stream metadata = GetMetadataReadStream(fileInfo);
      if (metadata == null)
      {
        OutputWindowScript.m_Instance.AddNewLine("Could not load: {0}", fileInfo.HumanName);
        return false;
      }
      using (var jsonReader = new JsonTextReader(new StreamReader(metadata)))
      {
        var jsonData = DeserializeMetadata(jsonReader);

        if (jsonData.RequiredCapabilities != null)
        {
          var missingCapabilities = jsonData.RequiredCapabilities.Except(
            Enum.GetNames(typeof(PlaybackCapabilities))).ToArray();
          if (missingCapabilities.Length > 0)
          {
            Debug.LogFormat("Lacking playback capabilities: {0}",
              String.Join(", ", missingCapabilities));
            OutputWindowScript.m_Instance.AddNewLine(
              $"Lacking a capability to load {fileInfo.HumanName}. " +
              $"Upgrade {App.kAppDisplayName}?");
            return false;
          }
        }

        scene = jsonData.SceneTransformInRoomSpace;
        head = jsonData.ThumbnailCameraTransformInRoomSpace;
      }

      return true;
    }

    /// Follows the "force-superseded by" chain until the end is reached, then returns that brush
    /// If the passed Guid is invalid, returns it verbatim.
    static Guid GetForceSupersededBy(Guid original)
    {
      var brush = BrushCatalog.m_Instance.GetBrush(original);
      // The failure will be reported downstream
      if (brush == null) { return original; }
      while (brush.m_SupersededBy != null && brush.m_SupersededBy.m_LooksIdentical)
      {
        brush = brush.m_SupersededBy;
      }
      return brush.m_Guid;
    }

    /// bAdditive is an experimental feature.
    /// XXX: bAdditive is buggy; it re-draws any pre-existing strokes.
    /// We never noticed before because the duplicate geometry draws on top of itself.
    /// It begins to be noticeable now that loading goes into the active canvas,
    /// which may not be the canvas of the original strokes.
    public bool Load(SceneFileInfo fileInfo, bool bAdditive = false)
    {
      //Debug.LogFormat("Loading {0} {1}", fileInfo.HumanName, fileInfo.FullPath);
      m_LastSceneFile = fileInfo;
      m_LastThumbnailBytes = null;
      if (!m_LastSceneFile.IsHeaderValid())
      {
        Debug.LogErrorFormat(
            "Could not load: {0}", fileInfo.HumanName);
        return false;
      }

      m_LastSceneIsLegacy = false;
      Stream metadata = GetMetadataReadStream(m_LastSceneFile);
      if (metadata == null)
      {
        OutputWindowScript.m_Instance.AddNewLine("Could not load: {0}", fileInfo.HumanName);
        return false;
      }
      using (var jsonReader = new JsonTextReader(new StreamReader(metadata)))
      {
        var jsonData = DeserializeMetadata(jsonReader);
        if (LastMetadataError != null)
        {
          Debug.LogWarning(string.Format("Error reading meteadata for {0}.\n{1}",
                                         fileInfo.FullPath,
                                         SaveLoadScript.m_Instance.LastMetadataError));
        }
        if (jsonData.RequiredCapabilities != null)
        {
          var missingCapabilities = jsonData.RequiredCapabilities.Except(
            Enum.GetNames(typeof(PlaybackCapabilities))).ToArray();
          if (missingCapabilities.Length > 0)
          {
            Debug.LogFormat("Lacking playback capabilities: {0}",
              String.Join(", ", missingCapabilities));
            return false;
          }
        }

        //App.Scene.Pose = jsonData.SceneTransformInRoomSpace;
        //Coords.CanvasLocalPose = TrTransform.identity;
        if (jsonData.CanvasTransformInSceneSpace != TrTransform.identity)
        {
          Debug.LogWarning("This file has an unsupported, experimental Canvas Transform specified.");
#if (UNITY_EDITOR || EXPERIMENTAL_ENABLED)
          if (Config.IsExperimental)
          {
            //Coords.CanvasLocalPose = jsonData.CanvasTransformInSceneSpace;
          }
#endif
        }
        LastThumbnail_SS = App.Scene.Pose.inverse *
                           jsonData.ThumbnailCameraTransformInRoomSpace;

        SketchControlsScript.m_Instance.SketchPlaybackMode =
            SketchControlsScript.m_Instance.m_DefaultSketchPlaybackMode;

        // Load sketch
        using (var stream = m_LastSceneFile.GetReadStream(TiltFile.FN_SKETCH))
        {
          Guid[] brushGuids = jsonData.BrushIndex.Select(GetForceSupersededBy).ToArray();
          bool legacySketch;
          bool success = SketchWriter.ReadMemory(stream, brushGuids, bAdditive, out legacySketch);
          m_LastSceneIsLegacy |= legacySketch;
          if (!success)
          {
            OutputWindowScript.m_Instance.AddNewLine(
                "Could not load: {0}", fileInfo.HumanName);
            // Prevent it from being overwritten
            m_LastSceneIsLegacy = false;
            return false;
          }
        }

        if (jsonData.GuideIndex != null)
        {
          foreach (Guides guides in jsonData.GuideIndex)
          {
            StencilWidget.FromGuideIndex(guides);
          }
        }

        // Pass even if null; null is treated as empty
        CustomColorPaletteStorage.m_Instance.SetColorsFromPalette(jsonData.Palette);
      }

      return true;
    }

    public SketchMetadata DeserializeMetadata(JsonTextReader jsonReader)
    {
      m_LastJsonMetadatError = null;
      var metadata = m_JsonSerializer.Deserialize<SketchMetadata>(jsonReader);
      return metadata;
    }

    private void HandleDeserializationError(object sender,
                                            Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
    {
      var currentError = errorArgs.ErrorContext.Error.Message;
      Debug.LogWarning(currentError);
      m_LastJsonMetadatError = currentError;
      errorArgs.ErrorContext.Handled = true;
    }

    public void SignalPlaybackCompletion()
    {
      if (DevOptions.I.ResaveLegacyScenes && m_LastSceneIsLegacy)
      {
        Debug.Log("Rewriting legacy file: " + m_LastSceneFile.HumanName + TILT_SUFFIX);
        StartCoroutine(SaveOverwrite());
      }
    }

    private IEnumerator ShowErrorAfterDelay(string error, float delay)
    {
      yield return new WaitForSeconds(delay);
      OutputWindowScript.Error(error);
    }

    /// Like the SketchSnapshot constructor, but also populates the snapshot with icons.
    public async Task<SketchSnapshot> CreateSnapshotWithIconsAsync()
    {
      var snapshot = CreateSnapshotWithIcons(out var coroutine);
      await coroutine;  // finishes off the snapshot
      return snapshot;
    }

    /// Like the SketchSnapshot constructor, but also populates the snapshot with icons.
    /// As with the constructor, you must run the coroutine to completion before the snapshot
    /// is usable.
    public SketchSnapshot CreateSnapshotWithIcons(out IEnumerator<Timeslice> coroutine)
    {
      IEnumerator<Timeslice> timeslicedConstructor;
      SketchSnapshot snapshot = new SketchSnapshot(
          m_JsonSerializer, m_SaveIconCapture, out timeslicedConstructor);
      coroutine = CoroutineUtil.Sequence(
          timeslicedConstructor,
          null);
      return snapshot;
    }
  }

}  // namespace TiltBrush
