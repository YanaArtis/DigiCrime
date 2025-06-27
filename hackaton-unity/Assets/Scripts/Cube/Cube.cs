using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cube : MonoBehaviour
{
    public delegate void OnAssembled(Cube _);
    public OnAssembled onAssembled;

    public List<CubeItem> items;
    public float sideSize = 1f;

    public bool IsCubeAssembled
    {
        get
        {
            foreach (var i in items)
                if (i.transform.position != i.BasePosition)
                    return false;
            return true;
        }
    }

    public bool IsComplete
    {
        get
        {
            foreach (var i in items)
            {
                if (i.transform.position != i.BasePosition && !i.IsWrong)
                    return false;

                if (i.transform.position == i.BasePosition && i.IsWrong)
                    return false;
            }
            return true;
        }
    }

    public bool IsLocked => isLocked;

    float dist = 0;
    bool isLocked = false;

    private void Start()
    {
        items.ForEach(i => {
            var drag = i.GetComponent<VRTouchObjectDistancedDrag>();
            drag.onReleaseEvent += OnItemRelease;
        });
    }

    void OnItemRelease(VRTouchedObject t)
    {
        var item = t.GetComponent<CubeItem>();
        var dv = item.BasePosition - item.transform.position;
        if (dv.magnitude < dist / 5f || (item.transform.position - transform.position).magnitude < sideSize / 5f)
            item.transform.position = item.BasePosition;

        Debug.Log(IsCubeAssembled + " isComplete=" + IsComplete);

        if (IsCubeAssembled && onAssembled != null)
            onAssembled.Invoke(this);
    }

    public void Lock(bool isLocked = true)
    {
        this.isLocked = isLocked;
        items.ForEach(i => i.GetComponent<VRTouchObjectDistancedDrag>().Lock());
    }

    public void Disassemble(float dist, int countWrong = 0, int countWrongSides = 0)
    {
        this.dist = dist;
        Vector3 normal = (transform.position - Camera.main.transform.position).normalized;
        float angleStep = 360f / items.Count;

        if (countWrong > 0)
        {
            Randomizer rnd = new Randomizer(items.Count);
            for (int i = 0; i < countWrong; i++)
                items[rnd.GetRandom()].SetWrongIndicators(countWrongSides);
        }

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            float angle = i * angleStep;

            Quaternion rotation = Quaternion.AngleAxis(angle, normal);
            var dir = rotation * Camera.main.transform.right;

            item.SetBasePosition(item.transform.position);
            item.transform.position = item.BasePosition + dir * dist;

            var drag = item.GetComponent<VRTouchObjectDistancedDrag>();
            drag.lineMoveNormal = (item.BasePosition - item.transform.position).normalized;
        }
    }

    public void HideWrong()
    {
        items.ForEach(i => {
            if (i.IsWrong)
                i.gameObject.SetActive(false);
        });
    }
}
