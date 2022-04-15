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

using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SceneStatePayload = TiltBrush.ExportUtils.SceneStatePayload;
using VertexLayout = TiltBrush.GeometryPool.VertexLayout;

using static TiltBrush.ExportUtils;

namespace TiltBrush {
class ExportCollector {

  // Constants that govern the BrushDescriptor and material properties for Reference Images
  // TODO: maybe reference the BrushDescriptor directly eg from Config.cs?
  static readonly Guid kPbrTransparentGuid = new Guid("19826f62-42ac-4a9e-8b77-4231fbd0cfbf");
  // TODO: Non-metallic materials should generally use 0.02-0.05
  const float kRefimageMetallicFactor = 0;

  // -------------------------------------------------------------------------------------------- //
  // Public API
  // -------------------------------------------------------------------------------------------- //

  public static SceneStatePayload GetExportPayload(
      AxisConvention axes,
      bool includeLocalMediaContent=false,
      string temporaryDirectory=null) {

    var payload = new SceneStatePayload(axes, temporaryDirectory);

    BuildGenerator(payload);
    BuildEnvironment(payload);
    BuildLights(payload);

    BuildBrushMeshes(payload);

    return payload;
  }

// Unused and untested
#if false
  /// Converts the existing payload to the new color space and vector basis.
  /// Note that scale is never converted and only gamma -> linear is currently supported.
  public static void ConvertPayload(ColorSpace newColorSpace,
                                    Vector3[] newVectorBasis,
                                    SceneStatePayload payload) {
    // We don't handle uninitialized color spaces or linear -> srgb conversions.
    // This is just not currently part of the export logic, so it's not implemented here.
    if (newColorSpace == ColorSpace.Uninitialized
        || payload.colorSpace == ColorSpace.Uninitialized
        || (payload.colorSpace == ColorSpace.Linear && newColorSpace == ColorSpace.Gamma)) {
      throw new NotImplementedException();
    }

    if (newColorSpace != payload.colorSpace) {
      ExportUtils.ConvertToLinearColorspace(payload.env);
      ExportUtils.ConvertToLinearColorspace(payload.lights);
      foreach (var group in payload.groups) {
        ExportUtils.ConvertToLinearColorspace(group.brushMeshes);
      }
      ExportUtils.ConvertToLinearColorspace(payload.modelMeshes);
    }

    if (newVectorBasis[0] != payload.vectorBasis[0]
        || newVectorBasis[1] != payload.vectorBasis[1]
        || newVectorBasis[2] != payload.vectorBasis[2]) {
      // Never apply a scale change when changing basis.
      Matrix4x4 basis = ExportUtils.GetBasisMatrix(payload);
      foreach (var group in payload.groups) {
        ExportUtils.ChangeBasis(group.brushMeshes, basis);
      }
      ExportUtils.ChangeBasis(payload.modelMeshes, basis);
      ExportUtils.ChangeBasis(payload.referenceImages, basis);
      ExportUtils.ChangeBasis(payload.referenceModels, basis);
    }

    payload.colorSpace = newColorSpace;
    payload.vectorBasis = newVectorBasis;
  }
#endif

  // -------------------------------------------------------------------------------------------- //
  // Generator
  // -------------------------------------------------------------------------------------------- //

  static void BuildGenerator(SceneStatePayload payload) {
    payload.generator = string.Format(payload.generator,
                                      App.Config.m_VersionNumber,
                                      App.Config.m_BuildStamp);
  }

  // -------------------------------------------------------------------------------------------- //
  // Environment
  // -------------------------------------------------------------------------------------------- //

  static void BuildEnvironment(SceneStatePayload payload, bool includeSkyCubemap = false) {
    var settings = SceneSettings.m_Instance;
    payload.env.guid = settings.GetDesiredPreset().m_Guid;
    payload.env.description = settings.GetDesiredPreset().m_Description;
    if (includeSkyCubemap) {
      // Most of the environment payload is very small data but if the skybox cubemap is included
      // here, it will export the full texture as part of the export process, which is usually not
      // what we want to do.
      payload.env.skyCubemap = settings.GetDesiredPreset().m_RenderSettings.m_SkyboxCubemap;
    }
    payload.env.useGradient = settings.InGradient;
    payload.env.skyColorA = settings.SkyColorA;
    payload.env.skyColorB = settings.SkyColorB;
    payload.env.skyGradientDir = settings.GradientOrientation * Vector3.up;
    payload.env.fogColor = settings.FogColor;
    payload.env.fogDensity = settings.FogDensity;
  }

  // -------------------------------------------------------------------------------------------- //
  // Lights
  // -------------------------------------------------------------------------------------------- //

  // Returns lights in "srgb-ish" color space.
  // Note that light values may be > 1, which is not really kosher in sRGB
  static void BuildLights(SceneStatePayload payload) {
    var lightPayload = payload.lights;
    lightPayload.ambientColor = RenderSettings.ambientLight;

    for (int i = 0; i < App.Scene.GetNumLights(); ++i) {
      var transform = App.Scene.GetLight(i).transform;
      Light unityLight = transform.GetComponent<Light>();
      Debug.Assert(unityLight != null);

      Color lightColor = unityLight.color * unityLight.intensity;
      lightColor.a = 1.0f;  // No use for alpha with light color.

      // because transform.name might not be unique
      int uniquifyingId = payload.idGenerator.GetIdFromInstanceId(transform);
      // Match the old gltf export logic for easier diffing
      var transformNameNoSpaces = transform.name.Replace(" ", "_");
      lightPayload.lights.Add(new ExportUtils.LightPayload {
          legacyUniqueName = $"{transformNameNoSpaces}_i{uniquifyingId}",
          name = transformNameNoSpaces,
          type = unityLight.type,
          lightColor = lightColor,
          xform = ExportUtils.ChangeBasis(transform, payload)
      });
    }
  }

  // -------------------------------------------------------------------------------------------- //
  // Xform only, no mesh data
  // -------------------------------------------------------------------------------------------- //

  static void BuildEmptyXforms(SceneStatePayload payload, IEnumerable<MediaWidget> widgets) {
    foreach (var mediaWidget in widgets) {
      payload.referenceThings.Add(new ExportUtils.XformPayload(
                                      payload.groupIdMapping.GetId(mediaWidget.Group)) {
          name = mediaWidget.GetExportName(),
          xform = ExportUtils.ChangeBasis(mediaWidget.transform, payload)
      });
    }
  }

  // -------------------------------------------------------------------------------------------- //
  // Brush Meshes
  // -------------------------------------------------------------------------------------------- //

  static void BuildBrushMeshes(SceneStatePayload payload) {
    BuildBrushMeshesFromExportCanvas(payload);
  }

  static void BuildBrushMeshesFromExportCanvas(SceneStatePayload payload) {
    foreach (var exportGroup in ExportUtils.ExportMainCanvas().SplitByGroup()) {
      payload.groups.Add(BuildGroupPayload(payload, exportGroup));
    }
  }

  static ExportUtils.GroupPayload BuildGroupPayload(SceneStatePayload payload,
                                                    ExportUtils.ExportGroup exportGroup) {
    var group = new ExportUtils.GroupPayload();
    group.id = exportGroup.m_group == SketchGroupTag.None ? 0
        : payload.groupIdMapping.GetId(exportGroup.m_group);

    // Flattens the two-level (brush, batch) iteration to a single list of meshes
    // with heterogeneous materials.
    foreach (ExportUtils.ExportBrush brush in exportGroup.SplitByBrush()) {
      var desc = brush.m_desc;
      foreach (var (batch, batchIndex) in brush.ToGeometryBatches().WithIndex()) {
        GeometryPool geometry = batch.pool;
        List<Stroke> strokes = batch.strokes;

        string legacyUniqueName = $"{desc.m_DurableName}_{desc.m_Guid}_{group.id}_i{batchIndex}";
        string friendlyGeometryName = $"brush_{desc.m_DurableName}_g{group.id}_b{batchIndex}";

        UnityEngine.Profiling.Profiler.BeginSample("ConvertToMetersAndChangeBasis");
        ExportUtils.ConvertUnitsAndChangeBasis(geometry, payload);
        UnityEngine.Profiling.Profiler.EndSample();

        if (payload.reverseWinding) {
          // Note: this triangle flip intentionally un-does the Unity FBX import flip.
          ExportUtils.ReverseTriangleWinding(geometry, 1, 2);
        }

        if (App.PlatformConfig.EnableExportMemoryOptimization &&
            payload.temporaryDirectory != null) {
          string filename = Path.Combine(
              payload.temporaryDirectory,
              legacyUniqueName + ".Gpoo");
          geometry.MakeGeometryNotResident(filename);
        }
        group.brushMeshes.Add(new ExportUtils.BrushMeshPayload(
                                  payload.groupIdMapping.GetId(exportGroup.m_group)) {
            legacyUniqueName = legacyUniqueName,
            // This is the only instance of the mesh, so the node doesn't need an extra instance id
            nodeName = friendlyGeometryName,
            xform = Matrix4x4.identity,
            geometry = geometry,
            geometryName = friendlyGeometryName,
            exportableMaterial = brush.m_desc,
            strokes = strokes,
        });
      }
    }
    return group;
  }

  // -------------------------------------------------------------------------------------------- //
  // Model Meshes
  // -------------------------------------------------------------------------------------------- //

#if UNITY_EDITOR && GAMEOBJ_EXPORT_TO_GLTF
  // Get a material for the given mesh. If a _MainTex texture exists, reference to it from
  // UnityEditor.AssetBase.
  static IExportableMaterial GetMaterialForMeshData(MeshData data) {
    // Create parameter dictionaries.
    Dictionary<string, string> textureUris = new Dictionary<string, string>();
    Dictionary<string, Vector2> textureSizes = new Dictionary<string, Vector2>();
    Dictionary<string, Vector3> vectorParams = new Dictionary<string,Vector3>();
    Dictionary<string, float> floatParams = new Dictionary<string, float>();
    Dictionary<string, Color> colorParams = new Dictionary<string, Color>();

    // Get material for the mesh.
    Material mat = data.renderer.sharedMaterial;
    Texture2D mainTex = (Texture2D) mat.GetTexture("_MainTex");
    string uriBase = "";
    if (mainTex) {
      string texturePath = ExportUtils.GetTexturePath(mainTex);
      textureUris.Add("BaseColorTex", ExportUtils.kTempPrefix + texturePath);
      Vector2 uvScale = mat.GetTextureScale("_MainTex");
      Vector2 uvOffset = mat.GetTextureOffset("_MainTex");
      colorParams.Add("UvAdjust", new Color(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y));
    } else {
      textureUris.Add("BaseColorTex", ExportUtils.kBuiltInPrefix + "whiteTextureMap.png");
      colorParams.Add("UvAdjust", new Color(1.0f, 1.0f, 0.0f, 0.0f));
    }

    // Check for secondary light map texture.
    bool hasLightMap = false;
    Texture2D lightMapTex = null;
    if (mat.HasProperty("_DetailAlbedoMap")) {
      lightMapTex = (Texture2D)mat.GetTexture("_DetailAlbedoMap");
    }
    if (lightMapTex == null && mat.HasProperty("_LightMap")) {
      lightMapTex = (Texture2D)mat.GetTexture("_LightMap");
    }
    if (lightMapTex) {
      hasLightMap = true;
      string texturePath = ExportUtils.GetTexturePath(lightMapTex);
      textureUris.Add("LightMapTex", ExportUtils.kTempPrefix + texturePath);
    } else {
      textureUris.Add("LightMapTex", ExportUtils.kBuiltInPrefix + "whiteTextureMap.png");
    }

    // As much as possible, this should mimic the way that GltfMaterialConverter.ConvertMaterial()
    // converts materials from gltf2 to UnityMaterial methods.
    if (mat.HasProperty("_Color")) {
      colorParams.Add("BaseColorFactor", mat.color);
    } else {
      colorParams.Add("BaseColorFactor", Color.black);
    }

    // Create the exportable material.
    BrushDescriptor exportDescriptor =
        hasLightMap ?
        Resources.Load<BrushDescriptor>("Brushes/Poly/Environments/EnvironmentDiffuseLightMap") :
        Resources.Load<BrushDescriptor>("Brushes/Poly/Environments/EnvironmentDiffuse");

    return new DynamicExportableMaterial(
        // Things that come from the template
        blendMode: exportDescriptor.BlendMode,
        vertexLayout: exportDescriptor.VertexLayout,
        vertShaderUri: exportDescriptor.VertShaderUri,
        fragShaderUri: exportDescriptor.FragShaderUri,
        enableCull: exportDescriptor.EnableCull,
        // Things that vary
        durableName: data.renderer.gameObject.GetInstanceID().ToString(),
        emissiveFactor: 0.0f,
        // TODO: Should the export texture be the main pbr texture?
        hasExportTexture: false,
        exportTextureFilename: null,
        uriBase: uriBase,
        textureUris: textureUris,
        textureSizes: textureSizes,
        floatParams: floatParams,
        vectorParams: vectorParams,
        colorParams: colorParams);
  }

  public static SceneStatePayload GetExportPayloadForGameObject(GameObject gameObject,
                                                            AxisConvention axes,
                                                            Environment env) {
    var payload = new SceneStatePayload(axes);
    payload.env.skyCubemap = env == null ? null : env.m_RenderSettings.m_SkyboxCubemap;
    payload.lights = new ExportUtils.LightsPayload();
    // unused test code
#if TEST_ENVIRONMENTS_WITH_SCENE_LIGHTS
    {
      var lights = payload.lights;
      lights.ambientColor = RenderSettings.ambientLight;

#if true
      // Scene lights from Standard enviroment
      lights.elements.Add(new ExportUtils.Light {
          name = "SceneLight0",
          id = 0,
          type = LightType.Directional,
          lightColor = new Color32(198, 208, 253),
          xform = ExportUtils.ChangeBasisNonUniformScale(
              payload, Matrix4x4.TRS(
                  new Vector3(0f, 0.21875f, -.545875f),
                  Quaternion.Euler(60, 0, 26),
                  Vector3.one))
        });

      lights.elements.Add(new ExportUtils.Light {
        name = "SceneLight1",
        id = 1,
        type = LightType.Directional,
        lightColor = new Color32(109, 107, 88),
        xform = ExportUtils.ChangeBasisNonUniformScale(
            payload,
            Matrix4x4.TRS(
                new Vector3(0f, 0.21875f, -.545875f),
                Quaternion.Euler(140, 0, 40),
                Vector3.one)),
        });
#else
      // Scene lights from Dress Form environment
      lights.elements.Add(new ExportUtils.Light {
          name = "SceneLight0",
          id = 0,
          type = LightType.Directional,
          lightColor = new Color32(255, 234, 198),
          xform = ExportUtils.ChangeBasisNonUniformScale(
              payload, Matrix4x4.TRS(
                  new Vector3(0f, 0.7816665f, -.220875f),
                  Quaternion.Euler(50, 42, 26),
                  Vector3.one))
        });

      lights.elements.Add(new ExportUtils.Light {
        name = "SceneLight1",
        id = 1,
        type = LightType.Directional,
        lightColor = new Color32(91, 90, 74),
        xform = ExportUtils.ChangeBasisNonUniformScale(
            payload,
            Matrix4x4.TRS(
                new Vector3(0f, .636532f, -.6020539f),
                Quaternion.Euler(140, 47, 40),
                Vector3.one)),
        });
#endif
    }
#endif

    // The determinant can be used to detect if the basis-change has a mirroring.
    // This matters because a mirroring turns the triangles inside-out, requiring
    // us to flip their winding to preserve the surface orientation.
    bool reverseTriangleWinding = (ExportUtils.GetBasisMatrix(payload).determinant < 0);

    BuildModelMeshesFromGameObject(gameObject, payload, reverseTriangleWinding);
    return payload;
  }

  static void BuildModelMeshesFromGameObject(
      GameObject gameObject, SceneStatePayload payload, bool reverseWinding) {
    foreach (var modelMesh in GetModelMeshesFromGameObject(gameObject, payload, reverseWinding)) {
      payload.modelMeshes.Add(BuildModelMeshPayload(modelMesh));
    }
  }

  // Returns meshes in meters, in the proper basis, with proper winding
  static IEnumerable<ModelMesh> GetModelMeshesFromGameObject(GameObject obj,
                                                             SceneStatePayload payload,
                                                             bool reverseWinding) {
    int i = -1;
    string exportName = obj.name;
    foreach (MeshData data in VisitMeshes(obj)) {
      i++;
      var mesh = data.mesh;
      for (int sm = 0; sm < mesh.subMeshCount; sm++) {
        var exportableMaterial = GetMaterialForMeshData(data);

        // TODO: The geometry pools could be aggregated, for faster downstream rendering.
        var geo = new GeometryPool();
        geo.Layout = exportableMaterial.VertexLayout;
        geo.Append(mesh, geo.Layout);

        // Important: This transform should only be applied once, since the pools are shared, and
        // cannot include any mesh-local transformations.
        ExportUtils.ConvertToMetersAndChangeBasis(geo, ExportUtils.GetBasisMatrix(payload));

        if (reverseWinding) {
          // Note: this triangle flip intentionally un-does the Unity FBX import flip.
          ExportUtils.ReverseTriangleWinding(geo, 1, 2);
        }

        var objectMesh = new ModelMesh {
          model = null,  // The model is only used for uniquefying the texture names.
          name = exportName + "_" + i + "_" + sm,
          id = data.renderer.gameObject.GetInstanceID(),
          pool = geo,
          xform = ExportUtils.ChangeBasisNonUniformScale(payload, data.renderer.localToWorldMatrix),
          exportableMaterial = exportableMaterial,
          meshIndex = i
        };

        yield return objectMesh;
      }
    }
  }
#endif
}
} // namespace TiltBrush
