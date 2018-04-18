using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Assets.Code.Bon.Nodes
{
    [Serializable]
    public class ActionNode : Node
    {
        #region Public

        public float Delay = 0;

        public GameObject TargetGameObject;

        public string ComponentTypeName;

        public string MethodName;

        public List<string> ParameterValueStrings = new List<string>();

        #endregion

        #region Private

        private MethodInfo _methodInfo;
        private object _object;
        private object[] _parameters;
        private bool _isRunning = false;

        #endregion

        //****************************

        #region Constructor & Initialize

        public ActionNode(int id, Graph parentGraph) : base(id, parentGraph)
        {
            WindowRect.height = Height = 150;
            WindowRect.width = 300;

            if (Sockets.Count == 0)
            {
                Sockets.Add(new Socket(
                    this,
                    true));

                Sockets.Add(new Socket(
                    this,
                    false));
            }
        }
        #endregion

        //*************

        #region Duplicate

        public override Node Duplicate()
        {
            ActionNode actionNode = (ActionNode)base.Duplicate();

                    actionNode.Delay = Delay;

            actionNode.TargetGameObject=TargetGameObject;

            actionNode.ComponentTypeName=ComponentTypeName;

            actionNode.MethodName=MethodName;

            actionNode.ParameterValueStrings = new List<string>();

            foreach (string parameterValueString in ParameterValueStrings)
                actionNode.ParameterValueStrings.Add(parameterValueString);

            return actionNode;
        }
        
        #endregion

        #region Run 

        public override void Run()
        {

            #region Get componentType

            Type componentType = GetComponentType();

            if (componentType == null)
            {
                RunHasEnded();
                return;
            }

            #endregion

            #region Get methodInfo

            _methodInfo = GetMethodInfo(componentType);

            if (_methodInfo == null)
            {
                RunHasEnded();
                return;
            }

            #endregion

            #region Get component

            if (componentType != typeof(GameObject))
            {
                _object = TargetGameObject.GetComponent(componentType);

                if (_object == null)
                {
                    RunHasEnded();
                    return;
                }
            }
            else
                _object = TargetGameObject;

            #endregion

            #region Get parameters

            ParameterInfo[] parameterInfos = _methodInfo.GetParameters();

            _parameters = new object[parameterInfos.Length];

            for (int i = 0; i < _parameters.Length; i++)
            {
                ParameterInfo parameterInfo = parameterInfos[i];
                string valueString = ParameterValueStrings[i];

                if (parameterInfo.ParameterType == typeof(int)) //*** int
                {
                    int value;
                    if (!int.TryParse(valueString, out value))
                        value = 0;

                    _parameters[i] = value;
                }
                else if (parameterInfo.ParameterType == typeof(float)) //*** float
                {
                    float value;
                    if (!float.TryParse(valueString, out value))
                        value = 0;

                    _parameters[i] = value;
                }
                else if (parameterInfo.ParameterType == typeof(bool)) //*** bool
                {
                    bool value;
                    if (!bool.TryParse(valueString, out value))
                        value = false;

                    _parameters[i] = value;
                }
                else if (parameterInfo.ParameterType == typeof(string)) //*** string
                {
                    _parameters[i] = valueString;
                }
                else //*** Others
                {
                    _parameters[i] = null;
                }
            }

            #endregion

            #region Invoke

            if (_methodInfo.ReturnType.Name == "IEnumerator" || Delay > 0)
                ParentGraph.ParentStateMachine.StartCoroutine(RunAsCoroutine());
            else
            {
                base.Run();

                _methodInfo.Invoke(_object, _parameters);
                RunHasEnded();
            }

            #endregion

        }


        private IEnumerator RunAsCoroutine()
        {
            _isRunning = true;

            if (Delay > 0)
                yield return new WaitForSeconds(Delay);

            base.Run();

            if (_methodInfo.ReturnType.Name == "IEnumerator")
                yield return (IEnumerator) _methodInfo.Invoke(_object, _parameters);
            else
                _methodInfo.Invoke(_object, _parameters);

            _isRunning = false;

            RunHasEnded();
        }

        #endregion

        #region RunHasEnded

        private void RunHasEnded()
        {
            Sockets[1].RunConnectedNodes();
        }

        #endregion

        #region GetMethodInfo

        private MethodInfo GetMethodInfo(Type componentType)
        {
            List<MethodInfo> methods = componentType.GetMethods()
                .Where(mi => mi.DeclaringType == componentType)
                .Where(mi => mi.ReturnType.Name == "Void" || mi.ReturnType.Name == "IEnumerator")
                .ToList();

            List<string> methodNames = methods.Select(mi => mi.ToString()).ToList();

            int selectedMethodIndex = methodNames.IndexOf(MethodName);

            MethodInfo methodInfo = null;

            if (selectedMethodIndex != -1)
                methodInfo = methods[selectedMethodIndex];

            return methodInfo;
        }

        #endregion

        #region GetComponentType

        private Type GetComponentType()
        {
            Type _componentType = null;

            if (TargetGameObject != null)
            {
                if (ComponentTypeName == "GameObject")
                    return typeof(GameObject);

                List<Type> componentsTypes =
                    TargetGameObject
                        .GetComponents<Component>()
                        .Select(c => c.GetType()).ToList();

                List<string> componentTypeNames = componentsTypes.Select(ct => ct.Name).ToList();

                int currentTypeIndex = componentTypeNames.IndexOf(ComponentTypeName);

                if (currentTypeIndex != -1)
                    _componentType = componentsTypes[currentTypeIndex];

            }

            return _componentType;
        }

        #endregion

    }
}
