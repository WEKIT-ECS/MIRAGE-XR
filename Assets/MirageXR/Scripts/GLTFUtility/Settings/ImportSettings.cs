﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Siccity.GLTFUtility {
	[Serializable]
	public class ImportSettings {

		public bool materials = true;
		[FormerlySerializedAs("shaders")]
		public ShaderSettings shaderOverrides = new ShaderSettings();
		public AnimationSettings animationSettings = new AnimationSettings();
		public bool useLegacyClips;
		public bool generateLightmapUVs;

		[Tooltip("Interpolation mode applied to all keyframe tangents. Use Import From File when mixing modes within an animation.")]
		public InterpolationMode interpolationMode = InterpolationMode.ImportFromFile;
	}
}