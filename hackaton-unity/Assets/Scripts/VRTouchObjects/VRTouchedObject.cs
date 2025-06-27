using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class VRTouchedObject : VRObjectBase
{
    public enum State
    {
        None = 0,
        StartTrack = 1,
        StopTrack = 2,
        StartTouch = 3,
        StopTouch = 4,
        Tap = 5,
    }

    public delegate void OnTrackEvent(VRTouchedObject touchObject);

    public OnTrackEvent onStartTrackEvent;
    public OnTrackEvent onStopTrackEvent;

    public OnTrackEvent onStartTouchEvent;
    public OnTrackEvent onStopTouchEvent;
    public OnTrackEvent onTapEvent;

    public List<HandTouchMode> touchModes = new List<HandTouchMode> { HandTouchMode.Hand, HandTouchMode.Beam };
    public float handTouchRadius = 0.01f;

    public AudioClip onStartTouchAudio;
    public List<AudioClip> onTouchAudio;
    public AudioClip onStopTouchAudio;

    public List<UnityEngine.Events.UnityEvent> onStartTouchEvents;
    public List<UnityEngine.Events.UnityEvent> onTapEvents;
    public List<UnityEngine.Events.UnityEvent> onStopTouchEvents;

    protected HandSide handSide;
    bool isTouched = false;
    bool isTracked = false;
    private XROrigin _xrOrigin;

    public void AddEvent(State state, OnTrackEvent e)
    {
        switch (state)
        {
            case State.StartTrack: onStartTrackEvent += e; break;
            case State.StopTrack: onStopTrackEvent += e; break;
            case State.StartTouch: onStartTouchEvent +=e; break;
            case State.StopTouch: onStopTouchEvent += e; break;
            case State.Tap: onTapEvent += e; break;
        }
    }

    public void RemoveEvent(State state, OnTrackEvent e)
    {
        switch (state)
        {
            case State.StartTrack: onStartTrackEvent -= e; break;
            case State.StopTrack: onStopTrackEvent -= e; break;
            case State.StartTouch: onStartTouchEvent -= e; break;
            case State.StopTouch: onStopTouchEvent -= e; break;
            case State.Tap: onTapEvent -= e; break;
        }
    }

    public XROrigin xrOrigin
    {
        get
        {
            if (_xrOrigin == null)
                _xrOrigin = Camera.main.GetComponentInParent<XROrigin>();

            return _xrOrigin;
        }
    }

    public bool IsTracked
    {
        get
        {
            return isTracked;
        }
    }

    public bool IsTouched
    {
        get
        {
            return isTouched;
        }
    }

    virtual protected void Start()
    {
        if (VRTouchedObjectsController.Instance == null)
            VRTouchedObjectsController.Init();

        VRTouchedObjectsController.Instance.RegisterTouchObject(this);
    }

    virtual protected void OnDestroy()
    {
        if (VRTouchedObjectsController.Instance != null)
            VRTouchedObjectsController.Instance.UnRegisterTouchObject(this);
    }

    virtual public void OnStartTrack(Vector3 touchPoint, HandSide handSide) //наехали лучём без нажатия на кнопку fire
    {
        //Debug.Log(name + " start track " + handSide);

        isTracked = true;
        this.handSide = handSide;

        if (onStartTrackEvent != null)
            onStartTrackEvent.Invoke(this);
    }

    virtual public void OnTrack(Vector3 touchPoint, HandSide handSide) //едем лучём без нажатия
    {
        //Debug.Log(name + " OnTrack " + handSide);
        this.handSide = handSide;
    }

    virtual public void OnStopTrack(Vector3 touchPoint, HandSide handSide) //уехали лучём без нажатия
    {
        //Debug.Log(name + " stop track");

        isTracked = false;
        if (onStopTrackEvent != null)
            onStopTrackEvent.Invoke(this);
    }

    virtual public void OnStartTouch(Vector3 touchPoint, HandSide handSide)
    {
        //Debug.Log(name + " StartTouch handSide=" + handSide + " onStartTouchEvent:" + (onStartTouchEvent != null));

        isTouched = true;
        this.handSide = handSide;

        if (onStartTouchEvent != null)
            onStartTouchEvent.Invoke(this);

        if (onStartTouchEvents != null)
            onStartTouchEvents.ForEach(e => e.Invoke());

        PlayAudio(onStartTouchAudio);
    }

    virtual public void OnTouch(Vector3 touchPoint, HandSide handSide)
    {
        this.handSide = handSide;
        if (onTouchAudio != null && onTouchAudio.Count > 0 && !IsAudioPlaying)
            PlayAudio(onTouchAudio);
    }

    virtual public void OnStopTouch(Vector3 touchPoint, HandSide handSide)
    {
        //Debug.Log(name + " StopTouch handSide=" + handSide + " onStopTouchEvent:" + (onStopTouchEvent != null));

        isTouched = false;
        
        if (onStopTouchEvent != null)
            onStopTouchEvent.Invoke(this);

        if (onStopTouchEvents != null)
            onStopTouchEvents.ForEach(e => e.Invoke());

        PlayAudio(onStopTouchAudio);
    }

    virtual public void OnTap(Vector3 touchPoint, HandSide handSide)
    {
        //Debug.Log(name + " Tap handSide=" + handSide + " onTapEvent:" + (onTapEvent != null));

        this.handSide = handSide;

        if (onTapEvent != null)
            onTapEvent.Invoke(this);

        if (onTapEvents != null)
            onTapEvents.ForEach(e => e.Invoke());
    }

    virtual public void OnGripButtonEvent(HandSide handSide, bool isPressed)
    {
        //Debug.Log(name + " OnGripButtonEvent handSide=" + handSide + " isPressed=" + isPressed);
    }
}