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

    public event System.Action<GestureType> GesturePerformed;

    [SerializeField] private float handOpenThreshold = 0.89f;
    [SerializeField] private float handClosedThreshold = 0.15f;
    [SerializeField] private float forwardVelocityThreshold = 5.0f;
    [SerializeField] private float sidewaysVelocityThreshold = 2.0f;
    [SerializeField] private float forwardAngularThreshold = 0.75f;
    
    [SerializeField] private HandGesture[] gesturesDatabank;

    private HandTrackingThresholds handTrackingThresholds;

    private Transform rightHandTrans;
    private Transform leftHandTrans;
    private List<HandGesture> gestures;

    private HandTracking leftHand;
    private HandTracking rightHand;


    private Controller controller;
   
    void Start ()
    {
        // Look for hand references in the scene
        bool foundLeftHand  = false;
        bool foundRightHand = false;
        
        foreach(HandReference handReference in Resources.FindObjectsOfTypeAll<HandReference>())
        {
            if (handReference.HandSide == HandSide.Left)
            {
                leftHandTrans = handReference.transform;
                foundLeftHand = true;
            }
            else if (handReference.HandSide == HandSide.Right)
            {
                rightHandTrans = handReference.transform;
                foundRightHand = true;
            }
        }
        if (foundRightHand == false)
        {
            Debug.LogError("Right hand reference not found in the scene!");
        }
        if (foundLeftHand == false)
        {
            Debug.LogError("Left hand reference not found in the scene!");
        }

        controller  = new Controller();

        // Set up thresholds struct
        handTrackingThresholds = new HandTrackingThresholds();
        handTrackingThresholds.ForwardAngularThreshold      = forwardAngularThreshold;
        handTrackingThresholds.ForwardVelocityThreshold     = forwardVelocityThreshold;
        handTrackingThresholds.SidewaysVelocityThreshold    = sidewaysVelocityThreshold;
        handTrackingThresholds.HandOpenThreshold            = handOpenThreshold;
        handTrackingThresholds.HandClosedThreshold          = handClosedThreshold;

        // Create hand instances
        leftHand    = new HandTracking(leftHandTrans,   handTrackingThresholds);
        rightHand   = new HandTracking(rightHandTrans,  handTrackingThresholds);

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
                bool justShowedUp = leftHand.IsVisible == false;
                leftHand.UpdateHandTracking(hand.GrabAngle, justShowedUp);
                leftHand.UpdateHandStates(Camera.main.transform, justShowedUp);
                leftHandSeen = true;
            }

            if (hand.IsRight)
            {
                bool justShowedUp = rightHand.IsVisible == false;
                rightHand.UpdateHandTracking(hand.GrabAngle, justShowedUp);
                rightHand.UpdateHandStates(Camera.main.transform, justShowedUp);
                rightHandSeen = true;

                foreach (HandState handState in rightHand.CurrentStates)
                {
                    if (rightHand.IsStateFreshlyTracked(handState))
                    {
                        foreach(HandGesture handGesture in gestures)
                        {
                            if (handGesture.ContainsHandState(handState))
                            {
                                bool gestureCompleted = handGesture.AddTrackedHandState(handState);

                                if (gestureCompleted == true)
                                {
                                    Debug.Log("Gesture " + handGesture.GestureType.ToString() + " performed");

                                }
                            }
                        }
                    }
                }
       



                DrawArrow.ForDebug(rightHand.Position, rightHand.Direction);
                DrawArrow.ForDebug(rightHand.Position, rightHand.VelocityDirection);
            }
        }

        // Update hands sight status
        leftHand.IsVisible = leftHandSeen;
        rightHand.IsVisible = rightHandSeen;
    }
}




//Vector palmPositionInLMSpace = Leap.Unity.UnityMatrixExtension.GetLeapMatrix(handRight).TransformPoint(hand.PalmPosition);
//palmPosition = Leap.Unity.UnityVectorExtension.ToVector3(palmPositionInLMSpace);