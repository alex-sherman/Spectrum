using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Graphics.Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content.ModelParsing
{
    class AnimationParser : MultiContentParser<AnimationData, AnimationData>
    {
        public AnimationParser() : base(new Dictionary<string, Parser>() {
            { "g3dj", G3DJReader.LoadAnimation },
        })
        {
            Prefix = "Models";
        }

        protected override AnimationData SafeCopy(AnimationData data)
        {
            return data;
        }
    }
}
