using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hitPoints;
    public int score;
    public float gravity;
    public float gravityMax;
    public GameObject deathEffectPrefab;
    public GameManager gameManager;
    public CameraShake cameraShake;
    protected List<GameObject> stuckArrows;
    protected CharacterController2D controller;
    protected Vector3 velocity;

    protected virtual void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
        controller = GetComponent<CharacterController2D>();
        controller.onTriggerEnterEvent += onTriggerEnterEvent;
    }

    public int GetHealth()
    {
        return hitPoints;
    }

    public void SetHealth(int health)
    {
        hitPoints = health;
    }

    public int GetScore()
    {
        return score;
    }
    
    public void TakeHit()
    {
        hitPoints--;
    }

    public void ArrowStick(GameObject arrow)
    {
        stuckArrows.Add(arrow);
    }

    public void KillEnemy()
    {
        // Destroys enemy and all arrows stuck in enemy when enemy dies
        Destroy(gameObject);
        foreach (GameObject arrow in stuckArrows)
            Destroy(arrow);
        
        Instantiate(deathEffectPrefab, transform.position, transform.rotation * Quaternion.Euler(-90, 0, 0));
        gameManager.UpdateScore(score);

    }

    void onTriggerEnterEvent(Collider2D other)
    {
        if (other.CompareTag("Hazard"))
        {
            KillEnemy();
        }
    }
}
