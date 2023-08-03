using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


//TO DO:
//
//Some iterations are not necessary, try to avoid them (ex. C# predicates);

[DisallowMultipleComponent]
public class LevelBuilder : SingletonMonobehaviour<LevelBuilder>
{
    public Dictionary<string, Room> levelBuilderRoomDictionary = new Dictionary<string, Room>();
    Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    List<RoomTemplateSO> roomTemplateList = null;
    RoomNodeTypeListSO roomNodeTypeList;
    bool levelBuildSuccessful;

    protected override void Awake()
    {
        //Call awake in the base class first --> singleton 
        base.Awake();

        //Load the room node type list
        LoadRoomNodeTypeList();

        //Set dimmed material to fully visible
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    //Load the room node type list
    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }


    //Generate random level, returns true if level built, false if failed
    public bool GenerateLevel(LevelSO currentLevel)
    {
        roomTemplateList = currentLevel.roomTemplateList;

        //Load the scriptable object from templates into the dictionary
        LoadRoomTemplatesIntoDictionary();

        levelBuildSuccessful = false;
        int levelBuildAttempts = 0;

        while (!levelBuildSuccessful && levelBuildAttempts < Settings.maxLevelBuildAttempts)
        {
            levelBuildAttempts++;

            //Select a random room node graph from the list
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentLevel.roomNodeGraphList);

            int levelRebuildAttempts = 0;

            levelBuildSuccessful = false;

            //Loop untill level successfully build or more than max attempts for node graph 
            while(!levelBuildSuccessful && levelRebuildAttempts <= Settings.maxLevelRebuildAttemptsForRoomGraph)
            {
                //Clear level room gameobjects and level room dictionary
                ClearLevel();

                levelRebuildAttempts++;

                //Attempt tp buold a random level for the selected room node graph
                levelBuildSuccessful = AttemptToBuildRandomLevel(roomNodeGraph);
            }

            if (levelBuildSuccessful)
            {
                //Instantiate room gameobjects
                InstantiateRoomGameobjects();
            }

        }

        return levelBuildSuccessful;
    }

    //Load the room templates intp the dictionary 
    private void LoadRoomTemplatesIntoDictionary()
    {
        //Clear room template dictionary
        roomTemplateDictionary.Clear();

        //Load room tempalte list into dictionary
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else 
            {
                Debug.Log("Duplicate Room Template Key In " + roomTemplateList);
            }
        }
    }

    //Attempt t0 randomly build the level for the specified room nodeGraph. Returns true if a 
    //successful random layout was generated, else returns false if a problem was encourtered and 
    //another attempt is required 
    bool AttemptToBuildRandomLevel(RoomNodeGraphSO roomNodeGraph)
    {
        //Create Open Room Node Queue
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        //Add entrance node to room node queue from room node graph
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("No Entrance Node");
            return false; //Level not built
        }

        //Start with no room overlaps
        bool noRoomOverlaps = true;

        //Process open room nodes queue
        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        //if all the room nodes have been processed and there hasn't been a room overlap then return true
        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Process rooms in the open room node queue, returning true if there are no room overlaps
    bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        //While room nodes in open room node queue & no room overlaps detected
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            //Get next room node from open room node queue. 
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            //Add child Nodes to queue from room node graph (with links to this parent room)
            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            //if the room is the entrance mark as positioned and add to room dictionary 
            if (roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;

                //Add room to room dictionary 
                levelBuilderRoomDictionary.Add(room.id, room);
            }
            else //if the room type isn't an entrance 
            {
                //Else get parent room for node
                Room parentRoom = levelBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                //See if room can be placed without overlaps
                noRoomOverlaps = CanPlaceRoomWithoutOverlaps(roomNode, parentRoom);
            }
        }

        return noRoomOverlaps;
    }

    //Attempts to place the room node in the level - if room can be place return the room, else return null
    bool CanPlaceRoomWithoutOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        //initialize and assume overlap until proven otherwise
        bool roomOverlaps = true;

        //Do while Room Overlaps - try to place against all available doorways of the parent until
        //the room is successfully placed without overlap 
        while (roomOverlaps)
        {
            //Select random unconnected available doorway for the parent
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorwayList).ToList();

            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                //If no more doorways to try then overlap failure
                return false; //room overlaps
            }

            Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            //Get a random room template for room node that is consistent with the parent door orientation 
            RoomTemplateSO roomtemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            //Create a room
            Room room = CreateRoomFromRoomTemplate(roomtemplate, roomNode);

            //Place the room - retruns true if the room doesn't overlap
            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                //If room doesn't overlap then set to false to exit while loop
                roomOverlaps = false;

                //Mark room as positioned
                room.isPositioned = true;

                //Add room to dictionary
                levelBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                roomOverlaps = true;
            }
        }

        return true; //no room overlaps
    }

    //Get Random room template for room node taking into account the parent doorway orientation
    RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomtemplate = null;

        //If room node is a corridor then select andom correct corridor room template based on parent doorway orientation
        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomtemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;
                
                case Orientation.east:
                case Orientation.west:
                    roomtemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;
                case Orientation.none:
                    break;
                default:
                    break;
            }
        }
        else //if not a corridor, just select random
        {
            roomtemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        return roomtemplate;
    }

    //Place the room - returns true if the room doesn't overlap, false otherwise 
    bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        //Get current room doorway position 
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorwayList);

        //Return if no doorway in room opposite to paretn doorway
        if (doorway == null)
        {
            //Just mark the parent doorway as unavailable so we don't try and connect it again 
            doorwayParent.isUnavailable = true;

            return false;
        }

        //Calculate 'world' grid parent doorway position 
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;
        //Calculate adjustment position offset based on room doorway position that we are trying to connect (e.g. if this doorway is west then we need 
        //to add (1,0) to the east parent doorway)

        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;
            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;
            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;
            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;
            case Orientation.none:
                break;
            default:
                break;
        }

        //Calculate room lower bounds and upper bounds based on positioning to align with parent doorway 
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForRoomOverlap(room);

        if (overlappingRoom == null)
        {
            //mark doorways as connected & unavailable
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;

            //return true to show rooms have been connected with no overlaps
            return true;
        }
        else 
        {
            //Just mark the parent doorway as unavailable so we don't try and connect it again
            doorwayParent.isUnavailable = true;
            return false;
        }

    }

    //Get the doorway from the doorway list that has the opposite orientation to doorway 
    Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {
        foreach (Doorway doorwayToCheck in doorwayList)
        {
            if (parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north)
            {
                return doorwayToCheck;
            }
        }

        return null;
    }

    //Check for rooms that overlap the upper and lower bounds parameters, and if there are overlapping rooms then return room else return null
    Room CheckForRoomOverlap(Room roomToTest)
    {
        //Iterate through all rooms
        foreach (KeyValuePair<string, Room> keyvaluepair in levelBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            //skip if same room as room to test or room hasn't been positioned
            if (room.id == roomToTest.id || !room.isPositioned)
                continue;
            
            //If room overlaps
            if (IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }
        }

        return null;
    }

    //Check if 2 rooms overlap each other - return true if they overlap or false if they don't overlap
    bool IsOverLappingRoom(Room room1, Room room2)
    {
        bool isOverlappingXAxis = IsOverlappinginterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);
        bool isOverlappingYAxis = IsOverlappinginterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);

        if (isOverlappingXAxis && isOverlappingYAxis)
        {
            return true;
        }
        else 
            return false;
    }

    //Check if interval 1 overlaps interval 2 - this method is used by the IsOverlappingRoom methid 
    bool IsOverlappinginterval(int imin1, int imax1, int imin2, int imax2)
    {
        if (Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Get a random room template from the roomtemplatelist that matches the roomType and return it 
    //(return null if no matching room templates found).
    RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        //Loop through room template list 
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            //Add matching room templates
            if(roomTemplate.roomnodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        //Return null if list is zero
        if (matchingRoomTemplateList.Count == 0)
            return null;
        
        //Select random room templ,ate from list and return 
        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }

    //Get unconnected doorways
    IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        //Loop through doorways list
        foreach (Doorway doorway in roomDoorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
                yield return doorway;
        }
    }

    //Create room based on rooomTempalte and layoutNode, and return the create room
    Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        //Initialise room from template
        Room room = new Room();

        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomnodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;

        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorwayList = CopyDoorwayList(roomTemplate.doorwayList);

        //Set parent ID for room
        if (roomNode.parentRoomNodeIDList.Count == 0) // ENTRANCE
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }

        return room;
    }

    //Select a random room node graph from the list of room node graphs
    RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("No room node graphs in List");
            return null;
        }
    }

    //Create deep copy of string list
    List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();

        foreach (string stringValue in oldStringList)
        {
            newStringList.Add(stringValue);
        }

        return newStringList;
    }

    //Create deep copy of doorway list
    List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();

        foreach (Doorway doorway in oldDoorwayList)
        {
            Doorway newDoorway = new Doorway();

            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorWayStartCopyPosition = doorway.doorWayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;

            newDoorwayList.Add(newDoorway);
        }

        return newDoorwayList;
    }

    //Instantiate the level room gameobjects from the prefabs
    void InstantiateRoomGameobjects()
    {
        //Iterate through all level rooms
        foreach (KeyValuePair<string, Room> keyvaluepair in levelBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            //Calculate room position (remember the room instantiation position needs to be adjusted by the room template lower bounds)
            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            //Instantiate room 
            GameObject roomGameobject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);
            
            //Get instantiated room component from instantiated prefab
            InstantiatedRoom instantiatedRoom = roomGameobject.GetComponentInChildren<InstantiatedRoom>();

            instantiatedRoom.room = room;

            //Initialise the instantiated room
            instantiatedRoom.Initialise(roomGameobject);

            //Save gameobject reference
            room.instantiatedRoom = instantiatedRoom;
        }
    }

    //Get a room template by room template ID, returns null if ID doesn't exist
    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
        {
            return null;
        }
    }

    //Get room by roomID, if no room exists with that ID return null
    public Room GetRoomByRoomID(string roomID)
    {
        if (levelBuilderRoomDictionary.TryGetValue(roomID, out Room room))
        {
            return room;
        }
        else
        {
            return null;
        }
    }


    //Clear level room gameobjects and level room dictionary 
    void ClearLevel()
    {
        //Destroy instantiated level gameobjects and clear level manager room dictionary 
        if (levelBuilderRoomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string, Room> keyValuePair in levelBuilderRoomDictionary)
            {
                Room room = keyValuePair.Value;

                if (room.instantiatedRoom != null)
                {
                    Destroy(room.instantiatedRoom.gameObject);
                }
            }

            levelBuilderRoomDictionary.Clear();
        }
    }
}
