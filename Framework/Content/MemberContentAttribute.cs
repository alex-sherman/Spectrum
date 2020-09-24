using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Spectrum.Framework.Content
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MemberContentAttribute : Attribute
    {
        public string Path { get; private set; }
        public MemberContentAttribute(string path)
        {
            Path = path;
        }
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ClassContentAttribute : Attribute
    {
        public string Member { get; private set; }
        public string Path { get; private set; }
        public ClassContentAttribute(string member, string path)
        {
            Member = member;
            Path = path;
        }
    }
}
