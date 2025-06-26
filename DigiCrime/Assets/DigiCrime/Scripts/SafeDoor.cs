using System.Collections;
using UnityEngine;

namespace com.ArtisLook.DigiCrime
{
  public class SafeDoor : MonoBehaviour
  {
    [SerializeField] private float _closedY = .75f;
    [SerializeField] private float _openedY = .2f;
    [SerializeField] private float _moveSpeed = 1;

    private Transform _transform;
    private bool _isOpened = false;
    private float _moveDirection = 0;
    private float _moveEndY = 0;
    private bool _willBeOpenedOnMoveEnd = false;

    private void Awake()
    {
      _transform = transform;
    }

    void Start ()
    {
      Reset();
    }

    public void Open ()
    {
      _moveDirection = Mathf.Sign(_openedY - _closedY);
      _moveEndY = _openedY;
      _willBeOpenedOnMoveEnd = true;
      StartCoroutine(Move_coroutine());
    }

    public void Close ()
    {
      _moveDirection = Mathf.Sign(_closedY - _openedY);
      _moveEndY = _closedY;
      _willBeOpenedOnMoveEnd = false;
      StartCoroutine(Move_coroutine());
    }

    IEnumerator Move_coroutine ()
    {
      Vector3 pos;
      while ((_transform.localPosition.y <= _closedY) && (_transform.localPosition.y >= _openedY))
      {
        pos = _transform.localPosition;
        pos.y = _transform.localPosition.y + Time.fixedDeltaTime * _moveDirection * _moveSpeed;
        _transform.localPosition = pos;
        yield return new WaitForFixedUpdate();
      }
      pos = _transform.localPosition;
      pos.y = _moveEndY;
      _transform.localPosition = pos;
      _isOpened = _willBeOpenedOnMoveEnd;
    }

    public void Reset ()
    {
      _isOpened = false;
      Vector3 pos = _transform.localPosition;
      pos.y = _closedY;
      _transform.localPosition = pos;
    }
  }
}
