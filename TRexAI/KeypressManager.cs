using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TRexAI
{
    public static class KeypressManager
    {
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, IntPtr dwExtraInfo);

        const byte KEYEVENTF_EXTENDEDKEY = 0x01;
        const byte KEYEVENTF_KEYUP = 0x02;
        const int VK_ARROW_UP = 0x26;
        const int VK_ARROW_DOWN = 0x28;

        static int jumpReleaseTimer = 0;

        enum KeyboardState { Idle, CrouchPressed, JumpPressed }

        static KeyboardState currentKeyboardState = KeyboardState.Idle;

        static void TapKey(byte keyCode)
        {
            keybd_event(keyCode, 0, KEYEVENTF_EXTENDEDKEY | 0, (IntPtr)0);
            keybd_event(keyCode, 0, KEYEVENTF_EXTENDEDKEY | 0, (IntPtr)0);
            keybd_event(keyCode, 0, KEYEVENTF_EXTENDEDKEY | 0, (IntPtr)0);
            keybd_event(keyCode, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (IntPtr)0);
        }

        static void HoldKey(byte keyCode)
        {
            keybd_event(keyCode, 0, KEYEVENTF_EXTENDEDKEY | 0, (IntPtr)0);
        }

        static void ReleaseKey(byte keyCode)
        {
            keybd_event(keyCode, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (IntPtr)0);
        }

        public static void TapJump()
        {
            if (currentKeyboardState == KeyboardState.CrouchPressed)
            {
                ReleaseKey(VK_ARROW_DOWN);
                currentKeyboardState = KeyboardState.Idle;
            }
            TapKey(VK_ARROW_UP);
        }

        public static void Jump()
        {
            if (currentKeyboardState == KeyboardState.CrouchPressed)
            {
                ReleaseKey(VK_ARROW_DOWN);
                currentKeyboardState = KeyboardState.Idle;
            }
            if (currentKeyboardState == KeyboardState.Idle)
            {
                currentKeyboardState = KeyboardState.JumpPressed;
                HoldKey(VK_ARROW_UP);
            }
            jumpReleaseTimer = 2;
        }

        public static void Crouch(bool forceCrouchKeypress)
        {
            if (currentKeyboardState == KeyboardState.CrouchPressed && forceCrouchKeypress)
            {
                ReleaseKey(VK_ARROW_DOWN);
                HoldKey(VK_ARROW_DOWN);
            }

            if (currentKeyboardState == KeyboardState.Idle)
            {
                HoldKey(VK_ARROW_DOWN);
                currentKeyboardState = KeyboardState.CrouchPressed;
            }
        }

        public static void UnCrouch()
        {
            if (currentKeyboardState == KeyboardState.CrouchPressed)
            {
                ReleaseKey(VK_ARROW_DOWN);
                currentKeyboardState = KeyboardState.Idle;
            }
        }

        public static void RunUpdate()
        {
            jumpReleaseTimer--;
            if (jumpReleaseTimer == 0)
            {
                ReleaseKey(VK_ARROW_UP);
                if (currentKeyboardState == KeyboardState.JumpPressed)
                    currentKeyboardState = KeyboardState.Idle;
            }
        }
    }
}
