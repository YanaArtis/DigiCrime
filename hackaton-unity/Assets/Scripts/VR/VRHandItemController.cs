using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRHandItemController : MonoBehaviour
{
    public Transform itemPlace;
    public VRHandController handController;

    HandSide _handSide = HandSide.Unknown;

    HandSide handSide
    {
        get
        {
            if (_handSide == HandSide.Unknown)
                _handSide = GetComponentInParent<VRHandTracker>().handSide;
            return _handSide;
        }
    }

    public Transform Pointer
    {
        get => null;
    }

    private void Start()
    {
        SetItemsBracerOn(false, true);
        
        VRHandTracker.onGripPressedEvent += OnGripPressedEvent;
        VRHandTracker.onGripReleasedEvent += OnGripReleasedEvent;
    }

    private void OnDestroy()
    {

        VRHandTracker.onGripPressedEvent -= OnGripPressedEvent;
        VRHandTracker.onGripReleasedEvent -= OnGripReleasedEvent;
    }

    void OnGripPressedEvent(HandSide handSide)
    {
        if (handSide == this.handSide)
            SetItemsBracerOn(true);
    }

    void OnGripReleasedEvent(HandSide handSide)
    {
        if (handSide == this.handSide)
            SetItemsBracerOn(false);
    }

    void SetItemsBracerOn(bool isOn, bool anyway = false)
    {

    }

    public void SetHandSide(HandSide handSide)  //!!! ƒÀﬂ Œ“À¿ƒ » !!!
    {
        _handSide = handSide;
    }
}
