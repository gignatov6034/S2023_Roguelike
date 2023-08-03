using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Scriptable objects are data containers that can be inherited from instead of regualr monobehaviour scripts
Any data applied to scriptable objects are persistent and saved in the unity editor and available for runtime
"SO" stands for scriptable object
*/

//Creates manu item that can be used to create the following scriptable object. 
[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Level/Room Node Graph")]

public class RoomNodeGraphSO : ScriptableObject
{
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList; //List of room node types
    [HideInInspector] public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();

    //string is a GUID (unique identifier) to the room node in this dictionary 
    //Reference: "https://learn.microsoft.com/en-us/dotnet/api/system.guid?view=net-7.0"
    [HideInInspector] public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>(); 

    void Awake()
    {
        LoadRoomNodeDictionary();
    }

    //Loads the room node dictionary from the room node list
    private void LoadRoomNodeDictionary()
    {
        roomNodeDictionary.Clear();

        //Populate dictionary
        foreach(RoomNodeSO node in roomNodeList)
        {
            roomNodeDictionary[node.id] = node;
        }
    }

    //Get room node by roomNodeType
    public RoomNodeSO GetRoomNode(RoomNodeTypeSO roomNodeType)
    {
        foreach (RoomNodeSO node in roomNodeList)
        {
            if (node.roomNodeType == roomNodeType)
            {
                return node;
            }
        }
        return null;
    }

    //Get room node by room nodeID
    public RoomNodeSO GetRoomNode(string roomNodeID)
    {
        if (roomNodeDictionary.TryGetValue(roomNodeID, out RoomNodeSO roomNode))
        {
            return roomNode;
        }
        return null;
    }

    //Get child room nodes for supplied parent room node
    public IEnumerable<RoomNodeSO> GetChildRoomNodes(RoomNodeSO parentRoomNode)
    {
        foreach (string childNodeID in parentRoomNode.childRoomNodeIDList)
        {
            yield return GetRoomNode(childNodeID);
        }
    }

    #region Editor Code

    //the following code should only be run in the Unity Editor
#if UNITY_EDITOR

    [HideInInspector] public RoomNodeSO roomNodeToDrawLineFrom = null;  //hold the room node that we start the dragging process from
    [HideInInspector] public Vector2 linePosition;                      //stores the position of the end of the drag line

    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }

    public void SetNodeToDrawConnectionLineFrom(RoomNodeSO node, Vector2 position)
    {
        roomNodeToDrawLineFrom = node;
        linePosition = position;
    }

#endif
    #endregion Editor Code
}
