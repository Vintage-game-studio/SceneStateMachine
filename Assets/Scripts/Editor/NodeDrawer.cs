using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Code.Bon;
using Assets.Code.Bon.Nodes;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace Assets.Editor.Bon
{
    public class NodeDrawer
    {
        #region private

        private Rect _tmpRect;
        private GUIStyle _style;

        #endregion

        // *********************

        #region OnGUI

        public void OnGUI(Node node)
        {
            _style = node.Setting.WindowStyle;

            _tmpRect = _style.padding.Remove(
                new Rect(Vector2.zero, node.WindowRect.size));

            GUILayout.BeginArea(_tmpRect);


            if (!node.Collapsed)
            {
                string name = EditorGUILayout.TextField(node.Name);

                if (name != node.Name)
                {
                    Undo.RecordObject(node.Container, "Change node name");
                    node.Name = name;
                    Event.current.Use();
                }

                if (node.GetType() == typeof(ActionNode))
                    OnGUIActionNode((ActionNode)node);

                if (node.GetType() == typeof(EventNode))
                    OnGUIEventNode((EventNode)node);
            }

            GUILayout.EndArea();
        }

        #endregion

        #region OnGUIEventNode

        private void OnGUIEventNode(EventNode eventNode)
        {
            #region Get TargetGameObject

            GameObject gameObject =
                (GameObject)
                    EditorGUILayout.ObjectField("Game Object", eventNode.TargetGameObject, typeof(GameObject), true);

            if (gameObject != eventNode.TargetGameObject)
            {
                Undo.RecordObject(eventNode.Container, "Change game object in event");
                eventNode.TargetGameObject = gameObject;
            }

            if (eventNode.TargetGameObject == null)
                return;

            #endregion

            #region Get ComponentType

            List<Type> componentsTypes =
                eventNode.TargetGameObject
                    .GetComponents<Component>()
                    .Select(c => c.GetType()).ToList();

            // Remove Transform component
            componentsTypes.RemoveAt(0);

            List<string> componentTypeNames = componentsTypes.Select(ct => ct.Name).ToList();

            int currentTypeIndex = componentTypeNames.IndexOf(eventNode.ComponentTypeName);

            if (currentTypeIndex == -1)
                currentTypeIndex = 0;

            string componentTypeName =
                componentTypeNames[EditorGUILayout.Popup("Component", currentTypeIndex, componentTypeNames.ToArray())];

            if (eventNode.ComponentTypeName != componentTypeName)
            {
                Undo.RecordObject(eventNode.Container, "Change Component type in eventNode");
                eventNode.ComponentTypeName = componentTypeName;
            }

            Type componentType = componentsTypes[currentTypeIndex];

            #endregion

            #region get property or field

            List<PropertyInfo> propertyInfos = componentType
                .GetProperties()
                .Where(mi => mi.PropertyType == typeof(UnityEvent) || mi.PropertyType.BaseType == typeof(UnityEvent))
                .ToList();

            List<FieldInfo> fieldInfos = componentType.GetFields()
                .Where(mi => mi.FieldType == typeof(UnityEvent) || mi.FieldType.BaseType == typeof(UnityEvent))
                .ToList();

            List<string> fieldNames = propertyInfos.Select(mi => mi.Name).ToList();
            fieldNames.AddRange(fieldInfos.Select(fi => fi.Name));

            if (fieldNames.Count > 0)
            {
                int selectedIndex = fieldNames.IndexOf(eventNode.FieldName);

                if (selectedIndex == -1)
                    selectedIndex = 0;


                selectedIndex = EditorGUILayout.Popup("Field", selectedIndex, fieldNames.ToArray());

                string methodName = fieldNames[selectedIndex];

                if (methodName != eventNode.FieldName)
                {
                    Undo.RecordObject(eventNode.Container, "Change field in eventNode");
                    eventNode.FieldName = methodName;
                }


            }

            #endregion
        }

        #endregion

        #region OnGUIActionNode

        private void OnGUIActionNode(ActionNode action)
        {
            #region Get delay

            float delay = EditorGUILayout.FloatField("Delay", action.Delay);

            if (action.Delay != delay)
            {
                Undo.RecordObject(action.Container, "Change delay in eventNode");
                action.Delay = delay;
            }

            #endregion

            #region Get TargetGameObject

            GameObject gameObject =
                (GameObject)
                    EditorGUILayout.ObjectField("Game Object", action.TargetGameObject, typeof(GameObject), true);

            if (gameObject != action.TargetGameObject)
            {
                Undo.RecordObject(action.Container, "Change game object in eventNode");
                action.TargetGameObject = gameObject;
            }

            if (action.TargetGameObject == null)
                return;

            #endregion

            #region Get ComponentType

            List<Type> componentsTypes =
                action.TargetGameObject
                    .GetComponents<Component>()
                    .Select(c => c.GetType()).ToList();

            // Remove Transform component
            // componentsTypes.RemoveAt(0);

            // add game object
            componentsTypes.Insert(0,typeof(GameObject));


            List<string> componentTypeNames = componentsTypes.Select(ct => ct.Name).ToList();

            int currentTypeIndex = componentTypeNames.IndexOf(action.ComponentTypeName);

            if (currentTypeIndex == -1)
                currentTypeIndex = 0;


            string componentTypeName =
                componentTypeNames[EditorGUILayout.Popup("Component", currentTypeIndex, componentTypeNames.ToArray())];

            if (action.ComponentTypeName != componentTypeName)
            {
                Undo.RecordObject(action.Container, "Change Component type in eventNode");
                action.ComponentTypeName = componentTypeName;

            }

            Type componentType = componentsTypes[currentTypeIndex];

            #endregion

            #region get ComponentMethodInfo

            List<MethodInfo> methods = componentType.GetMethods()
                .Where(mi => mi.DeclaringType == componentType)
                .Where(mi => mi.ReturnType.Name == "Void" || mi.ReturnType.Name == "IEnumerator")
                .ToList();

            MethodInfo methodInfo = null;

            if (methods.Count > 0)
            {
                List<string> methodNames = methods.Select(mi => mi.ToString()).ToList();

                int selectedMethodIndex = methodNames.IndexOf(action.MethodName);

                if (selectedMethodIndex == -1)
                    selectedMethodIndex = 0;


                selectedMethodIndex = EditorGUILayout.Popup("Method", selectedMethodIndex, methodNames.ToArray());

                string methodName = methodNames[selectedMethodIndex];
                methodInfo = methods[selectedMethodIndex];

                if (methodName != action.MethodName)
                {
                    Undo.RecordObject(action.Container, "Change method in eventNode");
                    action.MethodName = methodName;
                }


            }

            #endregion

            #region Get Parameters

            if (methodInfo == null)
                return;

            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Length > 0)
            {
                #region Remove extra parameters

                while (action.ParameterValueStrings.Count > parameters.Length)
                    action.ParameterValueStrings.RemoveAt(action.ParameterValueStrings.Count - 1);

                #endregion

                #region Add shortcoming parameters

                while (action.ParameterValueStrings.Count < parameters.Length)
                    action.ParameterValueStrings.Add("");

                #endregion

                #region Get parameters

                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameterInfo = parameters[i];
                    if (parameterInfo.ParameterType == typeof(int))
                    {
                        int value;
                        if (!int.TryParse(action.ParameterValueStrings[i], out value))
                            value = 0;

                        action.ParameterValueStrings[i] =
                            EditorGUILayout.IntField(parameterInfo.Name, value).ToString();
                    }
                    else if (parameterInfo.ParameterType == typeof(float))
                    {
                        float value;
                        if (!float.TryParse(action.ParameterValueStrings[i], out value))
                            value = 0;

                        action.ParameterValueStrings[i] =
                            EditorGUILayout.FloatField(parameterInfo.Name, value).ToString();
                    }
                    else if (parameterInfo.ParameterType == typeof(bool))
                    {
                        bool value;
                        if (!bool.TryParse(action.ParameterValueStrings[i], out value))
                            value = false;

                        action.ParameterValueStrings[i] =
                            EditorGUILayout.Toggle(parameterInfo.Name, value).ToString();
                    }
                    else if (parameterInfo.ParameterType == typeof(string))
                    {
                        action.ParameterValueStrings[i] =
                            EditorGUILayout.TextField(
                                parameterInfo.Name, action.ParameterValueStrings[i]);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(parameterInfo.Name,
                            "Unsupported parameter ComponentType of " + parameterInfo.ParameterType.Name);
                    }


                }

                #endregion
            }
            #endregion

        }

        #endregion

    }
}
