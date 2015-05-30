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
                LoadHelper.LoadTypes(coreTypes);
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
