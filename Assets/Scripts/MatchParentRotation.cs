using UnityEngine;


/// <summary>
/// Continuously sets this GameObject's rotation to match its parent's.
/// </summary>
public class MatchParentRotation : MonoBehaviour
{
	private Transform tr;

	void Awake()
	{
		tr = transform;
	}

	void FixedUpdate()
	{
		if (tr.parent != null) tr.rotation = tr.parent.rotation;
	}
}