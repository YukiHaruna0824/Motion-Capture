using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

public class Bvh
{
    public Joint Root_joint
    {
        get { return _root_joint; }
        set { _root_joint = value; }
    }

    public List<Joint> Joints
    {
        get { return _joints; }
        set { _joints = value; }
    }

    public int Num_frames
    {
        get { return _num_frames; }
        set { _num_frames = value; }
    }

    public double Frame_time
    {
        get { return _frame_time; }
        set { _frame_time = value; }
    }

    public int Num_channels
    {
        get { return _num_channels; }
        set { _num_channels = value; }
    }

    public void Add_joint(Joint joint)
    {
        _joints.Add(joint);
        _num_channels += joint.num_channels();
    }

    public void GetAllJointInfo()
    {
        foreach (Joint joint in _joints)
        {
            Debug.Log("Joint : " + joint.Name);
            Debug.Log("Joint Parent: " + joint.Parent.Name);
            Debug.Log("Joint Offset: " + joint.Offset.x + " " + joint.Offset.y + " " + joint.Offset.z);

            Debug.Log("Joint Channel order : ");
            for (int i = 0; i < joint.num_channels(); i++)
                Debug.Log(" " + joint.Channels_order[i]);

            Debug.Log("Joint Children : ");
            for (int i = 0; i < joint.Children.Count; i++)
                Debug.Log(joint.Children[i].Name + " ");

            for (int i = 0; i < joint.Channel_data.Count; i++)
            {
                Debug.Log("Frame " + i + " :");
                for (int j = 0; j < joint.Channel_data[i].Count; j++)
                {
                    Debug.Log(" " + joint.Channel_data[i][j]);
                }
            }

            Debug.Log("----------------------------------------------------");
        }
    }

    public void GetFrameInfo()
    {
        Debug.Log("Frame numbers : " + _num_frames);
        Debug.Log("Frame time : " + _frame_time);
        Debug.Log("Channel numbers : " + _num_channels);
    }

    private Joint _root_joint;
    private List<Joint> _joints = new List<Joint>();
    private int _num_frames = 0;
    private double _frame_time = 0;
    private int _num_channels = 0;
}
