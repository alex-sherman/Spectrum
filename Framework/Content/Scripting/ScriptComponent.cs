using Microsoft.Xna.Framework.Input;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content.Scripting
{
    public class ScriptComponent : Component, IUpdate
    {
        IScript Script;
        public ScriptComponent(IScript script)
        {
            Script = script;
        }
        public override void Initialize(Entity e)
        {
            base.Initialize(e);
            Script.AddObject("P", e);
            Script.GetFunction("Initialize")?.Invoke();
        }

        public void Update(float dt)
        {
            Script.AddObject("Input", InputState.Current);
            Script.GetFunction("Update")?.Invoke(dt);
        }
    }
}
