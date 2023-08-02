using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent] //Prevents adding the same component more than once 
public class GameManager : SingletonMonobehaviour<GameManager>
{
    #region Header GAME LEVELS
    [Space (10)]
    [Header("GAME LEVELS")]
    #endregion Header GAME LEVELS

    #region Tooltip
    
    [Tooltip("Populate with the level SO")]
    #endregion Tooltip

    [SerializeField] List<LevelSO> levelList;

    #region Tooltip
    
    [Tooltip("Populate with the starting game level for testing, first level = 0")]
    #endregion Tooltip

    [SerializeField] int currentGameLevelIndex = 0;

    //Holds the game current game state
    [HideInInspector] public GameState gameState;

    // Start is called before the first frame update
    void Start()
    {
        gameState = GameState.gameStarted;
    }

    // Update is called once per frame
    void Update()
    {
        HandleGameState();

        //For testing purposes
        //Regenerates the level when pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            gameState = GameState.gameStarted;
        }
    }

    //Manages all game states using switch case statements
    void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:

                //Play the first level 
                PlayLevel(currentGameLevelIndex);
                gameState = GameState.playingLevel;
                break;
            default:
                break;
        }
    }

    void PlayLevel(int levelListIndex)
    {
        //Build level 
        bool levelBuiltSucessfully = LevelBuilder.Instance.GenerateLevel(levelList[levelListIndex]);

        if(!levelBuiltSucessfully)
        {
            Debug.LogError("Couldn't build the level from specified rooms and node graphs");
        }
    }

    //Validation that the level has been popualted 
    #region Validation 
#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(levelList), levelList);
    }

#endif
    #endregion Validation
}
