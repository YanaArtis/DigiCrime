using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeItem : MonoBehaviour
{
    public List<GameObject> indicatorsWrong;
    public bool IsWrong => countWrong > 0;
    public Vector3 BasePosition => basePosition;

    int countWrong = 0;
    Vector3 basePosition;

    private void Start()
    {
        UpdateIndicators();
    }

    public void SetWrongIndicators(int count)
    {
        countWrong = count;
        UpdateIndicators();
    }

    public void SetBasePosition(Vector3 position)
    {
        basePosition = position;
    }

    void UpdateIndicators()
    {
        if (indicatorsWrong != null && indicatorsWrong.Count > 0)
        {
            Randomizer rnd = new Randomizer(indicatorsWrong.Count);

            indicatorsWrong.ForEach(i => i.SetActive(false));

            for (int i = 0; i < countWrong; i++)
            {
                int n = rnd.GetRandom();
                indicatorsWrong[n].SetActive(true);
            }
        }
    }
}
