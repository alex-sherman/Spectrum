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
    public class AnimationData : DefaultDict<string, AnimationClip> { }
    class AnimationParser : CachedContentParser<AnimationData, AnimationData>
    {
        public AnimationParser() : base("g3dj")
        {
            Prefix = "Models";
        }

        public static AnimationData GetAnimations(JObject jobj)
        {
            AnimationData output = new AnimationData();
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
                        keyframes.Add(new Keyframe((string)boneNode["boneId"], ((float)keyFrameNode["keytime"]) / 1000.0f, rotation, translation));
                    }
                }
                keyframes.Sort(delegate(Keyframe x, Keyframe y)
                {
                    return x.Time.CompareTo(y.Time);
                });
                foreach (var keyframe in keyframes)
                {

                    keyframe.NextTranslation = keyframes.Find((kf) => kf.Time > keyframe.Time && kf.Translation.HasValue && kf.Bone == keyframe.Bone);
                    keyframe.NextRotation = keyframes.Find((kf) => kf.Time > keyframe.Time && kf.Rotation.HasValue && kf.Bone == keyframe.Bone);
                }
                float duration = keyframes.Last().Time;
                string name = (string)animationNode["id"];
                output[name] = new AnimationClip(name, duration, keyframes);
            }
            return output;
        }

        protected override AnimationData LoadData(string path, string name)
        {
            JsonTextReader reader = new JsonTextReader(new StreamReader(path));
            JObject jobj = JObject.Load(reader);


            return GetAnimations(jobj);
        }

        protected override AnimationData SafeCopy(AnimationData data)
        {
            return data;
        }
    }
}
