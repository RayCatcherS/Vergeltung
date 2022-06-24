using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateAgentPositions : MonoBehaviour
{
	public float range = 5f;
	public int sample = 5;

	List<Vector3> randomNavMeshPositions = new List<Vector3>();


    public void Update() {
		randomPoint(gameObject.transform.position, range);

	}

    void randomPoint(Vector3 center, float range) {
		randomNavMeshPositions = new List<Vector3>();



		for (int i = 0; i < sample; i++) {

			Vector3 randomPos = new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y);
			Vector3 randomPoint = center + randomPos * range;

			UnityEngine.AI.NavMeshHit hit;
			if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, range, UnityEngine.AI.NavMesh.AllAreas)) {

				randomNavMeshPositions.Add(hit.position);

			}
		}
	}


#if UNITY_EDITOR
	void OnDrawGizmos() {
		if (randomNavMeshPositions.Count != 0) {
			Gizmos.color = Color.magenta;

			foreach(Vector3 position in randomNavMeshPositions) {

				Gizmos.DrawSphere(position, 0.25f);
			}
			
		}
	}
#endif
}
