using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Range
{
    List<Vector3> points;    //сначала самая длинная сторона "снизу-вверх", потом все остальные
    Vector3 place;           //место для игрока

    public List<Vector3> Points
    {
        get => points;
    }

    public Vector3 Place
    {
        get => place;
    }

    public Vector3 Dir
    {
        get => (points[1] - points[0]).normalized;
    }

    public Vector3 DirW
    {
        get => (Quaternion.AngleAxis(90, Vector3.up) * Dir).normalized;
    }

    public float Height
    {
        get => (points[1] - points[0]).magnitude;
    }

    public float Width
    {
        get => (points[2] - points[1]).magnitude;
    }

    public Vector3 Center
    {
        //get => (points[1] - points[0]) / 2f + (points[2] - points[1]) / 2f;
        get => new Vector3(
            points.Average(p => p.x),
            points.Average(p => p.y),
            points.Average(p => p.z)
        );
    }

    public Range(List<Vector3> points, Vector3 place)
    {
        //Debug.Log("before: " + Tools.ListToString(points));

        this.place = place;
        this.points = points;

        var dir = GetDir(points, place);
        Sort(dir);

        //Debug.Log("after: " + Tools.ListToString(this.points));
    }

    public void SetPlace(Vector3 place)
    {
        this.place = place;
    }

    public void Sort(Vector3 dir)
    {
        points = Sort(points, dir);

        if (IsComplanar(points[2] - points[1], DirW) != 1)
        {
            //если порядок не в то сторону, то переворачиваем
            points.Reverse();
            points = Sort(points, dir);
        }
    }

    public bool AbjustPlace(float border)
    {
        //true - если пришлось вписывать, false - если точка осталась на месте

        var place = this.place;
        this.place = PointChecker.AdjustPointPosition(place, points, border);// FitToRange(points, this.place, border);
        return place != this.place;
    }

    public void Move(Vector3 dv)
    {
        place += dv;
        for (int i = 0; i < points.Count; i++)
            points[i] += dv;
    }

    public void Rotate(float angle)
    {
        Rotate(angle, Center);
    }

    public void Rotate(float angle, Vector3 center)
    {
        for (int i = 0; i < points.Count; i++)
            points[i] = center + Quaternion.AngleAxis(angle, Vector3.up) * (points[i] - center);

        place = center + Quaternion.AngleAxis(angle, Vector3.up) * (place - center);

        var newDir = Quaternion.AngleAxis(angle, Vector3.up) * Dir;
        points = Sort(points, newDir);
    }

    public static Vector3 GetDir(List<Vector3> points, Vector3 place)
    {
        //выбираем самую длинную сторону
        //направление выбираем так, чтобы от place до кончика вектора было бОльшее расстояние, чем до начала

        if (points != null && points.Count > 3)
        {
            Vector3 dirMax = points[1] - points[0];

            for (int i = 0; i < points.Count; i++)
            {
                int n1 = i;
                int n2 = i < points.Count - 1 ? i + 1 : 0;
                Vector3 dir = points[n2] - points[n1];

                if (dir.magnitude > dirMax.magnitude)
                {
                    float s1 = Vector3.Project(place - points[n1], dir).magnitude;
                    float s2 = Vector3.Project(place - points[n2], dir).magnitude;

                    if (s1 > s2)
                        dir = -dir;

                    dirMax = dir;
                }
            }

            return dirMax.normalized;
        }

        return Vector3.zero;
    }

    public static List<Vector3> Sort(List<Vector3> points, Vector3 dir)
    {
        //пересобирает список так, чтобы первые 2 точки были вдоль dir

        //находим первый отрезок, который направлен вдоль dir
        int nFrom = 0;
        int nTo = 1;

        for (int i = 0; i < points.Count; i++)
        {
            int n1 = i;
            int n2 = i < points.Count - 1 ? i + 1 : 0;
            Vector3 dirThis = (points[n2] - points[n1]).normalized;

            int isComplanar = IsComplanar(dir, dirThis);
            switch (isComplanar)
            {
                case 1:
                    nFrom = n1;
                    nTo = n2;
                    break;

                case -1:
                    nFrom = n2;
                    nTo = n1;
                    break;
            }

            if (isComplanar != 0)
                break;
        }

        int di = nTo - nFrom;

        List<Vector3> res = new List<Vector3>();
        res.Add(points[nFrom]);

        int iTo = nFrom - di;
        if (iTo < 0)
            iTo = points.Count - 1;

        if (iTo > points.Count - 1)
            iTo = 0;

        int index = nFrom;

        do
        {
            index += di;

            if (index < 0)
                index = points.Count - 1;

            if (index > points.Count - 1)
                index = 0;

            res.Add(points[index]);
        }
        while (index != iTo);

        return res;
    }

    public static int IsComplanar(Vector3 v1, Vector3 v2)
    {
        //0 - угол межу векторами больше 0.1 градуса
        //1 - сонаправлены
        //-1 - направлены в разные стороны

        float angle = Mathf.Abs(Vector3.Angle(v1, v2));

        if (angle <= 0.1f)
            return 1;

        if (Mathf.Abs(180 - angle) <= 0.1f)
            return -1;

        return 0;
    }

    public (int n1, int n2) GetSide(Vector3 dir)
    {
        for (int i = 0; i < points.Count; i++)
        {
            int n1 = i;
            int n2 = i < points.Count - 1 ? i + 1 : 0;

            if (IsComplanar(dir, points[n2] - points[n1]) == 1)
                return (n1, n2);
        }

        return (0, 0);
    }

    public bool IsInRange(Vector3 point)
    {
        return IsPointInsidePolygon(point, points);
    }

    public bool IsInRange(List<Vector3> points)
    {
        foreach (var point in points)
            if (!IsPointInsidePolygon(point, points))
                return false;
        return true;
    }

    public static bool IsPointInRange(Vector3 dot, Range range)
    {
        return IsPointInsidePolygon(dot, range.points);
    }

    public static bool IsPointInsidePolygon(Vector3 dot, List<Vector3> points)
    {
        return PointChecker.IsPointInsidePolygon(dot, points);
    }

    new public string ToString()
    {
        string res = "{width=" + Width.ToString() + ", height=" + Height.ToString() + ", place=" + place.ToString() + ", dir=" + Dir.ToString() + ", center=" + Center.ToString() + "}";
        return res;
    }
}
