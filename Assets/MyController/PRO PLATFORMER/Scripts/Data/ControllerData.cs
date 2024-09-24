using UnityEngine;
using System;

namespace VEOController
{
    [Serializable]
    public class Detection
    {
        // Properties
        public bool debug;

        // Ground Detection
        [SerializeField] public LayerMask groundLayer;
        public Color groundDebugColor = Color.green;
        [SerializeField, Range(0.1f, 0.90f)] public float width = 0.90f;
        [SerializeField, Range(0.1f, 0.3f)] public float height = 0.15f;
        [SerializeField, Range(-0.05f, 0f)] public float pos = 0f;

        // Wall Detection
        [SerializeField] public LayerMask wallLayer;
        public Color wallDebugColor = Color.red;
        [SerializeField, Range(0.02f, 0.1f)] public float radius = 0.1f;
        [SerializeField, Range(-0.2f, 0.2f)] public float xPos = 0;
        [SerializeField, Range(-0.5f, 0.5f)] public float yPos = 0;
    }
    [Serializable]
    public class Physics
    {
        [Tooltip("This will multiply the current gravity 9.81")]
        [SerializeField, Range(0, 4)] public float gravityScale = 1;

        [Tooltip("This will increase the falling speed, so you will fall faster and reduces floating effect")]
        [SerializeField, Range(1, 3)] public float fallSpeedMultiplier = 1.5f;
        [SerializeField, Range(0, 100)] public float maxFallVelocity = 30;
    }
    [Serializable]
    public class Movement
    {
        // Properties
        [SerializeField] public bool canSprint = true;
        [SerializeField] public bool alwaysSprint = true;
        [SerializeField, Range(0f, 15f)] public float walkSpeed = 8;
        [SerializeField, Range(0f, 15f)] public float sprintSpeed = 10;
        [SerializeField, Range(0f, 1f)] public float acceleration = 0.95f;
        [SerializeField, Range(0f, 1f)] public float decceleration = 0.95f;
        [Tooltip("The maximum walkable slop angle in degrees")]
        [SerializeField, Range(0, 60)] public float maxWalkableSlop = 45;
        [Tooltip("The sliding speed when standing on a non-walkable slop (High Slop)")]
        [SerializeField, Range(5f, 15f)] public float slopSlidingSpeed = 5;
        [Tooltip("Speed multiplier while jumping or in the air, a value less than 1 will make the player slower in the air")]
        [SerializeField, Range(0f, 2f)] public float airSpeedModifier = 1;
        [Tooltip("Scale the running animation speed with the current movement speed, the higher this value the faster the animation will be")]
        [SerializeField, Range(1f, 2f)] public float animationSpeedBuffer = 1.15f;
        [Tooltip("How much the player can be controlled while falling, 1 for 100%")]
        [SerializeField, Range(0, 1)] public float fallControl = 1;
        [Tooltip("How much the player can be controlled while going up in the air, 1 for 100%")]
        [SerializeField, Range(0, 1)] public float upControl = 1;
    }
    [Serializable]
    public class Rotation
    {
        [SerializeField] public Transform skin;
        [SerializeField] public bool rotateToMoveDirection = true;
        [SerializeField] public bool rotateOnMouseClick;
        [SerializeField] public bool smoothRotation;
        [SerializeField, Range(0, 2)] public float speed = 1;
    }

    [Serializable]
    public class Attack
    {
        [Tooltip("Attack cooldown in seconds")]
        [SerializeField, Range(0.25f, 1)] public float attackCooldown = 0.5f;
    }

    [Serializable]
    public class Jump
    {
        [Tooltip("Reduce floating time while reaching the apex, this will make the jump fast and snappy")]
        public bool SnappyJump = true;
        [Tooltip("Release your jump button early to execute a shorter jump, hold it for a longer jump")]
        public bool JumpCut = true;
        [Tooltip("The higher this value is the shorter the jump will be when fast taping the jump key")]
        [Range(1, 10f)] public float JumpCutBuffer = 5f;

        public bool enableAirJump = true;
        public bool enableWallJump = true;
        public bool enableDashJump = true;


        [Serializable]
        public struct GroundJump
        {
            [Range(0, 30f)] public float Force;
            [Tooltip("This allows the player to jump shortly after leaving a platform")]
            [Range(0, 0.3f)] public float CoyoteTime;
            [Tooltip("If air jumping is enabled, this range determines whether the player should perform an air jump or utilize input buffering to jump upon landing.")]
            [Range(0, 1.5f)] public float GroundBuffering;
            [Tooltip("Give the player a small range where they can press jump button before landing and still execute the jump on landing")]
            [Range(0, 0.3f)] public float InputBuffering;
            public bool CanJumpOnSteepSlops;
        }
        [Serializable]
        public struct AirJump
        {
            public bool ResetOnGroundHit;
            public bool ResetOnWallHit;
            public int MaxAirJumps;
            [Range(0, 30f)] public float Force;
        }
        [Serializable]
        public struct WallJump
        {
            public bool Flip;
            public bool ShowAngle;

            [Range(0, 30f)] public float Force;
            [Range(10, 90f)] public float Angle;
            [Tooltip("The higher this value is the shorter the wall jump will be when fast taping the jump key")]
            [Range(1, 5f)] public float CutBuffer;
            [Tooltip("Disable air movement for a short amount of time to avoid snapping back to wall")]
            [Range(0, 1f)] public float LockTime;
            [Tooltip("How much you can control the character while wall jumping")]
            [Range(0, 20f)] public float Control;
            [Tooltip("Give the player a small range where they can press jump button before landing and still execute the jump on landing")]
            [Range(0, 0.3f)] public float InputBuffering;

        }
        [Serializable]
        public struct AttackJump
        {
            [Tooltip("Give the player a small range where they can press jump button before finishing attack and still execute the jump on finishing")]
            [Range(0, 0.5f)] public float InputBuffering;
        }

        [Serializable]
        public struct DashJump
        {
            public bool ShowAngle;

            [Range(0, 30f)] public float Force;
            [Range(10, 90f)] public float Angle;
        }

        public GroundJump groundJump = new GroundJump
        {
            Force = 13,
            CoyoteTime = 0.15f,
            GroundBuffering = 1f,
            InputBuffering = 0.2f,
        };
        public AirJump airJump = new AirJump
        {
            Force = 11,
            MaxAirJumps = 1,
            ResetOnGroundHit = true,
            ResetOnWallHit = true,
        };
        public WallJump wallJump = new WallJump
        {
            ShowAngle = false,
            Flip = true,
            Force = 15,
            Angle = 45,
            CutBuffer = 2,
            LockTime = 0.15f,
            Control = 10,
            InputBuffering = 0.2f
        };
        public AttackJump attackJump = new AttackJump
        {
            InputBuffering = 0.25f,
        };
        public DashJump dashJump = new DashJump
        {
            ShowAngle = false,
            Force = 13,
            Angle = 35,
        };
    }
    [Serializable]
    public class Dash
    {
        // Properties
        [SerializeField] public bool airDash = true;
        [SerializeField] public bool groundDash = true;
        [Tooltip("Turn this off if you want to dash to movement direction")]
        [SerializeField] public bool dashToMouseDirection = false;
        [Tooltip("Dash only in the X axis")]
        [SerializeField] public bool horizontalDashOnly = false;
        [Tooltip("If on slop, the player will follow the slop direction when dashing instead of input direction")]
        [SerializeField] public bool followSlopDirection = true;
        [SerializeField, Range(0, 10)] public float distance = 5;
        [SerializeField, Range(0, 10)] public float power = 5;
        [SerializeField]
        public ResetOn resetOn = new ResetOn
        {
            GroundHit = true,
            EndDash = true,
        };
        [SerializeField]
        public StopOn stopOn = new StopOn
        {
            GroundJump = true,
        };
        [Serializable]
        public struct ResetOn
        {
            [Header("Able To Dash Again if :")]
            public bool WallHit;
            public bool GroundHit;
            public bool EndDash;
            public bool ByCooldown;
            public float cooldown;
        }
        [Serializable]
        public struct StopOn
        {
            [Header("Interrupt The Dash if :")]
            public bool GroundJump;
            public bool AirJump;
            public bool PressedAgain;
            public bool Attack;
        }
    }
    [Serializable]
    public class WallGrab
    {
        public enum GrabType
        {
            Off,
            AlwaysGrab,
            Toggle,
            Hold,
        }
        public enum SlideType
        {
            Off,
            Auto,
            Toggle,
            Hold,
            PushAgainstWall,
        }
        public GrabType grabType = GrabType.Toggle;
        public SlideType slideType = SlideType.PushAgainstWall;

        [SerializeField] public bool climbUp = true;
        [SerializeField] public bool climbDown = true;
        [Space]
        [Range(0f, 10f)][SerializeField] public float climbUpSpeed = 5;
        [Range(0f, 10f)][SerializeField] public float climbDownSpeed = 5;
        [SerializeField] public bool fixedSlideSpeed = true;
        [Range(0f, 10f)][SerializeField] public float maxSlideSpeed = 10;
        [Range(0f, 10f)][SerializeField] public float slideSpeed = 5;
    }
}
