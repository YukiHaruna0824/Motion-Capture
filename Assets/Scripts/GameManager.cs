using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject ball;
    public GameObject linePrefab;

    public GameObject boneGenerator;
    public GameObject jointGenerator;

    private List<LineRenderer> bones = new List<LineRenderer>();
    private List<GameObject> joints = new List<GameObject>();

    private Bvh _bvh;
    private BvhParser _bp;
    private int _frameIndex;

    void Start()
    {
        _bvh = new Bvh();
        _bp = new BvhParser();
        _frameIndex = 0;

        Parse();
        GenerateJointBone();
        StartCoroutine(PlayAnimation());
    }

    void Update()
    {

    }

    IEnumerator PlayAnimation()
    {
        while (true)
        {
            UpdateJointBone(_frameIndex);
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
            joints.Add(jointObj);
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
                bones.Add(bone);
            }
        }
        
    }

    private void UpdateJointBone(int frame)
    {
        int index = 0;
        foreach (Joint joint in _bvh.Joints)
        {
            joints[index].transform.position = joint.Pos[frame];
            joints[index].transform.rotation = joint.Rot[frame];
            index++;
        }

        index = 0;
        foreach (Joint joint in _bvh.Joints)
        {
            foreach (Joint child in joint.Children)
            {
                bones[index].SetPosition(0, joint.Pos[frame]);
                bones[index].SetPosition(1, child.Pos[frame]);
                index++;
            }
        }
        
    }

    public void Parse()
    {
        string fp = System.IO.Path.Combine(Application.streamingAssetsPath, "walk_01.bvh");
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
