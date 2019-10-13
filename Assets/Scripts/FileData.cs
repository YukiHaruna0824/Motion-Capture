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
public class LevelData
{
    public PathData[] _paths;

    public LevelData(BezierPath[] paths)
    {
        _paths = new PathData[paths.Length];
        for (int i = 0; i < paths.Length; i++)
            _paths[i] = new PathData(paths[i]._ctrlPoint,paths[i].fpath);
    }
}

[Serializable]
public class PathData {
    public int _pointLength;
    public List<BezierPoint> _ctrlPoint;
    public string _fpath;

    public PathData(List<BezierPoint> ctrlPoint,string fpath) {
        _fpath = fpath;
        _pointLength = ctrlPoint.Count;
        _ctrlPoint = ctrlPoint;
    }
}
