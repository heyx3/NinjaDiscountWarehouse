using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class DeadNinja : MonoBehaviour
{
	public float KillDistanceFromPlayer = 100.0f;
	public float DeathWaitTime = 2.0f;
	public float ChanceOfInstaDeath = 0.5f;
	public float KillMaxSpeed = 0.5f;

	public bool WillDieSoon { get; private set; }

	private Transform tr;
	private Rigidbody rgd;


	private void AttachKillComponent()
	{
		WillDieSoon = true;
		KillAfterTime kat = gameObject.AddComponent<KillAfterTime>();
		kat.TimeTillDeath = DeathWaitTime;
	}

	void Awake()
	{
		tr = transform;
		rgd = rigidbody;

		WillDieSoon = Random.value > ChanceOfInstaDeath;
		if (WillDieSoon)
		{
			AttachKillComponent();
		}
	}

	void Update()
	{
		if (!WillDieSoon &&
			(HumanBehavior.Instance.MyTransform.position - tr.position).sqrMagnitude < (KillDistanceFromPlayer * KillDistanceFromPlayer) &&
			rgd.velocity.sqrMagnitude < (KillMaxSpeed * KillMaxSpeed))
		{
			AttachKillComponent();
		}
	}


	void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0.0f, 0.0f, 0.0f, 0.25f);
		Gizmos.DrawSphere(GameObject.FindObjectOfType<HumanBehavior>().transform.position, KillDistanceFromPlayer);
	}
}