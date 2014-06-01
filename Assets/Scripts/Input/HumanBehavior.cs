using System.Collections.Generic;
using UnityEngine;
using System.Linq;


/// <summary>
/// Handles behavior for the player playing this game.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class HumanBehavior : MonoBehaviour
{
	public static HumanBehavior Instance { get; private set; }
	private static Vector3 HorizontalMask(Vector3 inV) { return new Vector3(inV.x, 0.0f, inV.z); }


	public GameObject ThrowParticlesPrefab = null;

	public float NodYVelocity = -3.5f;
	public float JerkHorizontalSpeed = 8.0f;

	public float KinematicsTrackerDuration = 0.05f;
	public float DisableGesturesDuration = 0.25f;
	public int MaxLevitations = 8;

	private float timeSinceLastGesture = 0.0f;
	private AudioSource src;

	public KinematicsTracker HeadTracker = null,
							 FaceTracker = null;
	public Transform CameraTracker = null;
	

	public Transform MyTransform { get; private set; }
	public List<Levitatable> Levitators { get; private set; }
	public bool IsLevitating { get { return Levitators.Count > 0; } }


	private List<Levitatable> FindLevitators(Vector3 dir)
	{
		Vector3 horizontalDir = new Vector3(dir.x, 0.0f, dir.z);
		List<Levitatable> ret = new List<Levitatable>();

		//First cast a ray to see what wall is hit.
		RaycastHit wallCast = new RaycastHit();
		int layer = (1 << LayerMask.NameToLayer("Blockers"));
		if (!Physics.Raycast(new Ray(CameraTracker.position, horizontalDir), out wallCast, 99999.0f, layer))
		{
			Debug.LogError("Raycast for blockers didn't hit anything! The player is looking at a hole into the endless abyss.");
			return ret;
		}

		//Now cast a ray as far as the wall (plus a little extra) for throwable objects.
		float dist = wallCast.distance + 5.0f;
		layer = (1 << LayerMask.NameToLayer("Throwable"));
		RaycastHit[] hits = Physics.RaycastAll(CameraTracker.position, dir, dist, layer);
		//GameDirector.Instance.CreateLine(CameraTracker.position, dir);

		//Add each hit object to the list.
		for (int i = 0; i < hits.Length && ret.Count < MaxLevitations; ++i)
		{
			Levitatable levComponent = hits[i].transform.GetComponent<Levitatable>();
			if (levComponent == null)
				throw new UnityException("Object uses 'Throwable' layer but doesn't have a Levitatable component! Name: " + hits[i].transform.gameObject.name);
			ret.Add(levComponent);
		}


		return ret;
	}
	private Vector3 FindTargetLookPos()
	{
		Vector3 aimDir = FaceTracker.VelocityLogs[FaceTracker.GetLogIndex(KinematicsTrackerDuration)].normalized;

		//For each ninja cluster, cast some rays to see which ones are visible and pick the visible one closest to the direction the player aimed at.

		RaycastHit hit;
		int raycastLayers = (1 << LayerMask.NameToLayer("Blockers"));
		float bestDot = 0.0f;
		Vector3 bestPos = MyTransform.position + (aimDir * 9999.0f);

		foreach (NinjaCluster cluster in NinjaCluster.AllClusters)
		{
			//Get vectors towards the cluster and perpendicular to the cluster.
			Vector3 towardsCluster = (cluster.MyPathing.MyTransform.position - CameraTracker.position).normalized;
			Vector3 toSide = Vector3.Cross(towardsCluster, new Vector3(0.0f, 1.0f, 0.0f));
			
			//Cast a ray on the left, right, and center of the cluster's sphere.
			for (int i = -1; i <= 1; ++i)
			{
				Vector3 pos = cluster.MyPathing.MyTransform.position + (i * toSide * cluster.MaxNinjaDistance);
				Vector3 toPos = (pos - CameraTracker.position);
				float distance = toPos.magnitude;

				//If no blockers were hit, there is a line of sight to this spot on the cluster.
				if (!Physics.Raycast(CameraTracker.position, toPos / distance, out hit, distance, raycastLayers))
				{
					//See how close the cluster is to the direction the player apparently aimed at.
					float tempDot = Vector3.Dot(HorizontalMask(aimDir), HorizontalMask(towardsCluster));
					if (tempDot > bestDot)
					{
						bestDot = tempDot;
						bestPos = cluster.MyPathing.MyTransform.position;
						break;
					}
				}
			}
		}

		return bestPos;
	}


	void Awake()
	{
		if (Instance != null)
			throw new UnityException("More than one instance of 'HumanBehavior'!");
		Instance = this;
		
		if (HeadTracker == null)
			throw new UnityException("'HeadTracker' property is null!");
		
		if (FaceTracker == null)
			throw new UnityException("'FaceTracker' property is null!");
		
		if (CameraTracker == null)
			throw new UnityException("'CameraTracker' property is null!");

		MyTransform = transform;
		Levitators = new List<Levitatable>();

		src = audio;
	}

	void Update()
	{
		timeSinceLastGesture += Time.deltaTime;

		Levitators = Levitators.Where(lv => lv != null && lv.MyRigid != null).ToList();

		if (timeSinceLastGesture > DisableGesturesDuration)
		{
			if (HeadTracker.GetAverageVelocity(KinematicsTrackerDuration).y < NodYVelocity)
			{
				timeSinceLastGesture = 0.0f;

				Vector3 forward = HeadTracker.GetForwardVectorAtTime(KinematicsTrackerDuration).Forward;
				foreach (Levitatable lev in FindLevitators(forward))
					if (!Levitators.Contains(lev))
					{
						Levitators.Add(lev);
						lev.Levitate();
					}
			}
			else if (Levitators.Count > 0 &&
					    (HorizontalMask(FaceTracker.GetAverageVelocity(KinematicsTrackerDuration)).sqrMagnitude >= (JerkHorizontalSpeed * JerkHorizontalSpeed) ||
					     Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)))
			{
				Vector3 pos = FindTargetLookPos();
				foreach (Levitatable lev in Levitators)
				{
					lev.Throw((pos - lev.MyRigid.position).normalized);
				}
				Levitators.Clear();

				src.PlayOneShot(src.clip);
				Transform tr =((GameObject)Instantiate(ThrowParticlesPrefab)).transform;
				tr.position = CameraTracker.position + (CameraTracker.forward * 2.0f);
				tr.parent = CameraTracker;
			}
		}
	}
}