using UnityEngine;
using System.Collections.Generic;

public class VRTouchObjectDistanced : VRTouchedObject
{
    public GameObject indicatorFocusOn;
    public GameObject indicatorFocusOff;
    public List<VRHandTracker.Button> activationButtons = new List<VRHandTracker.Button> { VRHandTracker.Button.Fire, VRHandTracker.Button.Grip };

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
        if (IsTracked && handSide == this.handSide && activationButtons != null && activationButtons.Contains(button) && value > 0.5f)
        {
            Debug.Log(name + " OnButtonEvent: handSide=" + handSide + " button=" + button + " value=" + value);

            if (onTapEvents != null)
                foreach (var e in onTapEvents)
                    if (e != null)
                        e.Invoke();
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
}
