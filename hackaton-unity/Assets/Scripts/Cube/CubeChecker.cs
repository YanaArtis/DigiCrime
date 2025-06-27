using UnityEngine;

public class CubeChecker : MonoBehaviour
{
    public enum State
    {
        Default = 0,
        Correct = 1,
        Incorrect = 2,
    }

    public GameObject indicatorDefault;
    public GameObject indicatorCorrect;
    public GameObject indicatorIncorrect;

    public void Set(State state)
    {
        switch(state)
        {
            case State.Correct:
                indicatorCorrect.SetActive(true);
                indicatorIncorrect.SetActive(false);
                indicatorDefault.SetActive(false);
                break;

            case State.Incorrect:
                indicatorCorrect.SetActive(false);
                indicatorIncorrect.SetActive(true);
                indicatorDefault.SetActive(false);
                break;

            default:
                indicatorCorrect.SetActive(false);
                indicatorIncorrect.SetActive(false);
                indicatorDefault.SetActive(true);
                break;
        }    
    }
}
