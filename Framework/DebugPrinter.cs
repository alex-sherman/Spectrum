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
    public class DebugPrinter : Entity
    {
        private static List<string> strings = new List<string>();
        private SpriteFont font;
        private static List<IDebug> objects = new List<IDebug>();
        private ScreenManager screenManager;
        public DebugPrinter(ScreenManager manager, SpriteFont font)
        {
            screenManager = manager;
            this.font = font;
        }
        //TODO: Randomly can't get a filename and throws exception causing a crash
        public static void print(string msg)
        {
            StackFrame sf =  new StackFrame(1, true);
            lock (strings)
            {
                if (strings.Count > 20)
                {
                    strings.RemoveAt(0);
                }
                string[] msgStrings = msg.Split('\n');
                for (int i = 0; i < msgStrings.Length; i++)
                {
                    strings.Add(String.Format("{2} ({0}): {1}", sf.GetFileLineNumber(), msgStrings[i], sf.GetFileName().Split('\\').Last()));
                }
            }
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
        public static void print(string msg, params object[] args)
        {
            print(String.Format(msg, args));
        }

        public void GUIDraw(GameTime gameTime, float layer)
        {
            //base.Draw(gameTime);

            if (SpectrumGame.Game.Debug)
            {
                lock (strings)
                {
                    if (strings.Count > 0)
                    {
                        float strSize = font.MeasureString(strings[0]).Y;
                        for (int i = 0; i < strings.Count; i++)
                        {
                            screenManager.DrawString(font, strings[i], new Vector2(0, i * strSize), Color.White, layer);
                        }
                    }
                }
                float curPos = 0;
                if (objects.Count > 0)
                {
                    float strSize = font.MeasureString("foo").Y;
                    for (int i = 0; i < objects.Count; i++)
                    {
                        string toPrint = objects[i].Debug();
                        screenManager.DrawString(font, toPrint, new Vector2(0, curPos + (11) * strSize), Color.Blue, layer);
                        curPos += font.MeasureString(toPrint.ToString()).Y;
                    }
                }
            }
            if (SpectrumGame.Game.DebugDraw)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    objects[i].DebugDraw(gameTime, screenManager.SpriteBatch);
                }
            }
        }
    }
}
