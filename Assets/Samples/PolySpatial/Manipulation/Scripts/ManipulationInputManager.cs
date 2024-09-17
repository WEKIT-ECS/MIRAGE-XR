using System.Collections.Generic;
using Unity.PolySpatial.InputDevices;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace PolySpatial.Samples
{
    /// <summary>
    /// Current you can only select one object at a time and only supports a primary [0] touch
    /// </summary>
    public class ManipulationInputManager : MonoBehaviour
    {
        struct Selection
        {
            /// <summary>
            /// The piece that is selected
            /// </summary>
            public PieceSelectionBehavior Piece;

            /// <summary>
            /// The offset between the interaction position and the position selected object for an identity device rotation.
            /// This is computed at the beginning of the interaction and combined with the current device rotation and interaction position to translate the object
            /// as the user moves their hand.
            /// </summary>
            public Vector3 PositionOffset;

            /// <summary>
            /// The difference in rotations between the initial device rotation and the selected object.
            /// This is computed at the beginning of the interaction and combined with the current device rotation to rotate the object as the user moves their hand.
            /// </summary>
            public Quaternion RotationOffset;
        }

        internal const int k_Deselected = -1;
        readonly Dictionary<int, Selection> m_CurrentSelections = new();

        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        void Update()
        {
            foreach (var touch in Touch.activeTouches)
            {
                var spatialPointerState = EnhancedSpatialPointerSupport.GetPointerState(touch);
                var interactionId = spatialPointerState.interactionId;

                // Ignore poke input--piece will get stuck to the user's finger
                if (spatialPointerState.Kind == SpatialPointerKind.Touch)
                    continue;

                var pieceObject = spatialPointerState.targetObject;
                if (pieceObject != null)
                {
                    // Swap materials and record initial relative position & rotation from hand to object for later use when the piece is selected
                    if (pieceObject.TryGetComponent(out PieceSelectionBehavior piece) && piece.selectingPointer == -1)
                    {
                        var pieceTransform = piece.transform;
                        var interactionPosition = spatialPointerState.interactionPosition;
                        var inverseDeviceRotation = Quaternion.Inverse(spatialPointerState.inputDeviceRotation);
                        var rotationOffset = inverseDeviceRotation * pieceTransform.rotation;
                        var positionOffset = inverseDeviceRotation * (pieceTransform.position - interactionPosition);
                        piece.SetSelected(interactionId);

                        // Because events can come in faster than they are consumed, it is possible for target id to change without a prior end/cancel event
                        if (m_CurrentSelections.TryGetValue(interactionId, out var selection))
                            selection.Piece.SetSelected(k_Deselected);

                        m_CurrentSelections[interactionId] = new Selection
                        {
                            Piece = piece,
                            RotationOffset = rotationOffset,
                            PositionOffset = positionOffset
                        };
                    }
                }

                switch (spatialPointerState.phase)
                {
                    case SpatialPointerPhase.Moved:
                        if (m_CurrentSelections.TryGetValue(interactionId, out var selection))
                        {
                            // Position the piece at the interaction position, maintaining the same relative transform from interaction position to selection pivot
                            var deviceRotation = spatialPointerState.inputDeviceRotation;
                            var rotation = deviceRotation * selection.RotationOffset;
                            var position = spatialPointerState.interactionPosition + deviceRotation * selection.PositionOffset;
                            selection.Piece.transform.SetPositionAndRotation(position, rotation);
                        }

                        break;
                    case SpatialPointerPhase.None:
                    case SpatialPointerPhase.Ended:
                    case SpatialPointerPhase.Cancelled:
                        DeselectPiece(interactionId);
                        break;
                }
            }
        }

        void DeselectPiece(int interactionId)
        {
            if (m_CurrentSelections.TryGetValue(interactionId, out var selection))
            {
                // Swap materials back when the piece is deselected
                selection.Piece.SetSelected(k_Deselected);
                m_CurrentSelections.Remove(interactionId);
            }
        }
    }
}
