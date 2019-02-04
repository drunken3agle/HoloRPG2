using System;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using System.Collections;
using System.Collections.Generic;

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
        if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done) {
            // Disable MRTK AppState script (this.IsTheCaptainNow = true)
            AppState.GetComponent<HoloToolkit.Examples.SpatialUnderstandingFeatureOverview.AppState>().enabled = false;
            
            // Disable all elements in ToDisable
            foreach(GameObject g in ToDisable) {
                g.SetActive(false);
            }

            // Disable all elements in ToDisable
            foreach(GameObject g in ToEnable) {
                g.SetActive(true);
            }

            // Disable mesh rendering
            SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh = false;
            SpatialMappingManager.Instance.DrawVisualMeshes = false;
        }
    }

    public GameObject SpawnOnFloor(GameObject ToSpawn, float minDistance, float maxDistance, float minAngle, float maxAngle) {
        SpatialUnderstandingDllTopology.TopologyResult[] _resultsTopology = new SpatialUnderstandingDllTopology.TopologyResult[MaxResultCount];
        List<SpatialUnderstandingDllTopology.TopologyResult> validSpawnLocations = new List<SpatialUnderstandingDllTopology.TopologyResult>();

        IntPtr resultsTopologyPtr = SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(_resultsTopology);
        Vector3 dimensions = Vector3.one; //ToSpawn.GetComponent<Renderer>().bounds.size;
        
        int locationCount = SpatialUnderstandingDllTopology.QueryTopology_FindPositionsOnFloor(dimensions.x, dimensions.z, 
                                                                                               _resultsTopology.Length, resultsTopologyPtr);

        if(locationCount > 0) {
            GameObject newlySpawned = null;
            for (uint i = 0; i < locationCount; ++i) {
                float distanceFromPlayer = Vector3.Distance(_resultsTopology[i].position, Camera.main.transform.position);
                if (distanceFromPlayer > maxDistance || distanceFromPlayer < minDistance) { continue; }

                float relativeAngle = Vector3.Angle(Camera.main.transform.forward, _resultsTopology[i].position - Camera.main.transform.position);
                if (relativeAngle > maxAngle || relativeAngle < minAngle) { continue; } 

                // add this location to the valid spawn locations
                validSpawnLocations.Add(_resultsTopology[i]);
            }

            // Pick up one location from the valid locations found
            int randomIndex = Utils.GetRndIndex(validSpawnLocations.Count);
            SpatialUnderstandingDllTopology.TopologyResult spawnLocation = validSpawnLocations[randomIndex];
            Debug.Log("Spawning at index " + randomIndex + " at position : " + spawnLocation.position.ToString());
            
            // Spawn the object at the random valid location chosen
            newlySpawned = Instantiate(ToSpawn, spawnLocation.position, Quaternion.LookRotation(spawnLocation.normal, Vector3.up));

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
