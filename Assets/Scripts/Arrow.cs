using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float arrowSpeed;
    public float arrowLifetime;
    public GameObject player;
    public GameObject hitEffectPrefab;
    public CameraShake cameraShake;
    private bool directionIsRight;
    private bool hitObject = false;
    private RaycastHit2D hit;
    private GameObject objHit;
    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
        playerController = player.GetComponent<PlayerController>();

        // Determines which way the player is facing to shoot arrow in correct direction
        if (player.transform.localScale.x > 0f)
            directionIsRight = true;
        else
            directionIsRight = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Move arrow based on direction and speed until an object is hit
        if (directionIsRight && !hitObject)
            transform.Translate(Vector3.right * Time.deltaTime * arrowSpeed);
        else if (!hitObject)
            transform.Translate(Vector3.left * Time.deltaTime * arrowSpeed);

        // Check if the arrow has hit an object, if it hasn't already done so
        if (!hitObject)
        {
            DetectHit();
            arrowLifetime -= Time.deltaTime;
        }

        // Destroy arrow if it hasn't hit an object after specified time
        if (arrowLifetime <= 0 && !hitObject)  
            Destroy(gameObject);
    }

    void DetectHit()
    {
        // Cast ray based on direction arrow is moving
        if (directionIsRight)
            hit = Physics2D.Raycast(transform.position, Vector2.right);
        else
            hit = Physics2D.Raycast(transform.position, Vector2.left);        

        if (hit.collider != null)
            objHit = hit.transform.gameObject;

        // Check hit if the ray hits an object that is not the specified tags
        if (hit.collider != null && !objHit.CompareTag("Projectile") && !objHit.CompareTag("NoHitPlatform") && !objHit.CompareTag("Flag") && !objHit.CompareTag("Checkpoint") && !objHit.CompareTag("Hazard"))
        {
            // If distance between hit and arrow is less than 1 unit, confirm the hit, move the arrow in the object, and stop the arrow's movement
            float distance = Mathf.Abs(hit.point.x - transform.position.x);
            if (distance < 1)
            {
                transform.position = hit.point;
                hitObject = true;

                // Creates particle effect on hit, rotating to match arrow direction
                if (directionIsRight)
                    Instantiate(hitEffectPrefab, transform.position, (transform.rotation * Quaternion.Euler(0, -90, 0)));
                else
                    Instantiate(hitEffectPrefab, transform.position, (transform.rotation * Quaternion.Euler(0, 90, 0)));

                // If arrow hit an enemy, damage the enemy, stick the arrow, and kill the enemy if necessary
                if (objHit.CompareTag("Enemy"))
                {
                    transform.parent = objHit.transform;
                    gameObject.layer = 2;
                    Enemy enemy = objHit.GetComponent<Enemy>();
                    enemy.TakeHit();
                    enemy.ArrowStick(gameObject);
                    if (enemy.GetHealth() <= 0) { enemy.KillEnemy(); }
                    cameraShake.ShakeCamera(0.06f, 0.02f);
                }
                // Add arrow to player's shot arrows list
                else
                {
                    playerController.AddArrow(gameObject);
                    cameraShake.ShakeCamera(0.04f, 0.008f);
                }
            }
        }
    }
}
