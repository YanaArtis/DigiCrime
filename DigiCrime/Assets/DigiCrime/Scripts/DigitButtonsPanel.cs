using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

namespace com.ArtisLook.DigiCrime
{
  public class DigitButtonsPanel : MonoBehaviour
  {
    [SerializeField] private int _maxNumButtonsPressed = 4;
    [SerializeField] private string _pwd = "4865";
    [SerializeField] private Text[] _btnTxt;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _snd;

    private enum SafePanelSound { Button = 0, Correct = 1, Incorrect = 2 };

    private bool[] _isBtnPressed;
    private int _numBtnPressed = 0;
    private string _trial;
    private bool _isActive = true;

    void Awake ()
    {
      _isBtnPressed = new bool[_btnTxt.Length];
      Reset();
    }

    public void Reset ()
    {
      for (int i = 0; i < _isBtnPressed.Length; i++)
      { 
        _isBtnPressed[i] = false;
        SetButtonOff(i);
      }
      _numBtnPressed = 0;
      _trial = "";
      _isActive = true;
    }

    private void SetButtonOn (int n)
    {
      _btnTxt[n].text = $"<color=green>{n}</color>";
    }

    private void SetButtonOff (int n)
    {
      _btnTxt[n].text = $"<color=black>{n}</color>";
    }

    public void OnDigitButtonClick (int n)
    {
      if (!_isActive || _isBtnPressed[n]) { return; }
      _isBtnPressed[n] = true;
      SetButtonOn(n);
      ++_numBtnPressed;
      _trial = $"{_trial}{n}";

      if (_numBtnPressed >= _maxNumButtonsPressed)
      {
        if (_pwd.Equals(_trial))
        {
          Debug.Log("Safe UNLOCKED");
          _audioSource.PlayOneShot(_snd[(int)SafePanelSound.Correct]);
          Reset();
          _isActive = false;
          ScriptPlayer.Instance.AddUrgentCommand(ScriptPlayer.ScriptCmd.NewMessage(
            // "Стопэ, демо-версия закончилась! Анализом блокчейна мы займёмся в полной версии игры..."
            "That's all, folks!\n\nDemo is over. We’ll dig into the blockchain in the full version of the game..."
            , DialogWindow.FaceID.VRDeveloper));
          ScriptPlayer.Instance.AddUrgentCommand(ScriptPlayer.ScriptCmd.NewDelay(5));
          ScriptPlayer.Instance.AddUrgentCommand(ScriptPlayer.ScriptCmd.NewMessage(
            // "Как интересно! Может быть, эти цифры на стикере помогут открыть сейф?"
            "There is the paper with 12 words - the key for crypto wallet! Let's dive in the ocean of blockchain, NFTs and cryptocurrencies!"
            , DialogWindow.FaceID.RoboCat));
          ScriptPlayer.Instance.AddUrgentCommand(ScriptPlayer.ScriptCmd.NewDelay(5));
          ScriptPlayer.Instance.AddUrgentCommand(ScriptPlayer.ScriptCmd.NewOpenSafeDoor());
        }
        else
        {
          Debug.Log("Safe locked");
          _audioSource.PlayOneShot(_snd[(int)SafePanelSound.Incorrect]);
          Reset();
        }
      }
      else
      {
        _audioSource.PlayOneShot(_snd[(int)SafePanelSound.Button]);
      }
    }

    public void OnDigitButtonClick_0 ()
    {
      OnDigitButtonClick(0);
    }

    public void OnDigitButtonClick_1 ()
    {
      OnDigitButtonClick(1);
    }

    public void OnDigitButtonClick_2 ()
    {
      OnDigitButtonClick(2);
    }

    public void OnDigitButtonClick_3 ()
    {
      OnDigitButtonClick(3);
    }

    public void OnDigitButtonClick_4 ()
    {
      OnDigitButtonClick(4);
    }

    public void OnDigitButtonClick_5 ()
    {
      OnDigitButtonClick(5);
    }

    public void OnDigitButtonClick_6 ()
    {
      OnDigitButtonClick(6);
    }

    public void OnDigitButtonClick_7 ()
    {
      OnDigitButtonClick(7);
    }

    public void OnDigitButtonClick_8 ()
    {
      OnDigitButtonClick(8);
    }

    public void OnDigitButtonClick_9 ()
    {
      OnDigitButtonClick(9);
    }
  }
}
