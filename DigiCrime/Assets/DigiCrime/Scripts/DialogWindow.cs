using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

namespace com.ArtisLook.DigiCrime
{
  public class DialogWindow : MonoBehaviour
  {
    [SerializeField] private float _distanceToCam = 2;
    [SerializeField] private TextMeshProUGUI _txt;
    [SerializeField] private GameObject _goCanvas;
    [SerializeField] private GameObject[] _goFace;
    [SerializeField] private string[] _faceName;

    public enum FaceID { None = -1, RoboCat = 0, Girl = 1, VRDeveloper = 2 }

    public static DialogWindow Instance;

    private Transform _transform;
    private Transform _tCam;

    void Awake ()
    {
      if ((Instance != null) && (Instance != this))
      {
        Destroy(this);
        return;
      }
      Instance = this;

      HideAllFaces();
      Hide();

      _transform = transform;
      _tCam = Camera.main.transform;

      // Invoke("Test", 1);
    }

    private void Update()
    {
      Vector3 v3ToCam = (_transform.position - _tCam.position).normalized;
      v3ToCam.y = 0;
      if (v3ToCam.magnitude > float.Epsilon)
      {
        this.transform.rotation = Quaternion.LookRotation(v3ToCam);
      }
    }

    public void HideAllFaces ()
    {
      foreach (var face in _goFace)
      {
        face.SetActive(false);
      }
    }

    public void ShowMessage (string msg, FaceID face)
    {
      HideAllFaces();
      Show();
      if (face != FaceID.None)
      {
        int n = (int)face;
        _txt.text = $"{_faceName[n]}:\n{msg}";
        _goFace[n].SetActive(true);
      }
      else
      {
        _txt.text = $"{msg}";
      }

      Vector3 camFwd = _tCam.forward;
      camFwd.y = 0;
      Vector3 newPosition = _tCam.position + camFwd.normalized * _distanceToCam;
      newPosition.y = 1;
      _transform.position = newPosition;
    }

    public void Hide ()
    {
      _goCanvas.SetActive(false);
    }

    public void Show ()
    {
      _goCanvas.SetActive(true);
    }

    public void OnCloseButtonPressed ()
    {
      Hide();
    }

    public bool IsShown ()
    {
      return _goCanvas.activeSelf;
    }
  }
}
