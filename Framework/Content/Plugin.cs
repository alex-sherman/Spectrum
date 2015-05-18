using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Spectrum.Framework.Content
{
    public class Plugin
    {
        public static RealDict<string, Type> Types = new RealDict<string, Type>();
        public ContentHelper Content { get; private set; }

        public Plugin(string name, string path)
        {
            //Monogame's content manager doesn't work with absolute paths
            Content = new ContentHelper(new ContentManager(SpectrumGame.Game.Services, Path.Combine(path, "Content")));
            //Assembly.LoadFile requries and absolute path
            path = System.IO.Path.GetFullPath(path);
            string[] files = Directory.GetFiles(path, name + ".dll", SearchOption.TopDirectoryOnly);
            foreach (String filename in files)
            {
                Assembly coreAsm = Assembly.LoadFile(Path.Combine(path, filename));
                List<Type> coreTypes = coreAsm.GetTypes().ToList();
                foreach (Type type in coreTypes)
                {
                    #region PreloadContent
                    foreach (object attribute in type.GetCustomAttributes(true).ToList())
                    {
                        PreloadedContentAttribute preload = attribute as PreloadedContentAttribute;
                        if (preload != null)
                        {
                            ContentHelper.LoadType(preload.Type, preload.Path, preload.Plugin);
                        }
                    }
                    foreach (FieldInfo field in type.GetFields())
                    {
                        foreach (object attribute in field.GetCustomAttributes(true).ToList())
                        {
                            PreloadedContentAttribute preload = attribute as PreloadedContentAttribute;
                            if (preload != null)
                            {
                                ContentHelper.LoadType(field.FieldType, preload.Path, preload.Plugin);
                            }
                        }
                    }
                    #endregion

                    TypeHelper.Helper[type.Name] = type;
                }
            }
            try
            {
                files = Directory.GetFiles(Path.Combine(path, "Locations"), "*.loc", SearchOption.TopDirectoryOnly);
                foreach (String filename in files)
                {
                    using (FileStream file = new FileStream(Path.Combine(path, "Locations", filename), FileMode.Open))
                    {
                        try
                        {
                            //LocationPlacer.locations.Add(Location.FromFile(file, SpectrumGame.Game.MP));
                        }
                        catch (SerializationException)
                        {
                            continue;
                        }
                    }
                }
            }
            catch (DirectoryNotFoundException) { }
            catch (FileNotFoundException) { }
        }
    }
}
