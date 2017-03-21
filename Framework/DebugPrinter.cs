using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spectrum.Framework.Graphics;
using System.Diagnostics;
using Spectrum.Framework.Screens;
using Spectrum.Framework.Entities;

namespace Spectrum.Framework
{
    public class DebugPrinter : Element
    {
        private static List<string> strings = new List<string>();
        private static List<IDebug> objects = new List<IDebug>();
        private static Dictionary<string, Dictionary<string, List<double>>> timings = new Dictionary<string, Dictionary<string, List<double>>>();
        public static void display(IDebug o)
        {
            if (o != null)
            {
                objects.Add(o);
            }
        }
        public static void undisplay(IDebug o)
        {
            try
            {
                objects.Remove(o);
            }
            catch { }
        }
        public static void print(string msg, params object[] args)
        {
            msg = String.Format(msg, args);
            StackFrame sf = new StackFrame(1, true);
            lock (strings)
            {
                if (strings.Count > 20)
                {
                    strings.RemoveAt(0);
                }
                string[] msgStrings = msg.Split('\n');
                for (int i = 0; i < msgStrings.Length; i++)
                {
                    string filename = sf.GetFileName();
                    strings.Add(String.Format("{2} ({0}): {1}", sf.GetFileLineNumber(), msgStrings[i], (filename ?? "").Split('\\').Last()));
                }
            }
        }
        public static void time(string group, string name, double miliseconds)
        {
            if (!timings.ContainsKey(group))
                timings[group] = new Dictionary<string, List<double>>();
            if (!timings[group].ContainsKey(name))
                timings[group][name] = new List<double>();
            timings[group][name].Add(miliseconds);
            if (timings[group][name].Count > 60)
                timings[group][name].RemoveAt(0);
        }
        private void DrawTimes(int startLine, SpriteBatch spritebatch)
        {
            float curPos = (startLine) * Font.LineSpacing;
            foreach (var timeGroup in timings)
            {
                string toPrint = timeGroup.Key+"\n---------------";
                spritebatch.DrawString(Font, toPrint, new Vector2(ScreenManager.CurrentManager.Viewport.Width - Font.MeasureString(toPrint).X, curPos), Color.Blue, Z);
                curPos += Font.MeasureString(toPrint).Y;
                List<KeyValuePair<string, double>> renderTimes = timeGroup.Value.ToList().ConvertAll((kvp) => new KeyValuePair<string, double>(kvp.Key, kvp.Value.Average()));
                renderTimes.Sort((item, other) => -item.Value.CompareTo(other.Value));
                double sum = renderTimes.Sum(item => item.Value);
                for (int i = 0; i < 10 && i < renderTimes.Count; i++)
                {
                    toPrint = renderTimes[i].Key + ": " + String.Format("{0:0.00}", renderTimes[i].Value);
                    spritebatch.DrawString(Font, toPrint, new Vector2(ScreenManager.CurrentManager.Viewport.Width - Font.MeasureString(toPrint).X, curPos), Color.Blue, Z);
                    curPos += Font.LineSpacing;
                }
            }
        }
        public override void Draw(GameTime gameTime, SpriteBatch spritebatch)
        {
            base.Draw(gameTime, spritebatch);

            if (SpectrumGame.Game.Debug)
            {
                float strSize = Font.LineSpacing;
                lock (strings)
                {
                    if (strings.Count > 0)
                    {
                        strSize = Font.MeasureString(strings[0]).Y;
                        for (int i = 0; i < strings.Count; i++)
                        {
                            spritebatch.DrawString(Font, strings[i], new Vector2(0, i * strSize), FontColor, Z);
                        }
                    }
                }
                float curPos = 0;
                for (int i = 0; i < objects.Count; i++)
                {
                    string toPrint = objects[i].Debug();
                    spritebatch.DrawString(Font, toPrint, new Vector2(0, curPos + (11) * strSize), Color.Blue, Z);
                    curPos += Font.MeasureString(toPrint.ToString()).Y;
                }
                DrawTimes(2, spritebatch);

            }
            if(SpectrumGame.Game.DebugDrawAll)
            {
                foreach (var entity in SpectrumGame.Game.EntityManager)
                {
                    if (entity is GameObject)
                        ((GameObject)entity).DebugDraw(gameTime, spritebatch);
                }
            }
            if (SpectrumGame.Game.DebugDraw)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    objects[i].DebugDraw(gameTime, spritebatch);
                }
            }
            //timings = new Dictionary<string, Dictionary<string, double>>();
        }
    }
}
