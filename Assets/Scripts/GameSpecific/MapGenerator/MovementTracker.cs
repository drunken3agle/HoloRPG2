using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTracker : MonoBehaviour {

	private Vector3 lastPosition = Vector3.zero;
	private float distanceMoved = 0.0f;

	[SerializeField] private float movementThreshold = 0.2f;

	void Update() {
		float distanceSinceLastFrame = Vector3.Distance(lastPosition, transform.position);

		if (distanceSinceLastFrame > movementThreshold) {
			distanceMoved += distanceSinceLastFrame;
			lastPosition = transform.position;

			Debug.Log("Moved " + distanceSinceLastFrame + "m, Overall movement: " + distanceMoved + "m");
			
		}
	}

	public float GetDistanceMoved(){
		return distanceMoved;
	}

	public void SetDistanceMoved(float value){
		distanceMoved = value;
	}
}
