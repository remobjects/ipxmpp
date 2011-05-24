using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemObjects.InternetPack.XMPP.Elements
{

    public enum MessageType { message, groupchat }

    public class Message : Stanza
    {
        protected override void Init()
        {
            base.Init();

            Name = "message";
        }

        public string Body
        {
            get
            {
                Element el = FindElement(Namespaces.ClientStreamNamespace, "body");
                if (el != null)
                    return el.Text;
                return null;
            }
            set
            {
                Element el = FindElement(Namespaces.ClientStreamNamespace, "body");
                if (el != null)
                    el.Text = value;
                else
                {
                    el = new UnknownElement();
                    el.Parent = this;
                    el.Name = "body";
                    el.NamespaceURI = Namespaces.ClientStreamNamespace;
                    el.Text = value;
                    Elements.Add(el);
                }
            }
        }

        public MessageType MessageType
        {
            get
            {
                string s = GetAttributeByName(null, "type");
                if (s == "groupchat")
                    return XMPP.Elements.MessageType.groupchat;
                return XMPP.Elements.MessageType.message;
            }
            set
            {
                switch (value)
                {
                    case XMPP.Elements.MessageType.groupchat: AddOrReplaceAttribute(null, "type", "groupchat"); break;
                    default: 
                        AddOrReplaceAttribute(null, "type", "message"); break;
                }
            }
        }

        public override ElementType Type
        {
            get { return ElementType.Message; }
        }
    }
}
