using UnityEngine;


[RequireComponent(typeof(Levitatable))]
[RequireComponent(typeof(PathFollower))]
public class ConveyorBeltObject : MonoBehaviour
{
	public PathNode[] PathStarts = new PathNode[0];

	private PathFollower pf;
	private Levitatable lvt;
	private Rigidbody rgd { get { return lvt.MyRigid; } }

	void Awake()
	{
		pf = GetComponent<PathFollower>();
		lvt = GetComponent<Levitatable>();
	}

	void Start()
	{
		rgd.isKinematic = true;
		lvt.enabled = false;

		pf.OnPathEnd += (s, e) =>
			{
				pf.enabled = false;
				rgd.isKinematic = false;
			};
	}


	public void Respawn()
	{

	}
}