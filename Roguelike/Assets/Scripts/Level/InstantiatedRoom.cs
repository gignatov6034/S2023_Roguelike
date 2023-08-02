using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decorationFloorTilemap;
    [HideInInspector] public Tilemap decorationTilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap mapTilemap;
    [HideInInspector] public Bounds roomColliderBounds;

    BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        //Save room collider bounds
        roomColliderBounds = boxCollider2D.bounds;
    }

    //Initialise the instantiated Room
    public void Initialise(GameObject roomGameObject)
    {
        PopulateTilemapMemberVariables(roomGameObject);

        BlockOffUnusedDoorways();

        DisableCollisionTilemapRenderer();
    }

    //Populate the tilemap and grid member variables
    void PopulateTilemapMemberVariables(GameObject roomGameObject)
    {
        //Get the grid component
        grid = roomGameObject.GetComponentInChildren<Grid>();

        //Get tilemaps in children
        Tilemap[] tilemaps = roomGameObject.GetComponentsInChildren<Tilemap>();

        foreach(Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.tag == "groundTilemap")  
            {
                groundTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decorationFloorTilemap")
            {
                decorationFloorTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decorationTilemap")
            {
                decorationTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "frontTilemap")
            {
                frontTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "collisionTilemap")
            {
                collisionTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "mapTilemap")
            {
                mapTilemap = tilemap;
            }
        }
    }

    //Block unused Doorways in the room
    void BlockOffUnusedDoorways()
    {
        //Loop through all doorways
        foreach(Doorway doorway in room.doorwayList)
        {
            if (doorway.isConnected)
                continue;
            
            //Block unconnected corridors (doorways) using tiles on tilemaps
            if (collisionTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(collisionTilemap, doorway);
            }
            
            if (mapTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(mapTilemap, doorway);
            }

            if (groundTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(groundTilemap, doorway);
            }

            if (decorationTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(decorationTilemap, doorway);
            }

            if (decorationFloorTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(decorationFloorTilemap, doorway);
            }

            if (frontTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(frontTilemap, doorway);
            }
        }
    }

    //Block a doorway on a tilemap layer
    void BlockADoorwayOnTilemapLayer(Tilemap tilemap, Doorway doorway)
    {
        switch (doorway.orientation)
        {
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayHorizontally(tilemap, doorway);
                break;

            case Orientation.east:
            case Orientation.west:
                BlockDoorwayVertically(tilemap, doorway);
                break;

            case Orientation.none:
                break;
            default:
                break;
        }
    }

    //Block doorway horizontally - north and south corridors
    void BlockDoorwayHorizontally(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorWayStartCopyPosition;

        //Loop through all tiles to copy - x axis
        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            //y axis
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                //Get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                //Copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                //Set rotation of the copied tile
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix);
            }
        }
    }

    //Block doorway vertically - west and east corridors
    void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorWayStartCopyPosition;

        //Loop through all tiles to copy - y axis
        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
        {
            //x axis
            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
            {
                //Get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                //Copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                //Set rotation of the copied tile
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);
            }
        }
    }

    //Disable collision tilemap renderer
    void DisableCollisionTilemapRenderer()
    {
        //Disable collision tilemap renderer
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }
}
