using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum HandState
{
    Facing_Fowrawd,
    Palm_Open,
    Palm_Closed,
    Moving_Forward,
    Moving_Right,
    Moving_Left
}

public class HandTracking {

    public HandTracking(Transform handTransform, HandTrackingThresholds handTrackingThresholds)
    {
        this.handTransform = handTransform;
        this.handTrackingThresholds = handTrackingThresholds;
        IsVisible = false;
        currentStates = new List<HandState>();
        lastStates = new List<HandState>();
    }

    public bool IsVisible;
    public Vector3 Position { get { return position; } }
    public Vector3 Direction { get { return direction; } }
    public Vector3 VelocityDirection { get { return velocity.normalized; } }
    public float VelocityIntensity { get { return velocity.magnitude * 100.0f; } }
    public float HandOpenness { get { return handOpenness; } }
    public List<HandState> CurrentStates { get { return currentStates; } }
    public List<HandState> LastStates { get { return lastStates; } }

    private Transform handTransform;
    private HandTrackingThresholds handTrackingThresholds;

    private Vector3 lastPosition;
    private Vector3 position;
    private Vector3 velocity;
    private Vector3 direction;
    private float handOpenness;
    private List<HandState> currentStates;
    private List<HandState> lastStates;

    public void UpdateHandTracking(float grabAngle, bool handJustShowedUp = false)
    {
        // make sure to give a valid value to lastPosition
        if (handJustShowedUp == true)
        {
            lastPosition = handTransform.position;
        }
        else
        {
            lastPosition = position;
        }
        // update all other values from transform
        position = handTransform.position;
        velocity = position - lastPosition;
        direction = handTransform.forward;

        // update how proportional value how far the palm is open
        handOpenness = 1.0f - (grabAngle / 3.14f);
    }


    public void UpdateHandStates(Transform directionReferenceTransform, bool handJustShowedUp = false)
    {
        // make sure to give a valid value to lastStates
        if (handJustShowedUp == true)
        {
            lastStates.Clear();
        }
        else
        {
            HandState[] tempArray = new HandState[currentStates.Count];
            currentStates.CopyTo(tempArray);
            lastStates = tempArray.OfType<HandState>().ToList();
        }
        currentStates.Clear();

        // Is facing forward ?
        if (Vector3.Dot(directionReferenceTransform.forward, direction) > handTrackingThresholds.ForwardAngularThreshold)
        {
            currentStates.Add(HandState.Facing_Fowrawd);
        }

        // Is hand open?
        if (handOpenness > handTrackingThresholds.HandOpenThreshold)
        {
            currentStates.Add(HandState.Palm_Open);
        }

        // Is hand closed (fist)
        if (handOpenness < handTrackingThresholds.HandClosedThreshold)
        {
            currentStates.Add(HandState.Palm_Closed);
        }

        // Is moving forward ?
        if ((VelocityIntensity > handTrackingThresholds.ForwardVelocityThreshold) 
            && (Vector3.Dot(directionReferenceTransform.forward, VelocityDirection) > handTrackingThresholds.ForwardAngularThreshold))
        {
            currentStates.Add(HandState.Moving_Forward);
        }

        // Is moving to the right ?
        if ((VelocityIntensity > handTrackingThresholds.SidewaysVelocityThreshold) 
            && (Vector3.Dot(directionReferenceTransform.right, VelocityDirection) > handTrackingThresholds.ForwardAngularThreshold))
        {
            currentStates.Add(HandState.Moving_Right);
        }
         
        // Is moving to the left ?
        if ((VelocityIntensity > handTrackingThresholds.SidewaysVelocityThreshold) 
            && (Vector3.Dot(-directionReferenceTransform.right, VelocityDirection) > handTrackingThresholds.ForwardAngularThreshold))
        {
            currentStates.Add(HandState.Moving_Left);
        }
    }

    // returns true if the given state was not also tracked on previous frame 
    public bool IsStateFreshlyTracked(HandState trackedState)
    {
        return lastStates.Contains(trackedState) == false;
    }
}
