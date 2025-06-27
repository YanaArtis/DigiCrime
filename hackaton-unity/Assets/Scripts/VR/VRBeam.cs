using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VRBeam : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor xrRayInteractor;
    public VRHandTracker handTracker;
    public GameObject skinInactive;
    public GameObject skinActive;
    public GameObject dot;
    public Transform skinContainer;

    float currDist = 1;
    int hideCount = 0;

    public bool IsHided
    {
        get => hideCount > 0;
    }

    public void Hide(bool hide = true)
    {
        hideCount = Mathf.Max(0, hideCount + (hide ? 1 : -1));
        gameObject.SetActive(hideCount <= 0);
        dot.SetActive(hideCount <= 0);
    }

    void SetSkin(bool isActive, bool isVisible)
    {
        if (skinContainer != null)
        {
            if (skinActive != null)
                skinActive.SetActive(isActive && isVisible);

            if (skinInactive != null)
                skinInactive.SetActive(!isActive && isVisible);
        }
    }

    void ScaleSkin(bool isVisible)
    {
        if (skinContainer != null)
        {
            if (isVisible)
            {
                float dist = Vector3.Distance(skinContainer.position, dot.transform.position);
                float scale = dist * .9f;
                skinContainer.localScale = new Vector3(1, 1, scale);
            }

            skinContainer.gameObject.SetActive(isVisible);
        }
    }

    private void Awake()
    {
        if (dot == null)
        {
            dot = new GameObject("dot");
            dot.transform.SetParent(transform);
        }
    }

    private void Update()
    {
        if (handTracker != null)
        {
            var target = handTracker.Target;
            dot.SetActive(target != null && target.touchMode == HandTouchMode.Beam);
            if (target != null)
            {
                dot.transform.position = target.touchPoint;
                currDist = dot.transform.localPosition.magnitude;
            }
            else
            {
                currDist = Mathf.Max(currDist, 1);
                dot.transform.position = transform.position + transform.forward * currDist;
            }

            bool isVisible = true;// handTracker.targetType != TargetType.Hand;
            SetSkin(handTracker.targetType == TargetType.Beam, isVisible);
            ScaleSkin(isVisible);
        }
        else if (xrRayInteractor != null)
        {
            dot.transform.position = xrRayInteractor.rayEndPoint;
            transform.position = xrRayInteractor.rayOriginTransform.position;
            transform.LookAt(dot.transform);
            ScaleSkin(true);
        }
    }
}
