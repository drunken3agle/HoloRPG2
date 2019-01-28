using System;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

// Set SpatialUnderstandingSurface Base+Wire color to black for transparent rendering

/* Adapted from https://medium.com/southworks/how-to-use-spatial-understanding-to-query-your-room-with-hololens-4a6192831a6f */
public class ScanningManager : MonoBehaviour {

    [Tooltip("Tutorial NPC"), SerializeField]
    private GameObject TutorialNPC;
    
    [Tooltip("List of spawnable enemies. Sort by level!") , SerializeField]
    private GameObject[] Enemies;

    private uint CurrentLevel = 0; // use as index for enemies

    [Tooltip("Max number of possible positions returned"), Range(1, 64), SerializeField]
    private int MaxResultCount = 32;

    [Tooltip("GameObjects to disable when scanning has finsihed!"), SerializeField]
    private GameObject[] ToDisable;

   void Start() {
        SpatialUnderstanding.Instance.ScanStateChanged += ScanStateChanged;
    }

    private void ScanStateChanged() {   
        if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
        {
            Debug.Log("ScanningManager: Scan complete!");

            // Disable MRTK AppState script (this.IsTheCaptainNow = true)
            GetComponent<HoloToolkit.Examples.SpatialUnderstandingFeatureOverview.AppState>().enabled = false;
            Debug.Log("ScanningManager: Disabled MRTK AppState");
            
            // Disable all elements in ToDisable
            foreach(GameObject g in ToDisable) {
                Debug.Log("ScanningManager: Disabled " + g.name);
                g.SetActive(false);
            }

            InstantiateObjectOnFloor();
        }
    }

    private void OnDestroy() {
        SpatialUnderstanding.Instance.ScanStateChanged -= ScanStateChanged;
    }

    void Update() {
       
    }

    private void InstantiateObjectOnFloor() {
        SpatialUnderstandingDllTopology.TopologyResult[] _resultsTopology = new SpatialUnderstandingDllTopology.TopologyResult[MaxResultCount];

        var minLengthFloorSpace = 0.25f;
        var minWidthFloorSpace = 0.25f;

        var resultsTopologyPtr = SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(_resultsTopology);
        var locationCount = SpatialUnderstandingDllTopology.QueryTopology_FindPositionsOnFloor(minLengthFloorSpace, minWidthFloorSpace, 
                                                                                               _resultsTopology.Length, resultsTopologyPtr);

        if (locationCount > 0)
        {
            Instantiate(Enemies[CurrentLevel], _resultsTopology[0].position, Quaternion.LookRotation(_resultsTopology[0].normal, Vector3.up));
        } else {
            Debug.Log("No suitable spawn position!");
        }
    }

    private void SpawnOnFloor(GameObject ToSpawn, float distanceThreshold, float angleThreshold, bool behindPlayer) {
        SpatialUnderstandingDllTopology.TopologyResult[] _resultsTopology = new SpatialUnderstandingDllTopology.TopologyResult[MaxResultCount];
        IntPtr resultsTopologyPtr = SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(_resultsTopology);

        Vector3 dimensions = ToSpawn.GetComponent<Renderer>().bounds.size;
        int locationCount = SpatialUnderstandingDllTopology.QueryTopology_FindPositionsOnFloor(dimensions.x, dimensions.z, 
                                                                                               _resultsTopology.Length, resultsTopologyPtr);

        if(locationCount > 0) {
            bool successful = false;
            for (uint i = 0; i < locationCount; ++i) {
                float distanceFromPlayer = Vector3.Distance(_resultsTopology[i].position, transform.position);
                if (distanceFromPlayer > distanceThreshold) {
                    continue;
                }

                // Compare agains backwards vector if behindPlayer is true
                float relativeAngle = Vector3.Angle((behindPlayer ? -transform.forward : transform.forward), _resultsTopology[i].position - transform.position);
                if ((Math.Abs(relativeAngle) > angleThreshold)) {
                    continue;
                }

                // Suitable location found!
                Instantiate(ToSpawn, _resultsTopology[i].position, Quaternion.LookRotation(_resultsTopology[0].normal, Vector3.up));
                successful = true;
                break;
            } 
            
            if (!successful) { Debug.Log("Unable to spawn GameObject: No suitable location"); }
        } else {
            Debug.Log("Unable to spawn GameObject: No locations found");
        }
    }
}
