using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Screens.InputElements
{
    public class ListOption<T> : InputElement
    {
        public int Id { get; set; }
        public T Option { get; set; }
        public string Text { get { return text?.Text; } set { text.Text = value; } }
        private TextElement text;

        public ListOption() : this(0, null, default(T)) { }
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
            Width.Type = SizeType.MatchParent;
        }
    }
}
