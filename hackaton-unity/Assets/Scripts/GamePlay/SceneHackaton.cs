using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SceneHackaton : SceneBasic
{
    public AudioClip audioStart;
    public Transform startPlace;
    public List<Cube> cubes;
    public Transform cubesContainer;
    public float disassembleDist = 0.5f;
    public Vector3 cubeCompleteShift = Vector3.up;
    public List<CubeChecker> checkers;

    public AudioClip soundChecking;
    public AudioClip soundCorrect;
    public AudioClip soundIncorrect;

    public GameObject buttonOk;
    public int stepsCountBeforeAttack = 3;

    public ProofOfWorkController calculator;
    public GameObject indicatorProofSuccess;
    public GameObject indicatorProofFailed;

    List<Cube> cubesAssembled;
    Randomizer rnd;
    Cube cubeCurrent;

    override protected void OnSafeZoneChanged(List<Vector3> _)
    {
        Debug.Log("OnZoneReady");
        Go();
    }

    protected override void Start()
    {
        base.Start();
        calculator.onOkEvent += OnCalculatorOk;

        SetCheckers(false, CubeChecker.State.Default);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        calculator.onOkEvent -= OnCalculatorOk;
    }

    override protected void OnReady()
    {
        StartCoroutine(IEAction(1, Go));
    }

    void Go()
    {
        var cam = VRHeadTracker.Instance.xrOrigin.Camera;

        var euler = new Vector3(0, cam.transform.rotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Euler(euler);

        var shift = startPlace != null ? cam.transform.position - startPlace.position: cam.transform.position - transform.position;
        var pos = transform.position + shift;
        pos.y = 0;
        transform.position = pos;

        Darkness.Instance.FadeDown(StartNextCube);

        VRHandTracker.RequestShowBeam(true, HandSide.Left);
        VRHandTracker.RequestShowBeam(true, HandSide.Right);

        rnd = new Randomizer(cubes.Count);
    }

    void StartNextCube()
    {
        var cubeTemplate = cubes[rnd.GetRandom()];
        StartCube(cubeTemplate);
        SetCheckers(false, CubeChecker.State.Default);
        buttonOk.SetActive(true);
    }

    int Step => cubesAssembled != null ? cubesAssembled.Count : 0;

    void StartCube(Cube cubeTemplate)
    {
        var cube = Instantiate(cubeTemplate);
        cube.gameObject.SetActive(true);
        cube.transform.SetParent(cubesContainer);
        cube.transform.localPosition = Vector3.zero;
        cube.transform.localScale = Vector3.one;
        cube.transform.localRotation = Quaternion.identity;

        int step = Step;
        int countWrong = step > stepsCountBeforeAttack ? UnityEngine.Random.Range(1, Mathf.Max(1, Mathf.Min(cube.items.Count - 1, step / stepsCountBeforeAttack))) : 0;

        Debug.Log("StartCube step=" + Step + " wrongCount=" + countWrong);

        cube.Disassemble(disassembleDist, countWrong, 6);

        cube.onAssembled += OnCubeAssembled;

        cubeCurrent = cube;
    }

    void OnCubeAssembled(Cube cube)
    {

    }

    void CompleteCube(Cube cube)
    {
        cube.onAssembled -= OnCubeAssembled;
        cube.HideWrong();
        cube.Lock();

        if (cubesAssembled == null)
            cubesAssembled = new List<Cube>();

        iTween.MoveTo(cube.gameObject, iTween.Hash("position", cube.transform.position + cubeCompleteShift, "time", 1f, "oncompletetarget", gameObject, "oncomplete", nameof(StartNextCube)));

        cubesAssembled.ForEach(c => {
            var pos = c.transform.position + Vector3.up * cube.sideSize;
            iTween.MoveTo(c.gameObject, iTween.Hash("position", pos, "time", 1f));
        });

        cubesAssembled.Add(cube);
    }

    void RejectCube(Cube cube)
    {
        cube.onAssembled -= OnCubeAssembled;
        cube.Lock();
        Destroy(cube.gameObject);
        StartNextCube();
    }

    public void OnOk()
    {
        buttonOk.SetActive(false);
        //CheckConcensus();

        calculator.gameObject.SetActive(true);
        calculator.Reset();

        SetIndicatorProof(false, false);
    }

    void SetIndicatorProof(bool success, bool show = true)
    {
        indicatorProofFailed.SetActive(!success && show);
        indicatorProofSuccess.SetActive(success && show);
    }

    void OnCalculatorOk(bool success)
    {
        calculator.gameObject.SetActive(false);

        if (success)
        {
            SetIndicatorProof(true);
            var ie = IEAction(2, CheckConcensus);
            StartCoroutine(ie);
        }
        else
        {
            SetIndicatorProof(false);
            var ie = IEAction(2, OnOk);
            StartCoroutine(ie);
        }
    }

    void SetCheckers(bool enabled, CubeChecker.State state)
    {
        checkers.ForEach(c => {
            c.gameObject.SetActive(enabled);
            c.Set(state);

            if (state == CubeChecker.State.Default)
            {
                var a = GetComponent<Animator>();
                if (a != null)
                    a.StartPlayback();
            }
        });
    }

    public static IEnumerator IEAction(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        if (action != null)
            action.Invoke();
    }

    void CheckConcensus()
    {
        SetIndicatorProof(false, false);
        SetCheckers(true, CubeChecker.State.Default);
        var ie = IEAction(2, cubeCurrent.IsComplete ? CheckConcensusCorrect : CheckConcensusIncorrect);
        StartCoroutine(ie);
    }

    void CheckConcensusCorrect()
    {
        SetCheckers(true, CubeChecker.State.Correct);
        var ie = IEAction(2, CheckConcensusCorrectDone);
        StartCoroutine(ie);
    }

    void CheckConcensusCorrectDone()
    {
        CompleteCube(cubeCurrent);
    }

    void CheckConcensusIncorrect()
    {
        SetCheckers(true, CubeChecker.State.Incorrect);
        var ie = IEAction(2, CheckConcensusIncorrectDone);
        StartCoroutine(ie);
    }

    void CheckConcensusIncorrectDone()
    {
        RejectCube(cubeCurrent);
    }
}
