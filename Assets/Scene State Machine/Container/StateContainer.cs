using System.Collections;
using System.Collections.Generic;
using Assets.Code.Bon;
using Assets.Code.Bon.Nodes;
using UnityEngine;

public class StateContainer : NodeContainer
{
    public StateNode StateNode;

    public override Node Node
    {
        get { return StateNode; }
        set
        {
            StateNode = (StateNode) value;
            Initialize();
        }
    }
}
