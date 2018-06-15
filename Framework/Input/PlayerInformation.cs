using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Input
{
    [Flags]
    public enum PlayerInputs
    {
        Keyboard = 1,
        Gamepad1 = 2,
        Gamepad2 = 4,
        Gamepad3 = 8,
        Gamepad4 = 16,
        Joystick1 = 32,
        Joystick2 = 64,
        Joystick3 = 128,
        Joystick4 = 256,
    }
    public class PlayerInformation
    {
        public static PlayerInformation Default = new PlayerInformation(PlayerInputs.Keyboard | PlayerInputs.Gamepad1, "Default");

        public PlayerInputs PlayerType;
        public InputLayout Layout;

        public bool UsesKeyboard;
        public List<int> UsedGamepads;
        public List<int> UsedJoysticks;

        public PlayerInformation(PlayerInputs playerType, string inputLayout)
        {
            PlayerType = playerType;
            Layout = InputLayout.Profiles[inputLayout];
            UsedGamepads = new List<int>();
            UsesKeyboard = false;
            UsedJoysticks = new List<int>();
            foreach (PlayerInputs input in Enum.GetValues(typeof(PlayerInputs)))
            {
                switch (input)
                {
                    case PlayerInputs.Keyboard:
                        UsesKeyboard = true;
                        break;
                    case PlayerInputs.Gamepad1:
                        UsedGamepads.Add(0);
                        break;
                    case PlayerInputs.Gamepad2:
                        UsedGamepads.Add(1);
                        break;
                    case PlayerInputs.Gamepad3:
                        UsedGamepads.Add(2);
                        break;
                    case PlayerInputs.Gamepad4:
                        UsedGamepads.Add(3);
                        break;
                    case PlayerInputs.Joystick1:
                        UsedJoysticks.Add(0);
                        break;
                    case PlayerInputs.Joystick2:
                        UsedJoysticks.Add(1);
                        break;
                    case PlayerInputs.Joystick3:
                        UsedJoysticks.Add(2);
                        break;
                    case PlayerInputs.Joystick4:
                        UsedJoysticks.Add(3);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
