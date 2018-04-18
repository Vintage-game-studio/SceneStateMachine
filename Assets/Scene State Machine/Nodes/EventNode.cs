using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Code.Bon;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class EventNode : Node
{
    #region Public

    public GameObject TargetGameObject;

    public string ComponentTypeName;

    public string FieldName;

    #endregion

    #region Private

    private Type _componentType;
    private PropertyInfo _propertyInfo;
    private FieldInfo _fieldInfo;

    private Component _component;
    private UnityEvent _unityEvent;

    #endregion

    // **************

    #region Constructor & Initialize

    public EventNode(int id, Graph parentGraph) : base(id, parentGraph)
    {
        WindowRect.height = Height = 100;
        WindowRect.width = 250;
        if (Sockets.Count == 0)
        {
            Sockets.Add(new Socket(this, false));
        }
    }

    #endregion

    #region Awake 

    public override void Awake()
    {
        #region Get componentType

        if (_componentType == null)
        {
            _componentType = GetComponentType();

            if (_componentType == null)
            {
                Debug.LogError("Component " + ComponentTypeName + " not found !!!");
                return;
            }
        }

        #endregion

        #region Get component

        if (_component == null)
        {
            _component = TargetGameObject.GetComponent(_componentType);

            if (_component == null)
            {
                Debug.LogErrorFormat("Can't get component of type {0} in {1} not found !!!",
                    _componentType.Name,
                    TargetGameObject.name);
                return;
            }

        }
        #endregion

        #region Get _propertyInfo or _fieldInfo

        if (_propertyInfo == null && _fieldInfo == null)
        {
            _propertyInfo = GetPropertyInfo(_componentType);

            if (_propertyInfo == null)
                _fieldInfo = GetFieldInfo(_componentType);

            if (_propertyInfo == null && _fieldInfo == null)
            {
                Debug.LogError("Property or field --" + Id + " not found !!!");
                return;
            }
        }

        #endregion

        #region Get _unityEvent

        if (_unityEvent == null)
        {
            if (_fieldInfo != null)
                _unityEvent = _fieldInfo.GetValue(_component) as UnityEvent;

            if (_propertyInfo != null)
                _unityEvent = _propertyInfo.GetValue(_component, null) as UnityEvent;

        }
        #endregion

        #region Add Listener

        _unityEvent.AddListener(Run);
        #endregion
    }

    #endregion

    #region Run

    public override void Run()
    {
        base.Run();
        Sockets[0].RunConnectedNodes();
    }

    #endregion

    #region Duplicate

    public override Node Duplicate()
    {
        EventNode eventNode= (EventNode) base.Duplicate();

        eventNode.TargetGameObject= TargetGameObject;

        eventNode.ComponentTypeName=ComponentTypeName;

        eventNode.FieldName=FieldName;

        return eventNode;
    }

    #endregion

    #region GetPropertyInfo

    private PropertyInfo GetPropertyInfo(Type componentType)
    {
        return componentType
            .GetProperties()
            .FirstOrDefault(
                mi =>
                    mi.Name == FieldName &&
                    (mi.PropertyType == typeof(UnityEvent) || mi.PropertyType.BaseType == typeof(UnityEvent)));
    }

    #endregion

    #region GetFieldInfo

    private FieldInfo GetFieldInfo(Type componentType)
    {
        return componentType
            .GetFields()
            .FirstOrDefault(
                mi =>
                    mi.Name == FieldName &&
                    (mi.FieldType == typeof(UnityEvent) || mi.FieldType.BaseType == typeof(UnityEvent)));

    }

    #endregion

    #region GetComponentType

    private Type GetComponentType()
    {
        List<Type> componentsTypes =
            TargetGameObject
                .GetComponents<Component>()
                .Select(c =>
                {
                    if(c==null)
                        Debug.LogError("component error in "+ TargetGameObject.name);
                    return c.GetType();
                }).ToList();

        List<string> componentTypeNames = componentsTypes.Select(ct => ct.Name).ToList();

        int currentTypeIndex = componentTypeNames.IndexOf(ComponentTypeName);

        Type componentType = null;

        if (currentTypeIndex != -1)
            componentType = componentsTypes[currentTypeIndex];

        return componentType;
    }

    #endregion

}
