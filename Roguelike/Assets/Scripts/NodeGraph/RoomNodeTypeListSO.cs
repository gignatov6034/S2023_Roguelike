using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Creates manu item that can be used to create the following scriptable object. 
[CreateAssetMenu(fileName = "RoomNodeTypeListSO", menuName = "Scriptable Objects/Level/Room Node Type List")]


public class RoomNodeTypeListSO : ScriptableObject
{
    #region Header ROOM NODE TYPE LIST
    [Space(10)]
    [Header("ROOM NODE TYPE LIST")]
    #endregion
    #region Tooltip
    [Tooltip("This list should be populated with all the RoomBodeTypeSO for the game - it is used instead of an enum")]
    #endregion
    public List<RoomNodeTypeSO> list;

    #region Validation 
#if UNITY_EDITOR        //this part of the code runs only in Unity editor

    //Editor only function that Unity calls when the script is loaded or a value changes in the inspector 
    //Reference: "https://docs.unity3d.com/2021.1/Documentation/ScriptReference/MonoBehaviour.OnValidate.html"
    void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(list), list);
    }  
#endif
    #endregion
}
