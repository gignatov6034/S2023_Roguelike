using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room 
{
    //TO DO:
    //----- CHANGE EVERYTHING TO PRIVATE WITH GETTERS AND SETTERS IF POSSIBLE

    public string id;
    public string templateID;   //Room template id
    public GameObject prefab;
    public RoomNodeTypeSO roomNodeType;

    //Coordinated of the room being placed - not the same as bounds of a room template
    public Vector2Int lowerBounds;
    public Vector2Int upperBounds;

    //Original room template bounds
    public Vector2Int templateLowerBounds;
    public Vector2Int templateUpperBounds;

    public Vector2Int[] spawnPositionArray;

    //Store id-s of all child rooms (up to 3 ---> settings)
    public List<string> childRoomIDList;

    //Holds parent room id
    public string parentRoomID;

    public List<Doorway> doorwayList;

    //Was this room positioned yet or not 
    public bool isPositioned = false;

    public InstantiatedRoom instantiatedRoom;

    public bool isLit = false;
    public bool isClearedOfEnemies = false;
    public bool isPreviouslyVisited = false;


    //Constructor -> When room is created assign the following
    public Room()
    {
        childRoomIDList = new List<string>();
        doorwayList = new List<Doorway>();
    }

}
