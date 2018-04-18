using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Code.Bon;
using UnityEngine;

namespace Assets.Code.Bon.Nodes
{
    [Serializable]
    public class StartNode : Node
    {
        #region Constructor & Initialize

        public StartNode(int id, Graph parentGraph) : base(id, parentGraph)
        {
            WindowRect.height = Height = 100;
            WindowRect.width = 250;

            if (Sockets.Count == 0)
            {
                Sockets.Add(new Socket(this, false));
            }
        }
        #endregion

        #region Run

        public override void Awake()
        {
            base.Awake();

        }

        public override void Start()
        {
            base.Start();
            Run();
        }

        public override void Run()
        {
            base.Run();
            Sockets[0].RunConnectedNodes();
        }
        #endregion

    }
}
