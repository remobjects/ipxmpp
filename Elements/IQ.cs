using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemObjects.InternetPack.XMPP.Elements
{

    public enum IQType { get, set, result, error }

    
    public class IQ : Stanza
    {
        public IQType IQType
        {
            get
            {
                switch (this.GetAttributeByName(null, "type"))
                {
                    case "get": return InternetPack.XMPP.Elements.IQType.get;
                    case "set": return InternetPack.XMPP.Elements.IQType.set;
                    case "result": return InternetPack.XMPP.Elements.IQType.result;
                    default:
                        return InternetPack.XMPP.Elements.IQType.error;
                }
            }
            set
            {
                switch (value) {
                    case InternetPack.XMPP.Elements.IQType.get: this.AddOrReplaceAttribute(null, "type", "get"); return;
                    case InternetPack.XMPP.Elements.IQType.set: this.AddOrReplaceAttribute(null, "type", "set"); return;
                    case InternetPack.XMPP.Elements.IQType.result: this.AddOrReplaceAttribute(null, "type", "result"); return;
                    default: 
                        this.AddOrReplaceAttribute(null, "type", "error");
                        return;
                }
                
            }
        }

        protected override void Init()
        {
            base.Init();

            Name = "iq";
        }


        public override ElementType Type
        {
            get { return ElementType.IQ; }
        }
    }
}
