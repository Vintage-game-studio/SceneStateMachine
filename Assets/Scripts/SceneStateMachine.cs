using System.Collections.Generic;
using UnityEngine;
using Assets.Code.Bon.Nodes;


namespace Assets.Code.Bon
{
    public class SceneStateMachine : MonoBehaviour
    {
        //[HideInInspector]
        [SerializeField]
        public Graph _mainGraph;

        private StateNode _currentState;

        public Graph MainGraph
        {
            get
            {
                if (_mainGraph == null)
                    _mainGraph = new Graph(this);
                return _mainGraph;
            }
        }

        private void OnValidate()
        {
            MainGraph.Initialize(this);

        }


        [ContextMenu("Initiate Edges")]
        public void InitiateEdges()
        {
            foreach (EdgeContainer container in GetComponentsInChildren<EdgeContainer>())
            {
                container.Edge.Container = container;
            }
        }



        #region Awake & Start

        private void Awake()
        {
            MainGraph.Initialize(this);
            // Awake Nodes
            foreach (Node node in MainGraph.Nodes)
                node.Awake();
        }

        private void Start()
        {
            foreach (Node node in MainGraph.Nodes)
                node.Start();
        }


        #endregion


        #region ChangeState

        public void ChangeState(StateNode stateNode)
        {
            if (_currentState != null)
                _currentState.Exit();

            _currentState = stateNode;
            stateNode.Enter();
        }

        #endregion

        public void MakeDirty()
        {
            MainGraph.MakeDirty();
        }

        [ContextMenu("Fill childes")]
        public void FillChilds()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);

            foreach (Node node in MainGraph.Nodes)
                NodeContainer.CreateContainer(node, this);

            foreach (Edge edge in MainGraph.Edges)
                EdgeContainer.CreateContainer(edge, this);
        }

        public void RemoveContainer(Edge edge)
        {
            foreach (EdgeContainer container in GetComponentsInChildren<EdgeContainer>())
            {
                if (container.Edge == edge)
                    DestroyImmediate(container.gameObject);
            }
        }
    }
}


