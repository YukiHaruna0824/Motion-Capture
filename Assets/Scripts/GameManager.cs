using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject ball;
    public GameObject linePrefab;
    public GameObject bonePrefab;

    public GameObject boneGenerator;
    public GameObject jointGenerator;

    private List<LineRenderer> _lightbones = new List<LineRenderer>();
    private List<GameObject> _joints = new List<GameObject>();
    private List<GameObject> _bones = new List<GameObject>();

    private Bvh _bvh;
    private BvhParser _bp;
    private int _frameIndex;

    void Start()
    {
        _bvh = new Bvh();
        _bp = new BvhParser();
        _frameIndex = 0;

        Parse();

        //Generate By World Transform Matrix

        /*
        GenerateJointBone();
        StartCoroutine(PlayAnimation());
        */

        //Generate By Local 
        GenerateJointBone1();
        StartCoroutine(PlayAnimation());
    }

    void Update()
    {

    }

    IEnumerator PlayAnimation()
    {
        while (true)
        {
            //UpdateJointBone(_frameIndex);
            UpdateJointBone1(_frameIndex);
            yield return new WaitForSeconds((float)_bvh.Frame_time);
            _frameIndex = (++_frameIndex) % _bvh.Num_frames;
        }
    }

    private void GenerateJointBone()
    {
        //Create Joint
        foreach(Joint joint in _bvh.Joints)
        {
            GameObject jointObj = Instantiate(ball);
            jointObj.name = joint.Name;
            jointObj.transform.parent = jointGenerator.transform;
            _joints.Add(jointObj);
        }
        
        //Create Joint related bone
        foreach (Joint joint in _bvh.Joints)
        {
            foreach (Joint child in joint.Children)
            {
                GameObject boneObj = Instantiate(linePrefab);
                boneObj.transform.parent = boneGenerator.transform;
                boneObj.name = joint.Name + "_" + child.Name;

                LineRenderer bone = boneObj.GetComponent<LineRenderer>();
                bone.SetColors(Color.red, Color.red);
                bone.SetWidth(0.5f, 0.5f);
                _lightbones.Add(bone);
            }
        }
        
    }

    private void UpdateJointBone(int frame)
    {
        int index = 0;
        foreach (Joint joint in _bvh.Joints)
        {
            _joints[index].transform.position = joint.Pos[frame];
            _joints[index].transform.rotation = joint.Rot[frame];
            index++;
        }

        index = 0;
        foreach (Joint joint in _bvh.Joints)
        {
            foreach (Joint child in joint.Children)
            {
                _lightbones[index].SetPosition(0, joint.Pos[frame]);
                _lightbones[index].SetPosition(1, child.Pos[frame]);
                index++;
            }
        }
        
    }

    private void GenerateJointBone1()
    {
        //create joint
        foreach (Joint joint in _bvh.Joints)
        {
            GameObject jointObj = Instantiate(ball);
            jointObj.name = joint.Name;

            if(joint.Parent != null)
            {
                foreach(GameObject obj in _joints)
                {
                    if(obj.name == joint.Parent.Name)
                    {
                        jointObj.transform.parent = obj.transform;
                        break;
                    }
                }
            }
            else
                jointObj.transform.parent = jointGenerator.transform;
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

    private void UpdateJointBone1(int frame)
    {
        int index = 0;
        foreach (Joint joint in _bvh.Joints)
        {
            _joints[index].transform.localPosition = (index == 0) ? new Vector3(0, 0, 0) : joint.LocalPos[frame];
            _joints[index++].transform.localRotation = Quaternion.Euler(joint.LocalRot[frame]);
        }
    }

    private void Parse()
    {
        string fp = System.IO.Path.Combine(Application.streamingAssetsPath, "example.bvh");
        Debug.Log(fp);

        _bp.GetTokenInfo(fp);
        if (_bp.Parse(_bvh) == 0)
        {
            _bvh.Cal();
            /*_bvh.GetJoinTPosInfo();
            bvh.GetAllJointInfo();
            bvh.GetFrameInfo();*/
        }
    }
}
