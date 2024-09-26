using UnityEngine;
using UnityEditor;
using static VEOController.Jump;

namespace VEOController
{
    [CustomEditor(typeof(Controller))]
    public class EditorController : Editor
    {
        private readonly string[] tabs =
        {
        "Physics",
        "Detection",
        "Movement",
        "Rotation" ,
        "Attack & Heal",
        "Jump" ,
        "Dash" ,
        "Climb & Slide",
        "Animations",
        "Effects",
        "Inputs",
        "Callbacks",
        "Combat"
    };
        private int index = 0;

        private Controller controller;
        private SerializedObject soController;

        Texture2D HeaderImage;
        Rect LogoPos;

        #region Booleans
        private bool enablePhysics;
        private bool enableDetection;
        private bool enableMovement;
        private bool enableRotation;
        private bool enableJump;
        private bool enableDash;
        private bool enableGrab;
        private bool enableEffects;
        private bool enableAnimations;
        private bool enableCombat;
        #endregion

        #region Properties

        // Callbacks
        private SerializedProperty movementCB;
        private SerializedProperty jumpCB;
        private SerializedProperty dashCB;
        private SerializedProperty collisionCB;
        private SerializedProperty wallgrabCB;

        // Inputs
        private SerializedProperty rawAxis;
        private SerializedProperty actions;
        private SerializedProperty horizontalAxis;
        private SerializedProperty verticalAxis;

        // Physics
        private SerializedProperty gravityScale;
        private SerializedProperty fallSpeedMultiplier;
        private SerializedProperty maxFallVelocity;

        // Detection
        private SerializedProperty debug;
        private SerializedProperty groundLayer;
        private SerializedProperty width;
        private SerializedProperty height;
        private SerializedProperty pos;
        private SerializedProperty wallLayer;
        private SerializedProperty radius;
        private SerializedProperty xPos;
        private SerializedProperty yPos;
        private SerializedProperty groundDebugColor;
        private SerializedProperty wallDebugColor;

        // Movement
        private SerializedProperty canSprint;
        private SerializedProperty alwaysSprint;
        private SerializedProperty walkSpeed;
        private SerializedProperty sprintSpeed;
        private SerializedProperty acceleration;
        private SerializedProperty decceleration;
        private SerializedProperty maxWalkableSlop;
        private SerializedProperty slopSlidingSpeed;
        private SerializedProperty airSpeedModifier;
        private SerializedProperty animationSpeedBuffer;
        private SerializedProperty fallControl;
        private SerializedProperty upControl;

        // Rotation
        private SerializedProperty skin;
        private SerializedProperty rotateToMoveDirection;
        private SerializedProperty rotateOnMouseClick;
        private SerializedProperty smoothRotation;
        private SerializedProperty rotspeed;

        // Attack
        private SerializedProperty attackCooldown;

        // Jump
        private SerializedProperty JumpCut;
        private SerializedProperty SnappyJump;
        private SerializedProperty JumpCutBuffer;
        private SerializedProperty groundJump;
        private SerializedProperty attackJump;
        private SerializedProperty airJump;
        private SerializedProperty wallJump;
        private SerializedProperty dashJump;
        private SerializedProperty enableAirJump;
        private SerializedProperty enableWallJump;
        private SerializedProperty enableDashJump;

        // Dash
        private SerializedProperty airDash;
        private SerializedProperty groundDash;
        private SerializedProperty dashToMouseDirection;
        private SerializedProperty horizontalDashOnly;
        private SerializedProperty followSlopDirection;
        private SerializedProperty distance;
        private SerializedProperty power;
        private SerializedProperty resetOn;
        private SerializedProperty stopOn;

        // Grab
        private SerializedProperty grabType;
        private SerializedProperty slideType;
        private SerializedProperty climbUp;
        private SerializedProperty climbDown;
        private SerializedProperty fixedSlideSpeed;
        private SerializedProperty climbingUpSpeed;
        private SerializedProperty climbingDownSpeed;
        private SerializedProperty maxSlideSpeed;
        private SerializedProperty slideSpeed;
    
        // Effects
        private SerializedProperty warnings;
        private SerializedProperty effects;

        // Animations
        private SerializedProperty animator;
        private SerializedProperty animations;

        // Combat
        private SerializedProperty healthBar;
        private SerializedProperty health;
        private SerializedProperty damageCoolDownTime;

        #endregion
        Color defaultColor;
        // Functions
        private void OnEnable()
        {
            defaultColor = GUI.color;

            controller = target as Controller;
            soController = new SerializedObject(controller);

            HeaderImage = Resources.Load<Texture2D>("Button");

            SerializeCallbacks();
            SerializePhysics();
            SerializeDetection();
            SerializeMovement();
            SerializeRotation();
            SerializeAttack();
            SerializeJump();
            SerializeDash();
            SerializeGrab();
            SerializeEffects();
            SerializeAnimation();
            SerializeInputs();
            SerializeCombat();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SetBools();
            Header(); // Draw Inspector Header
            EditorGUILayout.BeginVertical();
            GUILayout.Space(2);
            //  GUI.backgroundColor = Color.magenta;
            index = GUILayout.SelectionGrid(index, tabs, 6, GUILayout.Height(45)); // Create The Tabs
                                                                                   // GUI.backgroundColor = Color.white;
            GUILayout.Space(15);
            EditorGUILayout.EndVertical();
            SetTab(); // Populate The Tabs


            if (EditorGUI.EndChangeCheck()) // Apply Changes
                soController.ApplyModifiedProperties();
        }
        private void SetTab()
        {
            switch (index)
            {
                case 0:
                    Title("PHYSICS");
                    ShowPhysics();
                    break;
                case 1:
                    Title("DETECTION");
                    ShowDetection();
                    break;
                case 2:
                    Title("MOVEMENT");
                    ShowMovement();
                    break;
                case 3:
                    Title("ROTATION");
                    ShowRotation();
                    break;
                case 4:
                    Title("ATTACK & HEAL");
                    ShowAttack();
                    break;
                case 5:
                    Title("JUMP");
                    ShowJump();
                    break;
                case 6:
                    Title("DASH");
                    ShowDash();
                    break;
                case 7:
                    Title("CLIMB & SLIDE");
                    ShowGrab();
                    break;
                case 8:
                    Title("ANIMATIONS");
                    ShowAnimation();
                    break;
                case 9:
                    Title("EFFECTS");
                    ShowEffects();
                    break;
                case 10:
                    Title("INPUTS");
                    ShowInputs();
                    break;
                case 11:
                    Title("CALLBACKS");
                    ShowCallbacks();
                    break;
                case 12:
                    Title("COMBAT");
                    ShowCombat();
                    break;
            }
        }

        // Serialize
        private void SerializeCallbacks()
        {
            movementCB = GetVariables("callbacks.movement");
            jumpCB = GetVariables("callbacks.jump");
            collisionCB = GetVariables("callbacks.collision");
            dashCB = GetVariables("callbacks.dash");
            wallgrabCB = GetVariables("callbacks.wallGrab");

        }
        private void SerializeAnimation()
        {
            animator = soController.FindProperty("anim.animator");
            animations = soController.FindProperty("anim.animationClips");
        }
        private void SerializeMovement()
        {
            canSprint = soController.FindProperty("movement.canSprint");
            alwaysSprint = soController.FindProperty("movement.alwaysSprint");
            walkSpeed = soController.FindProperty("movement.walkSpeed");
            sprintSpeed = soController.FindProperty("movement.sprintSpeed");
            acceleration = soController.FindProperty("movement.acceleration");
            decceleration = soController.FindProperty("movement.decceleration");
            airSpeedModifier = soController.FindProperty("movement.airSpeedModifier");
            animationSpeedBuffer = soController.FindProperty("movement.animationSpeedBuffer");
            fallControl = soController.FindProperty("movement.fallControl");
            upControl = soController.FindProperty("movement.upControl");
            maxWalkableSlop = soController.FindProperty("movement.maxWalkableSlop");
            slopSlidingSpeed = soController.FindProperty("movement.slopSlidingSpeed");

        }
        private void SerializePhysics()
        {
            gravityScale = soController.FindProperty("physics.gravityScale");
            fallSpeedMultiplier = soController.FindProperty("physics.fallSpeedMultiplier");
            maxFallVelocity = soController.FindProperty("physics.maxFallVelocity");
        }
        private void SerializeDetection()
        {
            debug = soController.FindProperty("detection.debug");
            groundLayer = soController.FindProperty("detection.groundLayer");
            width = soController.FindProperty("detection.width");
            height = soController.FindProperty("detection.height");
            pos = soController.FindProperty("detection.pos");
            wallLayer = soController.FindProperty("detection.wallLayer");
            radius = soController.FindProperty("detection.radius");
            xPos = soController.FindProperty("detection.xPos");
            yPos = soController.FindProperty("detection.yPos");
            groundDebugColor = soController.FindProperty("detection.groundDebugColor");
            wallDebugColor = soController.FindProperty("detection.wallDebugColor");
        }
        private void SerializeRotation()
        {
            skin = soController.FindProperty("rotation.skin");
            rotateToMoveDirection = soController.FindProperty("rotation.rotateToMoveDirection");
            rotateOnMouseClick = soController.FindProperty("rotation.rotateOnMouseClick");
            smoothRotation = soController.FindProperty("rotation.smoothRotation");
            rotspeed = soController.FindProperty("rotation.speed");
        }
        private void SerializeAttack()
        {
            attackCooldown = soController.FindProperty("attack.attackCooldown");
        }
        private void SerializeDash()
        {
            airDash = soController.FindProperty("dash.airDash");
            groundDash = soController.FindProperty("dash.groundDash");
            dashToMouseDirection = soController.FindProperty("dash.dashToMouseDirection");
            horizontalDashOnly = soController.FindProperty("dash.horizontalDashOnly");
            distance = soController.FindProperty("dash.distance");
            power = soController.FindProperty("dash.power");
            followSlopDirection = soController.FindProperty("dash.followSlopDirection");
            resetOn = soController.FindProperty("dash.resetOn");
            stopOn = soController.FindProperty("dash.stopOn");
        }
        private void SerializeJump()
        {
            JumpCut = soController.FindProperty("jump.JumpCut");
            SnappyJump = soController.FindProperty("jump.SnappyJump");
            JumpCutBuffer = soController.FindProperty("jump.JumpCutBuffer");

            groundJump = soController.FindProperty("jump.groundJump");
            attackJump = soController.FindProperty("jump.attackJump");
            airJump = soController.FindProperty("jump.airJump");
            wallJump = soController.FindProperty("jump.wallJump");
            dashJump = soController.FindProperty("jump.dashJump");

            enableAirJump = soController.FindProperty("jump.enableAirJump");
            enableWallJump = soController.FindProperty("jump.enableWallJump");
            enableDashJump = soController.FindProperty("jump.enableDashJump");
        }

        private void SerializeGrab()
        {
            grabType = soController.FindProperty("grab.grabType");
            slideType = soController.FindProperty("grab.slideType");
            climbUp = soController.FindProperty("grab.climbUp");
            climbDown = soController.FindProperty("grab.climbDown");
            fixedSlideSpeed = soController.FindProperty("grab.fixedSlideSpeed");
            climbingUpSpeed = soController.FindProperty("grab.climbUpSpeed");
            climbingDownSpeed = soController.FindProperty("grab.climbDownSpeed");
            maxSlideSpeed = soController.FindProperty("grab.maxSlideSpeed");
            slideSpeed = soController.FindProperty("grab.slideSpeed");
        }
        private void SerializeEffects()
        {
            warnings = GetVariables("effects.debug");
            effects = GetVariables("effects.effects");
        }
        private void SerializeInputs()
        {
            rawAxis = GetVariables("inputs.rawAxis");
            horizontalAxis = GetVariables("inputs.horizontalAxis");
            verticalAxis = GetVariables("inputs.verticalAxis");
            actions = GetVariables("inputs.keys");
        }
        private void SerializeCombat()
        {
            healthBar = soController.FindProperty("combat.healthBar");
            health = GetVariables("combat.health");
            damageCoolDownTime = GetVariables("combat.damageCoolDownTime");
        }

        // Draw
        private void ShowCallbacks()
        {
            EditorGUILayout.HelpBox("Here you can set additional callbacks actions without touching the script", MessageType.Info);
            Separator("You can also set callbacks actions via script in the CallbacksHandler script");

            Show(collisionCB);
            Show(movementCB);
            Show(jumpCB);
            Show(wallgrabCB);
            Show(dashCB);
        }
        private void ShowAttack()
        {
            Separator("Attack Settings");
            Space(5);
            Show(attackCooldown);
        }
        private void ShowMovement()
        {
            enableMovement = CheckActive("Movement", enableMovement);
            controller.enableMovement = enableMovement;

            if (controller.movement.canSprint == false)
                controller.movement.alwaysSprint = false;

            Separator("Walk & Sprint");
            Show(canSprint);
            ShowIf(alwaysSprint, controller.movement.canSprint, enableMovement);
            ShowIf(walkSpeed, !controller.movement.alwaysSprint, enableMovement);
            ShowIf(sprintSpeed, controller.movement.canSprint, enableMovement);
            Show(animationSpeedBuffer);

            Separator("Smoothness");
            Show(acceleration);
            Show(decceleration);

            Separator("Slops");
            Show(maxWalkableSlop);
            Show(slopSlidingSpeed);

            Separator("Air Control");
            Show(airSpeedModifier);
            Show(fallControl);
            Show(upControl);
        }
        private void ShowPhysics()
        {
            enablePhysics = CheckActive("Physics", enablePhysics);
            controller.enablePhysics = enablePhysics;
            Separator("Gravity");
            Show(gravityScale);
            Separator("Falling");
            Show(fallSpeedMultiplier);
            Show(maxFallVelocity);
        }
        private void ShowDetection()
        {
            enableDetection = CheckActive("Detection", enableDetection);
            controller.enableDetection = enableDetection;

            Separator("Ground Detection Settings");
            Show(groundLayer);
            Show(width);
            Show(height);
            Show(pos);
            Separator("Wall Detection Settings");
            Show(wallLayer);
            Show(radius);
            Show(xPos);
            Show(yPos);
            Separator("Debug Detection");
            Show(debug);
            if (controller.detection.debug)
            {
                Show(groundDebugColor);
                Show(wallDebugColor);
            }
        }
        private void ShowRotation()
        {
            enableRotation = CheckActive("Rotation", enableRotation);
            controller.enableRotation = enableRotation;

            Separator("Player Model");
            Show(skin);
            Separator("Rotation Type");
            Show(rotateToMoveDirection);
            Show(rotateOnMouseClick);
            Separator("Smooth Rotation");
            Show(smoothRotation);
            ShowIf(rotspeed, controller.rotation.smoothRotation, enableRotation);
        }
        private void ShowDash()
        {
            enableDash = CheckActive("Dash", enableDash);
            controller.enableDash = enableDash;

            Separator("Global Settings");
            Show(distance);
            Show(power);

            Separator("Enablers");
            Show(airDash);
            Show(groundDash);

            Separator("Direction");
            Show(dashToMouseDirection);
            Show(horizontalDashOnly);
            Show(followSlopDirection);

            Separator("Callbacks");
            Show(resetOn);
            Show(stopOn);

        }
        private void ShowJump()
        {
            enableJump = CheckActive("Jump", enableJump);
            controller.enableJump = enableJump;

            Separator("Global Settings");

            Show(SnappyJump);
            Space(2);
            Show(JumpCut);
            ShowIf(JumpCutBuffer, controller.jump.JumpCut, enableJump);

            Separator("Enablers");

            Show(enableAirJump);
            Show(enableDashJump);
            Show(enableWallJump);

            Separator("Jumps Settings");

            Show(groundJump);
            Show(attackJump);
            ShowIf(airJump, controller.jump.enableAirJump, enableJump);
            ShowIf(dashJump, controller.jump.enableDashJump, enableJump);
            ShowIf(wallJump, controller.jump.enableWallJump, enableJump);
        }
        private void ShowGrab()
        {
            enableGrab = CheckActive("Grab", enableGrab);
            controller.enableGrab = enableGrab;

            bool grabOff = controller.grab.grabType == WallGrab.GrabType.Off;
            bool slideOff = controller.grab.grabType == WallGrab.GrabType.AlwaysGrab;
            bool disableSlide = controller.grab.slideType == WallGrab.SlideType.Off || slideOff;

            Separator("Climb Settings");
            EditorGUILayout.HelpBox("When grabing into the wall, climb up or down", MessageType.Info);
            Space(5);
            Show(grabType);
            Space(5);
            ShowIf(climbUp, !grabOff, enableGrab);
            ShowIf(climbDown, !grabOff, enableGrab);
            ShowIf(climbingUpSpeed, controller.grab.climbUp && !grabOff, enableGrab);
            ShowIf(climbingDownSpeed, controller.grab.climbDown && !grabOff, enableGrab);

            Space(5);
            Separator("Slide Settings");
            EditorGUILayout.HelpBox("If not grabing into the wall, slide down", MessageType.Info);
            Space(5);
            ShowIf(slideType, !slideOff, enableGrab);
            Space(5);
            ShowIf(fixedSlideSpeed, !disableSlide, enableGrab);

            if (controller.grab.fixedSlideSpeed)
                ShowIf(slideSpeed, !disableSlide, enableGrab);
            else
                ShowIf(maxSlideSpeed, !disableSlide, enableGrab);
        }
        private void ShowEffects()
        {
            enableEffects = CheckActive("Effects", enableEffects);
            controller.effects.enable = enableEffects;
            effects.isExpanded = enableEffects;

            Separator("Debug Messages");
            Show(warnings);

            Space(10);
            Separator("Effects");
            EditorGUILayout.HelpBox("  Here you can add & remove effects and give them a tag. \n" +
                "  Tags are used to call the effect from scripts", MessageType.Info, true);
            Space(10);
            Show(effects);
        }
        private void ShowAnimation()
        {
            enableAnimations = CheckActive("Animations", enableAnimations);
            controller.enableAnimations = enableAnimations;

            Separator("Animator");
            Show(animator);
            Space(5);

            Separator("Clips");
            if (GUILayout.Button("Refresh Clips"))
            {
                controller.anim.PopulateClips();
                Debug.Log("Generate Animations Clips");
                animations.serializedObject.Update();
            }
            Space(5);

            EditorGUILayout.HelpBox("The animation name in the animator and the animation clip must have the exact same name", MessageType.Warning);
            GUI.enabled = false;
            animations.isExpanded = controller.anim.animator != null && enableAnimations;
            Show(animations);
            GUI.enabled = true;
        }
        private void ShowInputs()
        {
            actions.isExpanded = true;

            Separator("Axis");
            Show(rawAxis);
            Space(5);
            Show(horizontalAxis);
            Show(verticalAxis);
            Separator("Actions");
            EditorGUILayout.HelpBox("Here you can add new keys and actions, you can also add actions via script in the InputsHandler (recommended)", MessageType.Info, true);
            Show(actions);

            soController.ApplyModifiedProperties();
        }
        private void ShowCombat()
        {
            enableCombat = CheckActive("Combat", enableCombat);
            controller.enableCombat = enableCombat;

            Separator("Player Health");
            Show(healthBar);
            EditorGUILayout.HelpBox("The Max player health is 100", MessageType.Info);
            Show(health);
            Show(damageCoolDownTime);
        }

        // UI
        private void Separator(string title)
        {
            Space(5);
            GUI.backgroundColor = Color.gray;
            GUI.contentColor = Color.yellow;

            var separatorStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            separatorStyle.border.Add(GUILayoutUtility.GetLastRect());

            GUILayout.Label(title, separatorStyle);

            GUI.contentColor = defaultColor;
            GUI.backgroundColor = defaultColor;
            Space(2);
        }

        private void Header()
        {
            LogoPos.x = 0;
            LogoPos.y = 0;
            LogoPos.width = Screen.width;
            LogoPos.height = 100;
            GUI.DrawTexture(LogoPos, HeaderImage);
        }
        private void Title(string text)
        {
            EditorPrefs.SetInt("SelectedTab", index);

            var style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 18;
            style.alignment = TextAnchor.MiddleLeft;
            style.fontStyle = FontStyle.Bold;

            GUILayout.Label(text, style);
            GUILayout.Space(20);
        }

        // Helpers
        private SerializedProperty GetVariables(string property)
        {
            return soController.FindProperty(property);
        }
        private void Space(int space)
        {
            EditorGUILayout.Space(space);
        }
        private void Show(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property);
        }
        private void ShowIf(SerializedProperty property, bool condition1, bool condition2)
        {
            GUI.enabled = condition1 && condition2;
            GUI.backgroundColor = GUI.enabled ? defaultColor : Color.gray;
            EditorGUILayout.PropertyField(property);
            GUI.backgroundColor = defaultColor;
            GUI.enabled = condition2;
        }
        private bool CheckActive(string tag, bool condition)
        {
            GUI.backgroundColor = condition ? Color.green : Color.red;
            condition = EditorGUILayout.ToggleLeft("", condition);
            EditorGUILayout.Space(10);
            EditorPrefs.SetBool(tag, condition);
            GUI.backgroundColor = defaultColor;

            if (!condition)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Disabled! Tick the red box to enable it", MessageType.Error, true);
                EditorGUILayout.Space(10);
            }
            GUI.enabled = condition;

            return condition;
        }
        private void SetBools()
        {
            index = EditorPrefs.HasKey("SelectedTab") ? EditorPrefs.GetInt("SelectedTab") : 0;

            enablePhysics = Enabled("Physics");
            enableDetection = Enabled("Detection");
            enableMovement = Enabled("Movement");
            enableRotation = Enabled("Rotation");
            enableJump = Enabled("Jump");
            enableDash = Enabled("Dash");
            enableGrab = Enabled("Grab");
            enableEffects = Enabled("Effects");
            enableAnimations = Enabled("Animations");
            enableCombat = Enabled("Combat");

            static bool Enabled(string tag)
            {
                if (EditorPrefs.HasKey(tag))
                {
                    return EditorPrefs.GetBool(tag);
                }
                else
                {
                    EditorPrefs.SetBool(tag, true);
                    return true;
                }
            }
        }
    }
}