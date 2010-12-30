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

        Mechanism,

        StartTLS,
        StartTLSProceed,
        StartTLSFailure,

        SASLAuth,
        SASLChallenge,
        SASLResponse,
        SASLAbort,
        SASLSuccess,
        SASLFailure
    }

    public static class Namespaces
    {
        public const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
        public const string StreamNamespace = "http://etherx.jabber.org/streams";
        public const string ClientStreamNamespace = "jabber:client";
        public const string ServerStreamNamespace = "jabber:server";
        public const string StartTLSNamespace = "urn:ietf:params:xml:ns:xmpp-tls";
        public const string SaslNamespace = "urn:ietf:params:xml:ns:xmpp-sasl";
        public const string XMPPStreamNamespace = "urn:ietf:params:xml:ns:xmpp-streams";
    }

}
