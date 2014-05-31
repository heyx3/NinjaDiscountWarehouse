using UnityEngine;


/// <summary>
/// Scales the radius of this GameObject's collision sphere based on its distance to the player.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class ScaleSphereWithDistance : MonoBehaviour
{
	public float ScaleAmount = 0.5f;

	[System.NonSerialized] public float BaseSphereRadius;

	private SphereCollider sphere;

	void Awake()
	{
		sphere = GetComponent<SphereCollider>();
		BaseSphereRadius = sphere.radius;
	}

	void FixedUpdate()
	{
		sphere.radius = BaseSphereRadius * ScaleAmount * Vector3.Distance(sphere.bounds.center, HumanBehavior.Instance.MyTransform.position);
	}
}