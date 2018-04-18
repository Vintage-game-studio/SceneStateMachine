using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Assets.Code.Bon;
using Assets.Editor.Bon;
using System.Linq;
using Assets.Code.Bon.Nodes;
using Object = UnityEngine.Object;


namespace Assets.Editor
{

    public class BonWindow : EditorWindow
    {

        #region Const

        private const string Name = "Scene State Machine";

        #endregion

        #region Public


        #endregion

        #region Private

        private const float CanvasZoomMin = 0.1f;
        private const float CanvasZoomMax = 1.0f;

        private Vector2 _nextTranlationPosition;

        private SsmCanvas _canvas;
        private Rect _canvasRegion = new Rect();

        private Socket _dragSourceSocket = null;
        private Vector2 _lastMousePosition;

        private GenericMenu _menu;
        private GenericMenu _dragMenu;
        private Dictionary<string, Type> _menuEntryToNodeType;

        private Rect _tmpRect = new Rect();
        private SceneStateMachine _stateMachine;
        private GameObject _dragGameObject;
        private Rect _boxSelectionRect;
        private Vector2 _startDragWindow;
        private Vector2 _boxSelectionOrigin;
        private bool _fristTime = true;

        #endregion

        //***************

        #region Creation Menu

        [MenuItem("Window/" + Name)]
        static void OnCreateWindow()
        {
            BonWindow window = GetWindow<BonWindow>();
            window.Show();
        }

        #endregion

        #region Init

        public void OnEnable()
        {
            Init();
        }

        public void Init()
        {
            titleContent = new GUIContent(Name);
            wantsMouseMove = true;

            _menuEntryToNodeType = CreateMenuEntries();
            _menu = CreateGenericMenu();
            _dragMenu = CreateDragMenu();
            _boxSelectionRect = new Rect(-1, -1, 0, 0);

            #region Set Setting

            if (SssmSettings.Setting == null)
            {
                string[] assets = AssetDatabase.FindAssets("SSM_Settings");

                if (assets.Length == 0)
                {
                    Debug.LogError("Can't find SSM Settings !!!");
                    return;
                }

                if (assets.Length > 1)
                {
                    Debug.LogError("More than one SSM Settings !!!");
                    return;
                }

                SssmSettings.Setting = AssetDatabase.LoadAssetAtPath<SssmSettings>(AssetDatabase.GUIDToAssetPath(assets[0]));

            }

            #endregion

            _canvas = new SsmCanvas(GetStateMachine().MainGraph);

            EditorApplication.playmodeStateChanged += OnPlayStateChanged;

            Repaint();

        }

        private void OnPlayStateChanged()
        {
            _stateMachine = GameObject.FindObjectOfType<SceneStateMachine>();

            if (_stateMachine)
                _canvas.Graph = _stateMachine.MainGraph;
        }

        #endregion

        #region GetStateMachine

        private SceneStateMachine GetStateMachine()
        {
            if (_stateMachine == null)
                _stateMachine = GameObject.FindObjectOfType<SceneStateMachine>();

            if (_stateMachine == null)
                _stateMachine = new GameObject("Scene State Machine").AddComponent<SceneStateMachine>();

            return _stateMachine;
        }

        #endregion

        #region OnGUI ****************

        void OnGUI()
        {
            HandleCanvasTranslation();
            HandelDragObject();
            HandelSelection();
            HandleDragAndDrop();
            HandleDoubleClick();


            if (Event.current.type == EventType.ContextClick)
            {
                _menu.ShowAsContext();
                Event.current.Use();
            }

            if (GetStateMachine() == null) return;

            if (_canvas != null)
            {
                _canvasRegion.Set(0, SssmSettings.Setting.TopOffset, Screen.width, Screen.height -  SssmSettings.Setting.TopOffset - SssmSettings.Setting.BottomOffset);
                _canvas.Draw((EditorWindow)this, _canvasRegion, _dragSourceSocket, _boxSelectionRect);
            }

            HandelKeyBoard();


            if (_fristTime)
            {
                FrameNodes();
                _fristTime = false;
            }
            _lastMousePosition = Event.current.mousePosition;

            Repaint();
        }

        #endregion

        #region HandelKeyBoard

        private void HandelKeyBoard()
        {
            Event current = Event.current;

            if (current.type == EventType.KeyDown)
            {

                if (current.keyCode == KeyCode.Delete)
                {
                    
                    _canvas.Graph.GetSelectedNodes().ForEach(n =>
                    {
                        Undo.RecordObject(n.Container, "Delete Nodes");
                    });
                    _canvas.Graph.RemoveSelectedNodes();
                }

                if (current.keyCode == KeyCode.F)
                {
                    Undo.RecordObject(GetStateMachine(), "Frame Nodes");
                    FrameNodes();
                }
            }
            if (current.type==EventType.KeyUp && current.keyCode == KeyCode.D && current.control)
            {
                Undo.RecordObject(GetStateMachine(), "Duplicate Nodes");
                _canvas.Graph.DuplicateSelectedNodes();
            }
        }

        #endregion

        #region HandelSelection

        private void HandelSelection()
        {
            EventType eventType = Event.current.type;

            if (eventType == EventType.MouseDown && Event.current.button == 0)
            {
                Node node = _canvas.GetNodeAt(Event.current.mousePosition);

                if (node == null)
                {
                    _boxSelectionRect.min =
                        _boxSelectionRect.max =
                            _boxSelectionOrigin =
                                ConvertScreenCoordsToZoomCoords(_lastMousePosition);

                    _canvas.Graph.DeselectAll();
                    GUI.FocusControl("");
                }
                else
                {
                    GUI.FocusWindow(node.Id);

                    _startDragWindow = Event.current.mousePosition;

                    if (!node.IsSelected)
                    {
                        GUI.FocusControl("");
                        _canvas.Graph.SelectNode(node, Event.current.shift);
                    }
                }
            }

            if (eventType == EventType.MouseDrag && _boxSelectionRect.x >= 0 && Event.current.button == 0)
            {
                Vector2 coords = ConvertScreenCoordsToZoomCoords(_lastMousePosition);

                if (_boxSelectionOrigin.x <= coords.x)
                {
                    _boxSelectionRect.xMin = _boxSelectionOrigin.x;
                    _boxSelectionRect.xMax = coords.x;
                }
                else
                {
                    _boxSelectionRect.xMin = coords.x;
                    _boxSelectionRect.xMax = _boxSelectionOrigin.x;
                }

                if (_boxSelectionOrigin.y <= coords.y)
                {
                    _boxSelectionRect.yMin = _boxSelectionOrigin.y;
                    _boxSelectionRect.yMax = coords.y;
                }
                else
                {
                    _boxSelectionRect.yMin = coords.y;
                    _boxSelectionRect.yMax = _boxSelectionOrigin.y;
                }

                _canvas.Graph.BoxSelection(_boxSelectionRect);
            }

            if ((eventType == EventType.MouseUp || eventType == EventType.MouseMove) && _boxSelectionRect.x >= 0)
            {
                _boxSelectionRect.x = -1;
            }

            if (eventType == EventType.MouseUp && _startDragWindow == Event.current.mousePosition)
            {
                Node node = _canvas.GetNodeAt(Event.current.mousePosition);
                if (node != null)
                    _canvas.Graph.SelectNode(node, Event.current.shift);
            }


        }

        #endregion

        #region HandelDragObject 

        void HandelDragObject()
        {
            var eventType = Event.current.type;

            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

                if (eventType == EventType.DragPerform)
                {
                    if (_canvas.GetNodeAt(Event.current.mousePosition) != null)
                        return;

                    DragAndDrop.AcceptDrag();

                    foreach (Object o in DragAndDrop.objectReferences)
                        if (o is GameObject)
                        {
                            _dragGameObject = (GameObject)o;
                            _dragMenu.ShowAsContext();
                            Event.current.Use();
                            return;
                        }
                }

            }
        }

        private GenericMenu CreateDragMenu()
        {
            GenericMenu m = new GenericMenu();

            m.AddItem(new GUIContent("Action"), false, OnDragMenuClick, typeof(ActionNode));
            m.AddItem(new GUIContent("Event"), false, OnDragMenuClick, typeof(EventNode));
            return m;
        }

        private void OnDragMenuClick(object item)
        {
            if (_canvas != null)
            {
                if (item == typeof(ActionNode))
                {
                    ActionNode actionNode =
                        (ActionNode)_canvas.CreateNode(typeof(ActionNode), _lastMousePosition);

                    actionNode.TargetGameObject = (GameObject)_dragGameObject;
                }

                if (item == typeof(EventNode))
                {
                    EventNode eventNode =
                        (EventNode)_canvas.CreateNode(typeof(EventNode), _lastMousePosition);

                    eventNode.TargetGameObject = (GameObject)_dragGameObject;
                }
            }
        }

        #endregion

        #region HandleDoubleClick

        private void HandleDoubleClick()
        {
            if (Event.current.clickCount == 2 && Event.current.type == EventType.MouseDown)
            {
                Node node = _canvas.GetNodeTitleAt(Event.current.mousePosition);

                if (node == null)
                    return;

                if (node.Collapsed)
                    node.Expand();
                else
                    node.Collapse();

                Event.current.Use();
            }

        }

        #endregion

        #region Menu

        public Dictionary<string, Type> CreateMenuEntries()
        {
            Dictionary<string, Type> menuEntries = new Dictionary<string, Type>();

            IEnumerable<Type> classesExtendingNode = Assembly.GetAssembly(typeof(Node)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Node)));

            foreach (Type type in classesExtendingNode) menuEntries.Add(type.Name, type);

            menuEntries.OrderBy(x => x.Key);
            return menuEntries;
        }

        private GenericMenu CreateGenericMenu()
        {
            GenericMenu m = new GenericMenu();
            foreach (KeyValuePair<string, Type> entry in _menuEntryToNodeType)
                m.AddItem(new GUIContent(entry.Key), false, OnGenericMenuClick, entry.Value);
            return m;
        }

        private void OnGenericMenuClick(object item)
        {
            if (_canvas != null)
            {
                _canvas.CreateNode((Type)item, _lastMousePosition);
            }
        }

        #endregion

        #region Handle Zoom Pan Frame

        private void HandleCanvasTranslation()
        {
            if (_canvas == null) return;

            // Zoom
            if (Event.current.type == EventType.ScrollWheel)
            {
                Vector2 zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(Event.current.mousePosition);

                float zoomDelta = -Event.current.delta.y / 150.0f;
                float oldZoom = _canvas.Zoom;
                _canvas.Zoom = Mathf.Clamp(_canvas.Zoom + zoomDelta, CanvasZoomMin, CanvasZoomMax);

                Vector2 newzoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(Event.current.mousePosition);


                _nextTranlationPosition = _canvas.Position + newzoomCoordsMousePos - zoomCoordsMousePos;

                if (_nextTranlationPosition.x >= 0) _nextTranlationPosition.x = 0;
                if (_nextTranlationPosition.y >= 0) _nextTranlationPosition.y = 0;

                _canvas.Position = _nextTranlationPosition;


                Event.current.Use();
                return;
            }

            // Translate
            if (Event.current.type == EventType.MouseDrag &&
                (Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt) ||
                Event.current.button == 2)
            {
                Vector2 delta = Event.current.delta;
                delta /= _canvas.Zoom;

                _nextTranlationPosition = _canvas.Position + delta;
                if (_nextTranlationPosition.x >= 0) _nextTranlationPosition.x = 0;
                if (_nextTranlationPosition.y >= 0) _nextTranlationPosition.y = 0;

                _canvas.Position = _nextTranlationPosition;
                Event.current.Use();
            }
        }
        private Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords)
        {
            return (screenCoords - _canvasRegion.TopLeft()) / _canvas.Zoom - _canvas.Position;
        }
        private void FrameNodes()
        {
            List<Node> nodes = _canvas.Graph.GetSelectedNodes();

            if (nodes.Count == 0)
                nodes = _canvas.Graph.Nodes;

            if (nodes.Count == 0)
                return;

            Rect frameRect = new Rect(nodes[0].WindowRect);

            foreach (Node node in nodes)
                frameRect = frameRect.Expand(node.WindowRect);

            _canvas.Zoom = Mathf.Clamp(
                Mathf.Min(_canvasRegion.width / frameRect.width, _canvasRegion.height / frameRect.height) - 0.05f,
                CanvasZoomMin,
                CanvasZoomMax);

            _canvas.Position = -frameRect.center + _canvasRegion.size * 0.5f / _canvas.Zoom;
        }

        #endregion

        #region HandleDragAndDrop

        private void HandleSocketDrag(Socket dragSource)
        {
            if (dragSource != null)
            {
                if (dragSource.IsInput && dragSource.IsConnected())
                {
                    Edge edge = dragSource.Edges.Last();

                    _dragSourceSocket = edge.GetOtherSocket(dragSource);
                    _canvas.Graph.UnLink(dragSource, _dragSourceSocket);
                }
                if (!dragSource.IsInput) _dragSourceSocket = dragSource;

                Event.current.Use();
            }
            Repaint();
        }

        private void HandleSocketDrop(Socket dropTarget)
        {
            if (dropTarget != null && dropTarget.IsInput != _dragSourceSocket.IsInput)
            {
                if (dropTarget.IsInput)
                    _canvas.Graph.Link(dropTarget, _dragSourceSocket);

                Event.current.Use();
            }
            _dragSourceSocket = null;

            Repaint();
        }

        private void HandleDragAndDrop()
        {
            if (_dragSourceSocket != null)
                if (_dragSourceSocket.ParentNode == null)
                    _dragSourceSocket = null;

            if (_canvas == null) return;

            if (Event.current.type == EventType.MouseDown)
            {
                HandleSocketDrag(_canvas.GetSocketAt(Event.current.mousePosition));
            }

            if (Event.current.type == EventType.MouseUp && _dragSourceSocket != null)
            {
                HandleSocketDrop(_canvas.GetSocketAt(Event.current.mousePosition));
            }

            if (Event.current.type == EventType.MouseDrag)
            {
                if (_dragSourceSocket != null) Event.current.Use();
            }
        }

        #endregion

    }
}


