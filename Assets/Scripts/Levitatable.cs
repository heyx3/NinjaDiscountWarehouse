using UnityEngine;


/// <summary>
/// Provides behavior for an object that can be levitated.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Levitatable : MonoBehaviour
{
	private static HumanBehavior Human { get { return HumanBehavior.Instance; } }

	private static Vector3 HorizontalMask(Vector3 inV) { return new Vector3(inV.x, 0.0f, inV.z); }
	private static Vector3 HorizontalMaskNorm(Vector3 inV) { return new Vector3(inV.x, 0.0f, inV.z).normalized; }
	private static Vector3 VerticalMask(Vector3 inV) { return new Vector3(0.0f, inV.y, 0.0f); }
	private static Vector3 VerticalMaskNorm(Vector3 inV) { return new Vector3(0.0f, Mathf.Sign(inV.y), 0.0f); }


	public enum States
	{
		Inert,
		Levitated,
		Thrown,
	}
	public States State { get; private set; }


	[System.Serializable]
	public class LevitatedData
	{
		public Vector3 InitialRotImpulseVariance = new Vector3(100.0f, 100.0f, 100.0f);
		[System.NonSerialized] public Vector2 TargetDir = Vector2.right;

		[System.Serializable]
		public class SubData
		{
			public float TargetDistanceFromPlayer = 10.0f;
			public float VelocityTowardsTarget = 10.0f;
			public float VelocityDistanceScale = 1.0f;
		}
		public SubData HorizontalMovement, VerticalMovement;

		public Vector3 GetTargetPosition(Vector3 playerPos)
		{
			return playerPos +
				   (new Vector3(TargetDir.x, 0.0f, TargetDir.y) * HorizontalMovement.TargetDistanceFromPlayer) +
				   new Vector3(0.0f, VerticalMovement.TargetDistanceFromPlayer, 0.0f);
		}
	}
	public LevitatedData Levitating = new LevitatedData();


	[System.Serializable]
	public class ThrownData
	{
		public float Acceleration = 300.0f;
		public float AccelerationDuration = 1.5f;
		[System.NonSerialized] public float TimeTillInert = -1.0f;
		[System.NonSerialized] public Vector3 Direction = new Vector3(1.0f, 0.0f, 0.0f);
	}
	public ThrownData Throwing = new ThrownData();

	public float GravityAcceleration = 9.8f;
	public Renderer ThrowableMeshRenderer = null;


	public Rigidbody MyRigid { get; private set; }


	void Awake()
	{
		MyRigid = rigidbody;
		MyRigid.useGravity = false;
		State = States.Inert;

		if (ThrowableMeshRenderer != null)
			ThrowableMeshRenderer.enabled = false;
	}

	void FixedUpdate()
	{
		MyRigid.useGravity = false;

		switch (State)
		{
			case States.Inert:
				MyRigid.AddForce(new Vector3(0.0f, -GravityAcceleration, 0.0f), ForceMode.Acceleration);
				break;


			case States.Levitated:

				Vector3 playerPos = Human.MyTransform.position;
				Vector3 targetPos = Levitating.GetTargetPosition(playerPos),
					    towardsTarget = targetPos - MyRigid.position;

				//Treat horizontal and vertical accel separately.

				//Horizontal.
				Vector3 towardsTarget_partial = HorizontalMask(towardsTarget);
				if (towardsTarget_partial.sqrMagnitude != 0.0f)
					MyRigid.MovePosition(MyRigid.position +
										 (Time.deltaTime * Levitating.HorizontalMovement.VelocityTowardsTarget *
														   Levitating.HorizontalMovement.VelocityDistanceScale * towardsTarget_partial));

				//Vertical.
				towardsTarget_partial = VerticalMask(towardsTarget);
				if (towardsTarget_partial.sqrMagnitude != 0.0f)
					MyRigid.MovePosition(MyRigid.position +
										 (Time.deltaTime * Levitating.VerticalMovement.VelocityTowardsTarget *
														   Levitating.VerticalMovement.VelocityDistanceScale * towardsTarget_partial));

				break;


			case States.Thrown:

				//Update time until inert.
				Throwing.TimeTillInert -= Time.deltaTime;
				if (Throwing.TimeTillInert <= 0.0f)
				{
					State = States.Inert;
					Throwing.TimeTillInert = -1.0f;
				}
				else
				{
					//Apply the throw force.
					MyRigid.AddForce(Throwing.Acceleration * Throwing.Direction, ForceMode.Acceleration);
				}

				break;


			default: throw new System.NotImplementedException();
		}
	}


	/// <summary>
	/// Changes state to levitating.
	/// </summary>
	public void Levitate()
	{
		State = States.Levitated;
		Vector3 playerToMe = (MyRigid.position - Human.MyTransform.position).normalized;
		Levitating.TargetDir = new Vector2(playerToMe.x, playerToMe.z);
		MyRigid.AddTorque(new Vector3(Levitating.InitialRotImpulseVariance.x * (-1.0f + (2.0f * Random.value)),
									  Levitating.InitialRotImpulseVariance.y * (-1.0f + (2.0f * Random.value)),
									  Levitating.InitialRotImpulseVariance.z * (-1.0f + (2.0f * Random.value))),
						  ForceMode.Impulse);

		if (ThrowableMeshRenderer != null)
		{
			ThrowableMeshRenderer.enabled = true;
			ThrowableMeshRenderer.material = ThrowableMaterialController.Instance.ThrowableMat;
		}
	}
	/// <summary>
	/// Changes state to throwing.
	/// </summary>
	public void Throw(Vector3 dir)
	{
		State = States.Thrown;
		Throwing.TimeTillInert = Throwing.AccelerationDuration;
		Throwing.Direction = dir;

		if (ThrowableMeshRenderer != null)
		{
			ThrowableMeshRenderer.enabled = false;
		}
	}
}