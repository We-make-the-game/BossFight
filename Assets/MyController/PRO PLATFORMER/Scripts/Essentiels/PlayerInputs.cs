using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace VEOController
{

    [Serializable]
    public class PlayerInputs
    {

        public bool mobileAxis = false;



        [Serializable]
        public class Key
        {
            public string tag = "New Action";
            [Space(5)]
            public KeyboardButtons keyboard;
            public JoyStickButtons joystick;
            [Space(20)]
            public UnityEvent OnPressed = null;
            public UnityEvent OnReleased = null;

            private string name => tag;
        }

        #region Inspector
        [SerializeField] private bool rawAxis = true;
        [Space]
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private string verticalAxis = "Vertical";
        [Space(10)]
        [SerializeField] private List<Key> keys = new List<Key>();
        #endregion

        // Public Variables
        [HideInInspector] public Vector2 MoveInputs { get; private set; }
        [HideInInspector] public Vector2 MousePosition => Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Private Variables
        private HashSet<KeyCode> releasedKeys = new HashSet<KeyCode>();
        private bool movementInputsDisabled = false;
        private bool inputsDisabled = false;

        // Private Methods
        private void HandleKeysInputs()
        {
            HandleKeyPressed();
            HandleKeyReleased();
        }
        private void HandleKeyPressed()
        {
            if (Input.anyKeyDown)
            {
                foreach (var item in keys)
                {
                    if (Input.GetKeyDown((KeyCode)item.keyboard)
                        || Input.GetKeyDown((KeyCode)item.joystick))
                        item.OnPressed.Invoke();
                }
            }
        }
        private void HandleKeyReleased()
        {
            foreach (var item in keys)
            {
                if ((Input.GetKeyUp((KeyCode)item.keyboard)
                    || Input.GetKeyUp((KeyCode)item.joystick))
                    && !releasedKeys.Contains((KeyCode)item.keyboard))
                {
                    item.OnReleased.Invoke();
                    releasedKeys.Add((KeyCode)item.keyboard);
                }
            }

            releasedKeys.Clear();
        }
        private void HandleMovementInputs()
        {
            if (movementInputsDisabled) { return; }

            if (rawAxis)
                MoveInputs = new Vector2
                ((int)Input.GetAxisRaw(horizontalAxis), (int)Input.GetAxisRaw(verticalAxis));
            else
                MoveInputs = new Vector2
                (Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis));


        }

        // Public Methods
        public void AddAction(string tag, UnityAction onPressed, UnityAction onReleased)
        {
            foreach (Key key in keys)
            {
                if (key.tag == tag)
                {
                    if (onPressed != null)
                        key.OnPressed.AddListener(onPressed);

                    if (onReleased != null)
                        key.OnReleased.AddListener(onReleased);

                    return;
                }
            }

            Debug.LogError("The key with the tag (" + tag + ") Does not exist! Please add it first in the player inputs");
        }
        public void UpdateInputs()
        {
            if (inputsDisabled) { return; }

            HandleKeysInputs();
            HandleMovementInputs();
        }

        public string SetKeyboardBinding(string tag, KeyboardButtons key)
        {
            foreach (Key i in keys)
            {
                if (i.tag == tag)
                {
                    i.keyboard = key;
                    return i.keyboard.ToString();
                }

                Debug.LogError("The key with the tag (" + tag + ") Does not exist! Please add it first in the player inputs");
                return "";
            }

            return "";
        }
        public string SetJoystickBinding(string tag, JoyStickButtons key)
        {
            foreach (Key i in keys)
            {
                if (i.tag == tag)
                {
                    i.joystick = key;
                    return i.joystick.ToString();
                }

                Debug.LogError("The key with the tag (" + tag + ") Does not exist! Please add it first in the player inputs");
                return "";
            }

            return "";
        }


        // Public Enablers
        public void DisableMovementInputs()
        {
            movementInputsDisabled = true;
            MoveInputs = Vector2.zero;
        }
        public void EnableMovementInputs()
        {
            movementInputsDisabled = false;
        }
        public void DisableInputs()
        {
            inputsDisabled = true;
            MoveInputs = Vector2.zero;
        }
        public void EnableInputs()
        {
            inputsDisabled = false;
        }

    }
}