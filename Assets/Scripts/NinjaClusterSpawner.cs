using UnityEngine;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Spawns clusters of ninjas.
/// </summary>
public class NinjaClusterSpawner : MonoBehaviour
{
	private static ClusterStartNode[] ClusterStarts = null;


	public GameObject ClusterPrefab = null, NinjaPrefab = null;
	public int MaxNinja = 32;


	public float MinSpawnInterval = 2.5f,
				 MaxSpawnInterval = 4.5f;
	public int MinNumbNinjas = 5,
			   MaxNumbNinjas = 15;


	void Awake()
	{
		if (ClusterStarts == null)
			ClusterStarts = GameObject.FindObjectsOfType<ClusterStartNode>();
		if (ClusterStarts.Length == 0)
			Debug.LogError("No cluster start nodes!");

		if (ClusterPrefab == null)
			throw new UnityException("'ClusterPrefab' is null!");
		if (NinjaPrefab == null)
			throw new UnityException("'NinjaPrefab' is null!");
	}

	void Start()
	{
		StartCoroutine(SpawnClusterCoroutine());
	}


	private System.Collections.IEnumerator SpawnClusterCoroutine()
	{
		yield return new WaitForSeconds(Random.Range(MinSpawnInterval, MaxSpawnInterval));

		//Count the number of ninjas currently available.
		int totalNinjas = NinjaCluster.AllClusters.Sum(cl => cl.NinjaAIs.Count);

		//Create the cluster.
		NinjaCluster clust = ((GameObject)Instantiate(ClusterPrefab)).GetComponent<NinjaCluster>();
		int startIndex = Random.Range(0, ClusterStarts.Length);
		clust.MyPathing.Current = ClusterStarts[startIndex].GetComponent<PathNode>();
		clust.MyPathing.MyTransform.position = clust.MyPathing.Current.MyTransform.position;

		//Spawn each ninja.
		int nNinjas = Random.Range(MinNumbNinjas, MaxNumbNinjas);
		Vector3 spawnPos = clust.MyPathing.MyTransform.position;
		float spawnRadius = ClusterStarts[startIndex].NinjaSpawnRadius;
		for (int i = 0; i < nNinjas && (totalNinjas + i) < MaxNinja; ++i)
		{
			NinjaAIPlayerInput nj = ((GameObject)Instantiate(NinjaPrefab)).GetComponent<NinjaAIPlayerInput>();
			if (nj == null)
				throw new UnityException("Spawned ninja didn't have 'NinjaAIPlayerInput' component!");

			nj.Cluster = clust;
			nj.MyTransform.position = new Vector3(Random.value * spawnRadius, 0.0f, 0.0f);
			Quaternion randRot = Quaternion.AngleAxis(Random.value * 360.0f, new Vector3(0.0f, 1.0f, 0.0f));
			nj.MyTransform.position = spawnPos + (randRot * nj.MyTransform.position);
		}

		//Restart the countdown.
		StartCoroutine(SpawnClusterCoroutine());
	}
}