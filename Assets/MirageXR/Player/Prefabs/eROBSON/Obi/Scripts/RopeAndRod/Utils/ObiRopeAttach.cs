using UnityEngine;

namespace Obi
{
    public class ObiRopeAttach : MonoBehaviour
    {
        public ObiPathSmoother smoother;
        [Range(0,1)]
        public float m;

		public void LateUpdate()
		{
            if (smoother != null)
            {
                ObiPathFrame section = smoother.GetSectionAt(m);
                transform.position = smoother.transform.TransformPoint(section.position);
                transform.rotation = smoother.transform.rotation * (Quaternion.LookRotation(section.tangent, section.binormal));
            }
		}

	}
}