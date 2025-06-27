using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ProofOfWorkController : MonoBehaviour
{
    public delegate void OnOkEvent(bool success);
    public OnOkEvent onOkEvent;
    public float enterDelay = 0.5f;

    public Text textTaskResult;
    List<int> res;
    (int d1, int d2) task;
    float lastEnterTime = -1;

    (int d1, int d2) GetRandomTask()
    {
        int d1 = Random.Range(1, 9);
        int d2 = Random.Range(1, 9);
        return (d1, d2);
    }

    private void OnEnable()
    {
        lastEnterTime = Time.time;
    }

    string ListToString(List<int> list)
    {
        string res = "";
        if (list != null)
            for (int i = 0; i < list.Count; i++)
                res += list[i].ToString();
        return res;
    }

    void OutText(List<int> list)
    {
        var str = ListToString(list);
        OutText(str);
    }

    void OutText(string str)
    {
        string res = task.d1.ToString() + " + " + task.d2.ToString() + " = " + str;
        textTaskResult.text = res;
    }

    public void Reset()
    {
        if (res != null)
            res.Clear();
        else
            res = new List<int>();

        task = GetRandomTask();
        OutText(ListToString(res));
    }

    public void OnDigit(int digit)
    {
        if (Time.time - lastEnterTime > enterDelay)
        {
            lastEnterTime = Time.time;
            res.Add(digit);
            OutText(res);
        }
    }

    public void OnBsp()
    {
        if (Time.time - lastEnterTime > enterDelay)
        {
            lastEnterTime = Time.time;

            if (res != null && res.Count > 0)
                res.RemoveRange(res.Count - 1, 1);

            OutText(res);
        }
    }

    public void OnOk()
    {
        bool success = false;

        if (res != null && res.Count > 0)
        {
            int summ = int.Parse(ListToString(res));
            Debug.Log(ListToString(res) + " sum=" + summ + " vs " + task.d1 + "+" + task.d2 + "=" + (task.d1 + task.d2));

            success = task.d1 + task.d2 == summ;
        }

        if (onOkEvent != null)
            onOkEvent.Invoke(success);
    }
}
