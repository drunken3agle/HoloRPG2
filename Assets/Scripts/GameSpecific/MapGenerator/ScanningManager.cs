﻿using System;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class ScanningManager : Singleton<ScanningManager> {

    [Tooltip("Max number of possible positions returned"), Range(1, 64), SerializeField]
    private int MaxResultCount = 32;

    [Tooltip("GameObjects to disable when scanning has finsihed!"), SerializeField]
    private GameObject[] ToDisable;

    [Tooltip("GameObjects to enable when scanning has finsihed!"), SerializeField]
    private GameObject[] ToEnable;

    [Tooltip("AppState Manager"), SerializeField]
    private GameObject AppState;

    void Start() {
        SpatialUnderstanding.Instance.ScanStateChanged += ScanStateChanged;
    }

    private void ScanStateChanged() {   
        if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
        {
            Debug.Log("ScanningManager: Scan complete!");

            // Disable MRTK AppState script (this.IsTheCaptainNow = true)
            AppState.GetComponent<HoloToolkit.Examples.SpatialUnderstandingFeatureOverview.AppState>().enabled = false;
            Debug.Log("ScanningManager: Disabled MRTK AppState");
            
            // Disable all elements in ToDisable
            foreach(GameObject g in ToDisable) {
                // Debug.Log("ScanningManager: Disabled " + g.name);
                g.SetActive(false);
            }

            // Disable all elements in ToDisable
            foreach(GameObject g in ToEnable) {
                // Debug.Log("ScanningManager: Disabled " + g.name);
                g.SetActive(true);
            }

            // Disable mesh rendering
            SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh = false;
            SpatialMappingManager.Instance.DrawVisualMeshes = false;
        }
    }

    protected override void OnDestroy() {
        SpatialUnderstanding.Instance.ScanStateChanged -= ScanStateChanged;
        
        base.OnDestroy();
    }

    public GameObject SpawnOnFloor(GameObject ToSpawn, float minDistance, float maxDistance, float minAngle, float maxAngle) {
        Debug.Log("Trying to spawn " + ToSpawn.name);

        SpatialUnderstandingDllTopology.TopologyResult[] _resultsTopology = new SpatialUnderstandingDllTopology.TopologyResult[MaxResultCount];

        Debug.Log("Preparing results");

        IntPtr resultsTopologyPtr = SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(_resultsTopology);

        
        Debug.Log("Pinning results in memory");

        Vector3 dimensions = Vector3.one; //ToSpawn.GetComponent<Renderer>().bounds.size;

        
        Debug.Log("Renderer size received");

        int locationCount = SpatialUnderstandingDllTopology.QueryTopology_FindPositionsOnFloor(dimensions.x, dimensions.z, 
                                                                                               _resultsTopology.Length, resultsTopologyPtr);

        if(locationCount > 0) {
            Debug.Log("Result count " + locationCount);

            GameObject newlySpawned = null;
            for (uint i = 0; i < locationCount; ++i) {
                float distanceFromPlayer = Vector3.Distance(_resultsTopology[i].position, transform.position);
                if (distanceFromPlayer > maxDistance || distanceFromPlayer < minDistance) { continue; }

                Debug.Log("Found floor patch with correct size");

                float relativeAngle = Vector3.Angle(transform.forward, _resultsTopology[i].position - transform.position);
                if (relativeAngle > maxAngle || relativeAngle < minAngle) { continue; } 
                
                Debug.Log("Found floor patch at correct angle");

                // Suitable location found!
                newlySpawned = Instantiate(ToSpawn, _resultsTopology[i].position, Quaternion.LookRotation(_resultsTopology[0].normal, Vector3.up));
                break;
            } 
            
            if (newlySpawned != null) { 
                return newlySpawned;
            } else {
                Debug.Log("Unable to spawn GameObject: No suitable location"); 
            }
        } else {
            Debug.Log("Unable to spawn GameObject: No locations found");
        }

        return null;
    }
}
