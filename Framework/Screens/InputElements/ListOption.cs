using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{
    public class ListOption<T> : InputElement
    {
        public int Id { get; private set; }
        public T Option { get; private set; }
        private TextElement text;

        public ListOption(int id, string text, T tag)
            : base()
        {
            this.text = new TextElement(text);
            Id = id;
            Option = tag;
        }
        public override void Initialize()
        {
            base.Initialize();
            AddElement(text);
            text.Center();
            Height.Flat = text.Height.Flat;
            Width.Relative = 1.0f;
        }
    }
}
