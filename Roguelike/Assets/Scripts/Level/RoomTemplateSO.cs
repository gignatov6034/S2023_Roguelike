using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_", menuName = "Scriptable Objects/Level/Room")]
public class RoomTemplateSO : ScriptableObject
{
    //TO DO:
    //----- CHANGE EVERYTHING TO PRIVATE WITH GETTERS AND SETTERS IF POSSIBLE

    [HideInInspector] public string guid; //Unique id to identify certain tempaltes

    #region Header ROOM PREFAB
    [Space (10)]
    [Header("ROOM PREFAB")]
    #endregion Header ROOM PREFAB

    #region Tooltip
    
    [Header("The gameobject prefab for the room (this will contain all the tilemaps for the room and environment game objects)")]
    #endregion Header Tooltip

    public GameObject prefab; //Prefab of a single room 

    [HideInInspector] public GameObject previousPrefab; //this is used to regenerate the guid if the SO is copied and the prefab is changed 

    #region Header ROOM CONFIGURATION
    [Space (10)]
    [Header("ROOM CONFIGURATION")]
    #endregion Header ROOM CONFIGURATION

    #region Tooltip
    
    [Header("The room node type SO - The Room node types correspond to the room nodes used in the room node graph. The exceptions being with corridors - 2 types - CorridorNS and CorridorEW despite having only one type 'Corridor'")]
    #endregion Header Tooltip

    public Vector2Int lowerBounds; //Bottom-left position of a room or corridor

    public Vector2Int upperBounds; //Top-right position of a room or corridor 

    [SerializeField] public List<Doorway> doorwayList; 

    public Vector2Int[] spawnPositionArray; //it is a spawn position array - array of coordinates of rooms where emenies, a player, or treasures should spawn 

    //Return the list of doorways (entrances)
    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

    #region Validation 
#if UNITY_EDITOR
    //Validate SO fields
    private void OnValidate()
    {
        //Set unique GUID if empty or the prefab changes
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);

        //Check spawn positions populated
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPositionArray), spawnPositionArray);
    }
#endif
    #endregion Validation
}
