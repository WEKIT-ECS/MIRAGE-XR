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
using System.Collections.Generic;
using System.Linq;

namespace TiltBrush {
public static class MetadataUtils {
  public struct WidgetMetadata {
    public TrTransform xf;
    public bool pinned;
    public bool tinted;
    public uint groupId;
  }

  /// Sanitizes potentially-invalid data coming from the .tilt file.
  /// Returns an array of valid TrTransforms; may return the input array.
  public static TrTransform[] Sanitize(TrTransform[] data) {
    if (data != null) {
      for (int i = 0; i < data.Length; ++i) {
        if (!data[i].IsFinite()) {
          Debug.LogWarningFormat("Found non-finite TrTransform: {0}", data[i]);
          return data.Where(xf => xf.IsFinite()).ToArray();
        }
      }
    }

    return data;
  }

  private static float ByTranslation(WidgetMetadata meta) {
    return Vector3.Dot(meta.xf.translation, new Vector3(256 * 256, 256, 1));
  }

  private static float ByTranslation(TrTransform xf) {
    return Vector3.Dot(xf.translation, new Vector3(256 * 256, 256, 1));
  }

  private static string ByModelLocation(TiltModels75 models) {
    if (models.AssetId != null) {
      return "AssetId:" + models.AssetId;
    } else if (models.FilePath != null) {
      return "FilePath:" + models.FilePath;
    }
    Debug.LogWarning("Attempted to save model without asset id or filepath");
    return "";
  }

  public static Guides[] GetGuideIndex(GroupIdMapping groupIdMapping) {
    var stencils =
      WidgetManager.m_Instance.StencilWidgets.Where(s => s.gameObject.activeSelf).ToList();
    if (stencils.Count == 0) {
      return null;
    }
    Dictionary<StencilType, List<Guides.State>> guides =
      new Dictionary<StencilType, List<Guides.State>>();
    foreach (var stencil in stencils) {
      if (!guides.ContainsKey(stencil.Type)) {
        guides[stencil.Type] = new List<Guides.State>();
      }
      guides[stencil.Type].Add(stencil.GetSaveState(groupIdMapping));
    }
    List<Guides> guideIndex = new List<Guides>();
    foreach (var elem in guides) {
      guideIndex.Add(new Guides {
        Type = elem.Key,
        States = elem.Value.OrderBy(s => ByTranslation(s.Transform)).ToArray()
      });
    }
    return guideIndex.OrderBy(g => g.Type).ToArray();
  }

  // Append value to array, creating array if necessary
  static T[] SafeAppend<T>(T[] array, T value) {
    T[] asArray = { value };
    return (array == null) ? asArray : array.Concat(asArray).ToArray();
  }
}
} // namespace TiltBrush
