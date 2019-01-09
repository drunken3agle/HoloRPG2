using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	This class generates the game's content
 */
[RequireComponent(typeof(MovementTracker))]
public class Generator : MonoBehaviour {

	private List<GameRegion> regions;
	private int currentRegion = 0;

	private MovementTracker tracker;

	private List<GameObject> enemies;

	void Awake() {
		// TODO Initialize region list from json
		regions = new List<GameRegion>();

		enemies = new List<GameObject>();
	}
	
	void Start(){
		if(regions == null || regions.Count == 0) {
			Debug.Log("Generator: No regions defined");
		}

		tracker = GetComponent<MovementTracker>();
		if(tracker == null){
			Debug.Log("Generator: No tracking component available");
		}
	}

	void Update () {
		if (DistanceToNextRegion() < 0.0f && enemies.Count == 0) { // Advance to next region
			tracker.SetDistanceMoved(AccumulatedDistance());
			currentRegion++;

			if (regions[currentRegion].NPC != null){
				SpawnNPC();
			}
		} else { // Stay in current region
			if (regions[currentRegion].enemies.Count > 0 && SpawnThisFrame()){
				SpawnEnemy();
			}
		}
	}

	private void SpawnNPC(){
		// TODO implement
	}	

	private void SpawnEnemy(){
		// TODO implement		
	}

	private bool SpawnThisFrame(){
		// TODO implement
		return false;
	}

	private float AccumulatedDistance(){
		float overallDistance = 0.0f;
		for (int i = 0; i <= currentRegion; ++i) {
			overallDistance += regions[i].length;
		}

		return overallDistance;
	}

	private float DistanceToNextRegion(){
		return AccumulatedDistance() - tracker.GetDistanceMoved();
	}
}

/*
	Defines a single game region
 */
public struct GameRegion {
	public float length;
	public List<GameObject> enemies;
	public float spawnRate;
	public GameObject NPC;
}
