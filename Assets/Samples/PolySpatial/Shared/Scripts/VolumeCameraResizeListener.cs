using Unity.PolySpatial;
using UnityEngine;

namespace PolySpatial.Samples
{
    // On VisionOS, the volume camera output size is just a request,  and the OS can provide a size smaller
    // than the requested size.  The provided window can also be in a different aspect ratio (for example,
    // 1x1x1 may be requested, but 1x0.5x1 may be provided).
    //
    // The contents of a volume camera are mapped directly to the space defined by the output window.
    // This means that if the volume camera is 1x1x1 but the output is 1x0.5x1, the output will appear
    // squished.
    //
    // This script listens to events on the volume camera and configures it to preserve the desired
    // appearance. There is no way to clip content on visionOS except at the edges of the output window,
    // so applications will need to decide how to handle the "extra" area that they might end up with.
    //
    // This script modifies the Volume Camera Dimensions, but it could achieve the same effect by
    // modifying the transform scale. One or the other may be preferable, depending on how or if the
    // Volume Camera is used for effects (for example, scaling the volume camera to zoom in/out of
    // content).
    public class VolumeCameraResizeListener : MonoBehaviour
    {
        public enum ResizeMode
        {
            [Tooltip("Resize the volume camera, scaling the content so it fits fully in the output window")]
            ScaleToFit,

            [Tooltip("Resize the volume camera to match the output window")]
            MatchWindowSize,
        }

        public ResizeMode Mode;

        Vector3 m_OriginalVolumeCameraDimensions;

        void OnEnable()
        {
            var volumeCamera = GetComponent<VolumeCamera>();

            // Save the original volume camera dimensions so we know what we're starting with.
            // Note: in case you want to change these dimensions dynamically, you'll need to
            // adapt this script to take that into account
            m_OriginalVolumeCameraDimensions = volumeCamera.Dimensions;

            // The first time a window is opened for a volume camera, a WindowOpened event will be triggered.
            // If the volume window changes size after that, a WindowResized event will be triggered.
            // We want to handle both in the same way, so we can just add the same listener to both events.
            volumeCamera.OnWindowEvent.AddListener(VolumeWindowResized);
        }

        void OnDisable()
        {
            var volumeCamera = GetComponent<VolumeCamera>();
            volumeCamera.OnWindowEvent.RemoveListener(VolumeWindowResized);
        }

        // We are being informed of the actual dimensions of the opened window (windowDimensions).
        // In this function, the only thing that we can manipulate is the volume camera dimensions/scale/position itself,
        // or to change to an entirely different output configuration. We cannot affect the output window dimensions
        // in any way.
        //
        // The windowDimensions are the dimensions of the output window, in the platform's units. The contentDimensions
        // are the dimensions that your Volume Camera's dimensions are mapped to, in Unity's coordinate units.
        // (On visionOS, these will typically be the same, but they may not be on other platforms.)

        void VolumeWindowResized(VolumeCamera.WindowState windowState)
        {
            if (windowState.Mode == VolumeCamera.PolySpatialVolumeCameraMode.Unbounded)
                return;

            var volumeCamera = GetComponent<VolumeCamera>();

            // These are the desired output dimensions that we asked for. (volumeCamera.OutputDimensions will be the actual
            // dimensions, and will equal contentDimensions)
            var desiredOutputDimensions = volumeCamera.WindowConfiguration.Dimensions;

            // If they match, there's nothing to do; we got what we asked for.
            if (windowState.ContentDimensions == desiredOutputDimensions)
                return;

            // This is the original scale factor between the window dimensions and the volume camera dimensions, in order
            // to preserve whatever the original mapping is. Typically this will be a uniform scale.
            var originalScaleFactor = new Vector3(m_OriginalVolumeCameraDimensions.x / desiredOutputDimensions.x,
                    m_OriginalVolumeCameraDimensions.y / desiredOutputDimensions.y,
                    m_OriginalVolumeCameraDimensions.z / desiredOutputDimensions.z);

            // First, compute dimensions such that content remains the same size and shape as it would have if we had received
            // our requested dimensions. If we received smaller dimensions, this would cause the content to be cropped. If bigger,
            // surrounding content will be visible.
            var newDimensions = windowState.ContentDimensions;
            var originalDimensions = windowState.ContentDimensions;
            newDimensions.Scale(originalScaleFactor);

            if (Mode == ResizeMode.ScaleToFit)
            {
                // If instead we want to scale the content to fit, further scale these
                // dimensions based on the smallest output dimension. (This may not be
                // correct depending on your content, but it's a reasonable default.)
                var smallestSize = originalDimensions.x;
                float scale = desiredOutputDimensions.x / originalDimensions.x;

                if (originalDimensions.y < smallestSize)
                {
                    smallestSize = originalDimensions.y;
                    scale = desiredOutputDimensions.y / originalDimensions.y;
                }

                if (originalDimensions.z < smallestSize)
                {
                    scale = desiredOutputDimensions.z / originalDimensions.z;
                }

                newDimensions *= scale;
            }

            volumeCamera.Dimensions = newDimensions;

            Debug.Log($"Volume camera dimensions set to {newDimensions} (got window of size {originalDimensions}, expected {desiredOutputDimensions})");
        }
    }
}
