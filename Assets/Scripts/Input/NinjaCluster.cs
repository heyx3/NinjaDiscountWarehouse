using UnityEngine;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Represents a group of ninja acting and moving as a group.
/// </summary>
[RequireComponent(typeof(PathFollower))]
public class NinjaCluster : MonoBehaviour
{
	[System.NonSerialized] public List<NinjaAIPlayerInput> NinjaAIs = new List<NinjaAIPlayerInput>();

	public float LerpTowardsNinjasStrength = 0.0f;

	public PathFollower MyPathing { get; private set; }

	public PathNode CurrentNode { get { return MyPathing.Current; } }
	public Vector3 Velocity { get { return MyPathing.Velocity; } }


	void Awake()
	{
		MyPathing = GetComponent<PathFollower>();

		MyPathing.OnPathEnd += (s, e) =>
			{
				MyPathing.enabled = false;
			};
	}

	void Update()
	{
		if (NinjaAIs.Count == 0)
		{
			Destroy(gameObject);
			return;
		}

		if (NinjaAIs.Count > 0)
		{
			Vector3 avgPos = Vector3.zero;
			foreach (NinjaAIPlayerInput ninja in NinjaAIs)
				avgPos += ninja.MyTransform.position;
			avgPos /= NinjaAIs.Count;

			MyPathing.MyTransform.position = Vector3.Lerp(MyPathing.MyTransform.position, avgPos, LerpTowardsNinjasStrength);
		}
	}
}