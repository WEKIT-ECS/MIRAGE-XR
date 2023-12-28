using UnityEngine;

namespace Obi
{
	[RequireComponent(typeof(ObiRopeCursor))]
	public class ObiRopeReel : MonoBehaviour
	{
		private ObiRopeCursor cursor;
		private ObiRope rope;

		[Header("Roll out/in thresholds")]
		public float outThreshold = 0.8f;
		public float inThreshold = 0.4f;

		[Header("Roll out/in speeds")]
		public float outSpeed = 0.05f;
		public float inSpeed = 0.15f;

		public void Awake()
		{
			cursor = GetComponent<ObiRopeCursor>();
			rope = GetComponent<ObiRope>();
		}

		public void OnValidate()
		{
			// Make sure the range thresholds don't cross:
			outThreshold = Mathf.Max(inThreshold, outThreshold);
		}

		// Update is called once per frame
		void Update()
		{
			// get current and rest lengths:
			float length = rope.CalculateLength();
			float restLength = rope.restLength;

			// calculate difference between current length and rest length:
			float diff = Mathf.Max(0, length - restLength);

			// if the rope has been stretched beyond the reel out threshold, increase its rest length:
			if (diff > outThreshold)
				restLength += diff * outSpeed;

			// if the rope is not stretched past the reel in threshold, decrease its rest length:
			if (diff < inThreshold)
				restLength -= diff * inSpeed;

			// set the new rest length:
			cursor.ChangeLength(restLength);
		}
	}
}
