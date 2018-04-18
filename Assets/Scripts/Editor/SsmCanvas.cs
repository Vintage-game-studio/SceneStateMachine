using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using Assets.Code.Bon;

namespace Assets.Editor.Bon
{
    [Serializable]
    public class SsmCanvas
    {

        #region Public

        [SerializeField]
        public float Zoom = 1;
        [SerializeField]
        public Vector2 Position = new Vector2(1, 1) * -CanvasSize / 2;

        [NonSerialized]
        public Graph Graph;

        #endregion

        #region Private


        private const float CanvasSize = 100000;

        private Rect _drawArea = new Rect();

        private Color _backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);
        private Color _backgroundLineColor01 = new Color(0.14f, 0.14f, 0.14f, 1f);
        private Color _backgroundLineColor02 = new Color(0.10f, 0.10f, 0.10f, 1f);
        private GUIStyle _backgroundStyle = new GUIStyle();

        private NodeDrawer _nodeDrawer = new NodeDrawer();
        private Rect _tempRect;
        private List<Node> _tempNodeList;

        #endregion

        //*************************************

        #region Constructor

        public SsmCanvas(Graph graph)
        {
            Graph = graph;
            _backgroundStyle.normal.background = CreateBackgroundTexture();
            _backgroundStyle.normal.background.wrapMode = TextureWrapMode.Repeat;
            _backgroundStyle.fixedHeight = CanvasSize;
            _backgroundStyle.fixedWidth = CanvasSize;
        }

        #endregion

        #region CreateBackgroundTexture

        private Texture2D CreateBackgroundTexture()
        {
            var texture = new Texture2D(100, 100, TextureFormat.ARGB32, false);
            for (var x = 0; x < texture.width; x++)
            {
                for (var y = 0; y < texture.width; y++)
                {
                    bool isVerticalLine = (x % 11 == 0);
                    bool isHorizontalLine = (y % 11 == 0);
                    if (x == 0 || y == 0) texture.SetPixel(x, y, _backgroundLineColor02);
                    else if (isVerticalLine || isHorizontalLine) texture.SetPixel(x, y, _backgroundLineColor01);
                    else texture.SetPixel(x, y, _backgroundColor);
                }
            }
            texture.filterMode = FilterMode.Trilinear;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.Apply();
            return texture;
        }

        #endregion

        //******** Draw

        #region Main Draw

        public void Draw(EditorWindow window, Rect region, Socket currentDragingSocket, Rect boxSelectionRect)
        {


            #region Draw Background

            if (_backgroundStyle.normal.background == null)
                _backgroundStyle.normal.background = CreateBackgroundTexture();

            GUI.DrawTextureWithTexCoords(region, _backgroundStyle.normal.background, 
                new Rect(Position.x*Zoom/-100, Position.y*Zoom/100, region.width/100, region.height/100));

            #endregion

            EditorZoomArea.Begin(Zoom, region);

            _drawArea.Set(Position.x, Position.y, CanvasSize, CanvasSize);
            GUILayout.BeginArea(_drawArea);

            #region BoxSelection

            if (boxSelectionRect.x >= 0 && boxSelectionRect.width > 1)
                GUI.Box(boxSelectionRect, "", SssmSettings.Setting.SelectionBoxStyle);

            #endregion

            #region DrawEdges

            DrawEdges();

            #endregion

            #region Draw Nodes

            window.BeginWindows();
            DrawNodes();
            window.EndWindows();

            #endregion

            #region Draw DragEdge

            DrawDragEdge(currentDragingSocket);

            #endregion

            #region Draw Sockets

            foreach (Node node in Graph.Nodes)
                node.GUIDrawSockets();

            #endregion

            GUILayout.EndArea();

            EditorZoomArea.End();
        }

        #endregion

        #region DrawDragEdge

        private void DrawDragEdge(Socket currentDragingSocket)
        {
            if (currentDragingSocket != null)
            {
                DrawEdge(
                    currentDragingSocket.Center,
                    currentDragingSocket.Tangent,
                    Event.current.mousePosition,
                    Event.current.mousePosition,
                    currentDragingSocket.Color);
            }
        }

        #endregion

        #region DrawNodes

        public void DrawNodes()
        {
            foreach (Node node in Graph.Nodes)
            {
                node.SetRect();
                _tempRect = GUI.Window(node.Id, node.WindowRect, GUIDrawNodeWindow, node.Name, node.WindowStyle);

                if (node.WindowRect.min != _tempRect.min)
                {
                    Undo.RecordObject(node.Container, "Move Node");

                    if (!node.IsSelected)
                        Graph.SelectNode(node, Event.current.shift);

                    Graph.MoveSelectedNodes(_tempRect.position - node.WindowRect.position, Event.current.control);
                }
            }
        }


        void GUIDrawNodeWindow(int nodeId)
        {
            Node node = Graph.GetNode(nodeId);

            if (node == null)
                return;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                GenericMenu m = new GenericMenu();
                m.AddDisabledItem(new GUIContent(node.Name + " (" + nodeId + ")"));
                m.AddSeparator("");
                m.AddItem(new GUIContent("Delete"), false, DeleteNode, nodeId);

                if (node.Collapsed) m.AddItem(new GUIContent("Expand"), false, ExpandNode, nodeId);
                else m.AddItem(new GUIContent("Collapse"), false, CollapseNode, nodeId);

                m.ShowAsContext();
                Event.current.Use();
            }


            _nodeDrawer.OnGUI(node);

            GUI.DragWindow();


        }

        #endregion

        #region DrawEdges

        public void DrawEdges()
        {
            foreach (Node node in Graph.Nodes)
                DrawNodeEdges(node);
        }

        private void DrawNodeEdges(Node node)
        {
            foreach (var socket in node.Sockets)
                if (socket.IsInput)
                    foreach (Edge edge in socket.Edges)
                        DrawEdge(edge);
        }

        private void DrawEdge(Edge edge)
        {
            Socket OutputSocket=edge.OutputSocket;
            Socket InputSocket=edge.InputSocket;

            if (InputSocket != null && OutputSocket != null)
                DrawEdge(OutputSocket.Center, OutputSocket.Tangent, InputSocket.Center, InputSocket.Tangent, OutputSocket.Color);
        }

        public static void DrawEdge(Vector2 position01, Vector2 tangent01, Vector2 position02, Vector2 tangent02, Color color)
        {
            Handles.DrawBezier(
                position01, position02,
                tangent01, tangent02, Color.black, null, 8);

            Handles.DrawBezier(
                position01, position02,
                tangent01, tangent02, color, null, 5);
        }

        #endregion

        //******** End Draw

        #region Collapse, Expand, Delete Nodes

        private void CollapseNode(object nodeId)
        {
            Node node = Graph.GetNode((int)nodeId);
            node.Collapse();
        }

        private void ExpandNode(object nodeId)
        {
            Graph.GetNode((int)nodeId).Expand();
        }


        private void DeleteNode(object nodeId)
        {
            
            Undo.RecordObject(Graph.GetNode((int)nodeId).Container, "Delete Node");
            Graph.RemoveNode((int)nodeId);
        }

        #endregion

        #region GetSocketAt

        public Socket GetSocketAt(Vector2 windowPosition)
        {
            Vector2 projectedPosition = ProjectToCanvas(windowPosition);

            foreach (Node node in Graph.Nodes)
            {
                Socket socket = node.SearchSocketAt(projectedPosition);
                if (socket != null)
                    return socket;
            }
            return null;
        }

        #endregion

        #region CreateNode

        public Node CreateNode(Type nodeType, Vector2 windowPosition)
        {

            Node node = (Node)Graph.CreateNode(nodeType);
            var position = ProjectToCanvas(windowPosition);
            node.WindowRect.position = position;
            Graph.AddNode(node);
            return node;
        }

        #endregion

        #region ProjectToCanvas

        public Vector2 ProjectToCanvas(Vector2 windowPosition)
        {
            windowPosition.y += (21) - ((SssmSettings.Setting.TopOffset * 2));
            windowPosition = windowPosition / this.Zoom;
            windowPosition.x -= (this._drawArea.x);
            windowPosition.y -= (this._drawArea.y);
            return windowPosition;
        }

        #endregion

        #region Get Node At

        public Node GetNodeTitleAt(Vector2 windowPosition)
        {
            Vector2 projectedPosition = ProjectToCanvas(windowPosition);

            foreach (Node node in Graph.Nodes)
                if (node.IntersectsTitle(projectedPosition))
                    return node;

            return null;
        }
        public Node GetNodeAt(Vector2 windowPosition)
        {
            if (_tempNodeList == null)
                _tempNodeList = new List<Node>();
            else
                _tempNodeList.Clear();

            Vector2 projectedPosition = ProjectToCanvas(windowPosition);

            foreach (Node node in Graph.Nodes)
                if (node.Intersects(projectedPosition))
                    _tempNodeList.Add(node);

            if (_tempNodeList.Count > 1)
                foreach (Node node in _tempNodeList)
                    if (node.IsSelected)
                        return node;

            if (_tempNodeList.Count == 0)
                return null;

            return _tempNodeList[0];
        }

        #endregion

    }



}


