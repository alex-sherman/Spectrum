using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Input
{
    public class KeyBinding
    {
        public static Dictionary<string, KeyBinding> KeyBindings = new Dictionary<string, KeyBinding>()
        {
            #region Movement
            {"MoveForward", new KeyBinding(Keys.W)},
            {"MoveBackward", new KeyBinding(Keys.S)},
            {"MoveLeft", new KeyBinding(Keys.A)},
            {"MoveRight", new KeyBinding(Keys.D)},
            {"Jump", new KeyBinding(Keys.Space)},
            {"Crouch", new KeyBinding(Keys.LeftControl)},
            {"Run", new KeyBinding(Keys.LeftShift)},
            {"Interact", new KeyBinding(Keys.E)},
            #endregion

            #region UI
            {"MenuLeft", new KeyBinding(Keys.Left)},
            {"MenuRight", new KeyBinding(Keys.Right)},
            {"MenuUp", new KeyBinding(Keys.Up)},
            {"MenuDown", new KeyBinding(Keys.Down)},
            {"MenuCycleF", new KeyBinding(Keys.Tab)},
            {"MenuCycleB", new KeyBinding(Keys.Tab, modifier: Keys.LeftShift)},
            {"GoBack", new KeyBinding(Keys.Escape)},
            {"Continue", new KeyBinding(Keys.Enter)},
            {"EnterConsole", new KeyBinding(Keys.OemTilde)},
            {"InventoryScreen", new KeyBinding(Keys.I)},
            {"AbilityScreen", new KeyBinding(Keys.K)},
            {"CraftingScreen", new KeyBinding(Keys.J)},
            {"PlayerInfoScreen", new KeyBinding(Keys.C)},
            {"MultiplayerScreen", new KeyBinding(Keys.P)},
            #endregion

            #region VoidWalking
            {"ToggleVoid", new KeyBinding(Keys.G)},
            {"VoidNextAttribute", new KeyBinding(Keys.OemPeriod)},
            {"VoidPrevAttribute", new KeyBinding(Keys.OemComma)},
            #endregion

            #region DebugStuff
            {"NoClip", new KeyBinding(Keys.V)},
            {"ShowDebug", new KeyBinding(Keys.F1)},
            {"ShowDebugDraw", new KeyBinding(Keys.F2)},
            {"WireFrame", new KeyBinding(Keys.F)},
            #endregion

            #region Abilities
            {"EvokeModifier", new KeyBinding(Keys.LeftControl)},
            {"HangModifier", new KeyBinding(Keys.E)},
            {"UseAbility1", new KeyBinding(Keys.D1, mouseButton: 0)},
            {"UseAbility2", new KeyBinding(Keys.D2, mouseButton: 2)},
            {"UseAbility3", new KeyBinding(Keys.D3, mouseButton: 1)},
            {"UseAbility4", new KeyBinding(Keys.D4, mouseButton: 3)},
            {"UseAbility5", new KeyBinding(Keys.D5, mouseButton: 4)},
            {"UseAbility6", new KeyBinding(Keys.D6)},
            {"UseAbility7", new KeyBinding(Keys.D7)},
            {"UseAbility8", new KeyBinding(Keys.D8)},
            {"UseAbility9", new KeyBinding(Keys.D9)},
            {"UseAbility0", new KeyBinding(Keys.D0)},
            #endregion
        };


        public Keys? key1 { get; set; }
        public Keys? key2 { get; set; }
        public Keys? modifier { get; set; }
        public int? mouseButton { get; set; }
        public KeyBinding(Keys? key1 = null, Keys? key2 = null, int? mouseButton = null, Keys? modifier = null)
        {
            this.key1 = key1;
            this.key2 = key2;
            this.modifier = modifier;
            this.mouseButton = mouseButton;
        }
    }
}
