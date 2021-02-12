using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gorilla : Enemy
{
    public float walkSpeed;
    public float walkTime;
    public float attackDistance;
    public float leapHeight;
    public float leapDelay;
    public float leapCooldown;
    public float moveDamping;
    private float origWalkTime;
    private bool walkingLeft = true;
    private bool startingLeap = false;
    private bool isLeaping = false;
    private bool inRange = false;
    private float origLeapCooldown;
    private float normalizedHorizontalSpeed;
    private GameObject player;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        stuckArrows = new List<GameObject>();
        origWalkTime = walkTime;
        origLeapCooldown = leapCooldown;
        leapCooldown = 0;
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Smoothing();
        
        // Move character controller
        controller.move(velocity * Time.deltaTime);

        // Retrieve velocity from character controller
        velocity = controller.velocity;
    }

    void Movement()
    {
        // Apply gravity
        if (velocity.y > gravityMax) { velocity.y += gravity * Time.deltaTime; }

        // Determines if player is in attack range
        if (player != null) { inRange = Mathf.Abs(transform.position.x - player.transform.position.x) <= attackDistance ? true : false; }
        else { inRange = false; }

        // Perform leap attack if player is in range and if not already leaping/on leap cooldown
        if (inRange && !startingLeap && !isLeaping && leapCooldown <= 0)
        {
            StartCoroutine(Leap());
        }
        // Follow player when in range
        else if (inRange && !startingLeap)
        {
            FacePlayer();
            if (transform.localScale.x > 0)
                normalizedHorizontalSpeed = -1;
            else
                normalizedHorizontalSpeed = 1;

            // Drop through platforms if player is below enemy
            if (player != null && transform.position.y - player.transform.position.y > 2)
                controller.ignoreOneWayPlatformsThisFrame = true;
        }
        // Continue walking left if player not in range
        else if (walkTime > 0 && walkingLeft && !startingLeap)
        {
            normalizedHorizontalSpeed = -1;
            if (transform.localScale.x < 0)
                FlipSprite();
            walkTime -= Time.deltaTime;
        }
        // Continue walking right if player not in range
        else if (walkTime > 0 && !walkingLeft && !startingLeap)
        {
            normalizedHorizontalSpeed = 1;
            if (transform.localScale.x > 0)
                FlipSprite();
            walkTime -= Time.deltaTime;
        }
        // Switch walking direction periodically
        else if (!startingLeap && !isLeaping)
        {
            walkingLeft = !walkingLeft;
            walkTime = origWalkTime;
        }
        // Stop moving
        else
        {
            normalizedHorizontalSpeed = 0;
        }

        // Identify when enemy is leaping
        if (velocity.y >= (leapHeight / 2))
        {
            startingLeap = false;
            isLeaping = true;
        }

        // Hits ground
        if (isLeaping && controller.isGrounded)
        {
            // Stops leap, resets cooldown, and shakes camera
            isLeaping = false;
            leapCooldown = origLeapCooldown; 
            cameraShake.ShakeCamera(0.2f, 0.3f);
        }

        // Decrement cooldown for leap attack
        leapCooldown -= Time.deltaTime;
    }

    void Smoothing()
    {
        // Applies damping to all movement
        velocity.x = Mathf.Lerp( velocity.x, normalizedHorizontalSpeed * walkSpeed, Time.deltaTime * moveDamping);
    }

    void FlipSprite()
    {
        // Inverts horizontal scale to flip sprite
        transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );
    }

    void FacePlayer()
    {
        // Flips sprite to face player depending on player position
        if (player != null && (player.transform.position.x - transform.position.x) < 0 && transform.localScale.x < 0)
            FlipSprite();
        else if (player != null && (player.transform.position.x - transform.position.x) > 0 && transform.localScale.x > 0)
            FlipSprite();
    }

    IEnumerator Leap()
    {
        // Faces player and stops moving for a short time
        startingLeap = true;
        FacePlayer();
        yield return new WaitForSeconds(leapDelay);

        // Leaps into the air
        velocity.y += leapHeight;    
    }
}
