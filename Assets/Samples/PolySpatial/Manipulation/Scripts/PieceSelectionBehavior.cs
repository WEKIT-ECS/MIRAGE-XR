using UnityEngine;

namespace PolySpatial.Samples
{
    [RequireComponent(typeof(Rigidbody))]
    public class PieceSelectionBehavior : MonoBehaviour
    {
        [SerializeField]
        MeshRenderer m_MeshRenderer;

        [SerializeField]
        Material m_DefaultMat;

        [SerializeField]
        Material m_SelectedMat;

        Rigidbody m_RigidBody;

        public int selectingPointer { get; private set; } = ManipulationInputManager.k_Deselected;

        void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
        }

        public void SetSelected(int pointer)
        {
            var isSelected = pointer != ManipulationInputManager.k_Deselected;
            selectingPointer = pointer;
            m_MeshRenderer.material = isSelected ? m_SelectedMat : m_DefaultMat;
            m_RigidBody.isKinematic = isSelected;
        }
    }
}
