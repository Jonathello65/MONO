using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Text timerText;
    public Text scoreText;
    public Text highScoreText;
    public Text bestTimeText;
    public GameObject deathText;
    public GameObject winText;
    public GameObject endGameText;
    private int totalScore;

    private float startTime;
    private float deathTime;
    private bool endGame = false;
    private bool playerIsDead = false;

    // Start is called before the first frame update
    void Start()
    {
        // Lock fps to 60 or physics become wonky
        Application.targetFrameRate = 60;

        // Hide cursor
        Cursor.visible = false;

        // Keep time if restarting from a checkpoint
        if (GlobalVariables.Time != 0)
            startTime = Time.time - GlobalVariables.Time;
        else
            startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (!endGame)
        {
            // Updates timer while the game is still running
            UpdateTimer();
        }
        else
        {
            // Allows player to restart when game is over
            RestartGame();
        }

        ExitGame();
    }

    void RestartGame()
    {
        // Reloads level upon pressing R
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Keeps time if loading a checkpoint
            if (GlobalVariables.PlayerSpawn != Vector3.zero)
                GlobalVariables.Time = deathTime;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void ExitGame()
    {
        // Exits game upon pressing ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ResetCheckpoint();
            SceneManager.LoadScene("Main Menu");
        }
    }

    public void UpdateScore(int score)
    {
        // Adds to score counter and updates text
        totalScore += score;
        scoreText.text = totalScore.ToString();
    }

    void UpdateTimer()
    {
        // Sets timer text
        timerText.text = FormatTime(GetTime());
    }

    float GetTime()
    {
        // Gets time since start
        return Time.time - startTime;
    }

    string FormatTime(float time)
    {
        // formats time as minutes:seconds
        string minutes = ((int) time / 60).ToString();
        string seconds = (time % 60).ToString("00");

        return minutes + ":" + seconds;
    }

    public void EndGame()
    {
        endGame = true;

        // Displays end game text based on if player died or not
        endGameText.SetActive(true);
        if (playerIsDead)
        {
            deathText.SetActive(true);
            winText.SetActive(false);
            deathTime = GetTime();
        }
        else
        {
            winText.SetActive(true);
            UpdateHighScore();
            UpdateBestTime();
            ResetCheckpoint();

            highScoreText.gameObject.SetActive(true);
            bestTimeText.gameObject.SetActive(true);
        }
    }

    void UpdateHighScore()
    {
        // Updates static high score value if new score is higher, then sets highscore text
        if (totalScore > GlobalVariables.HighScore)
            GlobalVariables.HighScore = totalScore;

        highScoreText.text = "HIGHSCORE: " + GlobalVariables.HighScore.ToString();
    }

    void UpdateBestTime()
    {
        // Updates static time value if new time is faster, then sets best time text
        float time = GetTime();
        if (time < GlobalVariables.BestTime || GlobalVariables.BestTime == 0)
            GlobalVariables.BestTime = time;

        bestTimeText.text = "BEST TIME: " + FormatTime(GlobalVariables.BestTime);
    }

    void ResetCheckpoint()
    {
        // Resets player spawn and in-game time
        GlobalVariables.PlayerSpawn = Vector3.zero;
        GlobalVariables.Time = 0;
    }

    public void PlayerDied()
    {
        playerIsDead = true;
    }
}
