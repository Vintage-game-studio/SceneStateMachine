using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Code.Bon;
using Assets.Code.Bon.Nodes;
using UnityEngine;

[ExecuteInEditMode]
public abstract class NodeContainer : MonoBehaviour
{
    private static readonly Dictionary<Type,Type> NodeToContainer=new Dictionary<Type, Type>()
    {
        {typeof(ActionNode),typeof(ActionContainer) },
        {typeof(EventNode),typeof(EventContainer) },
        {typeof(StartNode),typeof(StartContainer) },
        {typeof(StateNode),typeof(StateContainer) },
    };

    public abstract Node Node { get; set; }
    
    private void OnDestroy()
    {
        SceneStateMachine stateMachine = GetComponentInParent<SceneStateMachine>();

        if (stateMachine != null)
            stateMachine.MakeDirty();
    }

    protected void Initialize()
    {
        if (Node == null)
            return;

        SceneStateMachine machine = GetComponentInParent<SceneStateMachine>();

        if (machine != null)
            Node.Initialize(machine.MainGraph, this);
    }

    public static void CreateContainer(Node node, SceneStateMachine sceneStateMachine)
    {
        Type type = node.GetType();

        Transform category = sceneStateMachine.transform.Find(type.Name + "s");
        if (category == null)
        {
            GameObject cgo = new GameObject(type.Name+"s");
            cgo.transform.parent = sceneStateMachine.transform;
            category = cgo.transform;
        }

        GameObject go = new GameObject(string.Format("{1} - ({0})", node.Name, category.childCount + 1));
        go.transform.parent = category;

        NodeContainer container = (NodeContainer) go.AddComponent(NodeToContainer[type]);
        container.Node = node;
    }

    public static void RemoveContainer(Node node)
    {
        DestroyImmediate(node.Container.gameObject);
    }
}
