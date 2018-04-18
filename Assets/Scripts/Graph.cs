using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Code.Bon.Nodes;
using UnityEngine;

namespace Assets.Code.Bon
{

    [Serializable]
    public class Graph
    {
        #region Public

        public SceneStateMachine ParentStateMachine;

        #endregion

        #region private

        private List<Node> _nodes;
        private List<Edge> _edges;
        private List<Node> _movedNodes = new List<Node>();

        #endregion

        #region Properties

        public List<Node> Nodes
        {
            get
            {
                if (_nodes == null)
                    _nodes = ParentStateMachine
                        .GetComponentsInChildren<NodeContainer>()
                        .Select(c => c.Node)
                        .ToList();
                return _nodes;
            }
            set { _nodes = value; }
        }
        public List<Edge> Edges
        {
            get
            {
                if (_edges == null)
                    _edges = ParentStateMachine
                        .GetComponentsInChildren<EdgeContainer>()
                        .Select(c => c.Edge)
                        .ToList();
                return _edges;
            }
            set { _edges = value; }
        }


        #endregion


        //****************

        #region Constructor

        public Graph(SceneStateMachine parentGo)
        {
            ParentStateMachine = parentGo;

            Nodes = new List<Node>();

        }
        #endregion

        #region CreateNode

        public Node CreateNode<T>()
        {
            return CreateNode<T>(ObtainUniqueNodeId());
        }

        public Node CreateNode<T>(int id)
        {
            return CreateNode(typeof(T), id);
        }

        public Node CreateNode(Type type)
        {
            return CreateNode(type, ObtainUniqueNodeId());
        }

        public Node CreateNode(Type type, int id)
        {
            if (type == null) return null;
            try
            {
                return (Node)Activator.CreateInstance(type, id, this);
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat("Node {0} could not be created " + exception.Message, type.FullName);
            }
            return null;
        }

        public int ObtainUniqueNodeId()
        {
            var tmpId = 0;
            while (GetNode(tmpId) != null) tmpId++;
            return tmpId;
        }

        #endregion

        #region GetNode

        public Node GetNode(int nodeId)
        {
            if (Nodes == null) return null;
            foreach (var node in Nodes) if (node.Id == nodeId) return node;
            return null;
        }

        #endregion

        #region AddNode / RemoveNode

        public bool AddNode(Node node)
        {
            if (GetNode(node.Id) != null) return false;

            Nodes.Add(node);
            
            NodeContainer.CreateContainer(node,ParentStateMachine);

            return true;
        }

        public bool RemoveNode(Node node)
        {
            if (node == null) return false;

            foreach (var socket in node.Sockets)
                UnLink(socket);

            bool removed = Nodes.Remove(node);

            NodeContainer.RemoveContainer(node);

            return removed;
        }

        public bool RemoveNode(int id)
        {
            return RemoveNode(GetNode(id));
        }
        public void RemoveSelectedNodes()
        {
            List<Node> selectedNodes = GetSelectedNodes();

            foreach (Node node in selectedNodes)
                RemoveNode(node);
        }

        public List<Node> GetSelectedNodes()
        {
            List<Node> selectedNodes = new List<Node>();

            foreach (Node node in Nodes)
                if (node.IsSelected)
                    selectedNodes.Add(node);
            return selectedNodes;
        }

        #endregion

        #region Link / UnLink 

        public void UnLink(Socket inputSocket, Socket outputSocket)
        {

            if (inputSocket == null || outputSocket == null || !inputSocket.IsConnected() || !outputSocket.IsConnected())
                return;

            List<Edge> commonEdges = new List<Edge>();

            foreach (Edge inputSocketEdge in inputSocket.Edges)
                foreach (Edge outputSocketEdge in outputSocket.Edges)
                    if (inputSocketEdge == outputSocketEdge)
                        commonEdges.Add(inputSocketEdge);

            foreach (Edge commonEdge in commonEdges)
            {
                inputSocket.Edges.Remove(commonEdge);
                outputSocket.Edges.Remove(commonEdge);
                Edges.Remove(commonEdge);
                ParentStateMachine.RemoveContainer(commonEdge);
            }

        }

        public void UnLink(Socket socket)
        {
            if (socket == null || !socket.IsConnected()) return;

            Edge[] edgeCopy = new Edge[socket.Edges.Count];
            socket.Edges.CopyTo(edgeCopy);
            foreach (var edge in edgeCopy)
            {
                UnLink(socket, edge.GetOtherSocket(socket));
            }
        }

        public bool Link(Socket inputSocket, Socket outputSocket)
        {
            Edge edge = new Edge(outputSocket, inputSocket, this);
            inputSocket.Edges.Add(edge);
            outputSocket.Edges.Add(edge);
            return true;
        }


        #endregion

        #region AddEdge

        public void AddEdge(Edge edge)
        {
            if (!Edges.Contains(edge))
            {
                Edges.Add(edge);
                EdgeContainer.CreateContainer(edge,ParentStateMachine);
            }
        }
        

        #endregion

        #region Selection

        public void BoxSelection(Rect box)
        {
            foreach (Node node in Nodes)
                node.BoxSelection(box);
        }

        public void SelectNode(Node selectedNode, bool add)
        {
            if (add)
                selectedNode.Select();
            else
                foreach (Node node in Nodes)
                    if (node == selectedNode)
                        node.Select();
                    else
                        node.Deselect();
        }

        public void DeselectAll()
        {
            foreach (Node node in Nodes)
                if (node.IsSelected)
                    node.Deselect();
        }

        #endregion

        #region Move Nodes

        public void MoveSelectedNodes(Vector2 delta, bool propagate)
        {
            if (_movedNodes == null)
                _movedNodes = new List<Node>();

            _movedNodes.Clear();

            foreach (Node node in Nodes)
                if (node.IsSelected)
                    node.Move(delta, propagate, _movedNodes);

        }

        #endregion

        #region DuplicateSelectedNodes

        public void DuplicateSelectedNodes()
        {
            Dictionary<Node,Node> oldToNew= new Dictionary<Node, Node>();
            List<Node> selectedNodes = GetSelectedNodes();

            foreach (Node node in selectedNodes)
                if (node.IsSelected)
                {
                    oldToNew.Add(node, node.Duplicate());
                    node.Deselect();
                }

            foreach (Node oldNode in selectedNodes)
                for (int i = 0; i < oldNode.Sockets.Count; i++)
                {
                    Socket socket = oldNode.Sockets[i];
                    if (socket.IsInput)
                        foreach (Edge edge in socket.Edges)
                        {
                            Socket otherSocket = edge.GetOtherSocket(socket);
                            Node otherNode = otherSocket.ParentNode;

                            if (selectedNodes.Contains(otherNode))
                                Link(oldToNew[oldNode].Sockets[i],
                                    oldToNew[otherNode].Sockets[otherNode.Sockets.IndexOf(otherSocket)]);
                        }
                }


            foreach (Node newNode in oldToNew.Values)
                newNode.Select();
        }
        

        #endregion

        public void MakeDirty()
        {
            _nodes = null;
            _edges = null;
        }

        public void Initialize(SceneStateMachine sceneStateMachine)
        {
            ParentStateMachine = sceneStateMachine;

            NodeContainer[] nodeContainers = sceneStateMachine
                .GetComponentsInChildren<NodeContainer>();

            EdgeContainer[] edgeContainers = sceneStateMachine.GetComponentsInChildren<EdgeContainer>();

            _nodes = nodeContainers.Select(c => c.Node).ToList();
            _edges = edgeContainers.Select(c => c.Edge).ToList();

            foreach (NodeContainer container in nodeContainers)
                container.Node.Initialize(this, container);

            foreach (EdgeContainer container in edgeContainers)
                container.Edge.Initialize(this, container);
        }
    }
}