using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bull : Enemy
{
    public float walkSpeed;
    public float walkTime;
    public float attackDistance;
    public float chargingSpeed;
    public float chargingTime;
    public float chargeDelay;
    public float chargeCooldown;
    public float moveDamping;
    private float origWalkTime;
    private bool walkingLeft = true;
    private bool startingCharge = false;
    private bool isCharging = false;
    private float origChargeCooldown;
    private float normalizedHorizontalSpeed;
    private GameObject player;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        stuckArrows = new List<GameObject>();
        origWalkTime = walkTime;
        origChargeCooldown = chargeCooldown;
        chargeCooldown = 0;
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

        // Perform charge attack if player is in range and if not already charging/on charge cooldown
        if (player != null && Vector3.Distance(player.transform.position, transform.position) <= attackDistance && !startingCharge && !isCharging && chargeCooldown <= 0)
        {
            StartCoroutine(Charge());
        }
        // Continue walking left if not charging
        else if (walkTime > 0 && walkingLeft && !startingCharge && !isCharging)
        {
            normalizedHorizontalSpeed = -1;
            if (transform.localScale.x < 0)
                FlipSprite();
            walkTime -= Time.deltaTime;
        }
        // Continue walking right if not charging
        else if (walkTime > 0 && !walkingLeft && !startingCharge && !isCharging)
        {
            normalizedHorizontalSpeed = 1;
            if (transform.localScale.x > 0)
                FlipSprite();
            walkTime -= Time.deltaTime;
        }
        // Switch walking direction periodically
        else if (!startingCharge && !isCharging)
        {
            walkingLeft = !walkingLeft;
            walkTime = origWalkTime;
        }
        // Stop moving
        else
        {
            normalizedHorizontalSpeed = 0;
        }

        // Decrement cooldown for charge attack
        chargeCooldown -= Time.deltaTime;
    }

    void Smoothing()
    {
        // Applies damping to all movement
        if (!isCharging) { velocity.x = Mathf.Lerp( velocity.x, normalizedHorizontalSpeed * walkSpeed, Time.deltaTime * moveDamping ); }
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

    IEnumerator Charge()
    {
        // Faces player and stops moving for a short time
        startingCharge = true;
        FacePlayer();
        yield return new WaitForSeconds(chargeDelay);

        // Charges towards player at a constant speed for a short time
        FacePlayer();
        isCharging = true;
        startingCharge = false;

        if (transform.localScale.x > 0)
            velocity.x = -chargingSpeed;
        else
            velocity.x = chargingSpeed;

        cameraShake.ShakeCamera(chargingTime, 0.2f);
        yield return new WaitForSeconds(chargingTime);

        // Stops charge and resets cooldown
        isCharging = false;
        chargeCooldown = origChargeCooldown;         
    }
}
