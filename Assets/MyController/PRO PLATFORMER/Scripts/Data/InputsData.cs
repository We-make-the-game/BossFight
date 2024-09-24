using UnityEngine;

namespace VEOController
{
    public enum JoyStickButtons
    {
        South = KeyCode.JoystickButton0,
        East = KeyCode.JoystickButton1,
        West = KeyCode.JoystickButton2,
        North = KeyCode.JoystickButton3,

        RightShoulderTop = KeyCode.JoystickButton5,
        RightShoulderBot = KeyCode.JoystickButton5,

        LeftShoulderTop = KeyCode.JoystickButton4,
        LeftShoulderBot = KeyCode.JoystickButton4,

        RightStickPress = KeyCode.JoystickButton9,
        LeftStickPress = KeyCode.JoystickButton8,
    }
    public enum MouseButtons
    {
        Left = KeyCode.Mouse0,
        Right = KeyCode.Mouse1,
        Middle = KeyCode.Mouse2,
        Extra1 = KeyCode.Mouse3,
        Extra2 = KeyCode.Mouse4,
        Extra3 = KeyCode.Mouse5,
        Extra4 = KeyCode.Mouse6,
    }
    public enum KeyboardButtons
    {
        // Keybaord Extra
        Shift = KeyCode.LeftShift,
        Control = KeyCode.LeftControl,
        Space = KeyCode.Space,
        Tab = KeyCode.Tab,
        Alt = KeyCode.LeftAlt,
        Escape = KeyCode.Escape,

        // Keyboard Alph
        A = KeyCode.A,
        Z = KeyCode.Z,
        E = KeyCode.E,
        R = KeyCode.R,
        T = KeyCode.T,
        Y = KeyCode.Y,
        U = KeyCode.U,
        I = KeyCode.I,
        O = KeyCode.O,
        P = KeyCode.P,
        Q = KeyCode.Q,
        S = KeyCode.S,
        D = KeyCode.D,
        F = KeyCode.F,
        G = KeyCode.G,
        H = KeyCode.H,
        J = KeyCode.J,
        K = KeyCode.K,
        L = KeyCode.L,
        M = KeyCode.M,
        W = KeyCode.W,
        X = KeyCode.X,
        C = KeyCode.C,
        V = KeyCode.V,
        B = KeyCode.B,
        N = KeyCode.N,

        // Horizontal Numbers
        Num1 = KeyCode.Alpha1,
        Num2 = KeyCode.Alpha2,
        Num3 = KeyCode.Alpha3,
        Num4 = KeyCode.Alpha4,
        Num5 = KeyCode.Alpha5,
        Num6 = KeyCode.Alpha6,
        Num7 = KeyCode.Alpha7,
        Num8 = KeyCode.Alpha8,
        Num9 = KeyCode.Alpha9,
        Num0 = KeyCode.Alpha0,

        // Keypad Numbers
        LeftNum0 = KeyCode.Keypad0,
        LeftNum1 = KeyCode.Keypad1,
        LeftNum2 = KeyCode.Keypad2,
        LeftNum3 = KeyCode.Keypad3,
        LeftNum4 = KeyCode.Keypad4,
        LeftNum5 = KeyCode.Keypad5,
        LeftNum6 = KeyCode.Keypad6,
        LeftNum7 = KeyCode.Keypad7,
        LeftNum8 = KeyCode.Keypad8,
        LeftNum9 = KeyCode.Keypad9,
    }
}