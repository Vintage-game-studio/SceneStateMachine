using System;
using UnityEngine;

namespace Assets.Code.Bon.Nodes
{
    [Serializable]
    public class StateNode : Node
    {

        #region Constructor & Initialize

        public StateNode(int id, Graph parentGraph) : base(id, parentGraph)
        {
            WindowRect.height = Height = 100;
            WindowRect.width = 250;
            if (Sockets.Count == 0)
            {
                Sockets.Add(new Socket(
                    this,
                    true));

                Sockets.Add(new Socket(
                     this,
                     false));

                Sockets.Add(new Socket(
                    this,
                    false));
            }
        }


        #endregion

        #region Run

        public override void Run()
        {
            base.Run();
            ParentGraph.ParentStateMachine.ChangeState(this);
        }
        #endregion

        #region Enter / Exit

        public void Exit()
        {
            Sockets[2].RunConnectedNodes();
        }

        public void Enter()
        {
            Sockets[1].RunConnectedNodes();
        }
        

        #endregion
    }
}
