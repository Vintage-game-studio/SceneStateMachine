using System.Collections;
using System.Collections.Generic;
using Assets.Code.Bon;
using Assets.Code.Bon.Nodes;
using UnityEngine;

public class ActionContainer : NodeContainer
{
    public ActionNode ActionNode;

    public override Node Node
    {
        get { return ActionNode; }
        set
        {
            ActionNode = (ActionNode) value;
            Initialize();
        }
    }
}
