using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace RemObjects.InternetPack.XMPP.Elements
{
    public class Attribute 
    {
        public string Name { get ;set; }
        public string Prefix { get;set; }
        public string Value { get; set; }

        public override string ToString()
        {
            if (Prefix == null)
                return Name + "='" + Value + "'";
            return Prefix +":"+Name + "='" + Value + "'";
        }
    }

    public enum WriteMode { None = 0, Close = 1, Open = 2 } // open only writes the opening tag, no children

    public class WriteOptions
    {
        public WriteMode Mode { get; set; }
        public string StreamPrefix { get; set; }
    }

    public static class StringBuilderExtensions {
        public static void UriEscape(this StringBuilder value, string s)
        {
            if (s == null) return;
            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case '&': value.Append("&amp;"); break;
                    case '<': value.Append("&lt;"); break;
                    case '>': value.Append("&gt;"); break;
                    case '\"': value.Append("&quot;"); break;
                    case '\'': value.Append("&apos;"); break;
                    default:
                        value.Append(s[i]);
                        break;
                }
            }
        }
    }



    public class UnknownElement : Element
    {
        public override ElementType Type
        {
            get
            {
                return ElementType.Unknown; 
            }
        }
    }

    public abstract class Element
    {
        private List<Attribute> fAttributes = new List<Attribute>();
        public IList<Attribute> Attributes { get { return fAttributes; } }

        private List<Element> fElements = new List<Element>();
        public IList<Element> Elements { get { return fElements; } }

        public abstract ElementType Type { get; }
        private Element fParent;
        public Element Parent { get { return fParent; } internal set { fParent = value; } }
        
        public Element(): this(null, true)
        {
        }
        public Element(Element parent) : this(parent, true) { }

        public string Text { get; set; }

        public Element(Element parent, bool callInit)
        {
            fParent = parent;
            if (callInit) 
                Init();
        }

        protected virtual void Init() { }

        public string NamespaceURI
        {
            get
            {
                if (Prefix == null)
                {
                    Element cur = this;
                    do
                    {
                        string s = cur.GetAttributeByNameAndPrefix(null, "xmlns");
                        if (s != null) return s;
                        cur = cur.Parent;
                    } while (cur != null);
                    return null; 
                }
                else
                {
                    return ResolvePrefix(Prefix);
                }
            }
            set
            {
                if (NamespaceURI != value)
                {
                    Prefix = ResolveNamespace(value);
                    if (Prefix == null) 
                        AddOrReplaceAttribute(null, "xmlns", value);
                }
            }
        }

        public string ResolvePrefix(string prefix)
        {
            if (prefix == "xml") return Namespaces.XmlNamespace;
            Element cur = this;
            do
            {
                string n = cur.GetAttributeByNameAndPrefix("xmlns", prefix);
                if (n != null) return n;
                cur = cur.Parent;
            } while (cur != null);
            return null;
        }

        public string ResolveNamespace(string @namespace)
        {
            if (@namespace == Namespaces.XmlNamespace) return "xml";
            Element cur = this;
            do
            {
                for (int i = 0; i < cur.Attributes.Count; i++)
                {
                    if (cur.Attributes[i].Prefix == "xmlns" && cur.Attributes[i].Value == @namespace)
                        return cur.Attributes[i].Name;
                }
                cur = cur.Parent;
            } while (cur != null);
            return null;
        }

        public void RegisterNamespace(string prefix, string @namespace) 
        {
            string s = ResolvePrefix(prefix);
            if (s == null)
            {
                Attributes.Add(new Attribute { Prefix = "xmlns", Name = prefix, Value = @namespace });
            }
            else if (s != @namespace) throw new InvalidOperationException("Prefix already registered");
        }

        public string GetAttributeByName(string @namespace, string name)
        {
            string pref = null;
            if (@namespace != null)
            {
                pref = ResolveNamespace(@namespace);
                if (pref == null)
                {
                    if (this.NamespaceURI != @namespace)
                        return null;
                }
            }
            for (int i = 0; i < Attributes.Count; i++)
            {
                if (Attributes[i].Prefix == pref && Attributes[i].Name == name)
                    return Attributes[i].Value;
            }
            return null;
        }

        public string GetAttributeByNameAndPrefix(string pref, string name)
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                if (Attributes[i].Prefix == pref && Attributes[i].Name == name)
                    return Attributes[i].Value;
            }
            return null;
        }



        public void AddOrReplaceAttribute(string @namespace, string name, string value)
        {
            string pref = null;
            if (@namespace != null)
            {
                pref = ResolveNamespace(@namespace);
                if (pref == null && pref != Namespaces.XmlNamespace)
                {
                    for (int i = 0; ; i++)
                    {
                        pref = "n" + i;
                        if (ResolvePrefix(pref) == null) break;
                    }
                    Attributes.Add(new Attribute
                    {
                        Name = pref,
                        Prefix = "xmlns",
                        Value = @namespace
                    });
                    Attributes.Add(new Attribute
                    {
                        Name = name,
                        Prefix = pref,
                        Value = value
                    });
                    return;
                }
            }
            for (int i = 0; i < Attributes.Count; i++)
            {
                if (Attributes[i].Prefix == null && Attributes[i].Name == name)
                {
                    Attributes[i].Value = value;
                    return;
                }
            }
            Attributes.Add(new Attribute
            {
                Name = name,
                Prefix = pref,
                Value = value
            });
        }

        public void RemoveAttribute(string @namespace, string name)
        {
            string pref = null;
            if (@namespace != null)
            {
                pref = ResolveNamespace(@namespace);
                if (pref == null) return;
            }
            for (int i = 0; i < Attributes.Count; i++)
            {
                if (Attributes[i].Prefix == pref && Attributes[i].Name == name)
                {
                    Attributes.RemoveAt(i);
                    return;
                }
            }
            
        }

        public IEnumerable<Element> FindElements(string @namespace, string name) 
        {
            return Elements.Where(a => a.Name == name && a.NamespaceURI == @namespace);
        }

        public Element FindElement(string @namespace, string name)
        {
            for (int i= 0; i < Elements.Count; i++) {
                Element el = Elements[i];
                if (el.Name == name && el.NamespaceURI == @namespace)
                    return el;
            
            }
            return null;
        }

        public void ReplaceOrRemoveElement(string @namespace, string name, Element el)
        {
            Element oldel = FindElement(@namespace, name);
            if (oldel != null)
                Elements.Remove(oldel);
            if (el != null)
            Elements.Add(el);
        }

        public string Prefix { get; set; }
        public string Name { get; set;  }

        public virtual void AfterRead() { }

        public virtual void ToString(StringBuilder builder, WriteOptions wm)
        {
            if (wm.Mode != WriteMode.Close) {
                builder.Append('<');
                if (Prefix != null) {
                    builder.Append(Prefix);
                    builder.Append(':');
                }
                builder.Append(Name);
            
                for (int i = 0; i < Attributes.Count; i++)
                {
                    builder.Append(' ');
                    Attribute at = Attributes[i];
                    if (at.Prefix != null) {
                        builder.Append(at.Prefix);
                        builder.Append(':');
                    }
                    builder.Append(at.Name);
                    builder.Append("='");
                    builder.UriEscape(at.Value);
                    builder.Append('\'');
                }
            }
            if (wm.Mode == WriteMode.Close)
            {
                builder.Append("</");
                if (Prefix != null)
                {
                    builder.Append(Prefix);
                    builder.Append(':');
                }
                builder.Append(Name);
				builder.Append(">");
            
            } else 
            if (!String.IsNullOrEmpty(Text) || Elements.Count > 0 || wm.Mode == WriteMode.Open)
            {
                builder.Append('>');
                WriteMode oldwm = wm.Mode;
                wm.Mode = WriteMode.None;
                for (int i = 0; i < Elements.Count; i++)
                {
                    Elements[i].ToString(builder, wm);
                }
                wm.Mode = oldwm;
                if (wm.Mode == WriteMode.None && Text != null)
                    builder.UriEscape(Text);
                if (wm.Mode != WriteMode.Open)
                {
                    builder.Append("</");
                    if (Prefix != null)
                    {
                        builder.Append(Prefix);
                        builder.Append(':');
                    }
                    builder.Append(Name);
                    builder.Append('>');
                }
            }
            else
            {
                builder.Append(" />");
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            WriteOptions wo = new WriteOptions();
            wo.StreamPrefix = "stream";
            ToString(sb, wo);
            return sb.ToString();
        }

        internal bool Matches(string prefix, string name)
        {
            return this.Prefix == prefix && this.Name == name;
        }
    }

    public abstract class ToFromElement : Element
    {
        public JID To { get { return JID.FromJID(GetAttributeByName(null, "to")); } set { AddOrReplaceAttribute(null, "to", value.Value); } }
        public JID From { get { return JID.FromJID(GetAttributeByName(null, "from")); } set { AddOrReplaceAttribute(null, "to", value.Value); } }
    }


    public abstract class Stanza : ToFromElement
    {
        public string ID { get { return GetAttributeByName(null, "id"); } set { AddOrReplaceAttribute(null, "id", value); } }
        public string Lang { get { return GetAttributeByName(Namespaces.XmlNamespace, "lang"); } set { AddOrReplaceAttribute(Namespaces.XmlNamespace, "lang", value); } }
    }
    
}
