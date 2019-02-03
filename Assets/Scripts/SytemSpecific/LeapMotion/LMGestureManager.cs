using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using LeapInternal;


public struct HandTrackingThresholds
{
    public float HandOpenThreshold;
    public float HandClosedThreshold;
    public float ForwardVelocityThreshold;
    public float SidewaysVelocityThreshold;
    public float ForwardAngularThreshold;
}

public class LMGestureManager : Singleton<LMGestureManager> {

    public event System.Action<GestureType, HandTracking> GesturePerformed;
    public event System.Action<bool> FistClosed;

  

    [SerializeField] private float handOpenThreshold = 0.89f;
    [SerializeField] private float handClosedThreshold = 0.15f;
    [SerializeField] private float forwardVelocityThreshold = 5.0f;
    [SerializeField] private float sidewaysVelocityThreshold = 2.0f;
    [SerializeField] private float forwardAngularThreshold = 0.75f;
    
    [SerializeField] private HandGesture[] gesturesDatabank;

    private HandTrackingThresholds handTrackingThresholds;

    [SerializeField] private Transform rightHandTrans;
    [SerializeField] private Transform leftHandTrans;
    private List<HandGesture> gestures;

    private HandTracking leftHandTracking;
    private HandTracking rightHandTracking;

    private Controller controller;



    private void Start ()
    {
        controller  = new Controller();

        // Set up thresholds struct
        handTrackingThresholds = new HandTrackingThresholds();
        handTrackingThresholds.ForwardAngularThreshold      = forwardAngularThreshold;
        handTrackingThresholds.ForwardVelocityThreshold     = forwardVelocityThreshold;
        handTrackingThresholds.SidewaysVelocityThreshold    = sidewaysVelocityThreshold;
        handTrackingThresholds.HandOpenThreshold            = handOpenThreshold;
        handTrackingThresholds.HandClosedThreshold          = handClosedThreshold;

        // Create hand instances
        leftHandTracking    = new HandTracking(leftHandTrans,   handTrackingThresholds, HandSide.Left);
        rightHandTracking   = new HandTracking(rightHandTrans,  handTrackingThresholds, HandSide.Right);

        // Add gestures
        gestures = new List<HandGesture>();
        foreach(HandGesture handGesture in gesturesDatabank)
        {
            gestures.Add(Instantiate(handGesture.gameObject, transform).GetComponent<HandGesture>());
        }
    }
	
	void Update ()
    {
        Frame frame = controller.Frame();
        List<Hand> hands = frame.Hands;

        bool leftHandSeen = false;
        bool rightHandSeen = false;

        foreach(Hand hand in hands)
        {
            if (hand.IsLeft)
            {
                bool justShowedUp = leftHandTracking.IsVisible == false;
                leftHandTracking.UpdateHandTracking(hand.GrabAngle, leftHandTrans, justShowedUp);
                leftHandTracking.UpdateHandStates(Camera.main.transform, justShowedUp);
                CheckForPerformedGesture(leftHandTracking);
                leftHandSeen = true;
            }

            if (hand.IsRight)
            {
                bool justShowedUp = rightHandTracking.IsVisible == false;
                rightHandTracking.UpdateHandTracking(hand.GrabAngle, rightHandTrans, justShowedUp);
                rightHandTracking.UpdateHandStates(Camera.main.transform, justShowedUp);
                CheckForPerformedGesture(rightHandTracking);
                rightHandSeen = true;
            }
        }

        // Update hands sight status
        leftHandTracking.IsVisible = leftHandSeen;
        rightHandTracking.IsVisible = rightHandSeen;
    }



    private void CheckForPerformedGesture(HandTracking handTracking)
    {
        // Check if new hand state completes a gesture performance
        foreach (HandState handState in handTracking.CurrentStates)
        {
            if (handTracking.IsStateFreshlyTracked(handState))
            {
                foreach (HandGesture handGesture in gestures)
                {
                    if (handGesture.ContainsHandState(handState))
                    {
                        bool gestureCompleted = handGesture.AddTrackedHandState(handState);

                        if (gestureCompleted == true)
                        {
                            Debug.Log("Gesture " + handGesture.GestureType.ToString() + " performed on " + handTracking.HandSide.ToString());
                            if (GesturePerformed != null) GesturePerformed.Invoke(handGesture.GestureType, handTracking);
                        }
                    }
                }
            }
        }

        // Check fist status
        if ((handTracking.CurrentStates.Contains(HandState.Palm_Closed))
            && (handTracking.IsStateFreshlyTracked(HandState.Palm_Closed)))
        {
            if (FistClosed != null) FistClosed.Invoke(true);
        }
        else if ((handTracking.CurrentStates.Contains(HandState.Fist_Not_Closed))
            && (handTracking.IsStateFreshlyTracked(HandState.Fist_Not_Closed)))
        {
            if (FistClosed != null) FistClosed.Invoke(false);
        }


    }
}

