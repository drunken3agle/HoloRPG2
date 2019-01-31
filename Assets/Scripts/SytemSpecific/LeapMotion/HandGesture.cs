using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GestureType
{
    FireBall,
    Wave
}

[System.Serializable]
public class HandStateData
{
    [SerializeField] public HandState HandState;
    [SerializeField] public float UnvalidateAfter = 0.5f;
    [SerializeField] public int NumberOfTimesToTrack = 1;
    [SerializeField] public float DelayBeforeNextTracking = 1.0f;
}

public class TrackedHandState
{
    public HandState HandState;

    private int numberOfTimesTracked = 0;
    private float lastTimeWasTracked = 0.0f;

    private float unvalidateAfter;
    private int numberOfTimesToTrack;
    private float delayBeforeNextTracking;

    public TrackedHandState(HandStateData handStateData)
    {
        unvalidateAfter         = handStateData.UnvalidateAfter;
        numberOfTimesToTrack    = handStateData.NumberOfTimesToTrack;
        delayBeforeNextTracking = handStateData.DelayBeforeNextTracking;
    }

    public bool IsStateAchieved()
    {
        return numberOfTimesTracked >= numberOfTimesToTrack;
    }

    public void UnvalidateObseleteStates(float currentTime)
    {
        if ((numberOfTimesTracked > 0)
              && (currentTime - lastTimeWasTracked > unvalidateAfter))
        {
            numberOfTimesTracked--;
            lastTimeWasTracked = currentTime;                // ToDo: incremently decrease time in a while loop until either number of times reaches 0 or time is exceeded 

        }
    }

    public void IncrementTrackedState(float currentTime)
    {
        numberOfTimesTracked++;
        lastTimeWasTracked = currentTime;
    }

    public void ResetState()
    {
        numberOfTimesTracked = 0;
        lastTimeWasTracked = 0.0f;
    }
}

public class HandGesture : MonoBehaviour {

    public GestureType GestureType;

    // Array to edit in inspector
    [SerializeField] private HandStateData[] statesToTrackArray;

    // Dictionary to keep track of the tracked states
    private Dictionary<HandState, TrackedHandState> currentTrackingContext;



    private void Start()
    {
        // initialize dictionary with items from array that should has ben filled in editor 
        currentTrackingContext = new Dictionary<HandState, TrackedHandState>();
        foreach (HandStateData handStateData in statesToTrackArray)
        {
            currentTrackingContext.Add(handStateData.HandState, new TrackedHandState(handStateData));
        }

        
    }

    public bool ContainsHandState(HandState handState)
    {
        return currentTrackingContext.ContainsKey(handState);
    }

    public bool AddTrackedHandState(HandState handState)
    {
        // first update current tracking context (as it is not being updated in the Update() to save performance)
        foreach (TrackedHandState trackedHandState in currentTrackingContext.Values)
        {
            trackedHandState.UnvalidateObseleteStates(Time.time);
        }


        // Add tracked hand state to the tracking context
        TrackedHandState currentTrackedHandState;
        if (currentTrackingContext.TryGetValue(handState, out currentTrackedHandState))
        {
            currentTrackedHandState.IncrementTrackedState(Time.time);
        }


        // See if all states achieved
        bool isAllStatesAchieved = true;
        foreach (TrackedHandState trackedHandState in currentTrackingContext.Values)
        {
            if (trackedHandState.IsStateAchieved() == false)
            {
                isAllStatesAchieved = false;
                break;
            }
        }

        // Reset tracking context if all states achieved
        if (isAllStatesAchieved == true)
        {
            foreach (TrackedHandState trackedHandState in currentTrackingContext.Values)
            {
                trackedHandState.ResetState();
            }
        }
        return isAllStatesAchieved;
    }
}
