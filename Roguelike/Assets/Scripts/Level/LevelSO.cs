using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameLevel_", menuName = "Scriptable Objects/Level/Level Generation")]
public class LevelSO : ScriptableObject
{
    //TO DO:
    //----- CHANGE EVERYTHING TO PRIVATE WITH GETTERS AND SETTERS IF POSSIBLE

    #region Header LEVEL DETAILS
    [Space (10)]
    [Header("LEVEL DETAILS")]
    #endregion Header LEVEL DETAILS

    #region Tooltip
    
    [Header("The name for the level")]
    #endregion Header Tooltip

    public string levelName;

    #region Header LEVEL ROOM TEMPLATES
    [Space (10)]
    [Header("LEVEL ROOM TEMPLATES")]
    #endregion Header LEVEL ROOM TEMPLATES

    public List<RoomTemplateSO> roomTemplateList;

    #region Header LEVEL ROOM NODE GRAPHS
    [Space (10)]
    [Header("LEVEL ROOM NODE GRAPHS")]
    #endregion Header LEVEL ROOM NODE GRAPHS

    #region Tooltip
    
    [Header("Populate this list with the room node graphs which should be randomly selected from")]
    #endregion Header Tooltip

    public List<RoomNodeGraphSO> roomNodeGraphList;

    //Validate scriptable object details entered
    #region Validation
#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(levelName), levelName);

        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomTemplateList))
            return;
        
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
            return;

        //Check to make sure that room templates are specified for all the node types in the specified node graph 

        //Check if north or south corridor, east or west corridor, and entrance types have been specified
        //TO DO:
        //Optimize it? Not the most efficient ---> SWITCH CASE instead of IF 
        //Not necessary since it affects only UNITY EDITOR  
        bool isEWCorridor = false;
        bool isNSCorridor = false;
        bool isEntrance = false;

        //Loop through all the templates of rooms to check that the node type has been specified
        foreach(RoomTemplateSO roomTemplateSO in roomTemplateList)
        {
            if (roomTemplateSO == null)
                return;

            if (roomTemplateSO.roomnodeType.isCorridorEW)
                isEWCorridor = true;
            
            if (roomTemplateSO.roomnodeType.isCorridorNS)
                isNSCorridor = true;
            
            if (roomTemplateSO.roomnodeType.isEntrance)
                isEntrance = true;
        }

        if (isEWCorridor == false)
        {
            Debug.Log("In " + this.name.ToString() + " : No E/W Corridor specified");
        }

        if (isNSCorridor == false)
        {
            Debug.Log("In " + this.name.ToString() + " : No N/S Corridor specified");
        }

        if (isEntrance == false)
        {
            Debug.Log("In " + this.name.ToString() + " : No Entrance specified");
        }

        //Loop through all node grapths
        foreach (RoomNodeGraphSO roomNodeGraph in roomNodeGraphList)
        {
            if (roomNodeGraph == null)
                return;
            
            //Loop through all nodes in node graph
            foreach (RoomNodeSO roomNodeSO in roomNodeGraph.roomNodeList)
            {
                if (roomNodeSO == null)
                    continue;

                //Check that a room template has been specified for each roomNode type

                //Corridors and entrance already checked 
                if (roomNodeSO.roomNodeType.isEntrance || roomNodeSO.roomNodeType.isCorridorEW || roomNodeSO.roomNodeType.isCorridorNS ||
                    roomNodeSO.roomNodeType.isCorridor || roomNodeSO.roomNodeType.isNone)
                    continue;
                
                bool isRoomNodeTypeFound = false;

                //Loop through all room templates to check that this node type has been specified
                foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
                {
                    if (roomTemplateSO == null)
                        continue;
                    
                    if (roomTemplateSO.roomnodeType == roomNodeSO.roomNodeType)
                    {
                        isRoomNodeTypeFound = true;
                        break;
                    }
                }

                //No room tempalate found 
                if (!isRoomNodeTypeFound)
                    Debug.Log("In " + this.name.ToString() + " : No room tempalte " + roomNodeSO.roomNodeType.name.ToString() + " found for node graph "
                    + roomNodeGraph.name.ToString());
            }
        }
    }

#endif
    #endregion Validation
    
}
