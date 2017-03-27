using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Graphics.Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    class AnimationData 
    {
        public JObject jobj;
        public Dictionary<string, AnimationClip> animations;
        public AnimationData(JObject jobj, Dictionary<string, AnimationClip> animations)
        {
            this.jobj = jobj;
            this.animations = animations;
        }
    }
    class AnimationParser : CachedContentParser<AnimationData, AnimationPlayer>
    {
        public AnimationParser()
        {
            Prefix = @"Models\";
        }

        public static Dictionary<string, AnimationClip> GetAnimations(JObject jobj)
        {
            Dictionary<string, AnimationClip> output = new Dictionary<string, AnimationClip>();
            foreach (JToken animationNode in (JArray)jobj["animations"])
            {
                List<Keyframe> keyframes = new List<Keyframe>();
                foreach (JToken boneNode in animationNode["bones"])
                {
                    foreach (JToken keyFrameNode in boneNode["keyframes"])
                    {
                        Matrix? rotation = null;
                        Matrix? translation = null;
                        JToken rotationNode = keyFrameNode["rotation"];
                        if (rotationNode != null)
                            rotation = MatrixHelper.CreateRotation(rotationNode);
                        JToken translationNode = keyFrameNode["translation"];
                        if (translationNode != null)
                            translation = MatrixHelper.CreateTranslation(translationNode);
                        keyframes.Add(new Keyframe((string)boneNode["boneId"], new TimeSpan(0, 0, 0, 0, (int)keyFrameNode["keytime"]), rotation, translation));
                    }
                }
                keyframes.Sort(delegate(Keyframe x, Keyframe y)
                {
                    return x.Time.CompareTo(y.Time);
                });
                TimeSpan duration = keyframes.Last().Time;
                string name = (string)animationNode["id"];
                output[name] = new AnimationClip(name, duration, keyframes);
            }
            return output;
        }

        protected override AnimationData LoadData(string path, string name)
        {
            path = TryExtensions(path, ".g3dj");
            JsonTextReader reader = new JsonTextReader(new StreamReader(path));
            JObject jobj = JObject.Load(reader);


            return new AnimationData(jobj, GetAnimations(jobj));
        }

        protected override AnimationPlayer SafeCopy(AnimationData data)
        {
            return new AnimationPlayer(data.animations);
        }
    }
}
