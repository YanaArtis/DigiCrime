using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Unity.XR.CoreUtils;

public class VRHeadTracker : MonoBehaviour
{
    const float minSafeZoneSize = 1.5f; //минимальный размер одного измерения зоны безопасности, ниже которой зона считается Stationary
    public const iTween.EaseType turnEaseType = iTween.EaseType.linear;

    public static VRHeadTracker Instance;

    public delegate void OnHeadTrackedEvent();
    public static OnHeadTrackedEvent onHeadTrackedEvent;

    public delegate void OnSafeZoneChangedEvent(List<Vector3> _);
    public static OnSafeZoneChangedEvent onSafeZoneChanged;

    static List<Vector3> safeZone;
    static SafeZoneMode safeZoneMode;

    XROrigin _xrOrigin;
    XRInputSubsystem inputSubsystem;

    public static SafeZoneMode SafeZoneMode
    {
        get => SafeZoneMode.Stationary;
    }

    public static SafeZoneMode SafeZoneModeHardware
    {
        get => safeZoneMode;
    }

    public XROrigin xrOrigin
    {
        get
        {
            if (_xrOrigin == null)
                _xrOrigin = GetComponentInParent<XROrigin>();
            return _xrOrigin;
        }
    }

    public List<Vector3> SafeZone
    {
        get
        {
            if (safeZone != null && safeZone.Count > 3)
            {
                Transform parent = xrOrigin.transform;

                List<Vector3> res = new List<Vector3>();
                for (int i = 0; i < safeZone.Count; i++)
                {
                    Vector3 point = parent.TransformPoint(safeZone[i]);
                    point.y = 0;
                    res.Add(point);
                }

                return res;
            }

            return null;
        }
    }

    XRIDefaultInputActions controls;
    bool isHeadTracked = false;

    public static bool IsHeadTracked
    {
        get => Instance != null ? Instance.isHeadTracked : false;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        controls = new XRIDefaultInputActions();
        controls.Enable();
        controls.XRIHead.IsTracked.performed += OnHeadTracked;
    }

    private void OnDestroy()
    {
        controls.XRIHead.IsTracked.performed -= OnHeadTracked;
        if (inputSubsystem != null)
            inputSubsystem.boundaryChanged -= OnSafeZoneChanged;
    }


    void OnHeadTracked(InputAction.CallbackContext data)
    {
        if (!isHeadTracked)
        {
            isHeadTracked = true;
            InitSubsystem();

            if (onHeadTrackedEvent != null)
                onHeadTrackedEvent.Invoke();
        }
    }

    void InitSubsystem()
    {
        if (inputSubsystem == null)
        {
            try
            {
                var loader = XRGeneralSettings.Instance?.Manager?.activeLoader;
                if (loader != null)
                {
                    inputSubsystem = loader.GetLoadedSubsystem<XRInputSubsystem>();
                    if (inputSubsystem != null)
                    {
                        inputSubsystem.Start();
                        inputSubsystem.boundaryChanged += OnSafeZoneChanged;
                        ResetSafeZone();
                    }
                }
            }
            catch (System.Exception e)
            {
            }
        }
    }

    void OnSafeZoneChanged(XRInputSubsystem inputSubsystem)
    {
            ResetSafeZone(inputSubsystem);

            if (onSafeZoneChanged != null)
                onSafeZoneChanged.Invoke(safeZone);
    }

    public static void ResetSafeZone()
    {
        if (Instance != null)
            Instance.ResetSafeZone(Instance.inputSubsystem);
    }

    void ResetSafeZone(XRInputSubsystem inputSubsystem)
    {
        //получаем актуальные данные для SafeZone

        if (inputSubsystem != null)
        {
            List<Vector3> points = new List<Vector3>();
            if (inputSubsystem.TryGetBoundaryPoints(points))
            {
                var range = new Range(points, Vector3.zero);
                if (range.Width > minSafeZoneSize && range.Height > minSafeZoneSize)
                {
                    safeZoneMode = SafeZoneMode.RoomScale;
                    safeZone = points;
                }
                else
                {
                    safeZoneMode = SafeZoneMode.Stationary;
                    safeZone = null;
                }
            }
            else
            {
                safeZoneMode = SafeZoneMode.Stationary;
                safeZone = null;
            }
        }
        else
        {
            safeZoneMode = SafeZoneMode.Stationary;
            safeZone = null;
        }
    }

    public void SetHeightRate(float heightRate)
    {
    }

    public void SetHeight(float height)
    {
        Debug.Log("VRHeadTracker.SetHeight height=" + height);
        xrOrigin.CameraFloorOffsetObject.transform.localPosition = height * Vector3.up;
    }
}
