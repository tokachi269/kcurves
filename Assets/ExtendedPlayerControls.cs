using UnityEngine;

struct ExtendedPlayerControls
{
    Vector3 top;
    Vector3[] ps;
    Vector3 bottom;

    public Vector3 this[int i]
    {
        get => i == 0 ? top : i <= ps.Length ? ps[i - 1] : bottom;
        set
        {
            if (i == 0) top = value;
            else if (i <= ps.Length) ps[i - 1] = value;
            else bottom = value;
        }
    }

    public ExtendedPlayerControls(Vector3[] ps, BezierControls cs)
    {
        top = cs[cs.SegmentCount - 1, 1];
        this.ps = ps;
        bottom = cs[0, 1];
    }
}
struct ExtendedBezierControls
{
    Vector3 top;
    Vector3[] cs;
    Vector3 bottom;

    public Vector3 this[int i]
    {
        get => i == 0 ? top : i <= cs.Length / 2 ? cs[i * 2 - 1] : bottom;
        set
        {
            if (i == 0) top = value;
            else if (i <= cs.Length / 2) cs[i * 2 - 1] = value;
            else bottom = value;
        }
    }

    public ExtendedBezierControls(BezierControls cs)
    {
        top = cs[cs.SegmentCount - 1, 1];
        this.cs = cs.Points;
        bottom = cs[0, 1];
    }
}