using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings 
{
    #region LEVEL BUILD SETTINGS
    public const int maxLevelRebuildAttemptsForRoomGraph = 1000;
    public const int maxLevelBuildAttempts = 10;
    #endregion

    #region ROOM SETTINGS
    public const int maxChildCorridors = 3; // Max number of child corridors leading from a room (does not include the parent corridor)
    #endregion
}