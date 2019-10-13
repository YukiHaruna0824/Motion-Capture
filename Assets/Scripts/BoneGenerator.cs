using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneGenerator : MonoBehaviour
{
    public GameObject bonePrefab;
    public GameObject jointPrefab;

    private List<GameObject> _joints = new List<GameObject>();
    private List<GameObject> _bones = new List<GameObject>();

    private Bvh _bvh;
    private BvhParser _bp;
    private int _frameIndex;
    private int _pathIndex;

    private List<Vector3> _bpath = new List<Vector3>();

    public bool isPaused = false;

    public int GetPathIndex()
    {
        return _pathIndex;
    }

    public void Parse(string info)
    {
        _bvh = new Bvh();
        _bp = new BvhParser();
        _frameIndex = 0;
        _pathIndex = 0;

        _bp.GetTokenInfo(info);
        if(_bp.Parse(_bvh) == 0)
        {
            _bvh.Cal();
        }
    }

    public void SetPath(List<Vector3> path)
    {
        _bpath = path;
    }

    public List<Vector3> GetPath()
    {
        return _bpath;
    }

    public void GenerateJointBone()
    {
        //create joint
        foreach (Joint joint in _bvh.Joints)
        {
            GameObject jointObj = Instantiate(jointPrefab);
            jointObj.name = joint.Name;

            if (joint.Parent != null)
            {
                foreach (GameObject obj in _joints)
                {
                    if (obj.name == joint.Parent.Name)
                    {
                        jointObj.transform.parent = obj.transform;
                        break;
                    }
                }
            }
            else
                jointObj.transform.parent = this.transform;
            _joints.Add(jointObj);
        }

        //Create Joint related bone
        int index = 0;
        foreach (Joint joint in _bvh.Joints)
        {
            foreach (Joint child in joint.Children)
            {
                Vector3 midpoint = new Vector3(child.Offset.x / 2, child.Offset.y / 2, child.Offset.z / 2);
                if (midpoint.magnitude < 0.5)
                    continue;

                GameObject childObj = null;
                foreach (GameObject obj in _joints)
                {
                    if (child.Name == obj.name)
                    {
                        childObj = obj;
                        break;
                    }
                }

                GameObject boneObj = Instantiate(bonePrefab);
                boneObj.transform.parent = _joints[index].transform;
                boneObj.name = joint.Name + "_" + child.Name;
                boneObj.transform.localPosition = midpoint;
                boneObj.transform.localScale = new Vector3(0.75f, 0.75f, midpoint.magnitude * 2 - 1);
                boneObj.transform.LookAt(_joints[index].transform);
            }
            index++;
        }
    }

    public void Play()
    {
        StartCoroutine(PlayAnimation());
    }

    private void UpdateJointBone(int frame, int pathIndex)
    {
        int index = 0;
        foreach (Joint joint in _bvh.Joints)
        {
            _joints[index].transform.localPosition = (index == 0) ? new Vector3(0, 0, 0) : joint.LocalPos[frame];
            _joints[index++].transform.localRotation = Quaternion.Euler(joint.LocalRot[frame]);
        }

        transform.position = _bpath[pathIndex];
        if (_pathIndex != _bpath.Count - 1)
            transform.LookAt(_bpath[(pathIndex + 1) % _bpath.Count], Vector3.up);
    }

    IEnumerator PlayAnimation()
    {
        while (true)
        {

            UpdateJointBone(_frameIndex, _pathIndex);
            yield return new WaitForSeconds((float)_bvh.Frame_time);
            if (!isPaused)
            {
                _frameIndex = (++_frameIndex) % _bvh.Num_frames;
                _pathIndex = (++_pathIndex) % _bpath.Count;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
