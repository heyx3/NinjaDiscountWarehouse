using UnityEngine;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Represents a group of ninja acting and moving as a group.
/// </summary>
[RequireComponent(typeof(PathFollower))]
public class NinjaCluster : MonoBehaviour
{
	public static List<NinjaCluster> AllClusters = new List<NinjaCluster>();


	[System.NonSerialized] public List<NinjaAIPlayerInput> NinjaAIs = new List<NinjaAIPlayerInput>();

	public float LerpTowardsNinjasStrength = 0.0f;
	public float AutoAimHitSphereScale = 1.0f;

	public float MaxNinjaDistance { get; private set; }

	public PathFollower MyPathing { get; private set; }

	public PathNode CurrentNode { get { return MyPathing.Current; } }
	public Vector3 Velocity { get { return MyPathing.Velocity; } }


	void Awake()
	{
		AllClusters.Add(this);

		MaxNinjaDistance = 0.0f;

		MyPathing = GetComponent<PathFollower>();
		MyPathing.OnPathEnd += (s, e) =>
			{
				MyPathing.enabled = false;
			};
	}

	void OnDestroy()
	{
		if (!AllClusters.Remove(this))
			throw new UnityException("This NinjaCluster was removed from the global list before its destruction!");
	}

	void OnDrawGizmos()
	{
		Gizmos.color = new Color(0.2f, 1.0f, 0.2f, 0.4f);
		Gizmos.DrawSphere(transform.position, MaxNinjaDistance);
	}

	void Update()
	{
		//If this cluster is empty, destroy it.
		if (NinjaAIs.Count == 0)
		{
			Destroy(gameObject);
			return;
		}

		//Lerp towards the ninja's collective center a bit.
		//Also calculate the radius of this cluster's auto-aim hit sphere.
		Vector3 avgPos = Vector3.zero;
		float maxDist = 0.0f;
		foreach (NinjaAIPlayerInput ninja in NinjaAIs)
		{
			avgPos += ninja.MyTransform.position;

			float tempDist = (MyPathing.MyTransform.position - ninja.MyTransform.position).sqrMagnitude;
			if (tempDist > maxDist)
				maxDist = tempDist;
		}
		avgPos /= NinjaAIs.Count;
		MyPathing.MyTransform.position = Vector3.Lerp(MyPathing.MyTransform.position, avgPos, LerpTowardsNinjasStrength);
		MaxNinjaDistance = AutoAimHitSphereScale * Mathf.Sqrt(maxDist);
	}
}