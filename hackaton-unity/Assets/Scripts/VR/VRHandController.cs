using UnityEngine;

public class VRHandController : MonoBehaviour
{
    public Animator animator;
    public GameObject skin;

    public const string triggerPalm = "palm";
    public const string triggerIdle = "idle";
    public const string triggerPointer = "pointer";
    public const string triggerHold = "hold";
    public const string triggerHoldClub = "hold-club";
    public const string triggerHoldHummer = "hold-hummer";
    public const string triggerHoldKey = "hold-key";
    public const string triggerHoldKeySmall = "hold-key-small";
    public const string triggerHoldSphere = "hold-sphere";
    public const string triggerHoldTuner = "hold-tuner";
    public const string triggerHoldFinger = "hold-finger";
    public const string triggerPhone = "phone";
    public const string triggerGrab = "grab";
    public const string triggerGrabLarge = "grab-large";

    public Material materialHandInactive;
    public Material materialHandActive;

    string trigger;
    string triggerPrevious;

    public void SetActive(bool isActive)
    {
        skin.GetComponent<SkinnedMeshRenderer>().sharedMaterial = isActive ? materialHandActive : materialHandInactive;
    }

    public void SetIdle()
    {
        SetTrigger(triggerIdle);
    }

    public void SetPointer()
    {
        SetTrigger(triggerPointer);
    }

    public void SetPrevious()
    {
        SetTrigger(triggerPrevious);
    }

    public void SetTrigger(string trigger)
    {
        if (trigger != this.trigger)
        {
            triggerPrevious = this.trigger;
            this.trigger = trigger;
            animator.SetTrigger(trigger);
        }
    }

    private void Awake()
    {
        SetTrigger(triggerIdle);
    }
}
