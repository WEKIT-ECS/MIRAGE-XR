using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MirageXR
{
	public class FloorManagerWithFallback : MonoBehaviour
	{
		private static FloorManagerWrapper floorManager => RootObject.Instance.floorManager;

		[Tooltip("Layer mask that filters for objects belonging to the floor, e.g. the spatial mapping")]
		[SerializeField] private LayerMask _floorLayer;

		private float _heightOffset = 1.75f;

		private List<float> _raycastResults = new List<float>();

		public float GetFloorHeight(Vector3 position)
		{
			if (floorManager.isFloorDetected)
			{
				return floorManager.floorLevel;
			}
			else
			{
				return GetFloorHeight(position, 0.75f, 6);
			}
		}

		// Calculates the floor height
		// Casts a given number of rays in a circular shape around the user to find the floor heights
		// It finds the lowest found point, spans a 20cm zone above this and then takes the highest raycast hit within this zone
		// This should prevent the aura from jumping onto furniture, e.g. if the user sits on a chair
		// It should also lower the probability that the aura is placed underneath the floor, e.g. in environments with steps
		private float GetFloorHeight(Vector3 centerPos, float radius, int numberOfRaycasts)
		{
			float minimumValue = centerPos.y;

			_raycastResults.Clear();

			// create rays circular around a center position with a given radius

			for (int i = 0; i < numberOfRaycasts; i++)
			{
				float radians = 2 * Mathf.PI / numberOfRaycasts * i;

				float vertical = Mathf.Sin(radians);
				float horizontal = Mathf.Cos(radians);

				Vector3 spawnDir = new Vector3(horizontal, 0, vertical);

				Vector3 raycastOrigin = centerPos + spawnDir * radius;

				Ray ray = new Ray(raycastOrigin, Vector3.down);
				Debug.DrawLine(raycastOrigin, raycastOrigin + 2f * ray.direction, Color.white);
				if (Physics.Raycast(ray, out RaycastHit hit, 2f, _floorLayer))
				{
					_raycastResults.Add(hit.point.y);
					minimumValue = Mathf.Min(minimumValue, hit.point.y);
				}
			}

			float floorHeight;

			// if we did not find the floor with any raycast, we just take a default height were we assume the floor to be
			if (_raycastResults.Count == 0)
			{
				floorHeight = centerPos.y - _heightOffset;
			}
			// otherwise: take the lowest point, span a 20cm zone above it and take the highest floor hit in this zone
			else
			{
				floorHeight = minimumValue;
				float threshold = minimumValue + 0.2f;
				for (int i = 0; i < _raycastResults.Count; i++)
				{
					if (_raycastResults[i] <= threshold)
					{
						floorHeight = Mathf.Max(floorHeight, _raycastResults[i]);
					}
				}
			}

			return floorHeight;
		}
	}
}