using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Code.Bon;
using UnityEngine;

[ExecuteInEditMode]
public class EdgeContainer : MonoBehaviour
{
    [SerializeField]
    private Edge _edge;

    public Edge Edge
    {
        get { return _edge; }
        set
        {
            _edge = value;
            Initialize();
        }
    }


    private void OnDestroy()
    {
        SceneStateMachine stateMachine = GetComponentInParent<SceneStateMachine>();

        if (stateMachine != null)
            stateMachine.MakeDirty();
    }

 
    protected void Initialize()
    {
        if (Edge == null)
            return;

        SceneStateMachine machine = GetComponentInParent<SceneStateMachine>();

        if (machine != null)
            Edge.Initialize(machine.MainGraph, this);
    }

    public static void CreateContainer(Edge edge, SceneStateMachine sceneStateMachine)
    {
        Type type = edge.GetType();

        Transform category = sceneStateMachine.transform.Find(type.Name + "s");
        if (category == null)
        {
            GameObject cgo = new GameObject(type.Name + "s");
            cgo.transform.parent = sceneStateMachine.transform;
            category = cgo.transform;
        }

        GameObject go = new GameObject(string.Format("{0} ({1})", type.Name, category.childCount+1));
        go.transform.parent = category;

        EdgeContainer container = go.AddComponent<EdgeContainer>();
        container.Edge = edge;
        //edge.Container = container;
    }

    public static void RemoveContainer(Edge edge)
    {
        Debug.Log(edge.Container);
        DestroyImmediate(edge.Container.gameObject);
    }
}
