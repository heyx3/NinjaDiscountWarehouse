using UnityEngine;
using System.Collections.Generic;
using System.Linq;


[RequireComponent(typeof(Levitatable))]
[RequireComponent(typeof(PathFollower))]
public class ConveyorBeltObject : MonoBehaviour
{
	public static PathNode[] PathStarts = null;


	public float RespawnRadius = 100.0f;
	public float RespawnMaxSpeed = 0.5f;

	private static Dictionary<PathNode, ConveyorBeltObject> CurrentlySpawning = new Dictionary<PathNode, ConveyorBeltObject>();
	private bool isWaitingToSpawn;

	public bool IsOnBelt { get { return rgd.isKinematic && !lvt.enabled; } }

	private PathFollower pf;
	private Levitatable lvt;
	private Rigidbody rgd { get { return lvt.MyRigid; } }

	void Awake()
	{
		pf = GetComponent<PathFollower>();
		lvt = GetComponent<Levitatable>();

		if (PathStarts == null)
		{
			PathStarts = GameObject.FindObjectsOfType<ConveyorBeltStartNode>().Select(cbsn => cbsn.GetComponent<PathNode>()).ToArray();
			foreach (PathNode pn in PathStarts)
				CurrentlySpawning.Add(pn, null);
		}

		isWaitingToSpawn = true;
	}


	void Start()
	{
		rgd.isKinematic = true;
		lvt.enabled = false;

		pf.OnPathEnd += (s, e) =>
			{
				pf.MovingBackwards = false;
				pf.enabled = false;
				lvt.enabled = true;
				rgd.isKinematic = false;
			};
	}

	private void Respawn(int index)
	{
		isWaitingToSpawn = false;

		rgd.isKinematic = true;
		lvt.enabled = false;
		pf.enabled = true;

		pf.Current = PathStarts[index];
		CurrentlySpawning[pf.Current] = this;
		rgd.position = pf.Current.MyTransform.position;
	}


	void FixedUpdate()
	{
		if (isWaitingToSpawn)
		{
			rgd.position = new Vector3(9999.0f, 9999.0f, 9999.0f);

			for (int i = 0; i < PathStarts.Length; ++i)
			{
				if (CurrentlySpawning[PathStarts[i]] == null)
				{
					Respawn(i);
					break;
				}
			}
		}
		else if (IsOnBelt)
		{
			PathNode start = pf.Current.GetPathStart(false);
			if (CurrentlySpawning[start] == this && pf.Current != start)
			{
				CurrentlySpawning[start] = null;
			}
		}
		else if (rgd.velocity.sqrMagnitude < RespawnMaxSpeed * RespawnMaxSpeed &&
				 (HumanBehavior.Instance.MyTransform.position - rgd.position).sqrMagnitude > RespawnRadius * RespawnRadius)
		{
			isWaitingToSpawn = true;
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.05f);
		Gizmos.DrawSphere(GameObject.FindObjectOfType<HumanBehavior>().transform.position, RespawnRadius);
	}
}