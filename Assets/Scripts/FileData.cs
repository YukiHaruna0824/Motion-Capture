using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[Serializable]
public struct BezierPoint
{
    public Vector3 up, down, ori;

    public BezierPoint(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        up = v1;
        down = v2;
        ori = v3;
    }
    public void SetOri(Vector3 v) {
        ori = v;
    }
};

[Serializable]
public class PathData {
    public int _pointLength;
    public List<BezierPoint> _ctrlPoint;

    public PathData(List<BezierPoint> ctrlPoint) {
        _pointLength = ctrlPoint.Count;
        _ctrlPoint = ctrlPoint;
    }
}
