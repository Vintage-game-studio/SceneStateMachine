using System.Collections;
using System.Collections.Generic;
using Assets.Code.Bon;
using Assets.Code.Bon.Nodes;
using UnityEngine;

public class StartContainer : NodeContainer
{
    public StartNode StartNode;


    public override Node Node
    {
        get
        {
            return StartNode;
        }
        set
        {
            StartNode = (StartNode) value;
            Initialize();
        }
    }
}
