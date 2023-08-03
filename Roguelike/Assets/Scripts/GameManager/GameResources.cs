using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{
    static GameResources instance;

    //getter for instance object 
    public static GameResources Instance 
    {
        get
        {
            //loads an object of type GameResources into the instance if it is null
            if (instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }
            return instance;
        }
    }

    #region Header LEVEL
    [Space(10)]
    [Header("LEVEL")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the dungeon RoomNodeTypeListSO")]
    #endregion

    public RoomNodeTypeListSO roomNodeTypeList;


    #region Header MATERIALS
    [Space(10)]
    [Header("MATERIALS")]
    #endregion

    #region Tooltip
    [Tooltip("Dimmed Material")]
    #endregion

    public Material dimmedMaterial;
}
