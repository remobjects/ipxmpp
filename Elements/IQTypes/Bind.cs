using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemObjects.InternetPack.XMPP.Elements.IQTypes
{
    public class Bind: Element
    {
        protected override void Init()
        {
            base.Init();
            NamespaceURI = Namespaces.IQBindNamespace;
            Name = "bind";
        }

        public string Resource
        {
            get
            {
                return Elements.Where(a => a.Prefix == null && a.Name == "resource").Select(a => a.Text).FirstOrDefault();
            }
            set
            {
                Element el = Elements.Where(a => a.Prefix == null && a.Name == "resource").FirstOrDefault();
                if (el == null)
                {
                    el = new UnknownElement();
                    el.Name = "resource";
                    el.Text = value;
                    Elements.Add(el);
                }
                else
                    el.Text = value;
            }
        }
        public override ElementType Type
        {
            get { return ElementType.IQBind; }
        }
    }
}
