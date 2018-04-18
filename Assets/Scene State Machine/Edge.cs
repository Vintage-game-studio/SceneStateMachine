using System;
using UnityEngine;

namespace Assets.Code.Bon
{
    [Serializable]
    public class Edge
    {
        #region public static

        public static float EdgeTangent = 50;

        #endregion

        #region private

        private Socket _inputSocket;
        private Socket _outputSocket;

        [NonSerialized]
        public Graph ParentGraph;

        [NonSerialized]
        public EdgeContainer Container;
        #endregion

        #region SerializeField

        [SerializeField] private int _inputNodeId;
        [SerializeField] private int _inputSocketIndex;
        [SerializeField] private int _outputNodeId;
        [SerializeField] private int _outputSocketIndex;

        #endregion

        #region Properties

        public Socket InputSocket
        {
            get { return _inputSocket; }
            set
            {
                _inputSocket = value;

                _inputNodeId = _inputSocket.ParentNode.Id;
                _inputSocketIndex = _inputSocket.ParentNode.Sockets.IndexOf(InputSocket);
            }
        }

        public Socket OutputSocket
        {
            get { return _outputSocket; }
            set
            {
                _outputSocket = value;
                _outputNodeId = _outputSocket.ParentNode.Id;
                _outputSocketIndex = _outputSocket.ParentNode.Sockets.IndexOf(OutputSocket);

            }
        }

        #endregion

        //********************

        #region Constructor

        public Edge(Socket outputSocketSocket, Socket inputSocketSocket, Graph graph)
        {
            ParentGraph = graph;

            InputSocket = inputSocketSocket;
            OutputSocket = outputSocketSocket;

            ParentGraph.AddEdge(this);
        }

        #endregion

        #region GetOtherSocket

        public Socket GetOtherSocket(Socket socket)
        {
            if (socket == InputSocket) return OutputSocket;
            return InputSocket;
        }

        #endregion


        #region Initialize

        public bool Initialize(Graph graph, EdgeContainer edgeContainer)
        {
            ParentGraph = graph;

            Node inputNode = ParentGraph.GetNode(_inputNodeId);
            Node outputNode = ParentGraph.GetNode(_outputNodeId);

            if (inputNode == null || outputNode == null)
            {
                Debug.LogWarning("Try to create an edge but can not find at least on of the nodes.");
                return false;
            }

            if (_outputSocketIndex >= outputNode.Sockets.Count ||
                _inputSocketIndex >= inputNode.Sockets.Count)
            {
                Debug.LogWarning("Try to create an edge but can not find at least on of the sockets.");
                return false;
            }

            _inputSocket = inputNode.Sockets[_inputSocketIndex];
            _outputSocket = outputNode.Sockets[_outputSocketIndex];

            if(!_inputSocket.Edges.Contains(this))
                _inputSocket.Edges.Add(this);

            if(!_outputSocket.Edges.Contains(this))
                _outputSocket.Edges.Add(this);

            return true;
        }

        #endregion
    }

}


