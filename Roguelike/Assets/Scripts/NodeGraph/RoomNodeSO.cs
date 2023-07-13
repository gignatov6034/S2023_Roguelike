using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id; //auto generated GUID (id)
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>(); 
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    public RoomNodeTypeSO roomNodeType;

    #region Editor Code

    //the following code should only be run in the Unity Editor
#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    //Initialise node
    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        //Load room node type list
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    //Draw node with the nodestyle
    public void Draw(GUIStyle nodeStyle)
    {
        //Draw Node Box Using Begin Area
        GUILayout.BeginArea(rect, nodeStyle);

        //Start Region To Detect Popup Selection Changes
        EditorGUI.BeginChangeCheck();

        //
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            //Display a locked label 
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            //Display a popup using the RoomNodeType name values that can be selected from (default to the currently set roomNodeType)

            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            //If the room type selection has changed making child connections potentiially invalid 
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor || 
                !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        //Get child room node
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        //if the child room node is not null
                        if (childRoomNode != null)
                        {
                            //Remove childID from parent node
                            RemoveChildRoomNodeIDFFromRoomNode(childRoomNode.id);

                            //Remove parentID from child node
                            childRoomNode.RemoveParentRoomNodeIDFFromRoomNode(id);
                        }
                    }
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this); //make any changes we make in the displayed popup we are going to create saved
        
        GUILayout.EndArea();
    }

    //Populate a string array with the room node types to display that can be selected
    public string[] GetRoomNodeTypesDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
        }

        return roomArray;
    }

    //Process events for the node
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            //Process Mouse Down Events
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            //Process Mouse Up Events
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            //Process Mouse Down Events
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    //Process mouse down events
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //left click down
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        //right click down
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    //Process right click down event
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    //Process left click down event
    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;

        //Toggle node selection
        isSelected = !isSelected;
    }

    //Process mouse up events
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        //left click up
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    //Process left click up event
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
            isLeftClickDragging = false;

    }

    //Process mouse drag events
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        //left click drag event
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDragEvent(currentEvent);
        }
    }

    //Process left click drag event
    private void ProcessLeftClickDragEvent(Event currentEvent)
    {
        isLeftClickDragging = false;

        DragNode(currentEvent.delta); //Captures relative movement of the mouse compared to the alst event
        GUI.changed = true;
    }

    //This simply drags node
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    //Add childID to the node (returns true if the node has benn added, false otherwise)
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        //Check child node can be added validly to parent 
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }
        return false;
    }


    //Check if the child node can be added to the parent. Return true if yes or false otherwoise
    private bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossNodeAlready = false;

        //Check if there is an existing and connected boss room node 
        foreach(RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
                isConnectedBossNodeAlready = true;
        }

        //if the child node has a type of boss room connected then false 
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
            return false;

        //if a child has a type of none then false because a node should be specified 
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
            return false;
        
        //If a node tries to connect to the same child then false
        if (childRoomNodeIDList.Contains(childID))
            return false;

        //If a node tries to connect to itself then false
        if (id == childID)
            return false;
        
        //if this childID is already in the parentID list return false
        if (parentRoomNodeIDList.Contains(childID))
            return false;

        //if a child already has its parent
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
            return false;

        //if a corridor then return false when try to connect to another corridor 
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
            return false;

        //if both rooms then return false
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
            return false;

        //check if the max number of connections is exceeded, if yes return false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
            return false;

        //if entrance then return false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
            return false;
        
        //If connecting to a corridor check if the corridor already has a connection 
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
            return false;

        //Valid
        return true;
    }

    //Add parentID to the node (returns true if the node has been added, false otherwise)
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    //Remove childID from the node (returns true if the node has been removed, false otherwise)
    public bool RemoveChildRoomNodeIDFFromRoomNode(string childID)
    {
        //if the node contains the child ID then remove it
        if(childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }

        return false;
    }

    //Remove parentID from the node (returns true if the node has been removed, false otherwise)
    public bool RemoveParentRoomNodeIDFFromRoomNode(string parentID)
    {
        //if the node contains the child ID then remove it
        if(parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }

        return false;
    }

#endif
    #endregion Editor Code
}
