using UnityEngine;
[System.Serializable]

public class Doorway 
{
    //TO DO:
    //----- CHANGE TO PRIVATE WITH GETTER AND SETTER IF POSSIBLE
    
    public Vector2Int position;     //Doorway position
    public Orientation orientation; //ENUM - determines whether a corrider is EW or NS 
    public GameObject doorPrefab;   //Door that separates corridors from rooms

    #region Header
    [Header("The Upper Left Position To Start Copying From")]
    #endregion
    public Vector2Int doorWayStartCopyPosition;

    #region Header
    [Header("The width of tiles in the doorway to copy over")]
    #endregion
    public int doorwayCopyTileWidth;

    #region Header
    [Header("The height of tiles in the doorway to copy over")]
    #endregion
    public int doorwayCopyTileHeight;

    [HideInInspector] public bool isConnected = false; //Has the doorway been connected or not ----- CHANGE TO PRIVATE WITH GETTER AND SETTER ------
    [HideInInspector] public bool isUnavailable = false; //Is the doorway unavailable or not ----- CHANGE TO PRIVATE WITH GETTER AND SETTER ------

}

