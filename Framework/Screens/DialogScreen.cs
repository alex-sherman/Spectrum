using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Screens
{
    public class DialogScreen<T> : InGameScreen
    {
        public event Action<T> OnClose;
        public KeyBind CloseKey;

        public override bool HandleInput(bool otherTookInput, InputState input)
        {
            return base.HandleInput(otherTookInput, input);
        }

        public override void Close()
        {
            OnClose?.Invoke(default(T));
            base.Close();
        }

        public void Close(T choice)
        {
            OnClose?.Invoke(choice);
            base.Close();
        }
    }
}
