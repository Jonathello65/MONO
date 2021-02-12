using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariables
{
    // Static variables to be accessed and saved between scenes
    private static int highScore = 0;
    private static float bestTime = 0;
    private static float time = 0;
    private static Vector3 playerSpawn = Vector3.zero;

    // Getters and setters
    public static int HighScore
    {
        get
        {
            return highScore;
        }
        set
        {
            highScore = value;
        }
    }

    public static float BestTime
    {
        get
        {
            return bestTime;
        }
        set
        {
            bestTime = value;
        }
    }

    public static float Time
    {
        get
        {
            return time;
        }
        set
        {
            time = value;
        }
    }

    public static Vector3 PlayerSpawn
    {
        get
        {
            return playerSpawn;
        }
        set
        {
            playerSpawn = value;
        }
    }
}
