using UnityEngine;
using System.Collections.Generic;

public class MotionState
{
    public Vector3 dir;                 //направление: +1 = вперёд, -1 = назад
    public HandSide handSide;       //с какой руки запустили
    public bool isMoving;           //идём или нет

    new public string ToString()
    {
        return "{dir=" + dir + ", handSide=" + handSide + ", isMoving=" + isMoving + "}";
    }
}

public class MotionController : MonoBehaviour
{
    public const float heighOffset = 0.3f;     //высота смещения головы для MenuHandItemHeight

    public const float MoveSpeed = 4f;

    public const float border = 0.01f;   //минимальное расстояние, на которое можно подойти к границе области
    public GameObject origin;
    public float speed = 1f;        //meters per second
    public float turnSpeed = 5f;    //скорость поворота камеры в свободном режиме, градусы в секунду
    public iTween.EaseType easeType = iTween.EaseType.easeInOutQuad;//.easeInOutCubic;

    public const float turnDegrees = 45;// 22.5f;
    public iTween.EaseType easeTypeTurn = iTween.EaseType.easeInOutQuad;//.easeInOutCubic;

    protected MotionState movingForward;
    bool isMoving = false;
    bool isTurning = false;

    public bool IsBusy => isMoving || isTurning;

    public float Speed => MoveSpeed * speed;
    public float TurnSpeed => turnSpeed;
    public float turnTime => 1;   //время поворота на 45 градусов

    private Transform _originCamera;
    virtual protected Transform originCamera
    {
        get
        {
            if (_originCamera == null)
            {
                var cam = origin.GetComponent<Camera>();
                if (cam != null)
                    _originCamera = cam.transform;
                else
                {
                    cam = origin.GetComponentInChildren<Camera>();
                    if (cam != null)
                        _originCamera = cam.transform;
                    else
                        _originCamera = origin.transform;
                }
            }

            return _originCamera;
        }
    }

    public Vector3 Dir
    {
        get
        {
            var res = originCamera.transform.forward;
            res.y = 0;
            return res;
        }
    }

    protected void StopMovingForward()
    {
        if (movingForward != null)
        {
            movingForward.isMoving = false;
            movingForward.dir = Vector3.zero;
        }
        
        //Debug.Log("StopMovingForward");
    }

    float PathLength(List<Vector3> path)
    {
        float res = 0;
        if (path != null && path.Count > 1)
            for (int i = 1; i < path.Count; i++)
                res += Vector3.Distance(path[i], path[i - 1]);
        return res;
    }

    void LockHands(bool isLocked = true)
    {
        VRHandTracker.LockAll(isLocked);
    }

    protected void OnStartMove()
    {
        LockHands(true);
    }

    protected void OnStopMove()
    {
        LockHands(false);
    }

    public bool MoveToPath(List<Vector3> path, System.Action onComplete)
    {
        //Debug.Log("MoveToPath: IsBusy=" + IsBusy + " path.Count=" + (path != null ? path.Count : -1));

        if (!IsBusy)
        {
            OnStartMove();
            isMoving = true;
            float time = PathLength(path) / Speed;
            iTween.MoveTo(origin.gameObject, iTween.Hash("path", path.ToArray(), "movetopath", false, "time", time, "easetype", easeType, "oncompletetarget", gameObject, "oncompleteparams", onComplete, "oncomplete", nameof(MoveToComplete)));
            return true;
        }

        return false;
    }

    public bool MoveToPoint(Vector3 point, float time = 0, System.Action onComplete = null)
    {
        //Debug.Log("MoveToPoint: IsBusy=" + IsBusy + " point=" + point);

        if (!IsBusy)
        {
            Debug.DrawRay(origin.transform.position, point - origin.transform.position, Color.yellow);

            if (time > 0)
            {
                isMoving = true;
                OnStartMove();
                iTween.MoveTo(origin.gameObject, iTween.Hash("position", point, "time", time, "easetype", easeType, "oncompletetarget", gameObject, "oncompleteparams", onComplete, "oncomplete", nameof(MoveToComplete)));
            }
            else
                origin.transform.position = point;

            return true;
        }

        return false;
    }

    private void MoveToComplete(System.Action onComplete)
    {
        isMoving = false;
        OnStopMove();

        if (onComplete != null)
            onComplete.Invoke();
    }

    public bool MoveForward(HandSide handSide, float rate)
    {
        //вылавливаем сигнал на начало движения вперёд

        if (!IsBusy && rate != 0)
        {
            if (movingForward == null)
                movingForward = new MotionState();

            if (!movingForward.isMoving)
            {
                movingForward.isMoving = true;
                movingForward.dir = Mathf.Sign(rate) * Vector3.forward;
                movingForward.handSide = handSide;
            }
            return true;
        }
        return false;
    }

    public bool MoveSide(HandSide handSide, float rate)
    {
        //вылавливаем сигнал на начало движения вбок

        if (!IsBusy && rate != 0)
        {
            if (movingForward == null)
                movingForward = new MotionState();

            if (!movingForward.isMoving)
            {
                movingForward.isMoving = true;
                movingForward.dir = Mathf.Sign(rate) * Vector3.left;
                movingForward.handSide = handSide;
            }

            return true;
        }
        return false;
    }

    public bool MoveTilt(HandSide handSide, Vector2 tilt)    //tilt = вектор длиной от 0 до 1, длина = степень наклона джойстика
    {
        if (!IsBusy && handSide == VRHandTracker.HandSideMove)
        {
            if (tilt.magnitude > 0)
            {
                if (movingForward == null)
                    movingForward = new MotionState();

                movingForward.isMoving = true;
                movingForward.dir = new Vector3 { x = tilt.x, z = tilt.y };
                movingForward.handSide = handSide;
            }
            else if (movingForward != null && movingForward.isMoving)
                StopMovingForward();

            return true;
        }
        return false;
    }

    void MoveForward()
    {
        //если movingForward.dir - движение по джойстику в плоскости (x,z)

        float dist = Speed * Time.deltaTime;
        var dir = originCamera.transform.TransformDirection(movingForward.dir);
        dir.y = 0;

        var point = origin.transform.position + dist * dir;
        var dvHead = origin.transform.position - originCamera.transform.position;

        var pFrom = origin.transform.position - dvHead;
        var pTo = point - dvHead;

        pFrom.y = pTo.y = 0;

        MoveToPoint(point);
    }

    public bool MoveToDir(Vector3 dir)
    {
        return MoveToPoint(origin.transform.position + dir.normalized * Speed * Time.deltaTime, 0, null);
    }

    public void AbortMoving()
    {
        iTween.Stop(origin.gameObject);
    }

    public void Place(Vector3 place)
    {
        var dv = origin.transform.position - originCamera.transform.position;
        dv.y = 0;
        origin.transform.position = place + dv;
    }

    //TURN

    public void Turn(float turnRate)    //повернуться с учётом степени наклона джойстика
    {
        Turn(turnRate * Time.deltaTime * TurnSpeed, 0, null);
    }

    public void Turn(bool right, System.Action onTurnComplete)
    {
        float angle = (right ? 1 : -1) * turnDegrees;
        Turn(angle, turnTime, onTurnComplete);
    }

    public void Turn(float angle, float turnTime, System.Action onTurnComplete)   //повернуться на угол angle за turnTime секунд
    {
        if (!IsBusy)
        {
            if (turnTime > 0)
            {
                isTurning = true;
                if (VRHandTracker.TurnMode == TurnMode.Sharp)
                    TurnSharp(angle, onTurnComplete);
                else
                    TurnSmooth(angle, turnTime, onTurnComplete);
            }
            else
                RotateAroundCameraPosition(angle);
        }
        else
        {
            OnRotationComplete();

            if (onTurnComplete != null)
                onTurnComplete.Invoke();
        }
    }

    List<System.Action> onTurnCompleteList = new List<System.Action>();

    void TurnSmooth(float angle, float turnTime, System.Action onTurnComplete)
    {
        onTurnCompleteList.Add(onTurnComplete);
        lastTurnTime = Time.realtimeSinceStartup;
        angleTo = angle;
        iTween.ValueTo(gameObject, iTween.Hash(
            "from", 0, "to", angle, "time", turnTime, "easetype", easeTypeTurn,
            "onupdatetarget", gameObject, "onupdate", nameof(SetRotationFrom),
            "oncompletetarget", gameObject, "oncomplete", nameof(OnRotationComplete)));
    }

    void TurnSharp(float angle, System.Action onTurnComplete)
    {
        Darkness.Instance.FadeUp(delegate
        {
            onTurnCompleteList.Add(onTurnComplete);
            RotateAroundCameraPosition(angle);
            Darkness.Instance.FadeDown(OnRotationComplete);
        });
    }

    virtual protected void RotateAroundCameraPosition(float angle)
    {
        //Debug.Log("RotateAroundCameraPosition: origin=" + origin + " originCamera=" + originCamera + " angle=" + angle);
        origin.transform.RotateAround(originCamera.transform.position, Vector3.up, angle);
    }

    float lastTurnTime = -1;
    float angleTo;

    void SetRotationFrom(float _)
    {
        float angle = (Time.realtimeSinceStartup - lastTurnTime) * angleTo / turnTime;
        lastTurnTime = Time.realtimeSinceStartup;
        RotateAroundCameraPosition(angle);
    }

    void OnRotationComplete()
    {
        isTurning = false;

        if (onTurnCompleteList != null && onTurnCompleteList.Count > 0)
        {
            foreach (var f in onTurnCompleteList)
                if (f != null)
                    f.Invoke();

            onTurnCompleteList.Clear();
        }
    }

    public void LookInDirection(Vector3 dir)
    {
        var dirCam = originCamera.transform.forward;
        float angle = Vector3.SignedAngle(dirCam, dir, Vector3.up);
        RotateAroundCameraPosition(angle);
    }

    private void Update()
    {
        if (movingForward != null && movingForward.isMoving)
            MoveForward();
    }
}
