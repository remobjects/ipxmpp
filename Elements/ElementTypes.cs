using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemObjects.InternetPack.XMPP.Elements
{
    public enum ElementType
    {
        Unknown,
        Stream, StreamFeatures, StreamError,

        IQ, Message, Presence,

        IQError,

        StartTLS,
        StartTLSProceed,
        StartTLSFailure,

        Mechanism,
        SASLAuth,
        SASLChallenge,
        SASLResponse,
        SASLAbort,
        SASLSuccess,
        SASLFailure,
        IQBind
    }

    public static class Namespaces
    {
        public const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
        public const string StreamNamespace = "http://etherx.jabber.org/streams";
        public const string ClientStreamNamespace = "jabber:client";
        public const string ServerStreamNamespace = "jabber:server";
        public const string StartTLSNamespace = "urn:ietf:params:xml:ns:xmpp-tls";
        public const string SaslNamespace = "urn:ietf:params:xml:ns:xmpp-sasl";
        public const string StanzasNamespace = "urn:ietf:params:xml:ns:xmpp-stanzas";
        public const string XMPPStreamNamespace = "urn:ietf:params:xml:ns:xmpp-streams";
        public const string GTalkAuth = "http://www.google.com/talk/protocol/auth";
        public const string IQBindNamespace = "urn:ietf:params:xml:ns:xmpp-bind";
        public const string SessionNamespace = "urn:ietf:params:xml:ns:xmpp-session";
    }

}
