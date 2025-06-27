using UnityEngine;
using System.Collections.Generic;
using Unity.XR.CoreUtils;

public class VRMotionController : MotionController
{
    public static VRMotionController Instance;

    protected XROrigin _xrOrigin;
    protected XROrigin xrOrigin
    {
        get
        {
            if (_xrOrigin == null)
                _xrOrigin = origin.GetComponent<XROrigin>();
            return _xrOrigin;
        }
    }

    protected override Transform originCamera => xrOrigin.Camera.transform;

    protected void Awake()
    {
        Debug.Log(name + " VRMotionController.Awake");

        if (Instance == null)
            Instance = this;
        else
            Debug.LogError("VRMotionController.Instance already exists!");
    }

    protected override void RotateAroundCameraPosition(float angle)
    {
        xrOrigin.RotateAroundCameraPosition(Vector3.up, angle);
    }
}
