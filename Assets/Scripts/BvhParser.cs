using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

using UnityEngine;

public class KeyWord
{
    public const string kChannels = "CHANNELS";
    public const string kEnd = "End";
    public const string kEndSite = "End Site";
    public const string kFrame = "Frame";
    public const string kFrames = "Frames:";
    public const string kHierarchy = "HIERARCHY";
    public const string kJoint = "JOINT";
    public const string kMotion = "MOTION";
    public const string kOffset = "OFFSET";
    public const string kRoot = "ROOT";

    public const string kXpos = "Xposition";
    public const string kYpos = "Yposition";
    public const string kZpos = "Zposition";
    public const string kXrot = "Xrotation";
    public const string kYrot = "Yrotation";
    public const string kZrot = "Zrotation";
}

public class BvhParser
{

    public void GetTokenInfo(string path)
    {
        if (File.Exists(path))
        {
            string text = File.ReadAllText(path);
            char[] delimiter = { ' ', '\t', '\n', '\r' };
            string[] token = text.Split(delimiter);
            foreach (string tok in token)
            {
                if (!string.IsNullOrEmpty(tok))
                    tokens.Add(tok);
            }
        }
        else
        {
            if (_debug)
                Debug.Log("File Open Failed");
        }
    }

    public int Parse(Bvh bvh)
    {
        _bvh = bvh;

        string token = tokens[tokenIndex];  //get Hierarchy
        tokenIndex++;
        if (token == KeyWord.kHierarchy)
        {
            int ret = Parse_Hierarchy();
            if (ret < 0)
            {
                if (_debug)
                    Debug.Log("Parsing Hierarchy Error!");
                return ret;
            }

        }
        else
        {
            if (_debug)
                Debug.Log("Bad structure of .bvh file. " + KeyWord.kHierarchy + " should be on the top of the file");
            return -1;
        }

        if (_debug)
            Debug.Log("Successfully parse file");
        return 0;
    }

    private int Parse_Hierarchy()
    {
        if (CheckTokenIndex())
        {
            string token = tokens[tokenIndex];
            tokenIndex++;

            if (token == KeyWord.kRoot)
            {
                Joint rootJoint = new Joint();
                Joint emptyJoint = null;

                int ret = Parse_Joint(ref emptyJoint, ref rootJoint);
                if (ret < 0)
                {
                    if (_debug)
                        Debug.Log("Parsing Joint Error !");
                    return ret;
                }
                _bvh.Root_joint = rootJoint;
            }
            else
            {
                if (_debug)
                    Debug.Log("Bad structure of .bvh file. Expected " + KeyWord.kRoot + ", but found " + token);
                return -1;
            }


            //Parsing Motion
            token = tokens[tokenIndex];
            if (token == KeyWord.kMotion)
            {
                int ret = Parsing_Motion();
                if (ret < 0)
                {
                    if (_debug)
                        Debug.Log("Parsing Motion Error !");
                    return ret;
                }
            }
            else
            {
                if (_debug)
                    Debug.Log("Bad structure of .bvh file. Expected " + KeyWord.kMotion + ", but found " + token);
                return -1;
            }
        }
        return 0;
    }

    private int Parse_Joint(ref Joint parent, ref Joint parsed)
    {
        if (!CheckTokenIndex())
            return -1;

        string name = tokens[tokenIndex];
        tokenIndex += 2;    //Consuming {
        Joint joint = new Joint()
        {
            Parent = parent,
            Name = name
        };

        if (_debug)
            Debug.Log("Joint name : " + joint.Name);

        if (!CheckTokenIndex())
            return -1;

        string token = tokens[tokenIndex];
        tokenIndex += 3;    //get offset
        if (!CheckTokenIndex())
        {
            if (_debug)
                Debug.Log("Failure Parsing " + joint.Name + " Offset Error !");
            return -1;
        }

        //Offset Parsing
        if (token == KeyWord.kOffset)
        {
            Offset offset;
            offset.x = float.Parse(tokens[tokenIndex - 2]);
            offset.y = float.Parse(tokens[tokenIndex - 1]);
            offset.z = float.Parse(tokens[tokenIndex]);
            joint.Offset = offset;
            tokenIndex++;
        }
        else
        {
            if (_debug)
                Debug.Log("Bad structure of .bvh file. Expected " + KeyWord.kOffset + ", but found " + token);
            return -1;
        }

        if (!CheckTokenIndex())
            return -1;

        //Channel Parsing
        token = tokens[tokenIndex];
        tokenIndex++;

        if (token == KeyWord.kChannels)
        {
            int ret = Parsing_Channel_Order(joint);
            //Parsing channel Error
            if (ret < 0)
                return ret;
            if (_debug)
                Debug.Log("Joint has " + joint.num_channels() + " data channels");
        }
        else
        {
            if (_debug)
                Debug.Log("Bad structure of .bvh file. Expected " + KeyWord.kChannels + ", but found " + token);
            return -1;
        }

        _bvh.Add_joint(joint);

        //Parsing Children Joint
        List<Joint> children = new List<Joint>();


        while (CheckTokenIndex())
        {
            if (!CheckTokenIndex())
                return -1;

            token = tokens[tokenIndex];
            tokenIndex++;

            if (token == KeyWord.kJoint)
            {
                //Parsing Child
                Joint child = new Joint();
                int ret = Parse_Joint(ref joint, ref child);
                children.Add(child);
            }
            else if (token == KeyWord.kEnd)
            {
                tokenIndex += 2;    //consuming "Site" and "{"

                //Parsing EndSite
                Joint tempJoint = new Joint()
                {
                    Parent = joint,
                    Name = KeyWord.kEndSite
                };
                children.Add(tempJoint);

                if (!CheckTokenIndex())
                    return -1;

                token = tokens[tokenIndex];
                tokenIndex += 3;    //get offset
                if (!CheckTokenIndex())
                {
                    if (_debug)
                        Debug.Log("Failure Parsing " + joint.Name + " Offset Error !");
                    return -1;
                }

                if (token == KeyWord.kOffset)
                {
                    Offset offset;
                    offset.x = float.Parse(tokens[tokenIndex - 2]);
                    offset.y = float.Parse(tokens[tokenIndex - 1]);
                    offset.z = float.Parse(tokens[tokenIndex]);
                    tempJoint.Offset = offset;
                    tokenIndex++;
                }
                else
                {
                    if (_debug)
                        Debug.Log("Bad structure of .bvh file. Expected " + KeyWord.kOffset + ", but found " + token);
                    return -1;
                }

                tokenIndex++;   //consuming EndSite "}"
                _bvh.Add_joint(tempJoint);
            }
            else if (token == "}")
            {
                joint.Children = children;
                parsed = joint;
                return 0;
            }
        }

        if (_debug)
            Debug.Log("Cannot parse joint, unexpected end of file. Last token : " + token);

        return -1;
    }

    private int Parsing_Channel_Order(Joint joint)
    {
        if (!CheckTokenIndex())
            return -1;

        int num = int.Parse(tokens[tokenIndex]);
        tokenIndex += num;  //get channel order

        if (!CheckTokenIndex())
            return -1;

        List<Channel> channels = new List<Channel>();

        for (int index = num - 1; index >= 0; index--)
        {
            string tok = tokens[tokenIndex - index];
            if (tok == KeyWord.kXpos)
                channels.Add(Channel.XPOSITION);
            else if (tok == KeyWord.kYpos)
                channels.Add(Channel.YPOSITION);
            else if (tok == KeyWord.kZpos)
                channels.Add(Channel.ZPOSITION);
            else if (tok == KeyWord.kXrot)
                channels.Add(Channel.XROTATION);
            else if (tok == KeyWord.kYrot)
                channels.Add(Channel.YROTATION);
            else if (tok == KeyWord.kZrot)
                channels.Add(Channel.ZROTATION);
            else
            {
                if (_debug)
                    Debug.Log("Invalid Channel!");
                return -1;
            }
        }

        joint.Channels_order = channels;
        tokenIndex++;
        return 0;
    }

    private int Parsing_Motion()
    {
        tokenIndex++;
        string token = tokens[tokenIndex];

        if (token == KeyWord.kFrames)
        {
            tokenIndex++;
            int frame = int.Parse(tokens[tokenIndex]);
            _bvh.Num_frames = frame;
        }
        else
        {
            if (_debug)
                Debug.Log("Bad structure of .bvh file. Expected " + KeyWord.kFrames + ", but found " + token);
            return -1;
        }

        tokenIndex++;
        token = tokens[tokenIndex];
        if (token == KeyWord.kFrame)
        {
            tokenIndex += 2;    //consuming "Time:"
            if (!CheckTokenIndex())
                return -1;

            double frame_time = double.Parse(tokens[tokenIndex]);
            _bvh.Frame_time = frame_time;

            for (int i = 0; i < _bvh.Num_frames; i++)
            {
                foreach (Joint joint in _bvh.Joints)
                {
                    List<float> data = new List<float>();
                    for (int j = 0; j < joint.num_channels(); j++)
                    {
                        tokenIndex++;
                        if (!CheckTokenIndex())
                            return -1;
                        float num = float.Parse(tokens[tokenIndex]);
                        data.Add(num);
                    }
                    joint.Add_motion_data(data);
                }
            }
        }
        else
        {
            if (_debug)
                Debug.Log("Bad structure of .bvh file. Expected " + KeyWord.kFrame + ", but found " + token);
            return -1;
        }

        return 0;
    }

    private bool CheckTokenIndex()
    {
        if (tokenIndex >= tokens.Count)
            if (_debug)
                Debug.Log("Parsing Error!");
        return tokenIndex < tokens.Count ? true : false;
    }


    private List<string> tokens = new List<string>();
    private int tokenIndex = 0;
    private Bvh _bvh;


    private bool _debug = false;
}


