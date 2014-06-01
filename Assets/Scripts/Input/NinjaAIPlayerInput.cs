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

	public float MaxSeparationForce = 10.0f,
				 MaxSeparationForceDistance = 10.0f,
				 SeparationForceDistancePower = 1.0f,
				 ClusterForce = 10.0f;


	void Start()
	{
		if (Cluster == null)
			throw new UnityException("No cluster associated with this Ninja!");
		Cluster.NinjaAIs.Add(this);
	}
	void OnDestroy()
	{
		Cluster.NinjaAIs.Remove(this);
	}

	void FixedUpdate()
	{
		//Rotate to face the player.
		MyTransform.forward = (HumanBehavior.Instance.MyTransform.position - MyTransform.position).normalized;


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