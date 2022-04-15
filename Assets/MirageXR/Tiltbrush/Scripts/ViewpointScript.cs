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

namespace TiltBrush {

  // Ordering:
  // - Viewpoint must come before InputManager (InputManager uses Viewpoint)
  public class ViewpointScript : MonoBehaviour {
    static public ViewpointScript m_Instance;

    /// On VR platforms, this may be distinct from the eye camera(s)
    /// On non-VR platforms, this is the same thing as the active camera.
    public static Transform Head {
      get {
        return m_Instance.GetHeadTransform();
      }
    }

    public static Ray Gaze {
      get {
        Transform head = Head;
        return new Ray(head.position, head.forward);
      }
    }

    private bool m_MirrorModeEnabled = false;

    // A solid color fullscreen overlay which can be faded in.
    private enum FullScreenFadeState {
      Default,
      FadingToColor,
      FadingToScene
    }
    // A ground plane grid which can be faded in.
    private enum GroundPlaneFadeState {
      Default,
      FadingIn,
      FadingOut
    }

    public void Init() {
      m_Instance = this;
    }

    /// Returns transform of the head. On non-VR platforms, this is the same
    /// thing as the active camera's transform.
    Transform GetHeadTransform() {
      return App.VrSdk.GetVrCamera().transform;
    }

    Transform GetEyeTransform() {
      return App.VrSdk.GetVrCamera().transform;
    }

    public void ToggleScreenMirroring() {
      m_MirrorModeEnabled = !m_MirrorModeEnabled;
      App.VrSdk.SetScreenMirroring(m_MirrorModeEnabled);
    }
  }
}  // namespace TiltBrush
