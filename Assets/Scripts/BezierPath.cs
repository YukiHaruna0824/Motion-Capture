using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierPath : MonoBehaviour
{

    [HideInInspector]
    public List<BezierPoint> _ctrlPoint;
    [HideInInspector]
    public Transform[] nodes;

    [HideInInspector]
    public bool nodeChange;
    [HideInInspector]
    public List<Vector3> simulateBezierPath = new List<Vector3>();
    [HideInInspector]
    public float[] BezierSpeed;
    [HideInInspector]
    public int[] BezierArea;
    private int _area;

    [HideInInspector]
    public string fpath;

    [HideInInspector]
    public float moveSpeed = 1.0f;

    [HideInInspector]
    public int CameraMode = 0;
    [HideInInspector]
    public bool CameraFollowing = false;

    [HideInInspector]
    BoneGenerator boneGenerator;

    public void Preview()
    {
         if (nodes.Length == 1)
        {
            simulateBezierPath.Clear();
            return;
        }

        SimulateBezier();
    }

    private void SimulateBezier()
    {
        if (nodes == null)
            return;
        BezierSpeed = new float[nodes.Length - 1];
        BezierArea = new int[nodes.Length - 1];
        float[] tmpDistance = new float[nodes.Length - 1];
        float totalDistance = 0;
        float arcLength = 0.05f * moveSpeed;
        int totalStep = 300;
        if (simulateBezierPath != null)
            simulateBezierPath.Clear();

        // 模擬路徑
        for (int i = 0; i < nodes.Length - 1; i++)
        {
            tmpDistance[i] = 0;
            BezierArea[i] = 0;
            for (float j = 0; j < 1; j += 1f / totalStep)
                simulateBezierPath.Add(CalcBezier(nodes[i].position, nodes[i].GetChild(1).position,
                    nodes[i + 1].GetChild(0).position, nodes[i + 1].position, j));
            for (int j = i + 1; j < totalStep + i; j++)
                tmpDistance[i] += Vector3.Distance(simulateBezierPath[j + i * totalStep], simulateBezierPath[j + 1 + i * totalStep]);
            totalDistance += tmpDistance[i];
        }

        // 計算平均速度
        for (int i = 0; i < nodes.Length - 1; i++)
            BezierSpeed[i] = totalDistance / tmpDistance[i];

        // 省去多餘的路徑點&調整速度分配區段
        _area = 0;
        int count = 0;
        for (int i = 0; i < simulateBezierPath.Count - 1; i++)
        {
            if (Vector3.Distance(simulateBezierPath[i], simulateBezierPath[i + 1]) < arcLength)
            {
                simulateBezierPath.RemoveAt(i + 1);
                i--;
            }
            else
                BezierArea[_area]++;
            count++;
            if (count >= totalStep)
            {
                count = 0;
                if (_area + 1 == BezierArea.Length - 1)
                    BezierArea[BezierArea.Length - 1] = simulateBezierPath.Count;
                else if (BezierArea.Length == 1)
                    BezierArea[0] = simulateBezierPath.Count;
                else
                {
                    BezierArea[_area + 1] += BezierArea[_area];
                    _area++;
                }
            }
        }

        count = Mathf.CeilToInt(BezierSpeed[_area]) == 0 ? 1 : Mathf.CeilToInt(BezierSpeed[_area]);
        List<Vector3> tmpPath = new List<Vector3>();
        // 針對點與點距離過遠進行補償
        for (int i = 0; i < simulateBezierPath.Count - 1; i += count)
        {
            if (i >= BezierArea[_area])
            {
                _area++;
                count = Mathf.CeilToInt(BezierSpeed[_area]) == 0 ? 1 : Mathf.CeilToInt(BezierSpeed[_area]);
            }
            tmpPath.Add(simulateBezierPath[i]);
            if (i + count < simulateBezierPath.Count && Vector3.Distance(simulateBezierPath[i], simulateBezierPath[i + count]) > arcLength * 2)
            {
                Vector3 n = (simulateBezierPath[i] + simulateBezierPath[i + count]) / 2;
                tmpPath.Add(n);
            }
            else if (i + count >= simulateBezierPath.Count)
            {
                tmpPath.Add(simulateBezierPath[simulateBezierPath.Count - 1]);
            }
        }
        simulateBezierPath = tmpPath;
         
        CutBezier();
    }

    private void CutBezier()
    {

        List<Vector3> _simulateBezierPath = new List<Vector3>();
        float basePieceLength = 0.1f;
        float _pieceLength;

        _simulateBezierPath.Clear();

        float borrow = 0f, remain = 0f, pieces = 0f;
        Vector3 first, second = Vector3.zero;

        //2nd Pass: 計算平均速度
        for (int i = 0; i < simulateBezierPath.Count - 1; i++)
        {
            _pieceLength = basePieceLength;

            if (i == 0)
            {
                first = simulateBezierPath[0];
                second = simulateBezierPath[1];
            }
            else
            {
                first = second;
                second = simulateBezierPath[i + 1];
            }

            float dist = Vector3.Distance(first, second) - borrow;

            pieces = Mathf.Floor(dist / _pieceLength);
            remain = dist - pieces * _pieceLength;
            borrow = _pieceLength - remain;

            Vector3 dir = (second - first).normalized;
            second = first + dir * (dist + borrow);

            pieces += 1;

            for (int j = 0; j < (int)pieces; j++)
            {
                _simulateBezierPath.Add(first * ((pieces - j) / pieces)
                    + second * (j / pieces));
            }
        }

        simulateBezierPath = _simulateBezierPath;
    }

    private Vector3 CalcBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float x = p0.x * Mathf.Pow(1 - t, 3)
            + 3 * p1.x * Mathf.Pow(1 - t, 2) * t
            + 3 * p2.x * (1 - t) * t * t
            + p3.x * Mathf.Pow(t, 3);
        float y = p0.y * Mathf.Pow(1 - t, 3)
            + 3 * p1.y * t * Mathf.Pow(1 - t, 2)
            + 3 * p2.y * Mathf.Pow(t, 2) * (1 - t)
            + p3.y * Mathf.Pow(t, 3);
        float z = p0.z * Mathf.Pow(1 - t, 3)
            + 3 * p1.z * t * Mathf.Pow(1 - t, 2)
            + 3 * p2.z * Mathf.Pow(t, 2) * (1 - t)
            + p3.z * Mathf.Pow(t, 3);
        return new Vector3(x, y, z);
    }

    // Start is called before the first frame update
    void Start()
    {

        boneGenerator = GetComponentInChildren<BoneGenerator>();
        if (boneGenerator.Parse(fpath) == 0)
        {
            boneGenerator.SetPath(simulateBezierPath);
            boneGenerator.GenerateJointBone();
            boneGenerator.Play();
        }
        else
            Debug.Log("Bvh Data Error!");
    }

    // Update is called once per frame
    void Update()
    {
        List<Vector3> bpath = boneGenerator.GetPath();
        if (CameraFollowing)
            switch (CameraMode)
            {
                case 0:
                    Camera.main.transform.rotation = Quaternion.Euler(0, 180, 0);
                    Camera.main.transform.position = new Vector3(8, 16, 25);
                    break;
                case 1:
                    if (bpath.Count > 0)
                    {
                        Vector3 backVector = new Vector3(0, 0, 0);
                        int index = boneGenerator.GetPathIndex();
                        if (index + 1 < bpath.Count)
                            backVector = bpath[index] - bpath[index + 1];
                        else
                            backVector = bpath[index - 1] - bpath[index];
                        backVector.Normalize();
                        backVector *= 6f;
                        Camera.main.transform.position = bpath[index] + new Vector3(0, 5, 0) + backVector;
                        Camera.main.transform.LookAt(bpath[index] + backVector * -4f, Vector3.up);
                    }
                    break;
                case 2:
                    if (bpath.Count > 0)
                    {
                        int index = boneGenerator.GetPathIndex();
                        Camera.main.transform.position = bpath[index] + new Vector3(0, 8, 0) ;
                        Camera.main.transform.LookAt(bpath[index], Vector3.up);
                    }
                    break;
                case 3:
                    if (bpath.Count > 0)
                    {
                        Vector3 frontVector, sideVector;
                        frontVector = sideVector = new Vector3(0, 0, 0);
                        int index = boneGenerator.GetPathIndex();
                        if (index + 1 < bpath.Count)
                            frontVector = bpath[index + 1] - bpath[index];
                        else
                            frontVector = bpath[index] - bpath[index - 1];

                        sideVector = Vector3.Cross(frontVector, Vector3.up);

                        sideVector.Normalize();
                        sideVector *= 12f;
                        Camera.main.transform.position = bpath[index] + new Vector3(0, 5, 0) + sideVector;
                        Camera.main.transform.LookAt(bpath[index] + sideVector * -3f, Vector3.up);
                    }
                    break;
                case 4:
                    if (bpath.Count > 0)
                    {
                        Vector3 frontVector, sideVector;
                        frontVector = sideVector = new Vector3(0, 0, 0);
                        int index = boneGenerator.GetPathIndex();
                        if (index + 1 < bpath.Count)
                            frontVector = bpath[index + 1] - bpath[index];
                        else
                            frontVector = bpath[index] - bpath[index - 1];

                        sideVector = Vector3.Cross(frontVector, Vector3.up);

                        sideVector.Normalize();
                        sideVector *= -12f;
                        Camera.main.transform.position = bpath[index] + new Vector3(0, 5, 0) + sideVector;
                        Camera.main.transform.LookAt(bpath[index] + sideVector * -3f, Vector3.up);
                    }
                    break;
            }
    }
}
