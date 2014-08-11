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
	public bool RespawnOnStart = true;

	private static ConveyorBeltObject spawningObject;
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
		}

		spawningObject = null;

		if (RespawnOnStart)
		{
			isWaitingToSpawn = true;
		}
		else
		{
			isWaitingToSpawn = false;
		}
	}


	void Start()
	{
		pf.OnPathEnd += (s, e) =>
			{
				pf.MovingBackwards = false;
				pf.enabled = false;
				lvt.enabled = true;
				rgd.isKinematic = false;
			};

		pf.enabled = false;

		if (RespawnOnStart)
		{
			rgd.isKinematic = true;
			lvt.enabled = false;
		}
		else
		{
			rgd.isKinematic = false;
			lvt.enabled = true;
		}
	}

	private void Respawn(int index)
	{
		isWaitingToSpawn = false;

		rgd.isKinematic = true;
		lvt.enabled = false;
		pf.enabled = true;

		pf.Current = PathStarts[index];
		spawningObject = this;
		rgd.position = pf.Current.MyTransform.position;
	}


	void FixedUpdate()
	{
		if (isWaitingToSpawn)
		{
			if (spawningObject == null)
			{
				Respawn(Random.Range(0, PathStarts.Length));
			}
			else
			{
				rgd.position = new Vector3(9999.0f, 9999.0f, 9999.0f);
			}
		}
		else if (IsOnBelt)
		{
			if (spawningObject == this && pf.Current.Previous != null)
			{
				spawningObject = null;
			}
		}
		else if (rgd.velocity.sqrMagnitude < RespawnMaxSpeed * RespawnMaxSpeed &&
				 (HumanBehavior.Instance.MyTransform.position - rgd.position).sqrMagnitude > RespawnRadius * RespawnRadius)
		{
			isWaitingToSpawn = true;
			rgd.isKinematic = true;
			lvt.enabled = false;
		}
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.05f);
		Gizmos.DrawSphere(GameObject.FindObjectOfType<HumanBehavior>().transform.position, RespawnRadius);
	}
}