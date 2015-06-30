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
        private string path;
        public string Name { get; private set; }
        public Plugin(string name, string path)
        {
            //Monogame's content manager doesn't work with absolute paths
            Content = new ContentHelper(new ContentManager(SpectrumGame.Game.Services, Path.Combine(path, "Content")));
            Name = name;
            this.path = System.IO.Path.GetFullPath(path);
            //try
            //{
            //    files = Directory.GetFiles(Path.Combine(this.path, "Locations"), "*.loc", SearchOption.TopDirectoryOnly);
            //    foreach (String filename in files)
            //    {
            //        using (FileStream file = new FileStream(Path.Combine(this.path, "Locations", filename), FileMode.Open))
            //        {
            //            try
            //            {
            //                //LocationPlacer.locations.Add(Location.FromFile(file, SpectrumGame.Game.MP));
            //            }
            //            catch (SerializationException)
            //            {
            //                continue;
            //            }
            //        }
            //    }
            //}
            //catch (DirectoryNotFoundException) { }
            //catch (FileNotFoundException) { }
        }

        public void Initialize()
        {
            //Assembly.LoadFile requries and absolute path
            string[] files = Directory.GetFiles(path, Name + ".dll", SearchOption.TopDirectoryOnly);
            foreach (String filename in files)
            {
                Assembly coreAsm = Assembly.LoadFile(Path.Combine(path, filename));
                List<Type> coreTypes = coreAsm.GetTypes().ToList();
                LoadHelper.LoadTypes(coreTypes);
            }
        }
    }
}
