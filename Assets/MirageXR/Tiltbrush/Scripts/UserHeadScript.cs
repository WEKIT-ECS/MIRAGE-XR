﻿// Copyright 2020 The Tilt Brush Authors
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

public class UserHeadScript : MonoBehaviour {
  [SerializeField] private Renderer m_HeadMesh;
  [SerializeField] private DropCamWidget m_DropCam;
  [SerializeField] private GameObject m_MultiCam;

  void Awake(){
    m_HeadMesh.enabled = false;
  }

  // void Update() {
  //   m_HeadMesh.enabled = m_MultiCam.activeSelf || m_DropCam.ShouldHmdBeVisible();
  // }
}

} // namespace TiltBrush