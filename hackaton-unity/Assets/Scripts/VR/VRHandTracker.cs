using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public enum SafeZoneMode
{
    Unknown = 0,
    Stationary = 1,
    RoomScale = 2,
}

public enum MoveMode
{
    Free = 0,   //свободное перемещение джойстиком
    Smooth = 1, //мягкий перелёт до точки назначения
    Sharp = 2,	//телепорт через затемнение до точки назначения
}

public enum TurnMode
{
    Free = 0,   //свободное перемещение джойстиком
    Smooth = 1, //мягкий поворот на фиксированное число градусов
    Sharp = 2,  //поворот через затемнение на фиксированное число градусов
}

public enum HandSide
{
    Unknown = 0,
    Left = 1,
    Right = 2
}

public enum HandTouchMode
{
    Beam = 0,
    Hand = 1,
    Unknown = 99,
}

public class HandTouchItem
{
    public GameObject item;         //затронутый предмет
    public Vector3 touchPoint;      //точка на поверхности предмета, за которую его затронули
    public HandTouchMode touchMode; //режим - пальцем или лучём
    public HandSide handSide;
    public float time;              //время первого прикосновения

    new public string ToString()
    {
        return "{item=" + item.name + ", touchMode=" + touchMode + ", handSide=" + handSide + ", time=" + time + "}";
    }
}

public enum TargetType
{
    None = 0,
    Beam = 1,
    Hand = 2,
}

public class HitTarget
{
    public GameObject gameObject;
    public Vector3 point;
    public TargetType targetType;

    new public string ToString()
    {
        return "{gameObject=" + (gameObject != null ? gameObject.name : "null") + ", point=" + point.ToString() + ", targetType=" + targetType + "}";
    }
}

public class VRHandTracker : MonoBehaviour
{
    public enum Button
    {
        None = 0,
        Fire = 1,
        Grip = 2,
        JoystickX = 3,
        JoystickY = 4,
        JoystickButton = 5,
        Menu = 6,
    }

    const float rateFireMin = 0.3f;
    const float rateGripMin = 0.3f;
    const float rateJoystickMin = 0.5f;

    public delegate void OnButtonPressedEvent(HandSide handSide, Button button, float value);
    public static OnButtonPressedEvent onButtonPressedEvent;

    public delegate void OnJoystickTiltEvent(HandSide handSide, Vector2 value);
    public static OnJoystickTiltEvent onJoystickTiltEvent;

    public delegate void OnGripEvent(HandSide handSide);
    public static OnGripEvent onGripPressedEvent;
    public static OnGripEvent onGripReleasedEvent;

    public const float fingerTouchRadius = 0.01f; //радиус в метрах, когда засчитывается прикосновение пальцем к предмету
    public static List<VRHandTracker> handTrackers = null;

    public HandSide handSide;
    public VRHandController handController;
    public VRHandItemController handItemController;
    public Transform pointerBeam;   //кончик указательного пальца - указатель для луча
    public Transform pointerPalm;   //середина ладони - указатель для луча
    public VRBeam beam;

    public Transform handHitOrigin;
    public Transform handHitTarget;

    public List<HandTouchItem> trackedItems = new List<HandTouchItem>();    //по которым скользнули лучём без нажатия
    public List<HandTouchItem> touchedItems = new List<HandTouchItem>();    //по которым скользнули лучём с нажатием

    VRHandTrackerHiter handHiter;

    static int hideCount = 0;

    XRRayInteractor xrRayInteractor;
    XRIDefaultInputActions controls;
    float rateFire = 0;
    float rateGrip = 0;
    bool isFirePressed = false;
    bool isGripPressed = false;
    int lockCount = 0;
    TargetType _targetType;
    HandTouchItem target;
    TargetType targetTypeLast = TargetType.None;
    float rateJoystickX = 0;
    float rateJoystickY = 0;
    IEnumerator ieRotation;

    public const HandSide HandSideMove = HandSide.Left;     //рука, при отклонении джойстика которой перемещаемся в стороны в режиме !MoveTeleport

    bool isActiveSwitchLocked = false;  //переключение визуальных состояние Active/Default заблокировано

    public XRRayInteractor XrRayInteractor
    {
        get => xrRayInteractor;
    }

    public static bool IsHandMenuOn
    {
        get
        {
            var hand = GetHandTracker(HandSide.Left);
            if (hand != null && hand.IsHandMenu)
                return true;

            hand = GetHandTracker(HandSide.Right);
            if (hand != null && hand.IsHandMenu)
                return true;

            return false;
        }
    }

    public bool IsHandMenu
    {
        get => false;
    }

    public Transform PointerHand    //актуальный pointer, соответствующий состоянию руки
    {
        get
        {
            if (isGripPressed)
                return PointerFinger;
            return PointerPalm;
        }
    }

    public Transform PointerFinger
    {
        get => handItemController.Pointer != null ? handItemController.Pointer : pointerBeam;
    }

    public Transform PointerPalm
    {
        get => pointerPalm;
    }

    public Transform PointerBeam
    {
        get => pointerBeam;
    }

    public bool IsLocked
    {
        get
        {
            return lockCount > 0;
        }
    }

    public HandTouchItem Target
    {
        get
        {
            return target;
        }
    }

    public TargetType targetType
    {
        get
        {
            return _targetType;
        }
    }

    public static List<HandTouchItem> TrackedItems
    {
        get
        {
            if (handTrackers != null && handTrackers.Count > 0)
            {
                List<HandTouchItem> res = new List<HandTouchItem>();
                handTrackers.ForEach(t => res.AddRange(t.trackedItems));
                return res;
            }

            return null;
        }
    }

    public static List<HandTouchItem> TouchedItems
    {
        get
        {
            if (handTrackers != null && handTrackers.Count > 0)
            {
                List<HandTouchItem> res = new List<HandTouchItem>();
                handTrackers.ForEach(t => res.AddRange(t.touchedItems));
                return res;
            }

            return null;
        }
    }

    public static bool IsGripPressed(HandSide handSide)
    {
        if (handTrackers != null)
            foreach (var hand in handTrackers)
                if (hand.handSide == handSide)
                    return hand.isGripPressed;
        return false;
    }

    public static bool IsFirePressed(HandSide handSide)
    {
        if (handTrackers != null)
            foreach (var hand in handTrackers)
            if (hand.handSide == handSide)
                return hand.isFirePressed;
        return false;
    }

    public static VRHandTracker GetHandTracker(HandSide handSide)
    {
        if (handTrackers != null)
            foreach (var hand in handTrackers)
                if (hand.handSide == handSide)
                    return hand;
        return null;
    }

    public static void Hide(bool hide = true)
    {
        hideCount = Mathf.Max(0, hideCount + (hide ? 1 : -1));

        if (handTrackers != null)
            handTrackers.ForEach(t => 
            {
                t.Lock(hide);
                t.gameObject.SetActive(hideCount == 0); 
            });
        else
            Debug.LogError("no handTrackers available");

        Debug.Log("Hide: count=" + hideCount);
    }

    public static void UnHide()
    {
        Hide(false);
    }

    public static void LockAll(bool hide = true)
    {
        hideCount = Mathf.Max(0, hideCount + (hide ? 1 : -1));

        if (handTrackers != null)
            handTrackers.ForEach(t => t.Lock(hide));
        else
            Debug.LogError("no handTrackers available");

        //Debug.Log("Hide: count=" + hideCount);
    }

    public static void UnLockAll()
    {
        LockAll(false);
    }

    public void Lock(bool isLocked = true)
    {
        lockCount = Mathf.Max(0, lockCount + (isLocked ? 1 : -1));
    }

    public void UnLock()
    {
        Lock(false);
    }

    public bool IsActiveSwitchLocked
    {
        get => isActiveSwitchLocked;
    }

    public void LockHandActive(bool isLocked = true)
    {
        isActiveSwitchLocked = isLocked;
    }

    public void UnlockHandActive()
    {
        LockHandActive(false);
    }

    public bool IsTracking
    {
        get
        {
            if (controls != null)
            {
                switch (handSide)
                {
                    case HandSide.Left: return controls.XRILeftHand.IsTracked.phase == InputActionPhase.Performed;
                    case HandSide.Right: return controls.XRIRightHand.IsTracked.phase == InputActionPhase.Performed;
                }
            }
            return false;
        }
    }    

    public void SetHandActive(bool isActive)
    {
        handController.SetActive(isActive);
    }

    InputAction IsHandTracked
    {
        get
        {
            switch (handSide)
            {
                case HandSide.Right: return controls.XRIRightHand.IsTracked;
                case HandSide.Left: return controls.XRILeftHand.IsTracked;
            }
            return null;
        }
    }

    InputAction ActivateValue
    {
        get
        {
            switch(handSide)
            {
                case HandSide.Right: return controls.XRIRightHandInteraction.ActivateValue;
                case HandSide.Left: return controls.XRILeftHandInteraction.ActivateValue;
            }
            return null;
        }
    }

    InputAction SelectValue
    {
        get
        {
            switch (handSide)
            {
                case HandSide.Right: return controls.XRIRightHandInteraction.SelectValue;
                case HandSide.Left: return controls.XRILeftHandInteraction.SelectValue;
            }
            return null;
        }
    }

    InputAction ScaleToggle
    {
        get
        {
            switch (handSide)
            {
                case HandSide.Right: return controls.XRIRightHandInteraction.ScaleToggle;
                case HandSide.Left: return controls.XRILeftHandInteraction.ScaleToggle;
            }
            return null;
        }
    }

    InputAction ScaleDelta
    {
        get
        {
            switch (handSide)
            {
                case HandSide.Right: return controls.XRIRightHandInteraction.ScaleDelta;
                case HandSide.Left: return controls.XRILeftHandInteraction.ScaleDelta;
            }
            return null;
        }
    }

    InputAction TurnAction
    {
        get
        {
            switch (handSide)
            {
                case HandSide.Right: return controls.XRIRightHandLocomotion.Turn;
                case HandSide.Left: return controls.XRILeftHandLocomotion.Turn;
            }
            return null;
        }
    }

    InputAction Tilt
    {
        get
        {
            switch (handSide)
            {
                case HandSide.Right: return controls.XRIRightHandLocomotion.Move;
                case HandSide.Left: return controls.XRILeftHandLocomotion.Move;
            }
            return null;
        }
    }

    InputAction ButtonPrimary
    {
        get
        {
            switch (handSide)
            {
                case HandSide.Right: return controls.XRIRightHand.PrimaryButton;
                case HandSide.Left: return controls.XRILeftHand.PrimaryButton;
            }
            return null;
        }
    }

    InputAction ButtonSecondary
    {
        get
        {
            switch (handSide)
            {
                case HandSide.Right: return controls.XRIRightHand.SecondaryButton;
                case HandSide.Left: return controls.XRILeftHand.SecondaryButton;
            }
            return null;
        }
    }

    private void Awake()
    {
        if (handTrackers == null)
            handTrackers = new List<VRHandTracker>();
        handTrackers.Add(this);

        xrRayInteractor = GetComponentInParent<XRRayInteractor>();
        handHiter = new VRHandTrackerHiter(this);
    }

    void Start()
    {
        controls = new XRIDefaultInputActions();
        controls.Enable();

        IsHandTracked.performed += OnHandTracked;
        ActivateValue.performed += OnTriggerButton;    //кнопка "выстрел"
        SelectValue.performed += OnGripButton;         //кнопка "захват" на боку
        ScaleToggle.performed += OnJoystickButton;
        ButtonPrimary.performed += OnButtonPrimary;
        ButtonSecondary.performed += OnButtonSecondary;

        if (handSide == HandSide.Left)
            controls.XRILeftHandController.MenuButton.performed += OnMenuButton;
    }

    private void OnDisable()
    {
        isFirePressed = false;
        handController.SetIdle();
        touchedItems.Clear();
        trackedItems.Clear();
    }

    private void OnDestroy()
    {
        if (handTrackers.Contains(this))
            handTrackers.Remove(this);

        if (ieRotation != null)
            StopCoroutine(ieRotation);

        IsHandTracked.performed -= OnHandTracked;
        ActivateValue.performed -= OnTriggerButton;
        SelectValue.performed -= OnGripButton;
        ScaleToggle.performed -= OnJoystickButton;
        ButtonPrimary.performed -= OnButtonPrimary;
        ButtonSecondary.performed -= OnButtonSecondary;

        if (handSide == HandSide.Left)
            controls.XRILeftHandController.MenuButton.performed -= OnMenuButton;
    }

    public void ClearItem(string emptyHandTrigger)
    {
    }

    public Vector3 GetPointerPosition(HandTouchMode touchMode)
    {
        if (touchMode == HandTouchMode.Hand)
            return PointerFinger.position;

        return beam.transform.position;
    }

    public HitTarget HitObject(Vector3 pointFrom, Vector3 dir, float maxDist = -1, float radius = 0)
    {
        Ray touchBeam = new Ray(pointFrom, dir);
        RaycastHit hit;

        if (radius > 0)
        {
            if (maxDist > 0)
                Physics.SphereCast(touchBeam, radius, out hit, maxDist);
            else
                Physics.SphereCast(touchBeam, radius, out hit);
        }
        else
        {
            if (maxDist > 0)
                Physics.Raycast(touchBeam, out hit, maxDist);
            else
                Physics.Raycast(touchBeam, out hit);
        }

        if (hit.collider != null)
            return new HitTarget { gameObject = hit.collider.gameObject, point = hit.point };

        return null;
    }

    List<HandTouchItem> MergeItems(List<HandTouchItem> items, List<HandTouchItem> itemsLast)
    {
        List<HandTouchItem> newItems = new List<HandTouchItem>();

        if (items != null)
            foreach (var item in items)
            {
                var hasItem = itemsLast.Find(i => i.item == item.item);
                if (hasItem != null)
                {
                    hasItem.touchMode = item.touchMode;
                    hasItem.touchPoint = item.touchPoint;
                    hasItem.handSide = handSide;
                    newItems.Add(hasItem);
                }
                else
                    newItems.Add(item);
            }

        return newItems;
    }

    void UpdateTrackedItems(List<HandTouchItem> items)
    {
        List<HandTouchItem> newItems = MergeItems(items, trackedItems);
        trackedItems.Clear();
        trackedItems.AddRange(newItems);
    }

    void UpdateTouchedItems(List<HandTouchItem> items)
    {
        List<HandTouchItem> newItems = MergeItems(items, touchedItems);
        touchedItems.Clear();
        touchedItems.AddRange(newItems);
    }

    void OnHandTracked(InputAction.CallbackContext data)
    {
        //Debug.Log(handSide + " OnHandTracked: data=" + data);
    }

    static (float height, bool stored) heightRateStored;

    void OnButtonPrimary(InputAction.CallbackContext _)
    {
    }

    void OnButtonSecondary(InputAction.CallbackContext data)
    {
    }

    public static void OpenHandMenu(HandSide handSide)
    {
        if (handSide != HandSide.Unknown)
            GetHandTracker(handSide).OnMenuButton();
        else
            Debug.LogError("handSide == HandSide.Unknown");
    }

    public static void CloseHandMenu()
    {
    }

    void OnMenuButton(InputAction.CallbackContext _)
    {
        if (onButtonPressedEvent != null)
            onButtonPressedEvent.Invoke(handSide, Button.Menu, 0);
        OnMenuButton();
    }

    void OnMenuButton()
    {
    }

    void OnTriggerButton(InputAction.CallbackContext data)
    {
        //Debug.Log(data.ToString());
        var rate = data.ReadValue<float>();
        OnTriggerButton(rate);
    }

    //кнопка "Fire"
    void OnTriggerButton(float rate)
    {
        if (rate < rateFireMin)
            rate = 0;

        if (rate > rateFire)
        {
            if (!isFirePressed)
            {
                //нажали
                //Debug.Log("Fire is pressed");

                isFirePressed = true;
            }
        }
        else
        {
            if (isFirePressed)
            {
                //отпустили
                //Debug.Log("Fire is released");

                isFirePressed = false;
                touchedItems.Clear();
            }
        }

        rateFire = rate;

        if (rate > 0 && onButtonPressedEvent != null)
            onButtonPressedEvent.Invoke(handSide, Button.Fire, rate);
    }

    void OnGripButton(InputAction.CallbackContext data)
    {
        var rate = data.ReadValue<float>();
        OnGripButton(rate);
    }

    //кнопка "Grip"
    void OnGripButton(float rate)
    {
        if (rate < rateGripMin)
            rate = 0;

        if (rate > rateGrip)
        {
            //нажали
            if (!isGripPressed)
            {
                //Debug.Log("Grip press " + rate);

                isGripPressed = true;

                if (onGripPressedEvent != null)
                    onGripPressedEvent.Invoke(handSide);

                handController.SetPointer();
            }

            //isGripPressed = true;
        }
        else
        {
            //отпустили
            if (isGripPressed)
            {
                //Debug.Log("Grip release " + rate);

                handController.SetIdle();

                if (onGripReleasedEvent != null)
                    onGripReleasedEvent.Invoke(handSide);
            }

            isGripPressed = false;
        }

        rateGrip = rate;

        if (rate > 0 && onButtonPressedEvent != null)
                onButtonPressedEvent.Invoke(handSide, Button.Grip, rate);
    }

    void OnJoystickButton(InputAction.CallbackContext data)
    {
        var rate = data.ReadValue<float>();

        if (onButtonPressedEvent != null)
            onButtonPressedEvent.Invoke(handSide, Button.JoystickButton, rate);
    }

    public static MoveMode MoveMode => MoveMode.Free;
    public static TurnMode TurnMode => TurnMode.Free;

    void OnJoystickX(float dir)
    {
        var rate = Mathf.Abs(dir) > rateJoystickMin ? Mathf.Sign(dir) : 0;

        bool isEvent = rate != rateJoystickX;
        rateJoystickX = rate;

        if (VRHeadTracker.SafeZoneMode != SafeZoneMode.RoomScale && !IsHandMenu && !IsLocked)
        {
            //поворачиваем камеру

            if (TurnMode == TurnMode.Free)
            {
                if (handSide != HandSideMove || MoveMode != MoveMode.Free)
                {
                    //если это рука не для управления движением
                    //крутим камеру плавно кадр-за-кадром

                    VRMotionController.Instance.Turn(dir);
                }
            }
            else
            {
                //в остальных случаях крутим камеру на определённый градус

                if (Mathf.Abs(rateJoystickX) > 0)
                {
                    LockAll();
                    VRMotionController.Instance.Turn(rateJoystickX > 0, OnTurnComplete);
                }
            }
        }

        if (onButtonPressedEvent != null && isEvent)
            onButtonPressedEvent.Invoke(handSide, Button.JoystickX, rateJoystickX);
    }

    void OnTurnComplete()
    {
        UnLockAll();
    }

    void OnJoystickY(float rate)
    {
        //переключаем предметы
        rate = Mathf.Abs(rate) > rateJoystickMin ? Mathf.Sign(rate) : 0;

        bool isEvent = rate != rateJoystickY;
        rateJoystickY = rate;

        if (onButtonPressedEvent != null && isEvent)
            onButtonPressedEvent.Invoke(handSide, Button.JoystickY, rate);
    }

    Vector2 JoystickTilt
    {
        get
        {
            var tilt = Tilt.ReadValue<Vector2>();
            return tilt;
        }
    }

    void CheckJoystick2D()
    {
        var tilt = JoystickTilt;

        //отрабатываем наклоны джойстика по осям - для меню 

        if (Mathf.Abs(tilt.x) > Mathf.Abs(tilt.y))
            OnJoystickX(tilt.x);

        if (tilt.magnitude >= rateJoystickMin && Mathf.Abs(tilt.y) > Mathf.Abs(tilt.x))
            OnJoystickY(tilt.y);

        //отдельно отрабатываем перемещение по джойстику
        if (MoveMode == MoveMode.Free && !IsHandMenu && !IsLocked)
            VRMotionController.Instance.MoveTilt(handSide, tilt);

        if (onJoystickTiltEvent != null)
            onJoystickTiltEvent.Invoke(handSide, JoystickTilt);
    }

    public HitTarget VRHitObject()
    {
        //объект, в который ткнулся луч
        return handHiter.HitState != null && handHiter.HitState.transform != null ? new HitTarget { gameObject = handHiter.HitState.transform.gameObject, point = handHiter.HitState.point } : null;
    }

    public HitTarget VRNearObjects()
    {
        //ближайший VRTouchObject, у которого нет коллайдера, но который должен реагировать на руку вблизи
        if (VRTouchedObjectsController.Instance != null && VRTouchedObjectsController.Instance.RegisteredTouchObjects != null)
        {
            var list = VRTouchedObjectsController.Instance.RegisteredTouchObjects;
            if (list.Count > 0)
            {
                float distMin = float.MaxValue;
                foreach (var vrT in list)
                    if (vrT.gameObject.activeSelf && vrT.gameObject.activeInHierarchy && vrT.touchModes != null && vrT.touchModes.Contains(HandTouchMode.Hand) && vrT.GetComponent<Collider>() == null)
                    {
                        float radius = vrT != null ? vrT.handTouchRadius : fingerTouchRadius;
                        var pointerPosition = isGripPressed ? PointerFinger.position : PointerPalm.position;
                        float dist = (pointerPosition - vrT.transform.position).magnitude;
                        if (dist <= radius && dist < distMin)
                            return new HitTarget { gameObject = vrT.gameObject, point = pointerPosition, targetType = TargetType.Hand };
                    }
            }
        }
        return null;
    }

    void CheckIsFirePressed()
    {
        if (ActivateValue != null && isFirePressed)
        {
            var rate = ActivateValue.ReadValue<float>();
            if (rate < rateFireMin)
            {
                Debug.LogWarning("Fire is not released!");
                OnTriggerButton(rate);
            }
        }
    }

    void CheckIsGripPressed()
    {
        if (SelectValue != null && isGripPressed)
        {
            var rate = SelectValue.ReadValue<float>();
            if (rate < rateGripMin)
            {
                Debug.LogWarning("Grip is not pressed");
                OnGripButton(0);
            }
        }
    }

    void CheckIsJoystickPressed()
    {
        if (Mathf.Abs(rateJoystickY) > rateJoystickMin && ScaleDelta!= null)
        {
            var dir = ScaleDelta.ReadValue<Vector2>();
            if (dir.y != rateJoystickY)
                OnJoystickY(dir.y);
        }

        if (Mathf.Abs(rateJoystickX) > rateJoystickMin && ScaleDelta != null)
        {
            var dir = TurnAction.ReadValue<Vector2>();
            if (dir.x != rateJoystickX)
                OnJoystickX(dir.x);
        }
    }

    void HideBeam(bool hide)
    {
        if (hide != beam.IsHided)
            beam.Hide(hide);
    }

    static Dictionary<HandSide, int> requestShowBeamCount = new Dictionary<HandSide, int> { { HandSide.Left, 0 }, { HandSide.Right, 0 } };

    public static void RequestShowBeam(bool show, HandSide handSide)
    {
        int count = requestShowBeamCount.ContainsKey(handSide) ? requestShowBeamCount[handSide] : 0;
        count = Mathf.Max(0, count + (show ? 1 : -1));

        if (requestShowBeamCount.ContainsKey(handSide))
            requestShowBeamCount[handSide] = count;
        else
            requestShowBeamCount.Add(handSide, count);
    }

    private void Update()
    {
        handHiter.Update();

        var targetHand = VRNearObjects();                                   //объекты без коллайдера, до которых можно дотянуться
        var targetBeam = targetHand != null ? targetHand : VRHitObject();   //объекты с коллайдером, в которых тыкается луч

        //Debug.Log("handHiter: " + (handHiter.HitState != null ? handHiter.HitState.transform.name : "null") + " targetBeam=" + (targetBeam != null ? targetBeam.ToString() : "null"));

        bool isBeamLocked = isGripPressed || IsHandMenu || IsLocked || MoveMode == MoveMode.Free;
        bool requestShowBeam = requestShowBeamCount.ContainsKey(handSide) && requestShowBeamCount[handSide] > 0;

        if (!IsLocked && (targetBeam != null || targetHand != null))
        {
            if (handHiter.HitState != null && handHiter.HitState.normal != Vector3.zero)
            {
                Vector3 dv = handHiter.HitState.origin - GetPointerPosition(handHiter.HitState.touchMode);
                handController.transform.position = transform.position + dv;
            }
            else
                handController.transform.localPosition = Vector3.zero;

            var vrT = targetBeam.gameObject.GetComponent<VRTouchedObject>();
            float radius = vrT != null ? vrT.handTouchRadius : fingerTouchRadius;

            if (targetHand == null)
            {
                Vector3 origin = handHiter.HitState != null ? handHiter.HitState.origin : GetPointerPosition(HandTouchMode.Hand);
                targetHand = targetBeam.targetType == TargetType.Hand || Vector3.Distance(origin, targetBeam.point) <= radius ? targetBeam : null;
            }

            if (vrT != null)
            {
                if (targetHand != null && /*vrT != null &&*/ vrT.touchModes.Contains(HandTouchMode.Hand))
                {
                    _targetType = TargetType.Hand;
                    target = new HandTouchItem { item = targetHand.gameObject, touchPoint = targetHand.point, handSide = handSide, touchMode = HandTouchMode.Hand };
                }
                else
                {
                    _targetType = TargetType.Beam;
                    target = new HandTouchItem { item = targetBeam.gameObject, touchPoint = targetBeam.point, handSide = handSide, touchMode = HandTouchMode.Beam };
                }
            }
            else
            {
                _targetType = TargetType.None;
                target = null;
            }

            HideBeam((targetHand != null || isBeamLocked) && !requestShowBeam);
            List<HandTouchItem> itemsTracked = new List<HandTouchItem>();
            List<HandTouchItem> itemsTouched = new List<HandTouchItem>();

            if (targetHand != null)
            {
                var item = new HandTouchItem { item = targetHand.gameObject, touchPoint = targetHand.point, touchMode = HandTouchMode.Hand, handSide = handSide, time = Time.realtimeSinceStartup };
                itemsTracked.Add(item);
                itemsTouched.Add(item);
            }

            if (targetBeam != null && (targetHand == null || targetBeam.gameObject != targetHand.gameObject))
            {
                var item = new HandTouchItem { item = targetBeam.gameObject, touchPoint = targetBeam.point, touchMode = HandTouchMode.Beam, handSide = handSide, time = Time.realtimeSinceStartup };
                itemsTracked.Add(item);

                if (!isBeamLocked)
                    itemsTouched.Add(item);
            }

            UpdateTrackedItems(itemsTracked);

            if (isGripPressed || isFirePressed)
                UpdateTouchedItems(itemsTouched);
        }
        else
        {
            _targetType = TargetType.None;
            target = null;
            UpdateTrackedItems(null);
            UpdateTouchedItems(null);
            HideBeam(isBeamLocked && !requestShowBeam);
        }

        if (!IsLocked && !IsActiveSwitchLocked && _targetType != targetTypeLast)
        {
            targetTypeLast = _targetType;
            SetHandActive(targetTypeLast == TargetType.Hand);
        }

        CheckJoystick2D();
        CheckIsFirePressed();
        CheckIsGripPressed();
        CheckIsJoystickPressed();
    }
}
