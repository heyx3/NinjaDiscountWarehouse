using UnityEngine;


/// <summary>
/// Gives this GameObject a constant rotational velocity.
/// </summary>
public class ConstantRotation : MonoBehaviour
{
	public Vector3 RotVelocity = Vector3.zero;

	private Transform tr;

	void Awake()
	{
		tr = transform;
	}

	void FixedUpdate()
	{
		tr.eulerAngles += RotVelocity * Time.deltaTime;
	}
}