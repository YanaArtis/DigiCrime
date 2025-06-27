using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum DarknesState
{
	Unknown = 0,
	Full = 1,
	Clear = 2,
	FadingUp = 3,
	FadingDown = 4,
}

public class Darkness : MonoBehaviour
{
	public static Darkness		Instance = null;

	public delegate void OnOnComplete();
	public static OnOnComplete onOnComplete;

	public delegate void OnOffComplete();
	public static OnOffComplete onOffComplete;

	public string triggerFull = "full";
	public string triggerFadeUp = "on";
	public string triggerFadeDown = "off";
	public string triggerClear = "clear";

	public float	fadeTime = 1f;
    public Text     textState;
    public bool		fadeOffOnAwake = false;

	Animator		animator;
	DarknesState	state;

	public DarknesState State
    {
		get
        {
			return state;
        }
    }

	Animator GetAnimator()
	{
		if (animator == null) animator = GetComponent<Animator> ();
		return animator;
	}

    private void Awake()
    {
		if (Instance == null)
		{
			Instance = this;
			state = DarknesState.Full;
		}
		else Debug.LogWarning("Darkness already exists on scene!");
    }

    void Start()
	{
		if (Instance != this)
		{
			Debug.LogError("extra darkness on the scene!");
			return;
		}

		if (fadeOffOnAwake)
			SetState(DarknesState.FadingDown);
	}

    public void SetText(string text)
    {
        if (textState == null) textState = GetComponentInChildren<Text>();
        if (textState != null) textState.text = text;
    }

	void SetState(DarknesState state)
    {
		this.state = state;
		switch(state)
        {
			case DarknesState.Full: SetTrigger(triggerFull); break;
			case DarknesState.Clear: SetTrigger(triggerClear); break;
			case DarknesState.FadingUp: SetTrigger(triggerFadeUp); break;
			case DarknesState.FadingDown: SetTrigger(triggerFadeDown); break;
		}
    }

	void SetTrigger(string trigger)
	{
		if (GetAnimator () != null) GetAnimator ().SetTrigger (trigger);
	}

	void OnDestroy()
	{
		if (ieWait != null) StopCoroutine(ieWait);
		ieWait = null;
	}

	void OnDisable()
	{
		if (ieWait != null) StopCoroutine(ieWait);
		ieWait = null;
	}

	IEnumerator ieWait = null;

	IEnumerator IEWait(System.Action action, float delay)
	{
		yield return new WaitForSeconds(delay);
		ieWait = null;
		if (action != null)	action.Invoke();
	}

	public void FadeUp()
	{
		FadeUp(null);
	}
	
	public void FadeUp(OnOnComplete onCompleteAction = null)
	{
		//Debug.Log("Darkness.FadeUp: state=" + state);

		if (state == DarknesState.Full)
		{
			if (onCompleteAction != null) onCompleteAction.Invoke();
			return;
		}

		onOnComplete += onCompleteAction;

		if (state != DarknesState.FadingUp)
		{
			SetState(DarknesState.FadingUp);
			ieWait = IEWait(FadeUpComplete, fadeTime);
			StartCoroutine(ieWait);
		}
	}

	public void FadeDown(OnOffComplete onCompleteAction = null)
	{
		//Debug.Log("Darkness.FadeDown: state=" + state);

		if (state == DarknesState.Clear)
		{
			if (onCompleteAction != null) onCompleteAction.Invoke();
			return;
		}

		onOffComplete += onCompleteAction;

		if (state != DarknesState.FadingDown)
		{
			SetState(DarknesState.FadingDown);
			ieWait = IEWait(FadeDownComplete, fadeTime);
			StartCoroutine(ieWait);
		}
	}

	public void Clear()
	{
		SetState(DarknesState.Clear);
	}

	void FadeUpComplete()
	{
		Debug.Log("Darkness.FadeUpComplete");
		state = DarknesState.Full;
		if (onOnComplete != null)
		{
			onOnComplete.Invoke();
			onOnComplete = null;
		}
	}
	
	void FadeDownComplete()
	{
		Debug.Log("Darkness.FadeDownComplete");
		state = DarknesState.Clear;
		if (onOffComplete != null)
		{
			onOffComplete.Invoke();
			onOffComplete = null;
		}
	}

	public static bool IsOn
    {
        get
        {
			if (Instance != null)
				return Instance.state == DarknesState.Full || Instance.state == DarknesState.FadingUp;

            return false;
        }
    }
}
