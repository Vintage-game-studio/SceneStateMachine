using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SeneStateMachine/Setting")]
public class SssmSettings : ScriptableObject
{
    #region Static Access

    public static SssmSettings Setting;

    #endregion

    public int TopOffset=16;
    public int BottomOffset = 20;

    public GUIStyle SelectionBoxStyle;

    public List<NodeSetting> NodeSettings;

    [HideInInspector]
    public NodeSetting a;
}

[Serializable]
public class NodeSetting
{
    public string Name="Node";
    public string DefaultName="";
    public bool DefaultCollapse=false;
    public bool CanExpandCollapse = true;
    public float ExpandHeight=100;
    public float CollapeHeight=30;
    public int WidthOffset=10;
    public float MinWidth = 100;
    public float CollapeMinWidth = 100;
    public GUIStyle WindowStyle;
    public GUIStyle SelectedStyle;
    public List<SocketSettting> SocketSetttings;
}

[Serializable]
public class SocketSettting
{
    public GUIStyle ConnectedStyle ;
    public GUIStyle DisconnectedStyle ;
    public GUIStyle LabelStyle ;
    public Vector2 Offset=new Vector2(6,8);
    public Vector2 IconSize=new Vector2(20,20);
    public Vector2 LabelSize=new Vector2(100,30);
    public string Label;
    public Color Color;
}
