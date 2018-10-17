using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Pets
{
    [DataContract]
    public class LocalizedString : IEquatable<LocalizedString>
    {
        private static readonly ResourceManager rm = new ResourceManager("Pets.LocalizedStrings", typeof(LocalizedString).Assembly);

        [DataMember]
        public string Code { get; private set; }

        public LocalizedString(string code)
        {
            if (code == null) throw new ArgumentNullException();
            else if (code.Length == 0) throw new ArgumentException();
            this.Code = code;
        }

        public static LocalizedString FromNullable(string s)
        {
            if (s == null || s.Length == 0) return null;
            else return new LocalizedString(s);
        }

        public override string ToString()
        {
            try
            {
                string val = rm.GetString(Code);
                if (val == null) return Code;
                return val;
            }
            catch (MissingManifestResourceException)
            {
                return Code;
            }
        }

        public bool Equals(LocalizedString other)
        {
            if (object.ReferenceEquals(other, null)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.Code == other.Code;
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as LocalizedString);
        }

        public static bool operator==(LocalizedString lhs, LocalizedString rhs)
        {
            if (object.ReferenceEquals(lhs, null))
            {
                if (object.ReferenceEquals(rhs, null))
                {
                    return true;
                }
                return false;
            }
            return lhs.Equals(rhs);
        }
        public static bool operator!=(LocalizedString a, LocalizedString b)
        {
            return !(a == b);
        }
    }
}
