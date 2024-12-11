using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class RelativePositionPlacement : MonoBehaviour
	{
		[field: SerializeField] public Transform Target { get; set; }
		[field: SerializeField] public Vector3 Offset { get; set; }

		void Update()
		{
			if (Target != null)
			{
				transform.position = Target.position + Offset;
			}
		}
	}
}
