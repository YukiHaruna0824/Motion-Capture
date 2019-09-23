using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

public enum Axis
{
    X, Y ,Z
}

public static class MatrixUtils
{
    public static void Translate(ref Matrix4x4 mat, Vector3 vec)
    {
        mat.m03 += vec.x;
        mat.m13 += vec.y;
        mat.m23 += vec.z;
    }

    public static void Rotate(ref Matrix4x4 mat, float angle, Axis axis)
    {
        angle *= Mathf.Deg2Rad;
        float sina = Mathf.Sin(angle);
        float cosa = Mathf.Cos(angle);
        Matrix4x4 rot = Matrix4x4.identity;

        if(axis == Axis.X)
        {
            rot.m11 = cosa;
            rot.m12 = -sina;
            rot.m21 = sina;
            rot.m22 = cosa;
        }
        else if(axis == Axis.Y)
        {
            rot.m00 = cosa;
            rot.m02 = -sina;
            rot.m20 = sina;
            rot.m22 = cosa;
        }
        else if(axis == Axis.Z)
        {
            rot.m00 = cosa;
            rot.m01 = -sina;
            rot.m10 = sina;
            rot.m11 = cosa;
        }
        mat = mat * rot;
    }

    public static Vector3 ExtractPosition(Matrix4x4 mat)
    {
        return new Vector3(mat.m03, mat.m13, mat.m23);
    }


}

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

    public void Cal()
    {
        Calculate(ref _root_joint);
    }

    public void Calculate(ref Joint startJoint)
    {
        Vector3 offset = new Vector3(startJoint.Offset.x, startJoint.Offset.y, startJoint.Offset.z);
        Matrix4x4 offsetMat = Matrix4x4.Translate(offset);

        List<List<float>> data = startJoint.Channel_data;

        for(int i = 0; i < _num_frames; i++)
        {
            Matrix4x4 tran_mat = Matrix4x4.identity;
            Matrix4x4 rot_mat = Matrix4x4.identity;

            for (int j = 0; j < startJoint.Channels_order.Count; j++)
            {
                Channel channel_info = startJoint.Channels_order[j];
                if (channel_info == Channel.XPOSITION)
                    MatrixUtils.Translate(ref tran_mat, new Vector3(data[i][j], 0, 0));
                else if (channel_info == Channel.YPOSITION)
                    MatrixUtils.Translate(ref tran_mat, new Vector3(0, data[i][j], 0));
                else if (channel_info == Channel.ZPOSITION)
                    MatrixUtils.Translate(ref tran_mat, new Vector3(0, 0, data[i][j]));
                else if (channel_info == Channel.XROTATION)
                    MatrixUtils.Rotate(ref rot_mat, data[i][j], Axis.X);
                else if (channel_info == Channel.YROTATION)
                    MatrixUtils.Rotate(ref rot_mat, data[i][j], Axis.Y);
                else if (channel_info == Channel.ZROTATION)
                    MatrixUtils.Rotate(ref rot_mat, data[i][j], Axis.Z);
            }

            Matrix4x4 ltm;  //Local Transform Matrix
            if (startJoint.Parent != null)
                ltm = startJoint.Parent.Ltm[i] * offsetMat;
            else
                ltm = tran_mat * offsetMat;

            Vector3 pos = MatrixUtils.ExtractPosition(ltm);
            startJoint.Set_pos(pos, i);

            ltm = ltm * rot_mat;
            startJoint.Set_ltm(ltm, i);       
        }
        for (int i = 0; i < startJoint.Children.Count; i++)
        {
            Joint child = startJoint.Children[i];
            Calculate(ref child);
        }
    }

    public void GetAllJointInfo()
    {
        foreach (Joint joint in _joints)
        {
            Debug.Log("Joint : " + joint.Name);
            if(joint.Parent != null)
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

    public void GetJoinTPosInfo()
    {
        foreach(Joint joint in _joints)
        {
            Debug.Log("Joint Local Transform Matrix :" + joint.Ltm[0]);
            Debug.Log("Joint World Position :" + joint.Pos[0]);
            Debug.Log("---------------------");
        }
    }

    private Joint _root_joint;
    private List<Joint> _joints = new List<Joint>();
    private int _num_frames = 0;
    private double _frame_time = 0;
    private int _num_channels = 0;
}
