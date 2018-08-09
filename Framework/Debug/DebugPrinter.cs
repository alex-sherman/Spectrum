using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Spectrum.Framework.Screens;
using Spectrum.Framework.Entities;
using System.IO;

namespace Spectrum.Framework
{
    public class DebugPrinter : Element
    {
        private class DebugHolder : IDebug
        {
            readonly Func<string> text;
            readonly Action<float> draw;
            public DebugHolder(Func<string> text, Action<float> draw)
            {
                this.text = text;
                this.draw = draw;
            }
            public string Debug()
            {
                return text?.Invoke();
            }

            public void DebugDraw(float dt)
            {
                draw?.Invoke(dt);
            }
        }
        public static HashSet<string> onceMessages = new HashSet<string>();
        private static List<string> strings = new List<string>();
        private static List<IDebug> objects = new List<IDebug>();
        public static IDebug display(Func<string> text = null, Action<float> draw = null)
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
        public static void PrintOnce(string msg)
        {
            if (!onceMessages.Contains(msg))
            {
                Print(msg);
                onceMessages.Add(msg);
            }
        }
        public static void Print(string msg)
        {
            StackFrame sf; int sfi = 1;
            while ((sf = new StackFrame(sfi, true)).GetMethod().DeclaringType == typeof(DebugPrinter))
                sfi++;
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
                    var line = string.Format("{2} ({0}): {1}", sf.GetFileLineNumber(), msgStrings[i], (filename ?? "").Split('\\').Last());
                    try
                    {
                        File.AppendAllText("output.log", line + "\n");
                    }
                    catch { }
                    strings.Add(line);
                    Console.WriteLine(line);
                }
            }
        }
        [Obsolete("Use Print(string) instead")]
        public static void print(string msg, params object[] args)
        {
            if (args.Length > 0)
                msg = String.Format(msg, args);
            Print(msg);
        }
        string fform(double d)
        {
            return String.Format("{0:0.00}", d);
        }
        private float averageFPS = 0;
        private void DrawTimes(int startLine, SpriteBatch spritebatch, float dt)
        {
            float curPos = (startLine) * Font.LineSpacing;
            averageFPS = (float)(averageFPS * 0.95 + (1.0 / dt) * 0.05);
            string toPrint = averageFPS.ToString("0.00");
            spritebatch.DrawString(Font, toPrint, new Vector2(Parent.MeasuredWidth - Font.MeasureString(toPrint).X, curPos), Color.Black, Z);
            curPos += Font.MeasureString(toPrint).Y;
            foreach (var timeGroup in DebugTiming.Groups)
            {
                toPrint = string.Format("{0} ({1})\n---------------", timeGroup.Name, timeGroup.ShowCumulative ? "Sum" : "Avg");
                spritebatch.DrawString(Font, toPrint, new Vector2(Parent.MeasuredWidth - Font.MeasureString(toPrint).X, curPos), Color.Black, Z);
                curPos += Font.MeasureString(toPrint).Y;
                var times = timeGroup.FrameInfo().Take(10);
                foreach (var time in times)
                {
                    toPrint = time.Item1 + ": " + fform(time.Item2.TotalTime) + " (" + fform(time.Item2.AvgerageTime) + "x" + time.Item2.Count + ")";
                    spritebatch.DrawString(Font, toPrint, new Vector2(Parent.MeasuredWidth - Font.MeasureString(toPrint).X, curPos), Color.Black, Z);
                    curPos += Font.MeasureString(toPrint).Y;
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
                DrawTimes(2, spritebatch, gameTime.DT());

            }
        }
    }
}
