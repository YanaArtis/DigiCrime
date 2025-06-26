using UnityEngine;

namespace com.ArtisLook.DigiCrime
{
  public class Sticker : MonoBehaviour
  {
    // [SerializeField] private float _closedAngle = 25;
    // [SerializeField] private float _openedAngle = 120;
    [SerializeField] private GameObject _goFlipOpened;
    [SerializeField] private GameObject _goFlipClosed;

    private bool _isOpened = false;
    private bool _wasOpened = false;

    void Start()
    {
      Reset();
    }

    public void Reset()
    {
      Close();
      _wasOpened = false;
    }

    /*
    public void SetFlipAngle (float angleX)
    {
      Vector3 ea = _tFlip.localEulerAngles;
      ea.x = angleX;
      _tFlip.localEulerAngles = ea;
    }
    */

    public void Close ()
    {
      _isOpened = false;
      _goFlipClosed.SetActive(true);
      _goFlipOpened.SetActive(false);
      // SetFlipAngle(_closedAngle);
    }

    public void Open ()
    {
      if (!_wasOpened)
      {
        ScriptPlayer.Instance.AddUrgentCommand(ScriptPlayer.ScriptCmd.NewDelay(5));
        ScriptPlayer.Instance.AddUrgentCommand(ScriptPlayer.ScriptCmd.NewMessage(
          // "Как интересно! Может быть, эти цифры на стикере помогут открыть сейф?"
          "How interesting! Maybe these numbers on the sticker will help open the safe?"
          , DialogWindow.FaceID.RoboCat));
      }
      _isOpened = true;
      _wasOpened = true;
      _goFlipClosed.SetActive(false);
      _goFlipOpened.SetActive(true);
      // SetFlipAngle(_openedAngle);
    }

    public void Toggle ()
    {
      if (_isOpened)
      {
        Close();
      }
      else
      {
        Open();
      }
    }

    public bool IsOpened ()
    {
      return _isOpened;
    }

    public bool WasOpened ()
    {
      return _wasOpened;
    }

    /*
    private void OnTriggerEnter (Collider other)
    {
      if (_isOpened) { return; }
      Open();
    }
    */
  }
}
