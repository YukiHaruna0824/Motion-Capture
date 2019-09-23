using System.Collections;
using System.Collections.Generic;


using UnityEngine;
public struct Offset
{
    public float x;
    public float y;
    public float z;
};

public enum Channel
{
    XPOSITION,
    YPOSITION,
    ZPOSITION,
    ZROTATION,
    XROTATION,
    YROTATION
};

public class Joint
{
    public Joint Parent
    {
        get { return _parent; }
        set { _parent = value; }
    }
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public Offset Offset
    {
        get { return _offset; }
        set { _offset = value; }
    }

    public List<Channel> Channels_order
    {
        get { return _channels_order; }
        set { _channels_order = value; }
    }

    public List<Joint> Children
    {
        get { return _children; }
        set { _children = value; }
    }

    public List<List<float>> Channel_data
    {
        get { return _channel_data; }
        set { _channel_data = value; }
    }

    public int num_channels()
    {
        return _channels_order.Count;
    }

    public void Add_motion_data(List<float> data)
    {
        _channel_data.Add(data);
    }

    private Joint _parent;
    private string _name;
    private Offset _offset;
    private List<Channel> _channels_order = new List<Channel>();

    private List<Joint> _children = new List<Joint>();
    private List<List<float>> _channel_data = new List<List<float>>();
}

