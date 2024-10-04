using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
	public class ConstantRotation : MonoBehaviour
	{
		[SerializeField] private Vector3 rotationAnglePerSecond = new Vector3(0, 0, 10);

		// Update is called once per frame
		private void Update()
		{
			transform.Rotate(rotationAnglePerSecond * Time.deltaTime);
		}
	}
}
