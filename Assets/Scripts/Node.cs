using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Assets.Code.Bon.Nodes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Code.Bon
{
    [Serializable]
    public abstract class Node
    {
        #region Static


        #endregion

        #region SerializeField

        [SerializeField]
        public int Id;
        [SerializeField]
        public string Name;
        [SerializeField]
        public Rect WindowRect;
        [SerializeField]
        public bool Collapsed;
        [SerializeField]
        public float Height;

        #endregion

        #region NonSerialized

        [NonSerialized]
        public Graph ParentGraph;

        public NodeContainer Container;

        [SerializeField]
        public List<Socket> Sockets = new List<Socket>();

        #endregion

        #region private

        private Rect _tempRect;

        #endregion

        //**************************************************

        #region Constructor

        public Node(int id, Graph parentGraph)
        {
            ParentGraph = parentGraph;

            Id = id;

            // default size
            WindowRect.width = 100;
            WindowRect.height = 100;

            Name = Setting.DefaultName == "" ? GetType().Name : Setting.DefaultName;

            Collapsed = Setting.DefaultCollapse;

            if (Sockets == null)
                Sockets = new List<Socket>();

        }

        #endregion

        #region Initialize

        public void Initialize(Graph graph, NodeContainer nodeContainer)
        {
            ParentGraph = graph;
            Container = nodeContainer;

            if (Sockets == null)
                Sockets = new List<Socket>();

            foreach (Socket socket in Sockets)
                socket.Initialize(this);

        }

        #endregion
        
        #region Expand/Collapse

        public void Collapse()
        {
            if (!Setting.CanExpandCollapse)
                return;

            WindowRect.Set(WindowRect.x, WindowRect.y, WindowRect.width, Setting.CollapeHeight); // 18);

            Collapsed = true;
        }

        public void Expand()
        {
            if (!Setting.CanExpandCollapse)
                return;

            WindowRect.Set(WindowRect.x, WindowRect.y, WindowRect.width, Setting.ExpandHeight);
            Collapsed = false;
        }

        #endregion

        #region Intersects

        public bool Intersects(Vector2 canvasPosition)
        {
            return WindowRect.Contains(canvasPosition);
        }
        public bool IntersectsTitle(Vector2 canvasPosition)
        {
            Rect titleRect = new Rect(WindowRect);
            titleRect.height = Setting.WindowStyle.padding.top;
            return titleRect.Contains(canvasPosition);
        }

        #endregion

        #region Socket functions

        public Socket SearchSocketAt(Vector2 canvasPosition)
        {
            foreach (var socket in Sockets)
            {
                if (socket.Intersects(canvasPosition)) return socket;
            }
            return null;
        }

        #endregion

        #region Draw Sockets & Edges

        public void GUIDrawSockets()
        {

            for (int i = 0; i < Sockets.Count; i++)
                Sockets[i].Draw(Setting.SocketSetttings[i]);
        }


        #endregion

        #region Settings

        private int _settingIndex = 0;

        public NodeSetting Setting
        {
            get
            {
                if (_settingIndex == 0)
                {
                    string name = GetType().Name;

                    for (int i = 0; i < SssmSettings.Setting.NodeSettings.Count; i++)
                    {
                        if (SssmSettings.Setting.NodeSettings[i].Name == name)
                            _settingIndex = i + 1;
                    }
                    if (_settingIndex == 0)
                    {
                        Debug.LogError("Can't find Node setting for " + name);
                        return null;
                    }
                }
                return SssmSettings.Setting.NodeSettings[_settingIndex - 1];

            }
        }

        public GUIStyle WindowStyle
        {
            get
            {
                return _isSelected ? Setting.SelectedStyle : Setting.WindowStyle;
            }
        }


        #endregion

        #region SetRect

        public void SetRect()
        {
            WindowRect.height = Collapsed ? Setting.CollapeHeight : Setting.ExpandHeight;
            WindowRect.width = Math.Max(
                Collapsed ? Setting.CollapeMinWidth : Setting.MinWidth,
                Setting.WindowStyle.CalcSize(new GUIContent(Name)).x + Setting.WidthOffset);
        }
        #endregion

        #region Awake Start

        public virtual void Awake()
        {

        }

        public virtual void Start()
        {
        }

        #endregion

        #region Run

        public virtual void Run()
        {
        }


        #endregion

        #region Move

        public void Move(Vector2 deltaPos, bool transformChilds, List<Node> movedNodes)
        {
            if (movedNodes.Contains(this))
                return;
            else
                movedNodes.Add(this);

            WindowRect.position += deltaPos;

            if (transformChilds)
                foreach (Socket socket in Sockets)
                    if (!socket.IsInput)
                        foreach (Edge edge in socket.Edges)
                            edge.GetOtherSocket(socket).ParentNode.Move(deltaPos, transformChilds, movedNodes);
        }

        #endregion

        #region Selection

        private bool _isSelected = false;
        public bool IsSelected { get { return _isSelected; } }

        public void BoxSelection(Rect box)
        {
            _isSelected = box.Overlaps(WindowRect);
        }

        public void Select()
        {
            _isSelected = true;
        }

        public void Deselect()
        {
            _isSelected = false;
        }

        #endregion

        public virtual Node Duplicate()
        {
            Node node = ParentGraph.CreateNode(GetType());

            node.Name = Name;
            node.WindowRect = WindowRect;
            node.Collapsed = Collapsed;
            node.Height = Height;

            node.WindowRect.position += new Vector2(30, 30);

            ParentGraph.AddNode(node);
            return node;
        }
    }

}


