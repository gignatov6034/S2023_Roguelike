using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks; //Capture callbacks happening in the editor
using System;
using System.Collections.Generic;

//Unity Scripting Reference: "https://docs.unity3d.com/2021.1/Documentation/ScriptReference/index.html"

/*
This script creates, makes available and adds controls to 
the new windows within Level Editor menu item. 
This makes the process of creating levels much easier and faster. 
*/

public class RoomNodeGraphEditor : EditorWindow
{
    //GUIStyle styles information for GUI elements within windows 
    //Reference: "https://docs.unity3d.com/2021.1/Documentation/ScriptReference/GUIStyle.html"
    GUIStyle roomNodeStyle;
    GUIStyle roomNodeSelectedStyle; //Style used when node is selected

    Vector2 graphOffset; //How much we are going to offset the horizontal and vertical lines of the grid 
    Vector2 graphDrag;

    static RoomNodeGraphSO currentRoomNodeGraph;
    RoomNodeSO currentRoomNode = null;
    RoomNodeTypeListSO roomNodeTypeList;

    //Layout values used for padding and borders
    const float nodeWidth = 160f;
    const float nodeHeight = 75f;
    const int nodePadding = 25;
    const int nodeBorder = 12;

    //Connecting line values
    const float connectingLineWidth = 3f;
    const float connectingLineArrowSize = 6F;

    //Grid Spacing
    const float gridLarge = 100f;
    const float gridSmall = 25f;

    //Makes this editor window appear on Unity 
    //Reference: "https://docs.unity3d.com/2021.1/Documentation/ScriptReference/MenuItem.html"

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Level Editor/Room Grap Editor")]

    static void OpenWindow()
    {
        //Opens (gets) the window called "Room Node Graph Editor", created using MenuItem above
        //Returns the first EditorWindow of type t 
        //Reference: "https://docs.unity3d.com/2021.1/Documentation/ScriptReference/EditorWindow.GetWindow.html"

        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    //This tells what the GUI should contain 
    void OnEnable ()
    {
        //Delegates callback triggered when currently active/selected item has changed
        Selection.selectionChanged += InspectorSelectionChanged;

        //Specify the node styles
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D; //node1 is a default unity texture 
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        //Specify the selected node styles
        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D; //node1 on is a default unity texture 
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        //Load room node types
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        //Unsubscribe from the inspector selection changed event
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    //This opens then room node graph editor window if a room node graph scriptable object
    //asset is double clicked in the inspector 
    [OnOpenAsset(0)] //Need the namespace UnityEditor.Callbacks. The value specifies the number of methods that should be called when the asset is clicked on the inspector 
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        //Check if null
        if (roomNodeGraph != null)
        {
            OpenWindow();

            currentRoomNodeGraph = roomNodeGraph;

            return true;
        }

        return false;
    }

    //Draws controls for windows
    //Reference: "https://docs.unity3d.com/2021.1/Documentation/ScriptReference/EditorWindow.OnGUI.html"

    void OnGUI()
    {
        //If a scriptable object of type RoomNodeGraphSO has been selected then process
        if (currentRoomNodeGraph != null)
        {
            //Draw the grid
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            //Draw a line if being dragged
            DrawDraggedLine();
            
            //Process Events
            ProcessEvents(Event.current);

            //Draw connections between nodes
            DrawRoomConnections();

            //Draw Room Nodes
            DrawRoomNodes();

            if (GUI.changed)
                Repaint();
        }
    }
    
    //This draws a background grid for the editor
    void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + 
                gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + 
                gridOffset, new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
        }

        Handles.color = Color.white;
    }

    //Draws a dragged line 
    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            //Draw line from node to line position 
            //Reference: "https://docs.unity3d.com/2021.1/Documentation/ScriptReference/Handles.DrawBezier.html"
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.cyan, null, connectingLineWidth);
        }
    }

    void ProcessEvents(Event currentEvent) 
    {
        //Reset graph drag
        graphDrag = Vector2.zero;

        //Get room node that mouse is over if it's null or not currently being dragged
        if(currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        } 

        //If mouse isnt over a room node or we are currently dragging a line from the room node then process draph events
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        //else process room node events
        else 
        {
            //process room node events
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    //Checks to see to mouse is over a room node. If it is, then return the room node else null 
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }

        return null;
    }

    //Process Room Node Graph Events
    void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            //Process Mouse Down Events
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            //Process Mouse Drag Event
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            //Process Mouse Up Event
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            default:
                break;
        }
    }
    //Process mouse down events on the room node graph (not over a node)
    void ProcessMouseDownEvent(Event currentEvent)
    {
        //Process right click mouse down on graph event (show context menu)
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        //Process left mouse down on graph event
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    void ShowContextMenu(Vector2 mousePosition)
    {
        //Creates custom context menu OR drowdown menu
        //Reference: "https://docs.unity3d.com/2021.1/Documentation/ScriptReference/GenericMenu.html"
        GenericMenu menu = new GenericMenu();

        //Adds item to the context menu 
        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);

        //Shows the menu under the mouse when right-clicked
        menu.ShowAsContext();
    }

    //Create a room node at the mouse pos
    void CreateRoomNode(object userData)
    {
        //If current node graph empty then add entrance room node first
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(100f, 100f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        //userData is a mouse position in our case 
        CreateRoomNode(userData, roomNodeTypeList.list.Find(x => x.isNone)); //finds an x in the list that is none 
    }

    //Overloaded method: creates a room node at the mouse pos, but passes in RoomNodeType
    void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        //Create room node scriptable object asset
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        //Add room node to current room node graph room node list 
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        //Set room node values
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        //Add room node to room node graph scriptable object asset database 
        //Reference: "https://docs.unity3d.com/2021.1/Documentation/ScriptReference/AssetDatabase.AddObjectToAsset.html"
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        //Writes all unsaved asset changes to disk
        AssetDatabase.SaveAssets();

        //Refresh graph node dictionary
        currentRoomNodeGraph.OnValidate();
    }

    //This deletes all links between 2 or more selected nodes
    void DeleteSelectedRoomNodeLinks()
    {
        //Iterate through all nodes
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    //Get child room node
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    //if the child room node is selected
                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        //Remove childID from parent node
                        roomNode.RemoveChildRoomNodeIDFFromRoomNode(childRoomNode.id);

                        //Remove parentID from child node
                        childRoomNode.RemoveParentRoomNodeIDFFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        //Clear all selected nodes
        ClearAllSelectedRoomNodes();
    }

    //This removes selected nodes
    /*
    Since deleting an item in a collection we iterating through may cause 
    problems, instead we are going to move this item in a separate collection first 
    */
    void DeleteSelectedRoomNodes()
    {
        //First in frist out 
        Queue<RoomNodeSO> roomNodesToDeleteQueue = new Queue<RoomNodeSO>();

        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            //Check if node is selected
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodesToDeleteQueue.Enqueue(roomNode);

                //Iterate through child nodes
                foreach(string childRooMnodeID in roomNode.childRoomNodeIDList)
                {
                    //Retrieve child node
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRooMnodeID);

                    if (childRoomNode != null)
                    {
                        //Remove parentID from child node 
                        childRoomNode.RemoveParentRoomNodeIDFFromRoomNode(roomNode.id);
                    }
                }

                //Iterate through parent nodes
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    //Retrieve parent node
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        //Remove childID from parent node
                        parentRoomNode.RemoveChildRoomNodeIDFFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        //Delete queued room nodes
        while (roomNodesToDeleteQueue.Count > 0)
        {
            //Get room node from queue
            RoomNodeSO roomNodeToDelete = roomNodesToDeleteQueue.Dequeue();

            //Remove node from the dictionary 
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            //Remove node from the list 
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            //Remove node from the asset database
            DestroyImmediate(roomNodeToDelete, true);

            //Save asset database
            AssetDatabase.SaveAssets();
        }
    }

    //Clear selection from all room nodes
    void ClearAllSelectedRoomNodes()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }
    }

    //This selects all room nodes
    void SelectAllRoomNodes()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }

        GUI.changed = true;
    }

    //Process mouse up event
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        //if releasing the right mouse button and currently dragging a line
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            //Check if over a room node
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);
            
            //If room node is not null (mouse released above a node) then set it as a child of the parent room node if possible,
            //then set parentID in child room node 
            if (roomNode != null)
            {
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    //Process mouse drag event
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        //process right click drag event - draws a line
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        //Process left click drag event - drags node graph
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    //Process right mouse drag event - draws a line
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            DragConnectionLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    //Process left mouse drag event - drags room node graph
    void ProcessLeftMouseDragEvent (Vector2 dragDelta)
    {
        graphDrag = dragDelta;

        for (int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            currentRoomNodeGraph.roomNodeList[i].DragNode(dragDelta);
        }

        graphOffset += graphDrag;

        GUI.changed = true;
    }

    //Drag connection line from room node
    private void DragConnectionLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    //Clears line drag from a room node
    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    //Draws connections between nodes in the graph windows 
    void DrawRoomConnections()
    {
        //Loop through all nodes
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                //Loop through child nodes
                foreach(string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    //Get child room node from dictionary 
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    //Draws connection line between the parent room node and child room node
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        //Get line start and end position
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        //It uses perpendicular vectors to draw DIRECTION ARROWS on connection lines
        //
        //We are calculating a perpedicular vector to our line, then 
        //we are going to calculate a point on that perpendicular vector
        //After that, using one more perpendicular vector (opposite side), 
        //We are drawing a lines to our main line, creating an arrow.

        //Calculate a middle point
        Vector2 midPosition = (endPosition + startPosition)/2f;

        //Calculate vector from start to end position of line
        Vector2 direction = endPosition - startPosition;

        //Calculate normalized perpendicular positions from the mid point
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        //Calculate mid point offset position for arrow head
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        //Draw Arrow
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        //Draw line
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    //Draws room nodes in the graph window
    void DrawRoomNodes()
    {
        //Loop through all room nodes and draw them
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else
                roomNode.Draw(roomNodeStyle);
        }

        GUI.changed = true;
    }

    //Called every time unity detects an item selection change in the editor 
    void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }
}
