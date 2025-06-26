using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.ArtisLook.DigiCrime
{
  public class ScriptPlayer : MonoBehaviour
  {
    [SerializeField] private Sticker _sticker;
    [SerializeField] private DigitButtonsPanel _safePanel;
    [SerializeField] private SafeDoor _safeDoor;

    public static ScriptPlayer Instance;

//    private int _currentCmdNdx = -1;
    private ScriptCmd _currentCmd;
    private float _cmdFinishTime;
    private bool _isScriptFinished = false;

    public class ScriptCmd
    {
      public enum Operation { Delay, ShowMsg, ExitApp, OpenSafeDoor };
      public Operation op;
      public string s;
      public float v;
      public DialogWindow.FaceID face;

      public static ScriptCmd NewDelay (float delayTime)
      {
        ScriptCmd cmd = new ScriptCmd();
        cmd.op = Operation.Delay;
        cmd.v = delayTime;
        return cmd;
      }

      public static ScriptCmd NewMessage (string msg, DialogWindow.FaceID face)
      {
        ScriptCmd cmd = new ScriptCmd();
        cmd.op = Operation.ShowMsg;
        cmd.s = msg;
        cmd.face = face;
        return cmd;
      }

      public static ScriptCmd NewExitApp ()
      {
        ScriptCmd cmd = new ScriptCmd();
        cmd.op = Operation.ExitApp;
        return cmd;
      }

      public static ScriptCmd NewOpenSafeDoor ()
      {
        ScriptCmd cmd = new ScriptCmd();
        cmd.op = Operation.OpenSafeDoor;
        return cmd;
      }
    };

    private List<ScriptCmd> _cmdList = new List<ScriptCmd>()
    { ScriptCmd.NewDelay(5)
    , ScriptCmd.NewMessage(
      // "Ты — частный детектив, работающий вместе с воплощённым GPT — кибер-котом, гением цифровой безопасности.\n\nВчера при загадочных обстоятельствах погиб известный рэпер Вован-таракан. Его подруга Анжела, на чьё имя незадолго до этого он оформил завещание, пригласила вас помочь открыть сейф, доставшийся ей в наследство."
      "You’re a private detective, partnered with a Robo-Cat powered by GPT — a true mastermind of digital security.\n\nYesterday, famous rapper Bob the Cockroach died under mysterious circumstances. His girlfriend Angela, to whom he recently drawn up a will, has asked for your help to unlock the mysterious safe she had inherited."
      , DialogWindow.FaceID.None)
    , ScriptCmd.NewDelay(1)
    , ScriptCmd.NewMessage(
      // "Детектив, наконец-то вы тут! Помогите же мне открыть сейф, там наверняка самые ценные предметы моего наследства!"
      "Detective, finally! Please help me open open the safe, it probably contains the most valuable items of my inheritance!"
      , DialogWindow.FaceID.Girl)
    , ScriptCmd.NewDelay(1)
    , ScriptCmd.NewMessage(
      // "Люди часто беспечно оставляют пароли на видных местах, чтобы не забыть. Давай осмотримся?"
      "People tend to leave passwords lying around so they don’t forget. Let’s take a look around, shall we?"
      , DialogWindow.FaceID.RoboCat)
    };
    private LinkedList<ScriptCmd> _cmd = new LinkedList<ScriptCmd>();

    private void StartCommand (ScriptCmd cmd)
    {
      Debug.Log($"ScriptPlayer.StartCommand({cmd.op})");
      switch (cmd.op)
      {
        case ScriptCmd.Operation.Delay:
          _cmdFinishTime = Time.time + cmd.v;
          break;
        case ScriptCmd.Operation.ShowMsg:
          DialogWindow.Instance.ShowMessage(cmd.s, cmd.face);
          break;
        case ScriptCmd.Operation.ExitApp:
          Application.Quit();
          break;
        case ScriptCmd.Operation.OpenSafeDoor:
          _safeDoor.Open();
          break;
      }
    }

    private void UpdateCommand (ScriptCmd cmd)
    {
      bool isCmdFinished = false;
      switch (cmd.op)
      {
        case ScriptCmd.Operation.Delay:
          if (_cmdFinishTime <= Time.time)
          {
            isCmdFinished = true;
          }
          break;
        case ScriptCmd.Operation.ShowMsg:
          if (!DialogWindow.Instance.IsShown())
          {
            isCmdFinished = true;
          }
          break;
        case ScriptCmd.Operation.OpenSafeDoor:
          isCmdFinished = true;
          break;
        case ScriptCmd.Operation.ExitApp:
          isCmdFinished = true;
          break;
      }
      if (isCmdFinished)
      {
        Debug.Log($"ScriptPlayer.UpdateCommand({cmd.op}): COMMAND FINISHED");
        _currentCmd = null;
      }
    }

    void Awake ()
    {
      if ((Instance != null) && (Instance != this))
      {
        Destroy(this);
        return;
      }
      Instance = this;
    }

    void Start ()
    {
      StartGame();
    }

    public void AddUrgentCommand (ScriptCmd urgentCmd)
    {
      _cmd.AddFirst(urgentCmd);
    }

    void Update ()
    {
//      if (_isScriptFinished) { return; }
      if (_currentCmd != null)
      {
        UpdateCommand(_currentCmd);
      }
      else
      {
        if (_cmd.Count > 0)
        {
          // Start execute next command
          _currentCmd = _cmd.First.Value;
          _cmd.RemoveFirst();
          StartCommand(_currentCmd);
        }
        else
        {
          // All script commands executed
          Debug.Log("ScriptPlayer: All script commands executed.");
          _isScriptFinished = true;
        }
      }

      /*
      if (_currentCmd == null) // Start execute next command
      {
        if (_currentCmdNdx >= _cmd.Count)
        {
          // All script commands executed
          Debug.Log("ScriptPlayer: All script commands executed (#1)");
          _isScriptFinished = true;
        }
        else
        {
          ++_currentCmdNdx;
          if (_currentCmdNdx >= _cmd.Count)
          {
            // All script commands executed
            Debug.Log("ScriptPlayer: All script commands executed (#2)");
            _isScriptFinished = true;
          }
          else
          {
            _currentCmd = _cmd[_currentCmdNdx];
            StartCommand(_currentCmd);
          }
        }
      }
      else
      {
        UpdateCommand(_currentCmd);
      }
      */
    }

    public void StartGame ()
    {
      _sticker.Reset();
      _safePanel.Reset();
      _safeDoor.Reset();
      // _currentCmdNdx = -1;
      _currentCmd = null;
      _isScriptFinished = false;
      _cmd.Clear();
      for (int i = 0; i < _cmdList.Count; i++)
      {
        _cmd.AddLast(_cmdList[i]);
      }
    }
  }
}
