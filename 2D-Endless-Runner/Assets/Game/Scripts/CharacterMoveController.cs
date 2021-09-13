using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SocialPlatforms.Impl;

public class CharacterMoveController : MonoBehaviour
{
    #region Declare

    [Header("GameOver")]
    public GameObject gameOverScreen;
    public float fallPositionY;

    [Header("Camera")]
    public CameraMoveController gameCamera;

    [Header("Scoring")]
    public ScoreController score;
    public float scoringRatio;
    private float lastPositionX;

    [Header("Movements")]
    public float moveAccel;
    public float maxSpeed;

    [Header("Jump")]
    public float jumpAccel;


    [Header("Ground Raycast")]
    public float groundRaycastDistance;
    public LayerMask groundLayerMask;

    private bool isJumping;
    private bool isOnGround;

    private Rigidbody2D rig;

    private Animator anim;

    private CharacterSoundController sound;

    #endregion

    #region Start

    private void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sound = GetComponent<CharacterSoundController>();
    }

    #endregion

    #region Update

    private void Update()
    {
        //read input
        if(Input.GetMouseButtonDown(0))
        {
            if(isOnGround)
            {
                isJumping = true;

                sound.PlayJump();
            }
        }
        anim.SetBool("isOnGround", isOnGround);

        int distancePassed = Mathf.FloorToInt(transform.position.x - lastPositionX);
        int scoreIncrement = Mathf.FloorToInt(distancePassed / scoringRatio);

        if(scoreIncrement > 0)
        {
            score.IncreaseCurrentScore(scoreIncrement);
            lastPositionX += distancePassed;
        }

        if(transform.position.y < fallPositionY)
        {
            GameOver();
        }
    }

    #endregion

    #region Game Over

    private void GameOver()
    {
        score.FinishScoring();

        gameCamera.enabled = false;

        gameOverScreen.SetActive(true);

        enabled = false;
    }

    #endregion

    #region FixedUpdate

    private void FixedUpdate()
    {
        // raycast ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayerMask);
        if (hit)
        {
            if (!isOnGround && rig.velocity.y <= 0)
            {
                isOnGround = true;
            }
        }
        else
        {
            isOnGround = false;
        }

        // calculate velocity vector
        Vector2 velocityVector = rig.velocity;

        if (isJumping)
        {
            velocityVector.y += jumpAccel;
            isJumping = false;
        }

        // clamp agar menjepit value x agar tidak melebih min atau max
        velocityVector.x = Mathf.Clamp(velocityVector.x + moveAccel * Time.deltaTime, 0.0f, maxSpeed);

        rig.velocity = velocityVector;
    }

    #endregion

    #region OnDrawGizmos
    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + (Vector3.down * groundRaycastDistance), Color.white);
    }
    #endregion
}
