using UnityEngine;


/// <summary>
/// The player input for an AI-controlled ninja.
/// </summary>
public class NinjaAIPlayerInput : PlayerInput
{
	private static Vector3 HorizontalMask(Vector3 inV) { return new Vector3(inV.x, 0.0f, inV.z); }


	public override Vector2 MovementInput { get { return moveInput; } }
	private Vector2 moveInput = Vector2.zero;


	public NinjaCluster Cluster = null;
	public GameObject DeadNinjaPrefab = null;

	public float MaxSeparationForce = 10.0f,
				 MaxSeparationForceDistance = 10.0f,
				 SeparationForceDistancePower = 1.0f,
				 ClusterForce = 10.0f;
	public float MomentumToDie = 10.0f;


	void Start()
	{
		if (Cluster == null)
			Debug.LogWarning("No cluster associated with this Ninja!");
		else
			Cluster.NinjaAIs.Add(this);
	}
	void OnDestroy()
	{
		if (Cluster != null)
			Cluster.NinjaAIs.Remove(this);
	}

	void OnCollisionEnter(Collision coll)
	{
		Levitatable levt = coll.gameObject.GetComponent<Levitatable>();
		if (levt != null && (levt.MyRigid.mass * levt.MyRigid.velocity.magnitude) >= MomentumToDie)
		{
			if (DeadNinjaPrefab != null)
			{
				Transform tr = ((GameObject)Instantiate(DeadNinjaPrefab)).transform;
				tr.position = MyTransform.position;
				tr.rotation = MyTransform.rotation;

				Rigidbody rgd = tr.GetComponent<Rigidbody>();
				//Knock the ninja back.
				rgd.AddForce(levt.MyRigid.velocity * levt.MyRigid.mass, ForceMode.Impulse);
				//Also knock him onto his ass.
				Vector3 originalUp = new Vector3(0.0f, 1.0f, 0.0f);
				Vector3 newUp = HorizontalMask(levt.MyRigid.velocity);
				Quaternion toRotate = Quaternion.FromToRotation(originalUp, newUp);
				Vector3 axis;
				const float speed = 1.0f;
				float angle;
				toRotate.ToAngleAxis(out angle, out axis);
				rgd.AddTorque(axis * speed * levt.MyRigid.mass, ForceMode.Impulse);
			}

			Destroy(gameObject);
		}
	}

	void FixedUpdate()
	{
		//Rotate to face the player.
		MyTransform.forward = HorizontalMask((HumanBehavior.Instance.MyTransform.position - MyTransform.position)).normalized;

		if (Cluster == null)
		{
			moveInput = Vector2.zero;
			return;
		}


		//Move towards the cluster center but away from other ninjas.

		Vector3 towardsCluster = HorizontalMask(Cluster.MyPathing.MyTransform.position - MyTransform.position);
		towardsCluster = towardsCluster.normalized * ClusterForce;

		Vector3 awayFromNinja = Vector3.zero;
		foreach (NinjaAIPlayerInput otherNinja in Cluster.NinjaAIs)
		{
			if (otherNinja != this)
			{
				Vector3 away = HorizontalMask(MyTransform.position - otherNinja.MyTransform.position);
				float dist = away.magnitude;

				float lerp = 1.0f - (dist / MaxSeparationForceDistance);
				awayFromNinja += (away / dist) *
								 (Mathf.Pow(Mathf.Clamp01(lerp), SeparationForceDistancePower) * MaxSeparationForce);
			}
		}

		Vector3 move = towardsCluster + awayFromNinja;
		moveInput = new Vector2(Vector3.Dot(move, MyTransform.right), Vector3.Dot(move, MyTransform.forward)).normalized;
	}
}