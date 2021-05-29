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
        public DebugPrinter()
        {
            Positioning = PositionType.Relative;
            Width = 1.0;
            Height = 1.0;
        }
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
        private struct ShowMessage
        {
            public string Messsage;
            public double Time;
        }
        static HashSet<string> onceMessages = new HashSet<string>();
        static Dictionary<string, ShowMessage> showMessages = new Dictionary<string, ShowMessage>();
        static List<string> strings = new List<string>();
        static List<IDebug> objects = new List<IDebug>();
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
        public static void Show(string msg)
        {
            showMessages[GetStackString()] = new ShowMessage() { Messsage = msg, Time = DebugTiming.Now() };
        }
        private static StackFrame GetStackFrame()
        {
            StackFrame sf; int sfi = 1;
            while ((sf = new StackFrame(sfi, true)).GetMethod().DeclaringType == typeof(DebugPrinter))
                sfi++;
            return sf;
        }
        private static string GetStackString()
        {
            var sf = GetStackFrame();
            return $"{(sf.GetFileName() ?? "").Split('\\').Last()}:{sf.GetFileLineNumber()}";
        }
        public static void Print(string msg)
        {
            lock (strings)
            {
                if (strings.Count > 20)
                {
                    strings.RemoveAt(0);
                }
                string[] msgStrings = msg.Split('\n');
                for (int i = 0; i < msgStrings.Length; i++)
                {
                    string line = $"{GetStackString()}: {msgStrings[i]}";
                    try
                    {
                        File.AppendAllText("output.log", line + "\n");
                    }
                    catch { }
                    strings.Add(line);
                    Debug.Print(line);
                }
            }
        }
        string fform(double d)
        {
            return $"{d:0.00}";
        }
        private float averageFPS = 0;
        private void DrawTimes(int startLine, SpriteBatch spritebatch, float dt, float layer)
        {
            float curPos = (startLine) * Font.LineSpacing + Rect.Top;
            averageFPS = (float)(averageFPS * 0.95 + (1.0 / dt) * 0.05);
            string toPrint = averageFPS.ToString("0.00");
            spritebatch.DrawString(Font, toPrint, new Vector2(Parent.MeasuredWidth - Font.MeasureString(toPrint).X, curPos), Color.Black, layer);
            curPos += Font.MeasureString(toPrint).Y;
            foreach (var timeGroup in DebugTiming.Groups)
            {
                toPrint = string.Format("{0} ({1} {2})\n---------------", timeGroup.Name, timeGroup.ShowCumulative ? "Sum" : "Avg", fform(timeGroup.FrameAverages().Sum(t => t.Item2)));
                spritebatch.DrawString(Font, toPrint, new Vector2(Parent.MeasuredWidth - Font.MeasureString(toPrint).X, curPos), Color.Black, layer);
                curPos += Font.MeasureString(toPrint).Y;
                var times = timeGroup.FrameInfo().Take(10);
                foreach (var time in times)
                {
                    toPrint = time.Item1 + ": " + fform(time.Item2.TotalTime) + " (" + fform(time.Item2.AvgerageTime) + "x" + time.Item2.Count + ")";
                    spritebatch.DrawString(Font, toPrint, new Vector2(Parent.MeasuredWidth - Font.MeasureString(toPrint).X, curPos), Color.Black, layer);
                    curPos += Font.MeasureString(toPrint).Y;
                }
            }
        }
        public override void Draw(float gameTime, SpriteBatch spritebatch)
        {
            base.Draw(gameTime, spritebatch);

            if (SpectrumGame.Game.Debug)
            {
                Vector2 topLeft = (Vector2)Rect.TopLeft;
                spritebatch.DrawString(Font, "Debug:", Vector2.Zero + topLeft, FontColor, LayerDepth);
                float strSize = Font.LineSpacing;
                Vector2 offset = topLeft;
                lock (strings)
                {
                    if (strings.Count > 0)
                    {
                        for (int i = 0; i < strings.Count; i++)
                        {
                            offset.Y += strSize;
                            spritebatch.DrawString(Font, strings[i], offset, FontColor, LayerDepth);
                        }
                    }
                }
                offset = topLeft;
                offset.Y += strSize * 10;
                foreach (var kvp in showMessages)
                {
                    offset.Y += strSize;
                    spritebatch.DrawString(Font, $"{kvp.Key}: {kvp.Value.Messsage}", offset, Color.Black, LayerDepth);
                }
                float curPos = 0;
                for (int i = 0; i < objects.Count; i++)
                {
                    string toPrint = objects[i].Debug();
                    spritebatch.DrawString(Font, toPrint, new Vector2(0, curPos + (21) * strSize) + topLeft, Color.Black, LayerDepth);
                    curPos += Font.MeasureString(toPrint.ToString()).Y;
                }
                DrawTimes(2, spritebatch, gameTime, LayerDepth);
            }
            var now = DebugTiming.Now();
            showMessages = showMessages.Where(kvp => kvp.Value.Time > now - 5).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
