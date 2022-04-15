using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using static TiltBrush.SketchControlsScript;
using static TiltBrush.SketchMemoryScript;
using static TiltBrush.SketchWriter;

namespace TiltBrush
{
    public class SimpleSnapshot
    {
        private const int kNanoSecondsPerSnapshotSlice = 250;

        private List<Stroke> strokes;
        private List<AdjustedMemoryBrushStroke> adjustedStrokes;
        private JsonSerializer jsonSerializer;
        private SketchMetadata metadata;
        private GroupIdMapping m_GroupIdMapping;

        public bool IsPlaying => SketchMemoryScript.m_Instance.IsPlaying;
        public bool IsDonePlaying => SketchMemoryScript.m_Instance.IsDonePlaying;

        public SimpleSnapshot()
        {
            strokes = SketchMemoryScript.AllStrokes().ToList();

            jsonSerializer = new JsonSerializer();
            jsonSerializer.ContractResolver = new CustomJsonContractResolver();
        }

        private void LoadIntoMemory()
        {
            foreach (var stroke in strokes)
            {
                stroke.m_Type = Stroke.Type.NotCreated;
                SketchMemoryScript.m_Instance.MemoryListAdd(stroke);
            }
        }

        public void Play(bool instant = false)
        {
            if (!SketchMemoryScript.m_Instance.IsDonePlaying)
            {
                Resume();
                return;
            }

            if (SketchControlsScript.m_Instance.IsCommandAvailable(GlobalCommands.NewSketch))
                SketchControlsScript.m_Instance.IssueGlobalCommand(GlobalCommands.NewSketch);

            LoadIntoMemory();

            SketchMemoryScript.m_Instance.SetPlaybackMode(instant ? PlaybackMode.Distance : PlaybackMode.Timestamps, SketchControlsScript.m_Instance.m_DefaultSketchLoadSpeed);

            SketchMemoryScript.m_Instance.BeginDrawingFromMemory(bDrawFromStart: true);
        }

        public void Pause()
        {
            SketchMemoryScript.m_Instance.SetPlaybackPause(true);
        }

        private void Resume()
        {
            SketchMemoryScript.m_Instance.SetPlaybackPause(false);
        }

        public void Stop()
        {
            if (SketchControlsScript.m_Instance.IsCommandAvailable(GlobalCommands.NewSketch))
                SketchControlsScript.m_Instance.IssueGlobalCommand(GlobalCommands.NewSketch);

            SketchMemoryScript.m_Instance.EndDrawingFromMemory();
        }

        public async Task WriteToFile(string path)
        {
            await GenerateAdjustedMemoryBrushStroke();
            GenerateMetadata();

            try
            {
                using (var tiltWriter = new TiltFile.AtomicWriter(path))
                {
                    List<Guid> brushGuids;
                    using (var stream = tiltWriter.GetWriteStream(TiltFile.FN_SKETCH))
                    {
                        SketchWriter.WriteMemory(stream, adjustedStrokes, m_GroupIdMapping, out brushGuids);
                    }
                    metadata.BrushIndex = brushGuids.Select(GetForcePrecededBy).ToArray();

                    using (var jsonWriter = new CustomJsonWriter(new StreamWriter(
                        tiltWriter.GetWriteStream(TiltFile.FN_METADATA))))
                    {
                        jsonSerializer.Serialize(jsonWriter, metadata);
                    }

                    tiltWriter.Commit();
                }
            }
            catch (IOException ex)
            {
                UnityEngine.Debug.LogError(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                UnityEngine.Debug.LogError(ex.Message);
            }
        }

        private async Task GenerateAdjustedMemoryBrushStroke()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            long maxTicks = (Stopwatch.Frequency * kNanoSecondsPerSnapshotSlice) / 1000000;

            int numStrokes = strokes.Count;
            adjustedStrokes = new List<AdjustedMemoryBrushStroke>(numStrokes);
            foreach (var strokeSnapshot in SketchWriter.EnumerateAdjustedSnapshots(strokes))
            {
                if (stopwatch.ElapsedTicks > maxTicks)
                {
                    stopwatch.Reset();
                    await Task.Delay(1);
                    stopwatch.Start();
                }
                adjustedStrokes.Add(strokeSnapshot);
            }
        }

        private void GenerateMetadata()
        {
            metadata = new SketchMetadata
            {
                ThumbnailCameraTransformInRoomSpace = default(TrTransform),
                GuideIndex = MetadataUtils.GetGuideIndex(m_GroupIdMapping),
                Palette = CustomColorPaletteStorage.m_Instance.GetPaletteForSaving(),
                //SceneTransformInRoomSpace = Coords.AsRoom[App.Instance.m_SceneTransform],
                //CanvasTransformInSceneSpace = App.Scene.AsScene[App.Instance.m_CanvasTransform],
                SourceId = SaveLoadScript.m_Instance.TransferredSourceIdFrom(SaveLoadScript.m_Instance.SceneFile),
                AssetId = SaveLoadScript.m_Instance.SceneFile.AssetId,
                SchemaVersion = SketchMetadata.kSchemaVersion,
                ApplicationName = App.kAppDisplayName,
                ApplicationVersion = App.Config.m_VersionNumber,
            };
        }

        static Guid GetForcePrecededBy(Guid original)
        {
            var brush = BrushCatalog.m_Instance.GetBrush(original);
            if (brush == null)
            {
                UnityEngine.Debug.LogErrorFormat("Unknown brush guid {0:N}", original);
                return original;
            }
            // The reason this is okay is that at load time we re-upgrade the brush;
            // see GetForceSupersededBy().
            while (brush.m_Supersedes != null && brush.m_LooksIdentical)
            {
                brush = brush.m_Supersedes;
            }
            return brush.m_Guid;
        }
    }
}