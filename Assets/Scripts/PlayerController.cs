using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform ArrowSprite;
    public GameObject arrowPrefab;
    public GameObject deathEffectPrefab;
    public GameManager gameManager;
    public CameraShake cameraShake;
    public float gravity;
    public float gravityMax;
    public float walkSpeed;
    public float runSpeed;
    public float dodgeSpeed;
    public float groundDamping;
	public float inAirDamping;
    public float jumpHeight;
    public float extraJumpHeight;
    public float jumpTimer;
    public float wallJumpTimer;
    public float dodgeDuration;
    public float dodgeTimer;
    public float shotSpeed;
    private float origJumpTimer;
    private float origWallJumpTimer;
    private float origDodgeDuration;
    private float origDodgeTimer;
    private float normalizedHorizontalSpeed = 0;
    private CharacterController2D controller;
    private Vector3 velocity;
    private bool isJumping = false;
    private bool isRunning = false;
    private bool isDropping = false;
    private bool canDodge = true;
    private bool isDodgingLeft = false;
    private bool isDodgingRight = false;
    private bool canWallJumpLeft = false;
    private bool canWallJumpRight = false;
    private bool canShoot = true;
    private bool isShooting = false;
    private float droppingTimer = 0.1f;
    private Vector3 arrowStartPosition;
    private Vector3 arrowEndPosition;
    private List<GameObject> shotArrows = new List<GameObject>();

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
        controller = GetComponent<CharacterController2D>();
        controller.onTriggerEnterEvent += onTriggerEnterEvent;

        // Spawn player and camera at checkpoint if available
        if (GlobalVariables.PlayerSpawn != Vector3.zero)
            transform.position = GlobalVariables.PlayerSpawn;
        cameraShake.gameObject.transform.position = transform.position + new Vector3(0, 0, -15);

        origJumpTimer = jumpTimer;
        origWallJumpTimer = wallJumpTimer;
        origDodgeDuration = dodgeDuration;
        origDodgeTimer = dodgeTimer;

        arrowStartPosition = ArrowSprite.localPosition;
        arrowEndPosition = ArrowSprite.localPosition - new Vector3(0.3f, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Shooting();
    }

    void Movement ()
    {
        Walking();
        Running();
        Jumping();
        Falling();
        WallJumping();
        Smoothing();
        Dodging();

        // Move character controller
        controller.move(velocity * Time.deltaTime);

        // Retrieve velocity from character controller
        velocity = controller.velocity;
    }

    void Walking ()
    {
        // Stop falling when on ground
        if (controller.isGrounded)
        {
            velocity.y = 0;
        }

        // Move to right
        if (Input.GetKey(KeyCode.RightArrow))
        {
            normalizedHorizontalSpeed = 1;

            // Flip sprite right while not shooting
            if (transform.localScale.x < 0f && !isShooting)
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );
        }
        // Move to left
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            normalizedHorizontalSpeed = -1;

            // Flip sprite left while not shooting
            if (transform.localScale.x > 0f && !isShooting)
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );
        }
        // Stand still
        else
        {
            normalizedHorizontalSpeed = 0;
        }
    }

    void Running ()
    {
        // Run when on ground and run button is held
        if (Input.GetKey(KeyCode.LeftShift) && controller.isGrounded)
            isRunning = true;
        // Continue running if player was running when they left the ground
        else if (isRunning && !controller.isGrounded)
            isRunning = true;
        // Stop running 
        else
            isRunning = false;

        // Stop running if players moves against direction of velocity in air
        if (!controller.isGrounded && Input.GetKeyDown(KeyCode.LeftArrow) && velocity.x > 0)
            isRunning = false;
        if (!controller.isGrounded && Input.GetKeyDown(KeyCode.RightArrow) && velocity.x < 0)
            isRunning = false;
    }

    void Dodging ()
    {
        // Dodge right
        if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.RightArrow) && controller.isGrounded && canDodge)
        {
            isDodgingRight = true;
            dodgeDuration = origDodgeDuration;
            canDodge = false;
            dodgeTimer = origDodgeTimer;
        }
        // Dodge left
        else if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftArrow) && controller.isGrounded && canDodge)
        {
            isDodgingLeft = true;
            dodgeDuration = origDodgeDuration;
            canDodge = false;
            dodgeTimer = origDodgeTimer;
        }

        // Keep constant velocity during dodge duration
        if (isDodgingLeft && dodgeDuration > 0)
        {
            velocity.x = -dodgeSpeed;
        }
        if (isDodgingRight && dodgeDuration > 0)
        {
            velocity.x = dodgeSpeed;
        }

        // Decrement dodge duration (length of dodge) and dodge timer (time until next dodge)
        dodgeDuration -= Time.deltaTime;
        dodgeTimer -= Time.deltaTime;
        
        // Stop dodge when duration hits 0
        if (dodgeDuration < 0)
        {
            isDodgingLeft = false;
            isDodgingRight = false;
            dodgeDuration = origDodgeDuration;
        }

        // Reenable dodge when timer hits 0s
        if (dodgeTimer < 0)
        {
            canDodge = true;
        }
    }

    void Smoothing()
    {
        // Smooth movement by damping amount
		float smoothedMovementFactor = controller.isGrounded ? groundDamping : inAirDamping;

        // Apply horizontal movement using run speed
        if (isRunning)
		    velocity.x = Mathf.Lerp( velocity.x, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor );
        // Apply horizontal movement using walk speed
        else
            velocity.x = Mathf.Lerp( velocity.x, normalizedHorizontalSpeed * walkSpeed, Time.deltaTime * smoothedMovementFactor );
    }

    void Falling()
    {
        // Apply gravity
        if (velocity.y > gravityMax) { velocity.y += gravity * Time.deltaTime; }

        // Drop through one way platform when down key is pressed
        if (controller.isGrounded && Input.GetKey(KeyCode.DownArrow))
		{
			velocity.y -= 2f;
			isDropping = true;
            controller.ignoreOneWayPlatformsThisFrame = true;
		}

        // Continue dropping until enough time has passed to fall all the way through
        if (isDropping && droppingTimer > 0)
        {
            controller.ignoreOneWayPlatformsThisFrame = true;
            droppingTimer -= Time.deltaTime;
        }
        else
        {
            isDropping = false;
            droppingTimer = 0.1f;
        }
    }

    void Jumping()
    {
        // Initial jump
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded || Input.GetKeyDown(KeyCode.UpArrow) && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            isJumping = true;
        }

        // Hold jump for extra height
        if (isJumping && (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow)) && jumpTimer > 0)
        {
            velocity.y += extraJumpHeight * Time.deltaTime;
            jumpTimer -= Time.deltaTime;
        }
        // Stop jump when button is let go/timer runs out
        else
        {
            isJumping = false;
            jumpTimer = origJumpTimer;
        }
    }

    void WallJumping()
    {
        // Walljump left to right //
        if (controller.isTouchingWallLeft)
        {
            // Checks if player is touching wall and gives them 0.2 sec buffer to walljump
            canWallJumpLeft = true;
            wallJumpTimer = origWallJumpTimer;
        }

        // Perform walljump away from left wall
        if (!controller.isGrounded && canWallJumpLeft && wallJumpTimer > 0 && Input.GetKey(KeyCode.RightArrow) && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)))
        {
            velocity.x += 3f;
            velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            isJumping = true;
            canWallJumpLeft = false;
            wallJumpTimer = origWallJumpTimer;
        }

        // Walljump right to left // 
        if (controller.isTouchingWallRight)
        {
            // Checks if player is touching wall and gives them time buffer to walljump
            canWallJumpRight = true;
            wallJumpTimer = origWallJumpTimer;
        }

        // Perform walljump away from right wall
        if (!controller.isGrounded && canWallJumpRight && wallJumpTimer > 0 && Input.GetKey(KeyCode.LeftArrow) && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)))
        {
            velocity.x -= 3f;
            velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            isJumping = true;
            canWallJumpRight = false;
            wallJumpTimer = origWallJumpTimer;
        }

        // Decrement walljump timer
        wallJumpTimer -= Time.deltaTime;

        // Disable walljump after timer hits 0 seconds
        if (wallJumpTimer <= 0)
        {
            canWallJumpLeft = false;
            canWallJumpRight = false;
            wallJumpTimer = origWallJumpTimer;
        }
    }

    void Shooting ()
    {
        // Starts shooting arrow when shoot button is pressed and player is not currently shooting
        if (Input.GetKeyDown(KeyCode.X) && canShoot) { StartCoroutine(ShootArrow()); }
    }

    IEnumerator ShootArrow()
    {
        // Prevents player from shooting during animation
        canShoot = false;
        isShooting = true;
        float t = 0f;

        // Pulls back arrow based on time of shot speed (to be replaced with premade animation)
        while (t < 1)
        {
            t += Time.deltaTime / shotSpeed;
            ArrowSprite.localPosition = Vector3.Lerp(arrowStartPosition, arrowEndPosition, t);
            yield return null;
        }

        // Resets arrow position and launches new arrow from player
        ArrowSprite.localPosition = arrowStartPosition;
        Instantiate(arrowPrefab, ArrowSprite.position, ArrowSprite.rotation);

        // Allows player to shoot again
        isShooting = false;
        canShoot = true;
    }

    public void AddArrow(GameObject arrow)
    {
        // Start removing arrows that have been shot when the amount exceeds 10, starting with the first arrow
        shotArrows.Add(arrow);
        if (shotArrows.Count > 10)
        {
            Destroy(shotArrows[0]);
            shotArrows.RemoveAt(0);
        }
    }

    void KillPlayer()
    {
        // Kills player and launches end screen
        Destroy(gameObject);
        Instantiate(deathEffectPrefab, transform.position, transform.rotation * Quaternion.Euler(-90, 0, 0));
        cameraShake.ShakeCamera(0.25f, 0.35f);
        gameManager.PlayerDied();
        gameManager.EndGame();
    }

    void WinGame()
    {
        // Displays end screen
        Debug.Log("You Win!!!");
        gameManager.EndGame();
    }

    void SetCheckpoint(Transform checkpoint)
    {
        // Stores location in static variable
        GlobalVariables.PlayerSpawn = checkpoint.position;
    }

    void onTriggerEnterEvent(Collider2D other)
    {
        // Kill player if player touches enemy or hazard
        if (other.CompareTag("Enemy") || other.CompareTag("Hazard"))
        {
            KillPlayer();
        }

        // Win game if player touches flag
        if (other.CompareTag("Flag"))
        {
            WinGame();
        }

        // Save new spawn location when player touches checkpoint
        if (other.CompareTag("Checkpoint"))
        {
            SetCheckpoint(other.gameObject.transform);
        }
    }
}
