using UnityEngine;

/// <summary>
/// Indicates this game object is a path node that starts a path for ninja clusters.
/// </summary>
public class ClusterStartNode : UnityEngine.MonoBehaviour
{
	public float NinjaSpawnRadius = 10.0f;

	void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0.0f, 0.0f, 0.0f, 0.35f);
		Gizmos.DrawSphere(transform.position, NinjaSpawnRadius);
	}
}