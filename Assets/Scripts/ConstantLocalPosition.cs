using UnityEngine;


/// <summary>
/// Keeps a constant value for the local position.
/// </summary>
public class ConstantLocalPosition : MonoBehaviour
{
	public Vector3 LocalPos = Vector3.zero;
	private Transform tr;

	void Awake()
	{
		tr = transform;
	}
	void FixedUpdate()
	{
		tr.localPosition = LocalPos;
	}
}