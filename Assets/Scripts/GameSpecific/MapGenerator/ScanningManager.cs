using System;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;


/* Adapted from https://medium.com/southworks/how-to-use-spatial-understanding-to-query-your-room-with-hololens-4a6192831a6f */
public class ScanningManager : MonoBehaviour, IInputClickHandler {

    [Tooltip("3D Text for displaying scanning instructions")]
	[SerializeField] private TextMesh InstructionTextMesh;

    public Transform FloorPrefab;

    [Tooltip("List of all spawnable enemies")]
    [SerializeField] private GameObject[] SpawnableEnemies;

    [Tooltip("List of all spawnable NPCs")]
    [SerializeField] private GameObject[] SpawnableNPCs;

    [Tooltip("Set maximum number of results returned for a spatial query")]
    [SerializeField] private uint MaxResultCount = 32;

    void Start()
    {
        InputManager.Instance.PushFallbackInputHandler(this.gameObject);
        SpatialUnderstanding.Instance.RequestBeginScanning();
        SpatialUnderstanding.Instance.ScanStateChanged += ScanStateChanged;
    }

    private void ScanStateChanged()
    {
        if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning)
        {
            LogSurfaceState();
        }
        else if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
        {
            InstantiateObjectOnFloor();
        }
    }

    private void OnDestroy()
    {
        SpatialUnderstanding.Instance.ScanStateChanged -= ScanStateChanged;
    }

    void Update()
    {
        switch (SpatialUnderstanding.Instance.ScanState)
        {
            case SpatialUnderstanding.ScanStates.None:
                InstructionTextMesh.text = "State: No Scanning";
                break;
            case SpatialUnderstanding.ScanStates.ReadyToScan:
                InstructionTextMesh.text = "State: Ready To Scan";
                break;
            case SpatialUnderstanding.ScanStates.Scanning:
                LogSurfaceState();
                break;
            case SpatialUnderstanding.ScanStates.Finishing:
                InstructionTextMesh.text = "State: Finishing Scan";
                break;
            case SpatialUnderstanding.ScanStates.Done:
                InstructionTextMesh.text = "State: Scan Finished";
                break;
            default:
                break;
        }
    }

    private void LogSurfaceState()
    {
        IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
        if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) != 0)
        {
            var stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();
            this.InstructionTextMesh.text = string.Format("TotalSurfaceArea: {0:0.##}\nWallSurfaceArea: {1:0.##}\nHorizSurfaceArea: {2:0.##}", 
                                                          stats.TotalSurfaceArea, stats.WallSurfaceArea, stats.HorizSurfaceArea);
        }
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        InstructionTextMesh.text = "Requested Finish Scan";
        Debug.Log("Clicked!");

        SpatialUnderstanding.Instance.RequestFinishScan();
    }

    private void InstantiateObjectOnFloor()
    {
        SpatialUnderstandingDllTopology.TopologyResult[] _resultsTopology = new SpatialUnderstandingDllTopology.TopologyResult[MaxResultCount];

        var minLengthFloorSpace = 0.25f;
        var minWidthFloorSpace = 0.25f;

        var resultsTopologyPtr = SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(_resultsTopology);
        var locationCount = SpatialUnderstandingDllTopology.QueryTopology_FindPositionsOnFloor(minLengthFloorSpace, minWidthFloorSpace, 
                                                                                               _resultsTopology.Length, resultsTopologyPtr);

        if (locationCount > 0)
        {
            Instantiate(this.FloorPrefab, _resultsTopology[0].position, Quaternion.LookRotation(_resultsTopology[0].normal, Vector3.up));

            this.InstructionTextMesh.text = "Spawned something";
        }
        else
        {
            this.InstructionTextMesh.text = "No suitable spawn location!";
        }
}
}
