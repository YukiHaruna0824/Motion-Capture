using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace SteeringBehavior.LevelEditor
{
    [CustomEditor(typeof(BezierPath)), CanEditMultipleObjects]
    class PathInspector : Editor
    {
        private BezierPath _target;

        public bool first = true;

        public void DestroySelf()
        {
            if (EditorUtility.DisplayDialog("Path", "你確定要刪除路徑", "Yes", "No"))
            {
                EditorApplication.update -= Check;
                DestroyImmediate(_target.gameObject);
            }
        }

        private void OnEnable()
        {
            first = true;

            _target = target as BezierPath;

            _target.gameObject.SetActive(true);
            PathManagerWindow._Path = _target;
            _target.nodeChange = true;

            _target._ctrlPoint = new List<BezierPoint>();

            InitBezier();
            EditorApplication.update += Check;

        }

        private void OnDisable()
        {
            EditorApplication.update -= Check;
            PathManagerWindow._Path = null;
        }

        private void OnDestroy()
        {
            EditorApplication.update -= Check;
            PathManagerWindow._Path = null;
        }

        private void OnSceneGUI()
        {
            Event e = Event.current;

            //if (e.type == EventType.MouseUp && e.button == 0)
            //{
            //    _target.Preview();
            //    _target.nodeChange = true;
            //}
            _target.nodeChange = true;
            DrawPathGUI();
        }

        void Check()
        {
            if ((CheckBezier() || _target.nodeChange))
            {

                if (_target.nodeChange)
                    _target.nodeChange = false;

                _target.Preview();
                SceneView.RepaintAll();
            }
        }

        private bool CheckBezier()
        {
            if (_target._ctrlPoint == null || _target.nodes == null || _target == null)
                return false;
            bool res = false;

            // 找尋被更動的點，使該控制點為一直線
            for (int i = 0; i < _target._ctrlPoint.Count; i++)
            {
                //Debug.Log(_target._ctrlPoint.Count);
                //Debug.Log(_target.nodes.Length);
                Vector3 ori = _target.nodes[i].position;
                Vector3 up = _target.nodes[i].GetChild(0).position;
                Vector3 down = _target.nodes[i].GetChild(1).position;

                if (_target._ctrlPoint[i].ori != ori)
                {
                    up = _target._ctrlPoint[i].up - _target._ctrlPoint[i].ori + ori;
                    down = _target._ctrlPoint[i].down - _target._ctrlPoint[i].ori + ori;
                    _target._ctrlPoint.RemoveAt(i);
                    _target._ctrlPoint.Insert(i, new BezierPoint(up, down, ori));
                    res = true;
                }
                else if (_target._ctrlPoint[i].up != up && i != 0)
                {
                    float dist = Vector3.Distance(_target._ctrlPoint[i].down, ori);
                    if ((up.x > ori.x && down.x > ori.x) || (up.x < ori.x && down.x < ori.x))
                    {
                        down = ori - (up - ori);
                    }
                    else
                    {
                        if (up.x == ori.x)
                        {
                            down.x = ori.x;
                            if (up.z < ori.z)
                                down.z = ori.z + dist;
                            else
                                down.z = ori.z - dist;
                        }
                        else if (up.z == ori.z)
                        {
                            down.z = ori.z;
                            if (up.x < ori.x)
                                down.x = ori.z + dist;
                            else
                                down.x = ori.z - dist;
                        }
                        else
                        {
                            float slope = (ori.z - up.z) / ((ori.x - up.x) == 0 ? 1 : (ori.x - up.x));
                            float nZ = ori.z - slope * (ori.x - _target._ctrlPoint[i].down.x);
                            float nX = ori.x - (ori.z - _target._ctrlPoint[i].down.z) / slope;
                            float dx = Vector3.Distance(new Vector3(nX, _target._ctrlPoint[i].down.y, _target._ctrlPoint[i].down.z), ori);
                            float dz = Vector3.Distance(new Vector3(_target._ctrlPoint[i].down.x, _target._ctrlPoint[i].down.y, nZ), ori);
                            if (Mathf.Abs(dx - dist) > Mathf.Abs(dz - dist))
                                down = new Vector3(_target._ctrlPoint[i].down.x, _target._ctrlPoint[i].down.y, nZ);
                            else
                                down = new Vector3(nX, _target._ctrlPoint[i].down.y, _target._ctrlPoint[i].down.z);
                        }
                    }
                    _target._ctrlPoint.RemoveAt(i);
                    _target._ctrlPoint.Insert(i, new BezierPoint(up, down, ori));
                    _target.nodes[i].GetChild(1).position = down;
                    res = true;
                    break;
                }
                else if (_target._ctrlPoint[i].down != down && i != _target._ctrlPoint.Count - 1)
                {
                    float dist = Vector3.Distance(_target._ctrlPoint[i].up, ori);
                    if ((up.x > ori.x && down.x > ori.x) || (up.x < ori.x && down.x < ori.x))
                    {
                        up = ori - (down - ori);
                    }
                    else
                    {
                        if (down.x == ori.x)
                        {
                            up.x = ori.x;
                            if (down.z < ori.z)
                                up.z = ori.z + dist;
                            else
                                up.z = ori.z - dist;
                        }
                        else if (down.z == ori.z)
                        {
                            down.z = ori.z;
                            if (down.x < ori.x)
                                up.x = ori.x + dist;
                            else
                                up.x = ori.x - dist;
                        }
                        else
                        {
                            float slope = (down.z - ori.z) / ((down.x - ori.x) == 0 ? 1 : (down.x - ori.x));
                            float nZ = ori.z + slope * (_target._ctrlPoint[i].up.x - ori.x);
                            float nX = ori.x + (_target._ctrlPoint[i].up.z - ori.z) / slope;
                            float dx = Vector3.Distance(new Vector3(nX, _target._ctrlPoint[i].up.y, _target._ctrlPoint[i].up.z), ori);
                            float dz = Vector3.Distance(new Vector3(_target._ctrlPoint[i].up.x, _target._ctrlPoint[i].up.y,nZ), ori);
                            if (Mathf.Abs(dx - dist) > Mathf.Abs(dz - dist))
                                up = new Vector3(_target._ctrlPoint[i].up.x, _target._ctrlPoint[i].up.y, nZ);
                            else
                                up = new Vector3(nX, _target._ctrlPoint[i].up.y, _target._ctrlPoint[i].up.z);
                        }
                    }
                    _target._ctrlPoint.RemoveAt(i);
                    _target._ctrlPoint.Insert(i, new BezierPoint(up, down, ori));
                    _target.nodes[i].GetChild(0).position = up;
                    res = true;
                    break;
                }

            }
            return res;
        }


        private void InitBezier()
        {
            if (_target.nodes == null || _target.nodes.Length == 0)
            {
                Vector3 nodePos = _target.transform.position - _target.transform.forward * 2;
                GameObject goNode = new GameObject(string.Format("Node ({0})", _target.transform.childCount - 1));
                GameObject up = new GameObject("Slope_Up");
                GameObject down = new GameObject("Slope_Down");
                goNode.transform.SetParent(_target.transform);
                _target.nodes = new Transform[1];
                _target.nodes[0] = goNode.transform;
                goNode.transform.position = nodePos;
                up.transform.SetParent(_target.nodes[0].transform);
                down.transform.SetParent(_target.nodes[0].transform);
                _target.nodes[0].GetChild(0).position = _target.nodes[0].position - new Vector3(0.5f, 0f, 0f);
                _target.nodes[0].GetChild(1).position = _target.nodes[0].position + new Vector3(0.5f, 0f, 0f);
            }

            if (_target.nodes.Length != _target.transform.childCount-1)
            {
                _target.nodes = new Transform[_target.transform.childCount-1];
                Debug.Log(_target.nodes.Length);
            }
            for (int i = 1; i < _target.nodes.Length + 1; i++)
            {
                _target.nodes[i - 1] = _target.transform.GetChild(i);
                _target._ctrlPoint.Add(new BezierPoint(
                    _target.nodes[i - 1].GetChild(0).position,
                    _target.nodes[i - 1].GetChild(1).position,
                    _target.nodes[i - 1].position
                    ));
            }
        }

        readonly Color nodeColor = Color.green;
        readonly Color warnColor = Color.red;
        readonly Color slopeUpColor = Color.cyan;
        readonly Color slopeDownColor = Color.magenta;
        readonly Color slopeLineColor = Color.gray;

        readonly Color selectedColor = new Color(246f / 255, 242f / 255, 50f / 255, .89f);
        readonly Color preselectionColor = new Color(201f / 255, 200f / 255, 144f / 255, .89f);

        int HandlesIndex;
        bool hasHovered;

        void DrawPathGUI()
        {
            if (_target.nodes == null)
                return;

            bool isOnMe = Selection.activeGameObject == _target.gameObject;

            if (isOnMe)
            {
                for (int i = 0; i < _target.nodes.Length; i++)
                {
                    if (_target.nodes[i] == null)
                    {
                        continue;
                    }


                    Handles.color = nodeColor;

                    _target.nodes[i].position = Handles.Slider2D(
                        _target.nodes[i].position,
                            Vector3.up,
                            Vector3.right,
                            Vector3.forward,
                            0.5f,
                            Handles.SphereHandleCap,
                            Vector3.zero
                            );

                    if (i != 0)
                    {
                        Handles.color = slopeUpColor;
                        _target.nodes[i].GetChild(0).position = Handles.Slider2D(
                            _target._ctrlPoint[i].up,
                            Vector3.up,
                            Vector3.right,
                            Vector3.forward,
                            0.3f,
                            Handles.SphereHandleCap,
                            Vector3.zero
                            );
                        Handles.color = slopeLineColor;
                        Handles.DrawLine(_target.nodes[i].position, _target.nodes[i].GetChild(0).position);
                    }
                    if (i != _target.nodes.Length - 1)
                    {
                        Handles.color = slopeDownColor;
                        _target.nodes[i].GetChild(1).position = Handles.Slider2D(
                            _target._ctrlPoint[i].down,
                            Vector3.up,
                            Vector3.right,
                            Vector3.forward,
                            0.3f,
                            Handles.SphereHandleCap,
                            Vector3.zero
                            );
                        Handles.color = slopeLineColor;
                        Handles.DrawLine(_target.nodes[i].position, _target.nodes[i].GetChild(1).position);
                    }


                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.red + Color.yellow;

                    Handles.Label(_target.nodes[i].position + Vector3.up * 1.5f + Vector3.right * 0.5f,
                    "Node (" + i + ")\n", style);

                }
            }

            bool print = false;
            if (first == true)
            {
                print = true;
                first = false;
            }
            // Bezier Path
            for (int i = 0; _target.simulateBezierPath != null && i < _target.simulateBezierPath.Count - 1; i++)
            {
                //if (print == true)
                //Debug.Log(Vector3.Magnitude(_target.simulateBezierPath[i + 1] - _target.simulateBezierPath[i]));
                Handles.color = Color.blue;
                Handles.DrawLine(_target.simulateBezierPath[i], _target.simulateBezierPath[i + 1]);
            }

        }
    }
}