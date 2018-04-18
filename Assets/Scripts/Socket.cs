using System;
using System.Collections.Generic;
using Assets.Code.Bon.Nodes;
using UnityEngine;

namespace Assets.Code.Bon
{
    [Serializable]
    public class Socket
    {
        public bool IsInput;

        [NonSerialized]
        public Node ParentNode;

        [NonSerialized]
        public List<Edge> Edges;

        [SerializeField]
        private Rect _boxRect;
        [SerializeField]
        private Rect _labelRect;

        public Color Color;

        [SerializeField]

        public void Initialize(Node parent)
        {
            ParentNode = parent;

            if (Edges == null)
                Edges = new List<Edge>();
        }

        public Socket(Node parentNode, bool isInput)
        {
            ParentNode = parentNode;
            IsInput = isInput;
            Edges = new List<Edge>();
        }

        #region Properties

        public Vector2 Center
        {
            get { return _boxRect.center; }
        }

        public Vector2 Tangent
        {
            get
            {
                if (IsInput)
                    return Center + Vector2.left * Edge.EdgeTangent;

                return Center + Vector2.right * Edge.EdgeTangent;

            }
        }

        #endregion

        public bool IsConnected()
        {
            return Edges.Count > 0;
        }

        public bool Intersects(Vector2 nodePosition)
        {
            return _boxRect.Contains(nodePosition);
        }

        public void Draw(SocketSettting settting)
        {
            _boxRect.size = settting.IconSize;
            _labelRect.size = settting.LabelSize;

            _boxRect.y = ParentNode.WindowRect.y + settting.Offset.y;

            if (IsInput)
                _boxRect.x = ParentNode.WindowRect.xMin + settting.Offset.x;
            else
                _boxRect.x = ParentNode.WindowRect.xMax - settting.Offset.x - settting.IconSize.x;


            _labelRect.y = _boxRect.y + (_boxRect.height - _labelRect.height) / 2;

            if (IsInput)
                _labelRect.x = _boxRect.xMax;
            else
                _labelRect.x = _boxRect.xMin - _labelRect.width;


            GUI.Box(_boxRect, "", IsConnected() ? settting.ConnectedStyle : settting.DisconnectedStyle);

            GUI.Label(_labelRect, settting.Label ,settting.LabelStyle);
            Color = settting.Color;


        }

        public void RunConnectedNodes()
        {
            foreach (Edge edge in Edges)
                edge.GetOtherSocket(this).ParentNode.Run();
        }
    }

}


