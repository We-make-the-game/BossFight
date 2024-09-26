using System.Collections;
using UnityEngine;

// If you still have weird movement in edge collision, we recommand you to change the "Pos" in ground detection to "0". fromthe inspector

namespace VEOController
{
    [RequireComponent(typeof(Rigidbody2D))]
    // [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(CallbacksHandler))]
    [RequireComponent(typeof(InputsHandler))]

    public class Controller : MonoBehaviour
    {
        #region Calls
        public Rigidbody2D rb;
        public CapsuleCollider2D col;
        public PlayerCallbacks callbacks;
        public StatesHandler state;
        public Detection detection;
        public Physics physics;
        public Movement movement;
        public Rotation rotation;
        public Attack attack;
        public Jump jump;
        public Dash dash;
        public WallGrab grab;
        public PlayerInputs inputs;
        public PlayerEffects effects;
        public PlayerAnimations anim;
        public Combat combat;
        public CombatFunctions combatFunctions;
        #endregion

        #region Enablers
        // bools
        [HideInInspector] public bool enablePhysics = true;
        [HideInInspector] public bool enableDetection = true;
        [HideInInspector] public bool enableMovement = true;
        [HideInInspector] public bool enableRotation = true;
        [HideInInspector] public bool enableJump = true;
        [HideInInspector] public bool enableDash = true;
        [HideInInspector] public bool enableGrab = true;
        [HideInInspector] public bool enableAnimations = true;
        [HideInInspector] public bool enableCombat = true;
        #endregion

        #region States
        // States
        [HideInInspector] public bool isMoving = false;
        [HideInInspector] public bool isWalking = false;
        [HideInInspector] public bool isRunning = false;
        [HideInInspector] public bool isFalling = false;
        [HideInInspector] public bool isJumping = false;
        [HideInInspector] public bool isWallJumping = false;
        [HideInInspector] public bool isClimbingDown = false;
        [HideInInspector] public bool isClimbingUp = false;
        [HideInInspector] public bool isGrabing = false;
        [HideInInspector] public bool isHanging = false;
        [HideInInspector] public bool isSliding = false;
        [HideInInspector] public bool isDashing = false;
        [HideInInspector] public bool isAttacking = false;
        [HideInInspector] public bool isSlopSliding = false;
        [HideInInspector] private bool isFacingRight => rotation.skin.localEulerAngles == Vector3.zero;

        public bool isPushingAgainstWall
        {
            get
            {
                return touchingRight && inputs.MoveInputs.x == 1 || touchingLeft && inputs.MoveInputs.x == -1;
            }
        }
        public bool isFacingWall
        {
            get
            {
                return (touchingRight && isFacingRight) || (touchingLeft && !isFacingRight);
            }
        }
        public bool isPushingAwayWall
        {
            get
            {
                return (touchingRight && inputs.MoveInputs.x == -1) || (touchingLeft && inputs.MoveInputs.x == 1);
            }
        }
        #endregion

        #region Monobehaviour
        // MOBOBEHAVIOURS
        private void OnValidate()
        {
            if (col == null)
                col = GetComponent<CapsuleCollider2D>();

            anim?.PopulateClips();
            anim.controller = this;
        }
        private void Awake()
        {
            if (col == null)
                col = GetComponent<CapsuleCollider2D>();

            anim?.PopulateClips();
            anim.controller = this;
            combatFunctions = GetComponent<CombatFunctions>();

            GetCalls();
            SetRigidbody();
            SetCallbacks();
        }
        private void Update()
        {
            inputs?.UpdateInputs();
            state.TickState();
            anim.UpdateAnimations();

            HandleJumping();
            GravityModifiers();

            HandleSpeed();
            HandleRotation();
        }
        private void FixedUpdate()
        {
            HandleGroundDetection();
            HandleWallDetection();

            HandleGravity();
            HandleMovement();

            CheckGrabing();
            HandleLedgeGrab();
        }
        private void LateUpdate()
        {
            HandleStates();
        }
        #endregion

        #region General
        // General Functions
        private void SetRigidbody()
        {
            rb = GetComponent<Rigidbody2D>();


            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody2D>();


            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.drag = 0;
        }
        private void HandleStates()
        {
            bool _wasMoving = isGroundMoving;

            isMoving = currentSpeed > 2f && !isSlopSliding && inputs.MoveInputs.x != 0;
            isGroundMoving = isMoving && isGrounded;

            if (isGroundMoving && !_wasMoving)
                callbacks?.movement.OnWalkStart?.Invoke();

            if (!isGroundMoving && _wasMoving)
                callbacks?.movement.OnWalkStop?.Invoke();

            isFalling = rb.velocity.y <= 0 && !isGrounded;

            if (!isGrounded && isFalling)
                isJumping = false;

            isSlopSliding = rb.velocity.y < 0 && onSteepSlop;
        }
        private void GetCalls()
        {
            effects.audioSource = GetComponent<AudioSource>();
            state = new StatesHandler(this);
        }
        private void SetCallbacks()
        {
            callbacks.collision.OnGroundHit.AddListener(OnGroundHit);
            callbacks.collision.OnWallHit.AddListener(OnWallHit);
            callbacks.dash.OnDashStart.AddListener(OnDashStart);
            callbacks.dash.OnDashEnd.AddListener(OnDashEnd);
        }
        #endregion

        #region Detection
        // Public Variables
        [HideInInspector] public bool isGrounded;
        [HideInInspector] public bool isTouchingWall;
        [HideInInspector] public bool onWalkableSlop;
        [HideInInspector] public bool onSteepSlop;
        [HideInInspector] public bool touchingRight;
        [HideInInspector] public bool touchingLeft;
        [HideInInspector] public float surfaceAngle;
        [HideInInspector] public Vector2 feetPosition;
        [HideInInspector] public Vector2 wallHitPoint;
        [HideInInspector] public Vector2 surfaceNormal;

        // Private Variables
        private readonly RaycastHit2D[] rayHits = new RaycastHit2D[2];
        private bool isTouchingLedge = false;
        private bool canClimbLedge = false;
        private bool isFacingSteepSlop = false;
        private Vector2 detectionBoxSize;

        // Functions
        private void HandleGroundDetection()
        {
            if (!enableDetection) { return; }

            bool _wasGrounded = isGrounded;

            feetPosition = new Vector2(col.bounds.center.x, col.bounds.min.y + detection.pos);
            detectionBoxSize = new Vector3(col.bounds.size.x * detection.width, detection.height, 1);

            isGrounded = !isClimbing && Physics2D.BoxCastNonAlloc(feetPosition, detectionBoxSize, 0f, Vector2.zero, rayHits, 0f, detection.groundLayer) > 0;

            Vector2 facedSlopDetectorPos = new Vector2(feetPosition.x, feetPosition.y + 0.1f);
            Vector2 facedSlopDetectorDirection = new Vector2(isFacingRight ? 1 : -1, 0);

            RaycastHit2D facedRayHit = Physics2D.Raycast(facedSlopDetectorPos, facedSlopDetectorDirection, 0.3f, detection.groundLayer);
            RaycastHit2D surfaceRayHit = Physics2D.Raycast(this.feetPosition, new Vector2(0, -1), 1f, detection.groundLayer);

            bool wasFacingHighSlop = isFacingSteepSlop;

            float sideSlopAngle = Mathf.Abs(Vector2.Angle(facedRayHit.normal, Vector2.up));
            surfaceAngle = Mathf.Abs(Vector2.Angle(surfaceRayHit.normal, Vector2.up));

            bool _onSlop = surfaceAngle > 0 && surfaceAngle < 80 && isGrounded;

            surfaceNormal = surfaceRayHit.normal;
            onSteepSlop = _onSlop && surfaceAngle > movement.maxWalkableSlop;
            onWalkableSlop = _onSlop && surfaceAngle <= movement.maxWalkableSlop;
            isFacingSteepSlop = sideSlopAngle > movement.maxWalkableSlop && isGrounded && !onSteepSlop;

            if (isFacingSteepSlop && !wasFacingHighSlop && !isJumping)
            {
                rb.velocity = Vector2.zero;
            }

            if (isGrounded)
            {
                isSuperJumping = false;
                isJumping = false;
                if (!_wasGrounded) { callbacks.collision.OnGroundHit?.Invoke(); }
            }
        }
        private void HandleWallDetection()
        {
            if (!enableDetection) { return; }

            bool _WasTouchingRight = touchingRight;
            bool _WasTouchingLeft = touchingLeft;

            bool touchingLeftLedge;
            bool touchingRightLedge;

            Vector2 rightDetector = new Vector2(col.bounds.max.x + detection.xPos, col.bounds.center.y + detection.yPos);
            Vector2 leftDetector = new Vector2(col.bounds.min.x - detection.xPos, col.bounds.center.y + detection.yPos);
            Vector2 ledgeDetectorLeft = new Vector2(col.bounds.min.x - detection.xPos, col.bounds.max.y);
            Vector2 ledgeDetectorRight = new Vector2(col.bounds.max.x + detection.xPos, col.bounds.max.y);

            touchingRight = Physics2D.CircleCastNonAlloc(rightDetector, detection.radius, Vector2.zero, rayHits, 0, detection.wallLayer) > 0;
            touchingLeft = Physics2D.CircleCastNonAlloc(leftDetector, detection.radius, Vector2.zero, rayHits, 0, detection.wallLayer) > 0;

            touchingRightLedge = Physics2D.CircleCastNonAlloc(ledgeDetectorRight, detection.radius, Vector2.zero, rayHits, 0, detection.wallLayer) > 0;
            touchingLeftLedge = Physics2D.CircleCastNonAlloc(ledgeDetectorLeft, detection.radius, Vector2.zero, rayHits, 0, detection.wallLayer) > 0;

            isTouchingLedge = (touchingLeftLedge || touchingRightLedge);
            isTouchingWall = (touchingRight || touchingLeft);

            if (isTouchingWall && !isTouchingLedge && !canClimbLedge && isClimbingUp)
            {
                canClimbLedge = true;
            }


            // On Contact
            if (!_WasTouchingRight && touchingRight)
            {
                wallHitPoint = rightDetector;
                TurnRight();
                callbacks?.collision.OnWallHit?.Invoke();
            }
            if (!_WasTouchingLeft && touchingLeft)
            {
                wallHitPoint = leftDetector;
                TurnLeft();
                callbacks?.collision.OnWallHit?.Invoke();
            }
        }
        #endregion

        #region Physics
        // Public Physics
        [HideInInspector] public float Gravity => physics.gravityScale * Physics2D.gravity.y * -2;
        [HideInInspector] public bool ignoreGravity = false;
        [HideInInspector] public float gravityBuffer = 1;

        // Physic Functions
        public void HandleGravity()
        {
            if (!enablePhysics) { return; }

            bool _fallingFast = rb.velocity.y < -physics.maxFallVelocity;
            float _fallBuffer = rb.velocity.y < 0 ? physics.fallSpeedMultiplier : 1;

            if (!_fallingFast && !ignoreGravity && !isGrabing)
            {
                rb.velocity -= Gravity * (gravityBuffer * _fallBuffer) * Time.deltaTime * Vector2.up;
            }
        }
        #endregion

        #region Movement
        // Public Variables
        private bool isGroundMoving = false;
        [HideInInspector] private bool lockMovement = false;
        [HideInInspector] public float xVel => rb.velocity.x;
        [HideInInspector] public float yVel => rb.velocity.y;
        [HideInInspector] public float currentSpeed => Mathf.Abs(rb.velocity.x);

        // PrivaeVariables
        private bool sprintHold = false;
        private float refMovement;
        private float moveSpeed;
        private bool isSprintActive;

        // Inputs
        public void SprintPressed()
        {
            sprintHold = true;

            if (isGrounded)
                isSprintActive = true;
        }
        public void SprintReleased()
        {
            sprintHold = false;

            if (isGrounded)
                isSprintActive = false;
        }

        // Methods
        private void HandleSpeed()
        {
            bool _sprint = false;

            if (movement.canSprint)
            {
                if (movement.alwaysSprint)
                    _sprint = true;
                else
                    _sprint = isSprintActive;
            }

            bool _groundMoving = currentSpeed > 1f && isGrounded && !onSteepSlop;

            isRunning = _groundMoving && _sprint;
            isWalking = _groundMoving && !_sprint;

            if (!enableMovement) { return; }

            float _targetSpeed = (_sprint ? movement.sprintSpeed : movement.walkSpeed) * inputs.MoveInputs.x;
            float _smoothness = _targetSpeed == 0f ? movement.decceleration : movement.acceleration;

            if (!isGrounded) _targetSpeed *= movement.airSpeedModifier;

            if (!isFacingSteepSlop)
            {
                moveSpeed = Mathf.SmoothDamp(moveSpeed, _targetSpeed, ref refMovement, AirControlModifier(_smoothness));
            }
            else
            {
                _targetSpeed = 0;
                moveSpeed = 0;
            }

            SetAnimationSpeed(_smoothness);

            void SetAnimationSpeed(float smoothness)
            {
                if (isMoving)
                    anim.SetSpeed("RunSpeed", currentSpeed * 0.1f * movement.animationSpeedBuffer, 1 - smoothness, Time.deltaTime);
            }

            float AirControlModifier(float smoothness)
            {
                if (isGrounded)
                    return 1 - smoothness;
                else
                {
                    if (rb.velocity.y > 0) return 1 - smoothness * movement.upControl;
                    else return 1 - smoothness * movement.fallControl;
                }
            }

        }
        private void HandleMovement()
        {
            if (!enableMovement) { return; }

            if (lockMovement || (isGrabing && jump.enableWallJump) || isSuperJumping || isFacingSteepSlop) { return; }

            float speed = moveSpeed * 50 * Time.fixedDeltaTime;

            if (isWallJumping)
            {
                WallJumpMovement(speed);
                return;
            }
            if (onSteepSlop && !(isJumping || isSuperJumping))
            {
                SteepSlopMovement(movement.slopSlidingSpeed * 50 * Time.fixedDeltaTime * -surfaceNormal.x);
                return;
            }
            if (onWalkableSlop && !(isJumping || isSuperJumping))
            {
                SlopMovement(speed);
                return;
            }

            GroundMovement(speed);

        }
        private void WallJumpMovement(float speed)
        {
            Vector2 targetVelocity = new Vector2(speed, rb.velocity.y);
            rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, jump.wallJump.Control * Time.deltaTime);

            if (inputs.MoveInputs.x != 0)
            {

            }
        }
        private void SlopMovement(float speed)
        {

            if (isFacingWall)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            Vector2 slopPerp = Vector2.Perpendicular(surfaceNormal).normalized;
            rb.velocity = new Vector2(-speed * slopPerp.x, -speed * slopPerp.y);
        }
        private void SteepSlopMovement(float speed)
        {
            Vector2 slopPerp = Vector2.Perpendicular(surfaceNormal).normalized;
            rb.velocity = new Vector2(speed * slopPerp.x, speed * slopPerp.y);
        }
        private void GroundMovement(float speed)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
        }

        // Public Functions
        public void UnlockMovement()
        {
            lockMovement = false;
            rb.velocity = Vector2.zero;
        }
        public void LockMovement()
        {
            rb.velocity = Vector2.zero;
            lockMovement = true;
        }

        #endregion

        #region Rotation
        // Private Variables
        private float rotationDegree = 0;
        private bool canRotate = true;

        // Rotation Functions
        private void HandleRotation()
        {
            if (!canRotate || isGrabing || !enableRotation || isSliding) { return; }

            if (rotation.rotateToMoveDirection)
                RotateToMoveDirection();

            if (rotation.rotateOnMouseClick)
                RotateToMouseClick();

        }
        private void RotateToMoveDirection()
        {
            if (rotation.smoothRotation)
                SmoothRotation();
            else
                SnappyRotation();
        }
        private void RotateToMouseClick()
        {
            if (!Input.GetKeyDown(KeyCode.Mouse0)) { return; }

            if (inputs.MoveInputs.x != 0 || isGrabing) { return; }

            float _dir = inputs.MousePosition.x - transform.position.x;

            if (_dir < 0 && isFacingRight || _dir > 0 && !isFacingRight)
                FlipSprite();
        }
        private void SmoothRotation()
        {
            if (Mathf.Abs(xVel) > 0.1f)
                rotationDegree = xVel > 0 ? 0 : 180;

            float targetDegree = Mathf.MoveTowards(rotation.skin.eulerAngles.y, rotationDegree, 1000 * rotation.speed * Time.deltaTime);
            rotation.skin.eulerAngles = new Vector2(rotation.skin.eulerAngles.x, targetDegree);

            if (isTouchingWall)
                SnappyRotation();
        }
        private void SnappyRotation()
        {

            if (inputs.MoveInputs.x == 0) { return; }
            else if (inputs.MoveInputs.x > 0 && isFacingRight) { return; }
            else if (inputs.MoveInputs.x < 0 && !isFacingRight) { return; }

            FlipSprite();
        }
        private void FlipSprite()
        {
            rotation.skin.localEulerAngles = isFacingRight ? new Vector3(0, 180, 0) : new Vector3(0, 0, 0);
        }
        private void TurnLeft()
        {
            rotation.skin.localEulerAngles = new Vector3(0, 180, 0);
        }
        private void TurnRight()
        {
            rotation.skin.localEulerAngles = new Vector3(0, 0, 0);
        }
        #endregion

        #region Attack & HEAL
        private bool attackReady = true;
        private bool heavyAttackPressed = false;

        private void ReadyAttack()
        {
            isAttacking = false;
            attackReady =  true;
        }
        private IEnumerator Attacking()
        {
            if (isTouchingWall)
            {
                // On-wall attack
                attackReady = false;
                FlipSprite();
                callbacks.attack.OnWallAttack?.Invoke();
            }
            else if (isGrounded)
            {
                // Normal attack
                attackReady = false;
                callbacks.attack.OnAttack?.Invoke();
            }
            else
            {
                // Air attack
                attackReady = false;
                callbacks.attack.OnAirAttack?.Invoke();
            }

            // Cooldown
            yield return new WaitForSeconds(attack.attackCooldown);
            ReadyAttack();
        }

        public void AttackPressed()
        {
            if (!attackReady) return;
            StartCoroutine(Attacking());
            combatFunctions.TakeDamage(10);
        }

        private IEnumerator HeavyAttacking()
        {
            attackReady = false;
            callbacks.attack.OnHeavyAttackEnd?.Invoke();

            // Cooldown
            yield return new WaitForSeconds(attack.attackCooldown);
            ReadyAttack();
        }

        public void HeavyAttackPressed()
        {
            if (!attackReady) return;
            // Heavy attack
            attackReady = false;
            heavyAttackPressed = true;
            callbacks.attack.OnHeavyAttackStart?.Invoke();
        }

        public void HeavyAttackReleased()
        {
            if (!heavyAttackPressed) return;
            heavyAttackPressed = false;
            StartCoroutine(HeavyAttacking());
        }
        #endregion

        #region Jump
        // Variables
        private bool jumpHold = false;
        private bool isJumpPressed = false;
        private bool isSuperJumping = false;
        private float leftGroundTime = 100f;
        private float leftWallTime = 0f;
        private float pressedThreshold = 100f;
        private float wallJumpPressedThreshold = 100f;
        private float attackJumpPressedThreshold = 100f;
        private int currentAirJumps = 0;

        // Inputs
        public void JumpPressed()
        {
            if (enableJump)
            {
                if (onSteepSlop && !jump.groundJump.CanJumpOnSteepSlops) { return; }

                jumpHold = true;
                isJumpPressed = true;
            }
        }
        public void JumpReleased()
        {
            jumpHold = false;
        }

        // Methods
        private void TryJump()
        {
            if (isTouchingWall && jump.enableWallJump && (!isGrounded))
            {
                TryWallJump();
                return;
            }
            if (isDashing && jump.enableDashJump)
            {
                if (!isGrounded)
                {
                    if (currentAirJumps >= jump.airJump.MaxAirJumps || !jump.enableAirJump) { return; }

                    currentAirJumps += 1;
                }

                ExecuteDashJump();
                return;
            }
            if (leftGroundTime <= jump.groundJump.CoyoteTime && !isJumping)
            {
                TryGroundJump();
                return;
            }
            else if (jump.enableAirJump && !inGroundBuffer)
            {
                TryAirJump();
                return;
            }
        }
        private bool inGroundBuffer;
        private void HandleJumping()
        {
            leftGroundTime = isGrounded ? 0 : leftGroundTime + Time.deltaTime;
            leftWallTime = isTouchingWall ? 0 : leftWallTime + Time.deltaTime;
            pressedThreshold += Time.deltaTime;
            wallJumpPressedThreshold += Time.deltaTime;
            attackJumpPressedThreshold += Time.deltaTime;
            inGroundBuffer = Physics2D.Raycast(feetPosition, Vector2.down, jump.groundJump.GroundBuffering, detection.groundLayer);

            if (isJumpPressed && isAttacking)
            {
                isJumpPressed = false;
                attackJumpPressedThreshold = 0f;
                return;
            }

            if (!isAttacking)
            {
                if (attackJumpPressedThreshold < jump.attackJump.InputBuffering)
                {
                    TryJump();
                    pressedThreshold = 100f;
                    wallJumpPressedThreshold = 100f;
                    attackJumpPressedThreshold = 100f;
                    return;
                }
            }

            if (isGrounded)
            {
                if (pressedThreshold < jump.groundJump.InputBuffering)
                {
                    if (onSteepSlop && !jump.groundJump.CanJumpOnSteepSlops) { return; }

                    TryGroundJump();
                    pressedThreshold = 100f;
                    wallJumpPressedThreshold = 100f;
                    attackJumpPressedThreshold = 100f;
                    return;
                }
            }

            if (isTouchingWall && jump.enableWallJump)
            {
                if (wallJumpPressedThreshold < jump.wallJump.InputBuffering)
                {
                    TryWallJump();
                    pressedThreshold = 100f;
                    wallJumpPressedThreshold = 100f;
                    attackJumpPressedThreshold = 100f;
                    return;
                }
            }


            if (isJumpPressed)
            {
                isJumpPressed = false;

                if (isFalling)
                    pressedThreshold = 0;

                if (!isGrounded || isWallJumping)
                    wallJumpPressedThreshold = 0;

                TryJump();
            }
        }

        private void TryGroundJump()
        {
            isJumping = true;
            isSuperJumping = false;
            leftGroundTime = 100f;
            pressedThreshold = 100f;
            wallJumpPressedThreshold = 100f;
            attackJumpPressedThreshold = 100f;

            callbacks.jump.OnGroundJump?.Invoke();
            EndWallJump();
            ExecuteNormalJump(jump.groundJump.Force);
        }
        private void TryAirJump()
        {
            if ((currentAirJumps >= jump.airJump.MaxAirJumps)) { return; }

            pressedThreshold = 100f;
            wallJumpPressedThreshold = 100f;
            attackJumpPressedThreshold = 100f;
            isJumping = true;
            isSuperJumping = false;
            currentAirJumps++;

            callbacks.jump.OnAirJump?.Invoke();

            EndWallJump();

            ExecuteNormalJump(jump.airJump.Force);
        }
        private void TryWallJump()
        {
            if (isWallJumping) { return; }
            isWallJumping = true;
            isJumping = true;

            pressedThreshold = 100f;
            wallJumpPressedThreshold = 100f;
            attackJumpPressedThreshold = 100f;

            callbacks.jump.OnWallJump?.Invoke();
            Invoke(nameof(EndWallJump), jump.wallJump.LockTime);

            ExecuteWallJump();
        }


        private void ExecuteWallHop()
        {
            Vector2 angle = (Vector2)(Quaternion.Euler(0, 0, jump.wallJump.Angle) * Vector2.right);
            Vector2 jumpDir = new Vector2(angle.x * (touchingRight ? -1 : touchingLeft ? 1 : 0), angle.y);

            if (jump.wallJump.Flip)
                FlipSprite();

            rb.velocity = jumpDir * jump.wallJump.Force / 2;
        }
        private void ExecuteWallJump()
        {
            isWallJumping = true;
            leftWallTime = 100f;
            Vector2 angle = (Vector2)(Quaternion.Euler(0, 0, jump.wallJump.Angle) * Vector2.right);
            Vector2 jumpDir = new Vector2(angle.x * (touchingRight ? -1 : touchingLeft ? 1 : 0), angle.y);

            if (jump.wallJump.Flip)
                FlipSprite();

            rb.velocity = jumpDir * jump.wallJump.Force;
        }
        private void ExecuteDashJump()
        {
            isSuperJumping = true;

            Vector2 angle = (Vector2)(Quaternion.Euler(0, 0, jump.dashJump.Angle) * Vector2.right);
            Vector2 jumpDir = new Vector2(angle.x * inputs.MoveInputs.x, angle.y);

            rb.velocity = jumpDir * jump.dashJump.Force;

            InterruptDash();
            pressedThreshold = 100f;
            wallJumpPressedThreshold = 100f;
            attackJumpPressedThreshold = 100f;
            callbacks.jump.OnDashJump?.Invoke();
        }
        private void ExecuteNormalJump(float force)
        {
            float _buffer = jump.SnappyJump ? 1.125f : 1;

            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(force * _buffer * Vector2.up, ForceMode2D.Impulse);
        }

        private void GravityModifiers()
        {
            bool _releaseEarly = jump.JumpCut && rb.velocity.y > 0 && !jumpHold;
            // bool _shortWallJump = isWallJumping && !jumpHold;
            bool _snapJump = jump.SnappyJump && !isWallJumping && !isGrounded && rb.velocity.y < jump.groundJump.Force * 0.8f;

            float _earlyJumpBuffer = _releaseEarly ? jump.JumpCutBuffer : 1;
            //  float _earlyWallJumpBuffer = _shortWallJump ? jump.wallJump.CutBuffer : 1;
            float _snappyJumpBuffer = _snapJump ? 2 : 1;

            gravityBuffer = _snappyJumpBuffer * _earlyJumpBuffer  /*_earlyWallJumpBuffer*/;
        }
        private void EndWallJump()
        {
            if (!isWallJumping) { return; }

            lockMovement = false;
            isWallJumping = false;

            if (inputs.MoveInputs.x == 0)
                rb.velocity = Vector2.zero;

            callbacks.jump.OnEndWallJump?.Invoke();
        }
        public void ReadyAirJump()
        {
            callbacks.jump.OnAirJumpReady?.Invoke();
            currentAirJumps = 0;
        }
        #endregion

        #region Dash
        // Variables
        private bool stopDash = false;
        private bool dashReady = true;
        // Methods
        private IEnumerator Dashing()
        {
            float traveledDistance = 0;

            if (isSliding)
                FlipSprite();

            callbacks.dash.OnDashStart?.Invoke();

            Vector2 mouseDir = inputs.MousePosition - (Vector2)transform.position;
            Vector2 facingDir = inputs.MoveInputs.magnitude > 0 ? inputs.MoveInputs : new Vector2(isFacingRight ? 1 : -1, 0);

            Vector2 dashDirection;


            if (onWalkableSlop && dash.followSlopDirection)
            {
                Vector2 slopDir = Vector2.Perpendicular(surfaceNormal).normalized;
                dashDirection = new Vector2(slopDir.x * -facingDir.x, slopDir.y * -facingDir.x);
            }
            else
                dashDirection = (dash.dashToMouseDirection ? mouseDir : facingDir).normalized;

            Vector2 startPosition = transform.position;

            dashDirection.y = dash.horizontalDashOnly ? 0 : dashDirection.y;
            rb.velocity = dashDirection;

            // Start Dashing
            while (traveledDistance < dash.distance)
            {
                if (rb.velocity.magnitude < 1 || stopDash)
                    break;

                traveledDistance = Vector2.Distance(startPosition, transform.position);
                rb.velocity = dashDirection * dash.power * 5;

                yield return null;
            }
            // Finish Dashing

            callbacks.dash.OnDashEnd?.Invoke();

            // Cooldown
            if (dash.resetOn.ByCooldown)
            {
                yield return new WaitForSeconds(dash.resetOn.cooldown);
                ReadyDash();
            }
            else yield return null;
        }
        // Inputs
        public void DashPressed()
        {
            if (!enableDash) { return; }

            if (isDashing && dash.stopOn.PressedAgain)
                InterruptDash();

            if (isDashing || !dashReady || isGrabing || onSteepSlop) { return; }

            if (!isGrounded && !dash.airDash) { return; }

            if (isGrounded && !dash.groundDash) { return; }

            StartCoroutine(Dashing());
        }
        // Helpers
        private void ReadyDash()
        {
            if (dashReady) { return; }

            callbacks.dash.OnDashReady?.Invoke();
            dashReady = true;
        }
        private void InterruptDash()
        {
            callbacks.dash.OnDashInterruption?.Invoke();
            stopDash = true;
        }
        #endregion

        #region Climb & Slide
        // Variables
        private bool grabOn = true;
        private bool slideOn = true;
        private bool climbHold = false;
        private bool slideHold = false;
        private bool isClimbing => isHanging || isClimbingDown || isClimbingUp;

        // Grabing Functions
        private bool CanSlide()
        {
            if (isGrounded || !enableGrab || !isFacingWall)
                return false;

            return grab.slideType switch
            {
                WallGrab.SlideType.Auto => true,
                WallGrab.SlideType.PushAgainstWall => isPushingAgainstWall,
                WallGrab.SlideType.Toggle => slideOn,
                WallGrab.SlideType.Hold => slideHold,
                WallGrab.SlideType.Off => false,
                _ => false,
            };

            //return grab.slide && !isGrounded && !isJumping && isPushingAgainstWall;
        }
        private bool CanGrab()
        {
            if (!enableGrab || !isFacingWall)
                return false;

            return grab.grabType switch
            {
                WallGrab.GrabType.Off => false,
                WallGrab.GrabType.AlwaysGrab => true,
                WallGrab.GrabType.Toggle => grabOn,
                WallGrab.GrabType.Hold => climbHold,
                _ => false,
            };
        }

        // Grabing Functions
        private void ToggleSlide()
        {
            if (grab.slideType != WallGrab.SlideType.Toggle) { return; }

            slideOn = !slideOn;
        }
        private void ToggleGrab()
        {
            if (grab.grabType != WallGrab.GrabType.Toggle) { return; }

            grabOn = !grabOn;
        }

        private void CheckGrabing()
        {
            bool _wasSliding = isSliding;
            bool _wasClimbing = isClimbingUp || isClimbingDown;
            bool _wasGrabing = isGrabing;

            isGrabing = isHanging || isClimbingDown || isClimbingUp;

            HandleWallMovement();

            if (isGrabing && !_wasGrabing)
            {
                callbacks.wallGrab.OnWallGrab?.Invoke();
                rb.velocity = Vector2.zero;
            }

            if (!_wasSliding && isSliding)
            {
                callbacks.wallGrab.OnSlideStart?.Invoke();
            }

            if (_wasSliding && !isSliding)
                callbacks.wallGrab.OnSlideStop?.Invoke();

            if (!_wasClimbing && (isClimbingDown || isClimbingUp))
                callbacks.wallGrab.OnClimbStart?.Invoke();

            if (_wasClimbing && !(isClimbingDown || isClimbingUp))
                callbacks.wallGrab.OnClimbStop?.Invoke();

        }

        // Inputs Climb
        public void ClimbPressed()
        {
            climbHold = true;
            ToggleGrab();
        }
        public void ClimbReleased()
        {
            climbHold = false;
        }

        // Inputs Slide
        public void SlidePressed()
        {
            slideHold = true;
            ToggleSlide();
        }
        public void SlideReleased()
        {
            slideHold = false;
        }

        private bool HitHead()
        {
            Vector2 position = new Vector2(col.bounds.center.x, col.bounds.max.y);
            Vector2 size = new Vector2(0.2f, 0.2f);

            return Physics2D.BoxCastNonAlloc(position, size, 0f, Vector2.zero, rayHits, 0f, detection.groundLayer) > 0;
        }
        private bool HitGround()
        {
            Vector2 position = new Vector2(col.bounds.center.x, col.bounds.min.y);
            Vector2 size = new Vector2(0.2f, 0.2f);

            return Physics2D.BoxCastNonAlloc(position, size, 0f, Vector2.zero, rayHits, 0f, detection.groundLayer) > 0;
        }

        // Handlers
        private void HandleLedgeGrab()
        {
            if (canClimbLedge)
            {
                canClimbLedge = false;

                if (touchingRight)
                {
                    transform.position = new Vector2(transform.position.x + col.bounds.size.x + 0.05f, transform.position.y + col.bounds.size.y + 0.05f);
                }
                else if (touchingLeft)
                {
                    transform.position = new Vector2(transform.position.x - col.bounds.size.x - 0.05f, transform.position.y + col.bounds.size.y + 0.05f);
                }
            }
        }
        private void HandleWallMovement()
        {
            if (isTouchingWall && isTouchingLedge && !isWallJumping)
            {
                if (CanGrab())
                {
                    isSliding = false;

                    HandleClimb();
                    return;
                }
                else if (CanSlide())
                {
                    isClimbingUp = false;
                    isClimbingDown = false;
                    isHanging = false;

                    HandleSlide();
                    return;
                }

                isClimbingUp = false;
                isClimbingDown = false;
                isHanging = false;
                isSliding = false;

                return;
            }

            isClimbingUp = false;
            isClimbingDown = false;
            isHanging = false;
            isSliding = false;
        }
        private void HandleClimb()
        {
            if (!isFacingWall && isGrabing)
            {
                isClimbingUp = false;
                isClimbingDown = false;
                isHanging = false;
                return;
            }

            float upSpeed = grab.climbUp && isTouchingWall && isTouchingLedge ? grab.climbUpSpeed : 0;
            float downSpeed = grab.climbDown ? grab.climbDownSpeed : 0;
            float speed = inputs.MoveInputs.y < 0 ? downSpeed : upSpeed;

            if (!(HitHead() && inputs.MoveInputs.y > 0 || HitGround() && inputs.MoveInputs.y < 0))
                rb.velocity = new Vector2(rb.velocity.x, inputs.MoveInputs.y * speed * Time.fixedDeltaTime * 50);

            isClimbingUp = yVel > 0;
            isClimbingDown = yVel < 0;
            isHanging = yVel == 0;
        }
        private void HandleSlide()
        {
            if (!isFacingWall && isGrabing || isGrounded) { return; }

            isSliding = true;

            if (isJumping || isSuperJumping) { return; }

            if (grab.fixedSlideSpeed)
                rb.velocity = new Vector2(rb.velocity.x, -grab.slideSpeed * Time.fixedDeltaTime * 50);
            else
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -grab.maxSlideSpeed, float.MaxValue) * Time.fixedDeltaTime * 50);
        }
        #endregion

        #region Callbacks
        // CallBacks
        private void OnGroundHit()
        {
            if (isSprintActive && !sprintHold) isSprintActive = false;
            if (onWalkableSlop) rb.velocity = Vector2.zero;
            if (jump.airJump.ResetOnGroundHit) ReadyAirJump();
            if (isWallJumping) EndWallJump();
            if (dash.resetOn.GroundHit) ReadyDash();

            isSuperJumping = false;
            isJumping = false;

        }
        private void OnWallHit()
        {
            //isSuperJumping = false;
            //isJumping = false;

            if (jump.airJump.ResetOnWallHit) ReadyAirJump();
            if (isWallJumping) EndWallJump();
            if (isDashing) InterruptDash();
            if (dash.resetOn.WallHit) ReadyDash();
        }
        private void OnDashStart()
        {
            if (isWallJumping) EndWallJump();

            isSuperJumping = false;
            canRotate = false;
            stopDash = false;
            dashReady = false;
            isDashing = true;
            lockMovement = true;
        }
        private void OnDashEnd()
        {
            isDashing = false;
            canRotate = true;
            lockMovement = false;

            if (!stopDash) rb.velocity = Vector2.zero;
            if (dash.resetOn.EndDash) ReadyDash();
        }

        private void OnClimbStart()
        {
            isSuperJumping = false;
            isJumping = false;
            rb.velocity = Vector2.zero;
        }
        #endregion

        #region Debug
        private void OnDrawGizmos()
        {
            // Detection Debugs
            if (detection.debug)
            {
                Vector2 _rightDetector = new Vector2(col.bounds.max.x + detection.xPos, col.bounds.center.y + detection.yPos);
                Vector2 _leftDetector = new Vector2(col.bounds.min.x - detection.xPos, col.bounds.center.y + detection.yPos);
                Vector2 _groundDetector = new Vector2(col.bounds.center.x, col.bounds.min.y + detection.pos);

                Gizmos.color = detection.groundDebugColor;
                Gizmos.DrawWireCube(_groundDetector, new Vector3(col.bounds.size.x * detection.width, detection.height, 1));
                Gizmos.color = detection.wallDebugColor;
                Gizmos.DrawWireSphere(_leftDetector, detection.radius);
                Gizmos.DrawWireSphere(_rightDetector, detection.radius);
            }

            // Wall jump Debugs
            if (jump.wallJump.ShowAngle)
            {
                int dir = transform.eulerAngles.y == 0 ? 1 : -1;

                Vector2 angle = (Vector2)(Quaternion.Euler(0, 0, jump.wallJump.Angle) * Vector2.right);
                Vector2 jumpdir = new Vector2(angle.x * dir, angle.y) * 3;

                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, jumpdir);
            }
            if (jump.dashJump.ShowAngle)
            {
                int dir = transform.eulerAngles.y == 0 ? 1 : -1;

                Vector2 angle = (Vector2)(Quaternion.Euler(0, 0, jump.dashJump.Angle) * Vector2.right);
                Vector2 jumpdir = new Vector2(angle.x * dir, angle.y) * 3;

                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, jumpdir);
            }
        }
        #endregion

    }
}
