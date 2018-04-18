using System.Collections;
using System.Collections.Generic;
using Assets.Code.Bon;
using UnityEngine;

public class EventContainer : NodeContainer
{
    public EventNode EventNode;

    public override Node Node
    {
        get { return EventNode; }

        set
        {
            EventNode = (EventNode)value;
            Initialize();
        }
    }
}
