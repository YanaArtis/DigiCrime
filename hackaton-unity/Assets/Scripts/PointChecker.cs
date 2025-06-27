using System.Collections.Generic;
using UnityEngine;

public class PointChecker
{
    public static Vector3 AdjustPointPosition(Vector3 point, List<Vector3> range, float border)
    {
        if (range == null || range.Count < 3)
        {
            Debug.LogError("Недостаточно точек для определения области.");
            return point;
        }

        bool isInside = IsPointInsidePolygon(point, range);
        if (!isInside)
        {
            point = ProjectPointInside(point, range, border);
        }
        else
        {
            point = EnsureBorderDistance(point, range, border);
        }

        return point;
    }

    public static bool IsPointInsidePolygon(Vector3 point, List<Transform> polygon)
    {
        List<Vector3> vertexes = new List<Vector3>();
        for (int i = 0; i < polygon.Count; i++)
            vertexes.Add(polygon[i].position);

        return IsPointInsidePolygon(point, vertexes);
    }

    public static bool IsPointInsidePolygon(Vector3 point, List<Vector3> polygon)
    {
        int intersections = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector3 a = polygon[i];
            Vector3 b = polygon[(i + 1) % polygon.Count];

            if ((a.z > point.z) != (b.z > point.z))
            {
                float t = (point.z - a.z) / (b.z - a.z);
                float xIntersection = a.x + t * (b.x - a.x);
                if (point.x < xIntersection)
                {
                    intersections++;
                }
            }
        }
        return (intersections % 2) != 0;
    }

    private static Vector3 ProjectPointInside(Vector3 point, List<Vector3> polygon, float border)
    {
        Vector3 closestPoint = point;
        float minDist = float.MaxValue;

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector3 a = polygon[i];
            Vector3 b = polygon[(i + 1) % polygon.Count];
            Vector3 projected = ProjectPointOnSegment(point, a, b);
            float dist = Vector3.Distance(point, projected);

            if (dist < minDist)
            {
                minDist = dist;
                closestPoint = projected;
            }
        }

        Vector3 direction = (point - closestPoint).normalized;
        return closestPoint + direction * border;
    }

    private static Vector3 EnsureBorderDistance(Vector3 point, List<Vector3> polygon, float border)
    {
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector3 a = polygon[i];
            Vector3 b = polygon[(i + 1) % polygon.Count];
            Vector3 projected = ProjectPointOnSegment(point, a, b);
            float dist = Vector3.Distance(point, projected);

            if (dist < border)
            {
                Vector3 direction = (point - projected).normalized;
                point = projected + direction * border;
            }
        }
        return point;
    }

    public static Vector3 ProjectPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / Vector3.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }

    public static Vector3 ProjectPointOnLine(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        Vector3 ap = point - a;
        float t = Vector3.Dot(ap, ab) / Vector3.Dot(ab, ab);
        return a + t * ab;
    }

    public static (bool crossed, Vector3 intersection) LineLineIntersection(Vector3 linePoint1, Vector3 lineDir1, Vector3 linePoint2, Vector3 lineDir2)
    {
        //crossed = false - прямые параллельны
        //crossed = true - прямые пересекаются (даже если отрезки не пересекаются)

        Vector3 intersection;
        Vector3 lineDir3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineDir1, lineDir2);
        Vector3 crossVec3and2 = Vector3.Cross(lineDir3, lineDir2);

        float planarFactor = Vector3.Dot(lineDir3, crossVec1and2);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < 0.0001f
                && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2)
                    / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineDir1 * s);
            return (true, intersection);
        }

        return (false, Vector3.zero);
    }

    public static (bool crossed, Vector3 intersection) SegSegIntersection(Vector3 segPoint11, Vector3 segPoint12, Vector3 segPoint21, Vector3 segPoint22)
    {
        //crossed = false - отрезки параллельны
        //crossed = true - отрезки пересекаются

        var intersection = LineLineIntersection(segPoint11, segPoint12 - segPoint11, segPoint21, segPoint22 - segPoint21);
        if (intersection.crossed)
        {
            float dist1 = Vector3.Distance(segPoint11, segPoint12);
            bool res1 = dist1 >= Vector3.Distance(segPoint11, intersection.intersection) && dist1 >= Vector3.Distance(segPoint12, intersection.intersection);

            float dist2 = Vector3.Distance(segPoint21, segPoint22);
            bool res2 = dist2 >= Vector3.Distance(segPoint21, intersection.intersection) && dist2 >= Vector3.Distance(segPoint22, intersection.intersection);

            var res = res1 && res2;
            return (res, res ? intersection.intersection : Vector3.zero);
        }

        return (false, Vector3.zero);
    }

    public static (bool cross, Vector3 point) FindPointOnLine(Vector3 pFrom, Vector3 pTo, Vector3 p1, Vector3 p2, float border)
    {
        //находит на прямой pFrom->pTo точку, которая находится на расстоянии border от прямой p1->p2

        Vector3 pFromProj = Vector3.Project(pFrom, p2 - p1);
        Vector3 dv = (pFrom - pFromProj).normalized * border;
        Vector3 pn1 = p1 + dv;
        Vector3 pn2 = p2 + dv;

        return LineLineIntersection(pFrom, pTo - pFrom, pn1, pn2 - pn1);
    }

    public static bool IsPointInSegment(Vector3 p, Vector3 p1, Vector3 p2, float precice = 1e-3f)
    {
        Vector3 dv1 = p - p1;
        Vector3 dv2 = p - p2;
        Vector3 dv = p2 - p1;

        float dd = Mathf.Abs(dv.magnitude - dv1.magnitude - dv2.magnitude);
        //Debug.Log(dd + " vs " + precice);

        return dd < precice;
    }

    public static float DistanceToLine(Vector3 point, Vector3 a, Vector3 b)
    {
        var proj = ProjectPointOnLine(point, a, b);
        return Vector3.Distance(point, proj);
    }
}
