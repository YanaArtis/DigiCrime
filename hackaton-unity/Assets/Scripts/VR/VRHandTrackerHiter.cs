using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VRHitTarget
{
    public Vector3 origin;          //положение источника луча
    public Vector3 originShift;     //смещение источника луча относительно исходного
    public HandTouchMode touchMode;
    public Vector3 point;           //положение точки, куда ткнулся луч
    public Vector3 normal;          //нормаль плоскости, куда ткнулся луч
    public Transform transform;     //объект, в который ткнули

    public float Distance
    {
        get => (origin - point).magnitude;
    }

    public void Clear()
    {
        origin = point = Vector3.zero;
        normal = Vector3.zero;
        transform = null;
    }
}

public class VRHandTrackerHiter
{
    const float dirMaxTime = 0.1f;      //время, в течение которого хранится буфер dirs
    const float distMin = 0.005f;

    VRHitTarget state; 
    VRHandTracker handTracker;

    Vector3 lastXrRayInteractorOriginPosition;
    List<Vector3> dirs;

    Transform pointerOrigin
    {
        get => handTracker != null ? handTracker.handHitOrigin: null;
    }

    Transform pointerTarget
    {
        get => handTracker != null ? handTracker.handHitTarget: null;
    }

    XRRayInteractor xrRayInteractor
    {
        get => handTracker != null ? handTracker.XrRayInteractor : null;
    }

    public VRHitTarget HitState
    {
        get => state;
    }

    public VRHandTrackerHiter(VRHandTracker handTracker)
    {
        this.handTracker = handTracker;
        dirs = new List<Vector3>();
        lastXrRayInteractorOriginPosition = xrRayInteractor.rayOriginTransform.position;

        if (xrRayInteractor == null)
            Debug.LogError("xrRayInteractor is null");
    }

    VRHitTarget GetHit(Vector3 origin, Vector3 dir)
    {
        Ray ray = new Ray(origin, dir);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);
        return new VRHitTarget
        {
            origin = origin,
            point = hit.point,
            transform = hit.collider != null ? hit.collider.transform : null,
            normal = hit.normal
        };
    }

    VRHitTarget GetHit()
    {
        VRHitTarget res = null;
        return res != null ? res : GetBeamHit();
    }

    VRHitTarget GetBeamHit()
    {
        var touchMode = VRHandTracker.IsGripPressed(handTracker.handSide) ? HandTouchMode.Hand : HandTouchMode.Beam;
        var res = GetCollidedHit(handTracker.GetPointerPosition(touchMode), xrRayInteractor.rayOriginTransform.forward, state);
        if (res != null)
            res.touchMode = touchMode;
        return res;
    }

    VRHitTarget GetCollidedHit(Vector3 origin, Vector3 dir, VRHitTarget previous, float maxDist = -1)
    {
        //возвращает предмет, в который ткнули или в который должны были ткнуть, но проскочили

        var hit = GetHit(origin, dir);

        if (maxDist > 0 && hit.Distance > maxDist)
            return null;

        if (hit.transform != null)
        {
            //если ткнули в предмет
            if (hit.Distance > distMin)
                hit.normal = Vector3.zero;
            else
            {
                hit.origin = hit.point - dir.normalized * distMin;
                hit.originShift = hit.origin - origin;
            }

            return hit;
        }
        else if (previous != null)
        {
            //если ткнули мимо, но до этого тыкали в предмет и возможно рука проскочила через коллайдер

            Vector3 dv = Vector3.Project(previous.origin - origin, dir);
            hit = GetHit(origin + dv * 1.5f, dir);

            if (hit.transform != null)
            {
                //если рука проскочила через коллайдер
                hit.origin = hit.point - dir.normalized * distMin;
                hit.originShift = hit.origin - origin;
                return hit;
            }
        }

        return null;
    }

    void UpdateDirs()
    {
        dirs.Add(xrRayInteractor.rayOriginTransform.position - lastXrRayInteractorOriginPosition);
        int maxCount = Mathf.RoundToInt(dirMaxTime / Time.deltaTime);
        if (dirs.Count > maxCount)
            dirs.RemoveRange(0, dirs.Count - maxCount);

        lastXrRayInteractorOriginPosition = xrRayInteractor.rayOriginTransform.position;
    }

    void SetColor(Transform transform, Color color)
    {
        if (transform != null)
            transform.gameObject.GetComponent<MeshRenderer>().material.color = color;
    }

    public void Update()
    {
        UpdateDirs();

        if (state == null || state.normal == Vector3.zero)
        {
            //если до этого рука была далеко от предмета
            state = GetHit();

            if (state != null && state.Distance > distMin)
                state.normal = Vector3.zero;

            SetColor(pointerOrigin, Color.green);
        }
        else
        {
            //если рука уже была близко от предмета

            var touchMode = state.touchMode;
            Vector3 origin = handTracker.GetPointerPosition(touchMode);
            Vector3 dir = state.point - state.origin;

            state = GetCollidedHit(origin, dir, state);
            SetColor(pointerOrigin, Color.cyan);

            if (state != null)
            {
                state.touchMode = touchMode;
                if (state.transform == null || state.Distance > distMin)
                    state.normal = Vector3.zero;
            }
        }

        if (state != null)
        {
            if (pointerOrigin != null)
                pointerOrigin.position = state.origin;

            if (pointerTarget != null)
                pointerTarget.position = state.point;
        }
        else
        {
            if (pointerOrigin != null)
                pointerOrigin.localPosition = Vector3.zero;

            if (pointerTarget != null)
                pointerTarget.localPosition = Vector3.zero;
        }
    }
}
