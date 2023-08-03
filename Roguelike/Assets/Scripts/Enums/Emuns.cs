
//This script keeps all enums for the whole project 
//Necessary to hold everything in a separate place??? - might want to move it to other scripts 

//IMPORTANT//
/*
If deleting or replacing a certain enum ->
    DO NOT CHANGE THE ORDER OR THE NUMBER OF ENUMS 
    IT MIGHT BRAKE THE GAME
*/


//This enum is used to define the direction of a corridor 
public enum Orientation
{
    north,
    east,
    south,
    west,
    none
}

//This enum holds all game states 
public enum GameState
{
    gameStarted,
    playingLevel,
    engagingEnemies,
    bossStage,
    engaigngBoss,
    levelCompleted,
    gameWon,
    gameLost,
    gamePaused,
    restartGame
}
