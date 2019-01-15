using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Smoothly moves the camera through world space
public class SmoothCanvas : MonoBehaviour {

	private GameObject mainCamera;

	[Tooltip("Target distace relative to main camera")] 
	[SerializeField] private float targetDistance = 100f;

	// Use this for initialization
	void Start () {
		mainCamera = Camera.main.gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 currentTarget = mainCamera.transform.position + (mainCamera.transform.forward * targetDistance);
		float currentMagnitude = (transform.position - currentTarget).magnitude;
		currentMagnitude = Mathf.Min(currentMagnitude, 1f);

		transform.position = Vector3.Lerp(transform.position, currentTarget, currentMagnitude);
		transform.rotation = mainCamera.transform.rotation;
	}
}
