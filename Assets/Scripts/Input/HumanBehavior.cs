using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles behavior for the player playing this game.
/// </summary>
public class HumanBehavior : MonoBehaviour
{
	public static HumanBehavior Instance { get; private set; }


	public float NodYVelocity = -10.0f;
	public float KinematicsTrackerDuration = 0.05f;
	public float DisableGesturesDuration = 0.25f;
	public int MaxLevitations = 8;

	private float timeSinceLastGesture = 9999.0f;

	public KinematicsTracker HeadTracking = null;
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


	void Awake()
	{
		if (Instance != null)
			throw new UnityException("More than one instance of 'HumanBehavior'!");
		Instance = this;

		if (HeadTracking == null)
			throw new UnityException("'HeadTracking' property is null!");

		if (CameraTracker == null)
			throw new UnityException("'CameraTracker' property is null!");

		MyTransform = transform;
		Levitators = new List<Levitatable>();
	}


	void Update()
	{
		timeSinceLastGesture += Time.deltaTime;

		if (IsLevitating)
		{
			if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
			{
				foreach (Levitatable lev in Levitators)
					lev.Throw(new Vector3(1.0f, 0.0f, 1.0f).normalized);
				Levitators.Clear();
			}
		}
		else
		{
			if (timeSinceLastGesture > DisableGesturesDuration && HeadTracking.GetAverageVelocity(KinematicsTrackerDuration).y < NodYVelocity)
			{
				timeSinceLastGesture = 0.0f;

				Vector3 forward = HeadTracking.GetForwardVectorAtTime(KinematicsTrackerDuration).Forward;
				Levitators = FindLevitators(forward);
				foreach (Levitatable levs in Levitators)
					levs.Levitate();
			}
		}
	}
}