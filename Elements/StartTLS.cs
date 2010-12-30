using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemObjects.InternetPack.XMPP.Elements
{

    public class StartTLSProceed : Element
    {
        protected override void Init()
        {
            base.Init();
            NamespaceURI = Namespaces.StartTLSNamespace;
            Name = "proceed";
        }

        public override ElementType Type
        {
            get { return ElementType.StartTLSProceed; }
        }
    }

    public class StartTLSFailure : Element
    {
        protected override void Init()
        {
            base.Init();
            NamespaceURI = Namespaces.StartTLSNamespace;
            Name = "failure";
        }

        public override ElementType Type
        {
            get { return ElementType.StartTLSFailure; }
        }
    }

    public class StartTLS : Element
    {
        protected override void Init()
        {
            base.Init();
            NamespaceURI = Namespaces.StartTLSNamespace;
            Name = "starttls";
        }

        public override ElementType Type
        {
            get { return ElementType.StartTLS; }
        }

        public bool Required
        {
            get
            {
                return this.FindElement(null, "required") != null;
            }
            set
            {
                Element el = this.FindElement(null, "required");
                if (value != (el != null))
                {
                    if (value)
                        this.Elements.Add(new UnknownElement { Name = "required" });
                    else
                        this.Elements.Remove(el);
                }
            }
        }


    }
}
