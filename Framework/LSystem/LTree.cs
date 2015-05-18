using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.LSystem
{
    struct LState
    {
        public float D;
        public float R1;
        public float R2;
        public LNode node;
        public LState(LNode node, float D, float R1, float R2)
        {
            this.node = node;
            this.D = D;
            this.R1 = R1;
            this.R2 = R2;
        }
    }
    public class LTree
    {
        public float D = 1;
        //Children get a D of D0*DE
        public float DS = .95f;
        public float R1 = (float)Math.PI / 8;
        public float R2 = (float)Math.PI / 2;
        public List<string> rules = new List<string>();
        private string loutput = "X";
        public LTree(params string[] rules)
        {
            this.rules = rules.ToList();
        }
        private string pickRule(Random r)
        {
            int choice = r.Next(rules.Count);
            return rules[choice];
        }
        public string Parse(int n, Random r)
        {
            loutput = "X";
            for (int i = 0; i < n; i++)
            {
                int position = loutput.Length - 1;
                while (position >= 0)
                {
                    if (loutput[position] == 'X')
                    {
                        loutput = loutput.Remove(position, 1);
                        loutput = loutput.Insert(position, pickRule(r));
                    }
                    position--;
                }
            }
            return loutput;
        }
        private LNode Generate(int n, Random r)
        {
            LNode Root;
            Stack<LState> stack = new Stack<LState>();
            Root = new LNode(D, 0, 0, Matrix.CreateTranslation(Vector3.Zero));
            LState state = new LState();
            state.D = D;
            state.R1 = 0;
            state.R2 = 0;
            state.node = Root;
            int i = 0;
            string s = Parse(n,r);
            while (i < s.Length)
            {
                switch (s[i])
                {
                    case 'X':
                    case 'F':
                        LNode next = new LNode(state.D * DS, state.R1, state.R2, state.node.Transform);
                        state.node.Children.Add(next);
                        state.node = next;
                        state.D = D;
                        state.R1 = 0;
                        state.R2 = 0;
                        break;
                    case '+':
                        state.R1 += R1;
                        break;
                    case '-':
                        state.R1 += -R1;
                        break;
                    case '*':
                        state.R2 += R2;
                        break;
                    case '!':
                        state.R2 += -R2;
                        break;
                    case '[':
                        stack.Push(state);
                        break;
                    case ']':
                        state = stack.Pop();
                        break;
                    default:
                        break;
                }
                i++;
            }
            return Root;
        }
        public LNode Generate(int n, int seed)
        {
            return Generate(n, new Random(seed));
        }
        public LNode Generate(int n)
        {
            return Generate(n, new Random());
        }
    }
}
