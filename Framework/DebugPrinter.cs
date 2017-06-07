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
        private class DebugHolder : IDebug
        {
            Func<string> text;
            Action<GameTime, SpriteBatch> draw;
            public DebugHolder(Func<string> text, Action<GameTime,SpriteBatch> draw)
            {
                this.text = text;
                this.draw = draw;
            }
            public string Debug()
            {
                return text?.Invoke();
            }

            public void DebugDraw(GameTime gameTime, SpriteBatch spriteBatch)
            {
                draw?.Invoke(gameTime, spriteBatch);
            }
        }
        private static List<string> strings = new List<string>();
        private static List<IDebug> objects = new List<IDebug>();
        public static IDebug display(Func<string> text = null, Action<GameTime, SpriteBatch> draw = null)
        {
            IDebug output = new DebugHolder(text, draw);
            display(output);
            return output;
        }
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
        public static void log(string msg, params object[] args) { print(msg, args); }
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
                    strings.Add(string.Format("{2} ({0}): {1}", sf.GetFileLineNumber(), msgStrings[i], (filename ?? "").Split('\\').Last()));
                }
            }
        }
        private void DrawTimes(int startLine, SpriteBatch spritebatch)
        {
            float curPos = (startLine) * Font.LineSpacing;
            foreach (var timeGroup in DebugTiming.Groups)
            {
                string toPrint = string.Format("{0} ({1})\n---------------", timeGroup.Name, timeGroup.ShowCumulative ? "Sum" : "Avg");
                spritebatch.DrawString(Font, toPrint, new Vector2(ScreenManager.CurrentManager.Viewport.Width - Font.MeasureString(toPrint).X, curPos), Color.Black, Z);
                curPos += Font.MeasureString(toPrint).Y;
                var times = timeGroup.ShowCumulative ? timeGroup.CumulativeTimes : timeGroup.FrameAverages().Take(10);
                foreach (var time in times)
                {
                    toPrint = time.Item1 + ": " + String.Format("{0:0.00}", time.Item2);
                    spritebatch.DrawString(Font, toPrint, new Vector2(ScreenManager.CurrentManager.Viewport.Width - Font.MeasureString(toPrint).X, curPos), Color.Black, Z);
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
                    spritebatch.DrawString(Font, toPrint, new Vector2(0, curPos + (11) * strSize), Color.Black, Z);
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
