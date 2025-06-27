using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchItem
{
    public VRTouchedObject item;         //���������� �������
    public Vector3 touchPoint;      //����� �� ����������� ��������, �� ������� ��� ���������
    public HandTouchMode touchMode; //����� - ������� ��� �����
    public HandSide handSide;
    public float time;              //����� ������� �������������

    new public string ToString()
    {
        return "{item=" + item.name + ", touchMode=" + touchMode + ", handSide=" + handSide + ", time=" + time + "}";
    }
}

public class VRTouchedObjectsController : MonoBehaviour
{
    public const float tapDelay = 1.0f; //����� � ��������, � �������� ������� ������+��������� ��������� ��� tap
    public static VRTouchedObjectsController Instance;

    int lockCount = 0;
    List<TouchItem> touchedObjectsLast = null;
    List<TouchItem> trackedObjectsLast = null;
    List<VRTouchedObject> touchObjectsRegistered;

    public List<VRTouchedObject> RegisteredTouchObjects
    {
        get => touchObjectsRegistered;
    }

    public void Lock()
    {
        lockCount++;
        VRHandTracker.Hide(lockCount > 0);

    }

    public void Unlock()
    {
        lockCount = Mathf.Max(0, lockCount - 1);
        VRHandTracker.Hide(lockCount > 0);
    }

    public bool IsLocked
    {
        get
        {
            return lockCount > 0;
        }
    }

    int GetItemIndex(VRTouchedObject obj, List<TouchItem> list)
    {
        if (list != null && list.Count > 0)
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null && list[i].item == obj)
                    return i;
        return -1;
    }

    public static void Init()
    {
        if (Instance == null)
        {
            var obj = new GameObject("VRTouchedObjectsController");
            Instance = obj.AddComponent<VRTouchedObjectsController>();
        }
        else
            Debug.LogError("VRTouchedObjectsController.Instance already exists!");
    }

    public void RegisterTouchObject(VRTouchedObject touchObject)
    {
        if (touchObjectsRegistered == null)
            touchObjectsRegistered = new List<VRTouchedObject>();
        else if (touchObjectsRegistered.Contains(null))
        {
            List<VRTouchedObject> list = new List<VRTouchedObject>();
            touchObjectsRegistered.ForEach(t => 
            {
                if (t != null)
                    list.Add(t);
            });
            touchObjectsRegistered.Clear();
            touchObjectsRegistered.AddRange(list);
        }

        if (!touchObjectsRegistered.Contains(touchObject))
            touchObjectsRegistered.Add(touchObject);
    }

    public void UnRegisterTouchObject(VRTouchedObject touchObject)
    {
        if (touchObjectsRegistered != null && touchObjectsRegistered.Contains(touchObject))
            touchObjectsRegistered.Remove(touchObject);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        VRHandTracker.onGripPressedEvent += OnGripPressed;
        VRHandTracker.onGripReleasedEvent += OnGripReleased;
    }

    private void OnDestroy()
    {
        VRHandTracker.onGripPressedEvent -= OnGripPressed;
        VRHandTracker.onGripReleasedEvent -= OnGripReleased;

        Debug.LogWarning("!!!! VRTouchedObjectsController destroyed !!!!");
    }

    void OnGripPressed(HandSide handSide)
    {
        if (touchObjectsRegistered != null)
            touchObjectsRegistered.ForEach(t => t.OnGripButtonEvent(handSide, true));
    }

    void OnGripReleased(HandSide handSide)
    {
        if (touchObjectsRegistered != null)
            touchObjectsRegistered.ForEach(t => t.OnGripButtonEvent(handSide, false));
    }

    TouchItem ToTouchItem(HandTouchItem t, VRTouchedObject vrT)
    {
        return new TouchItem
        {
            item = vrT,
            touchPoint = t.touchPoint,
            touchMode = t.touchMode,
            handSide = t.handSide,
            time = t.time,
        };
    }

    void UpdateTrack()
    {
        var trackedItems = VRHandTracker.TrackedItems;

        List<TouchItem> list = new List<TouchItem>();
        if (trackedItems != null && trackedItems.Count > 0)
            foreach (var t in trackedItems)
            {
                var vrTouches = t.item.GetComponents<VRTouchedObject>();
                if (vrTouches != null && vrTouches.Length > 0)
                    foreach (var vrTouch in vrTouches)
                        if (vrTouch.touchModes.Contains(t.touchMode))
                            list.Add(ToTouchItem(t, vrTouch));
            }

        var trackedObjects = list;

        List<TouchItem> objsStartTrack = new List<TouchItem>();
        List<TouchItem> objsStopTrack = new List<TouchItem>();

        //�������� ��������������
        if (trackedObjects != null)
            foreach (var t in trackedObjects)
                if (GetItemIndex(t.item, trackedObjectsLast) < 0)
                    objsStartTrack.Add(t);

        //�������� ���������������
        if (trackedObjectsLast != null)
            foreach (var t in trackedObjectsLast)
                if (GetItemIndex(t.item, trackedObjects) < 0)
                    objsStopTrack.Add(t);

        //������������ ������� � �� ����� ������
        if (objsStopTrack.Count > 0)
            objsStopTrack.ForEach(t =>
            {
                    t.item.OnStopTrack(t.touchPoint, t.handSide);
            });

        if (objsStartTrack.Count > 0)
            objsStartTrack.ForEach(t =>
            {
                t.item.OnStartTrack(t.touchPoint, t.handSide);
            });

        if (trackedObjects != null && trackedObjects.Count > 0)
            trackedObjects.ForEach(t =>
            {
                //��� ���, ��� �� ��� �����
                t.item.OnTrack(t.touchPoint, t.handSide);
            });

        trackedObjectsLast = trackedObjects;
    }

    void UpdateTouch()
    {
        var touchedItems = VRHandTracker.TouchedItems;

        //�������� �� touchedItems ������ List<TouchItem>, ���������� VRTouchObjects � touchMode, ���������������� ������� ������ ������� (��� ��� ����)

        List<TouchItem> list = new List<TouchItem>();
        if (touchedItems != null && touchedItems.Count > 0)
            foreach (var t in touchedItems)
            {
                var vrTouchs = t.item.GetComponents<VRTouchedObject>();
                if (vrTouchs != null)
                    foreach (var vrTouch in vrTouchs)
                        if (vrTouch.touchModes.Contains(t.touchMode))
                            list.Add(ToTouchItem(t, vrTouch));
            }

        var touchedObjects = list;

        List<TouchItem> objsStartTouch = new List<TouchItem>();
        List<TouchItem> objsStopTouch = new List<TouchItem>();

        //�������� ������������
        if (touchedObjects != null && touchedObjects.Count > 0)
            foreach (var t in touchedObjects)
                if (GetItemIndex(t.item, touchedObjectsLast) < 0)
                    objsStartTouch.Add(t);

        //�������� ���������������
        if (touchedObjectsLast != null && touchedObjectsLast.Count > 0)
            foreach (var t in touchedObjectsLast)
                if (GetItemIndex(t.item, touchedObjects) < 0)
                    objsStopTouch.Add(t);

        //������������ ������� � �� ����� ������
        if (objsStopTouch.Count > 0)
            objsStopTouch.ForEach(t =>
            {
                t.item.OnStopTouch(t.touchPoint, t.handSide);
                float dt = Time.realtimeSinceStartup - t.time;
                //Debug.Log(t.item.name + " dt=" + dt);
                if (dt <= tapDelay)
                    t.item.OnTap(t.touchPoint, t.handSide);
            });

        if (objsStartTouch.Count > 0)
            objsStartTouch.ForEach(t =>
            {
                t.item.OnStartTouch(t.touchPoint, t.handSide);
            });

        if (touchedObjects != null && touchedObjects.Count > 0)
            touchedObjects.ForEach(t =>
            {
                //��� ���, ��� �� ��� �����
                t.item.OnTouch(t.touchPoint, t.handSide);
            });

        touchedObjectsLast = touchedObjects;
    }

    void Update()
    {
        if (!IsLocked)
        {
            UpdateTrack();
            UpdateTouch();
        }
    }
}
