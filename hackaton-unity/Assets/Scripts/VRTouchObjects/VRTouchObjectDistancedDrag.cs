using UnityEngine;
using System.Collections.Generic;

public class VRTouchObjectDistancedDrag : VRTouchedObject
{
    public Vector3 lineMoveNormal = Vector3.zero;
    public GameObject indicatorFocusOn;
    public GameObject indicatorFocusOff;
    public List<VRHandTracker.Button> activationButtons = new List<VRHandTracker.Button> { VRHandTracker.Button.Fire, VRHandTracker.Button.Grip };

    public OnTrackEvent onReleaseEvent;

    void SetIndicatorsOn(bool isOn)
    {
        if (indicatorFocusOn != null)
            indicatorFocusOn.SetActive(isOn);

        if (indicatorFocusOff != null)
            indicatorFocusOff.SetActive(!isOn);
    }

    protected override void Start()
    {
        base.Start();

        VRHandTracker.onButtonPressedEvent += OnButtonEvent;
    }

    void OnButtonEvent(HandSide handSide, VRHandTracker.Button button, float value)
    {
        if (!IsLocked && IsTracked && handSide == this.handSide && activationButtons != null && activationButtons.Contains(button) && value > 0.5f)
        {
            Debug.Log(name + " OnButtonEvent: handSide=" + handSide + " button=" + button + " value=" + value);

            if (onTapEvents != null)
                foreach (var e in onTapEvents)
                    if (e != null)
                        e.Invoke();

            if (!isDrag)
                StartDrag(handSide);
        }
    }

    public override void OnStartTrack(Vector3 touchPoint, HandSide handSide)
    {
        base.OnStartTrack(touchPoint, handSide);
        SetIndicatorsOn(true);
        VRHandTracker.RequestShowBeam(true, this.handSide);
    }

    public override void OnStopTrack(Vector3 touchPoint, HandSide handSide)
    {
        VRHandTracker.RequestShowBeam(false, this.handSide);
        SetIndicatorsOn(false);
        base.OnStopTrack(touchPoint, handSide);
    }

    bool isDrag = false;
    HandSide handSideDrag;

    Vector3 GetHandTrackPos(HandSide handSide)
    {
        var hand = VRHandTracker.GetHandTracker(handSide);
        float dist = (transform.position - hand.transform.position).magnitude;
        return hand.transform.forward * dist;
    }

    void StartDrag(HandSide handSide)
    {
        isDrag = true;
        lastHandPos = GetHandTrackPos(handSide);
        handSideDrag = handSide;

        Debug.Log("StartDrag: " + handSideDrag);
    }

    public override void OnStartTouch(Vector3 touchPoint, HandSide handSide)
    {
        base.OnStartTouch(touchPoint, handSide);
    }

    public override void OnTouch(Vector3 touchPoint, HandSide handSide)
    {
    }

    public override void OnStopTouch(Vector3 touchPoint, HandSide handSide)
    {
    }

    Vector3 lastHandPos;

    private void Update()
    {
        if (isDrag && !IsLocked)
        {
            if (VRHandTracker.IsFirePressed(handSideDrag) || VRHandTracker.IsGripPressed(handSideDrag))
            {
                var handPos = GetHandTrackPos(handSideDrag);
                var dv = handPos - lastHandPos;
                var dvMove = Vector3.Project(dv, lineMoveNormal);

                transform.position += dvMove;
                lastHandPos = handPos;
            }
            else
            {
                isDrag = false;
                if (onReleaseEvent != null)
                    onReleaseEvent.Invoke(this);
            }
        }
    }
}
