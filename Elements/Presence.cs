using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemObjects.InternetPack.XMPP.Elements
{

    public enum PresenceType { 
        Available, 
        Subscribe,
        Unsubscribe,
        Subscribed,
        Unsubscribed,
        Unavailable,
        Error
    
    }

    public enum PresenceShow
    {
        Online,
        Away,
        Chat,
        Dnd,
        XA
    }
    /*
     
     presence: type attribute
subscribe
unsubscribe
subscribed
unsubscribed
unavailable
error
  
     */

    public class Presence : Stanza
    {
        protected override void Init()
        {
            base.Init();

            Name = "presence";
        }

        public PresenceType PresenceType
        {
            get
            {
                var el = GetAttributeByName(null, "type");
                if (el == null) return XMPP.Elements.PresenceType.Available;
                switch (el)
                {
                    case "subscribe":
                        return XMPP.Elements.PresenceType.Subscribe;
                        case "unsubscribe":
                        return XMPP.Elements.PresenceType.Unsubscribe;
                        case "unsubscribed":
                        return XMPP.Elements.PresenceType.Unsubscribed;
                        case "subscribed":
                        return XMPP.Elements.PresenceType.Subscribed;
                        case "unavailable":
                        return XMPP.Elements.PresenceType.Unavailable;
                        case "error":
                        return XMPP.Elements.PresenceType.Error;
                    default:
                        return XMPP.Elements.PresenceType.Available;
                }
            }
            set
            {
                switch (value)
                {
                    case XMPP.Elements.PresenceType.Available: this.RemoveAttribute(null, "type"); return;
                    case XMPP.Elements.PresenceType.Error: AddOrReplaceAttribute(null, "type", "error"); return;
                    case XMPP.Elements.PresenceType.Subscribe: AddOrReplaceAttribute(null, "type", "subscribed"); return;
                    case XMPP.Elements.PresenceType.Subscribed: AddOrReplaceAttribute(null, "type", "subscribed"); return;
                    case XMPP.Elements.PresenceType.Unavailable: AddOrReplaceAttribute(null, "type", "unavailable"); return;
                    case XMPP.Elements.PresenceType.Unsubscribe: AddOrReplaceAttribute(null, "type", "unsubscribe"); return;
                    case XMPP.Elements.PresenceType.Unsubscribed: AddOrReplaceAttribute(null, "type", "unsubscribed"); return;
                }
            }
        }

        public string Status
        {
            get
            {
                var el = FindElement(Namespaces.ClientStreamNamespace, "status");
                if (el == null) return null;
                return el.Text;
            }
            set
            {
                var el = FindElement(Namespaces.ClientStreamNamespace, "status");
                if (el == null)
                {
                    el = new UnknownElement { NamespaceURI = Namespaces.ClientStreamNamespace, Name = "status", Text = value };
                    Elements.Add(el);
                }
                else
                    el.Text = value;
            }
        }

        public PresenceShow Show
        {
            get
            {
                var el = FindElement(Namespaces.ClientStreamNamespace, "show");
                if (el == null) return PresenceShow.Online;
                switch (el.Text)
                {
                    case "": return PresenceShow.Online; 
                    case "away": return PresenceShow.Away;
                    case "chat": return PresenceShow.Chat;
                    case "dnd": return PresenceShow.Dnd; 
                    case "xa": return PresenceShow.XA; 
                    default:
                    return PresenceShow.Online;
                }
            }
            set
            {
                var el = FindElement(Namespaces.ClientStreamNamespace, "show");
                if (el == null)
                {
                    el = new UnknownElement { NamespaceURI = Namespaces.ClientStreamNamespace, Name = "status" };
                    Elements.Add(el);
                }
                switch (value)
                {
                    case PresenceShow.Away: el.Text = "away"; break;
                    case PresenceShow.Chat: el.Text = "chat"; break;
                    case PresenceShow.Dnd: el.Text = "dnd"; break;
                    case PresenceShow.XA: el.Text = "xa"; break;
                    default:
                        el.Text = ""; break;
                        
                }
            }
        }


        public int Priority
        {
            get
            {
                var el = FindElement(Namespaces.ClientStreamNamespace, "priority");
                if (el == null) return 0;
                int res;
                if (Int32.TryParse(el.Text, out res))
                    return res;
                return 0;
            }
            set
            {
                var el = FindElement(Namespaces.ClientStreamNamespace, "priority");
                if (el == null)
                {
                    el = new UnknownElement { Name = "priority", NamespaceURI = Namespaces.ClientStreamNamespace };
                    Elements.Add(el);
                }
                el.Text = value.ToString();
            }
        }

        public override ElementType Type
        {
            get { return ElementType.Presence; }
        }
    }

}
