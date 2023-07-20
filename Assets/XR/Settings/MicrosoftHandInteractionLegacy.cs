// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Microsoft.MixedReality.OpenXR
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of hand interaction in OpenXR. It enables <see href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_MSFT_hand_interaction">XR_MSFT_hand_interaction</see> in the underyling runtime.
    /// This creates a new <see cref="InputDevice"/> with the <see cref="InputDeviceCharacteristics.HandTracking"/> characteristic.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Microsoft Hand Interaction Profile (Legacy)",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA },
        Company = "Microsoft",
        Desc = "Allows for mapping input to the hand interaction profile with legacy compatibility.",
        DocumentationLink = "https://aka.ms/openxr-unity",
        Version = "0.0.1",
        OpenxrExtensionStrings = extensionString,
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class MicrosoftHandInteractionLegacy : UnityEngine.XR.OpenXR.Features.Interactions.MicrosoftHandInteraction
    {
        private new const string featureId = "com.microsoft.openxr.feature.interaction.handtrackinglegacy";
        private const string kDeviceLocalizedName = "HoloLens Hand OpenXR Legacy";

        /// <summary>
        /// Registers the <see cref="Hand"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
            InputSystem.RegisterLayout(typeof(HoloLensHand),
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="Hand"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
            InputSystem.RemoveLayout(nameof(HoloLensHand));
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "microsofthandinteractionlegacy",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "Microsoft",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left,
                        userPath = UserPaths.leftHand
                    },
                    new DeviceConfig()
                    {
                        characteristics = InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right,
                        userPath = UserPaths.rightHand
                    }
                },
                actions = new List<ActionConfig>()
                {
                    // Select
                    new ActionConfig()
                    {
                        name = "select",
                        localizedName = "Select",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "Trigger"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = select,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Select Pressed
                    new ActionConfig()
                    {
                        name = "selectPressed",
                        localizedName = "Select Pressed",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "TriggerButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = select,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Squeeze
                    new ActionConfig()
                    {
                        name = "squeeze",
                        localizedName = "Squeeze",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "Grip"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = squeeze,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Squeeze Pressed
                    new ActionConfig()
                    {
                        name = "squeezePressed",
                        localizedName = "Squeeze Pressed",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "GripButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = squeeze,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Device Pose
                    new ActionConfig()
                    {
                        name = "devicePose",
                        localizedName = "Device Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Device"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = grip,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Pointer Pose
                    new ActionConfig()
                    {
                        name = "pointer",
                        localizedName = "Pointer Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Pointer"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = aim,
                                interactionProfileName = profile,
                            }
                        }
                    }
                }
            };

            AddActionMap(actionMap);
        }
    }
}
