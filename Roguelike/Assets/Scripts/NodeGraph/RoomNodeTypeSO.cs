using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Creates manu item that can be used to create the following scriptable object. 
[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Level/Room Node Type")]

public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName; //Name of a room node type

    //If this room node type should appear in the node graph editor the corresponding boolean will be set to true 
    //Also gives information to the editor what room node type is needed 

    #region Header
    [Header("Only flag the RoomNodeTypes that should be visible in the editor")]
    #endregion Header
    public bool displayInNodeGraphEditor = true;

    #region Header
    [Header("One Type Should Be A Corridor")]
    #endregion Header
    public bool isCorridor;

    #region Header
    [Header("One Type Should Be A CorridorNS")]
    #endregion Header
    public bool isCorridorNS;

    #region Header
    [Header("One Type Should Be A CorridorEW")]
    #endregion Header
    public bool isCorridorEW;

    #region Header
    [Header("One Type Should Be AN Entrance")]
    #endregion Header
    public bool isEntrance;

    #region Header
    [Header("One Type Should Be A Boss Room")]
    #endregion Header
    public bool isBossRoom;

    #region Header
    [Header("One Type Should Be None (Unassigned)")]
    #endregion Header
    public bool isNone;

    #region Validation 
#if UNITY_EDITOR        //this part of the code runs only in Unity editor

    //Editor only function that Unity calls when the script is loaded or a value changes in the inspector 
    //Reference: "https://docs.unity3d.com/2021.1/Documentation/ScriptReference/MonoBehaviour.OnValidate.html"
    void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }  
#endif
    #endregion
}
