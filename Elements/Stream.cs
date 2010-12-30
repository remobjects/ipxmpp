using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemObjects.InternetPack.XMPP.Elements
{

    public class ServerStream : Stream
    {
        protected override void Init()
        {
            base.Init();
            this.Attributes.Add(new Attribute
            {
                Name = "xmlns",
                Value = Namespaces.ServerStreamNamespace
            });
        }
    }

    public class ClientStream : Stream
    {
        protected override void Init()
        {
            base.Init();
            this.Attributes.Add(new Attribute
            {
                Name = "xmlns",
                Value = Namespaces.ClientStreamNamespace
            });
        }
    }
    public abstract class Stream : ToFromElement
    {
        public Version Version { get { return new Version(GetAttributeByName(null, "version")); } set { AddOrReplaceAttribute(null, "version", value.ToString(2)); } }

        protected override void Init()
        {
            this.Prefix = "stream";
            this.Attributes.Add(new Attribute
            {
                Prefix = "xmlns",
                Name = this.Prefix,
                Value = Namespaces.StreamNamespace
            });
            this.Version = new Version(1, 0);
        }


        public override ElementType Type
        {
            get
            {
                return ElementType.Stream;
            }
        }
    }

    public class StreamFeatures : Element
    {
        protected override void Init()
        {
            base.Init();
            NamespaceURI = Namespaces.StreamNamespace;
            Name = "features";

        }


        public override ElementType Type
        {
            get { return ElementType.StreamFeatures; }
        }
    }


    public enum StreamErrorKind
    {
        Unknown,
        BadFormat,
        BadNamespace,
        Conflict,
        ConnectionTimeout,
        HostGone,
        HostUnknown,
        ImproperAddressing,
        InternalServerError,
        InvalidFrom,
        InvalidId,
        InvalidNamespace,
        InvalidXml,
        NotAuthorized,
        PolicyViolation,
        RemoteConnectionFailed,
        ResourceConstraint,
        RestrictedXml,
        SeeOtherHost,
        SystemShutdown,
        UndefinedCondition,
        UnsupportedEncoding,
        UnsupportedStanzaType,
        UnsupportedVersion,
        XmlNotWellFormed
    }

    public class StreamError : Element
    {
        public StreamErrorKind Error { get; set; }
        public string ErrorText { get; set; }
        public string Lang { get; set; }

        private static readonly Dictionary<StreamErrorKind, string> Mapping;
        private static readonly Dictionary<string, StreamErrorKind> ReverseMapping;

        static StreamError()
        {
            Mapping = new Dictionary<StreamErrorKind, string>();
            ReverseMapping = new Dictionary<string, StreamErrorKind>();
            Mapping.Add(StreamErrorKind.BadFormat, "bad-format");
            Mapping.Add(StreamErrorKind.BadNamespace, "bad-namespace");
            Mapping.Add(StreamErrorKind.Conflict, "conflict");
            Mapping.Add(StreamErrorKind.ConnectionTimeout, "connection-timeout");
            Mapping.Add(StreamErrorKind.HostGone, "host-gone");
            Mapping.Add(StreamErrorKind.HostUnknown, "host-unknown");
            Mapping.Add(StreamErrorKind.ImproperAddressing, "improper-addressing");
            Mapping.Add(StreamErrorKind.InternalServerError, "internal-server-error");
            Mapping.Add(StreamErrorKind.InvalidFrom, "invalid-from");
            Mapping.Add(StreamErrorKind.InvalidId, "invalid-id");
            Mapping.Add(StreamErrorKind.InvalidNamespace, "invalid-namespace");
            Mapping.Add(StreamErrorKind.InvalidXml, "invalid-xml");
            Mapping.Add(StreamErrorKind.NotAuthorized, "not-authorized");
            Mapping.Add(StreamErrorKind.PolicyViolation, "policy-violation");
            Mapping.Add(StreamErrorKind.RemoteConnectionFailed, "remote-connection-failed");
            Mapping.Add(StreamErrorKind.ResourceConstraint, "resource-constraint");
            Mapping.Add(StreamErrorKind.RestrictedXml, "restricted-xml");
            Mapping.Add(StreamErrorKind.SeeOtherHost, "see-other-host");
            Mapping.Add(StreamErrorKind.SystemShutdown, "system-shutdown");
            Mapping.Add(StreamErrorKind.UndefinedCondition, "undefined-condition");
            Mapping.Add(StreamErrorKind.UnsupportedEncoding, "unsupported-encoding");
            Mapping.Add(StreamErrorKind.UnsupportedStanzaType, "unsupported-stanza-type");
            Mapping.Add(StreamErrorKind.UnsupportedVersion, "unsupported-version");
            Mapping.Add(StreamErrorKind.XmlNotWellFormed, "xml-not-well-formed");
            foreach (var el in Mapping)
                ReverseMapping.Add(el.Value, el.Key);
        }


        protected override void Init()
        {
            base.Init();
            NamespaceURI = Namespaces.StreamNamespace;
            Name = "error";
        }

        public override ElementType Type
        {
            get { return ElementType.StreamError; }
        }

       
        public override void AfterRead()
        {
            base.AfterRead();
            for (int i = 0; i < this.Elements.Count; i++)
            {
                Element el = Elements[i];
                if (el.NamespaceURI == Namespaces.XMPPStreamNamespace)
                {
                    StreamErrorKind et;
                    if (el.Name == "text")
                    {
                        ErrorText = el.Text;
                        Lang = el.GetAttributeByName("xml", "lang");
                    }
                    else if (ReverseMapping.TryGetValue(el.Name, out et))
                    {
                        Error = et;
                    }
                    else
                    {
                        Error = StreamErrorKind.Unknown;
                    }
                }
            }
        }

        public override void ToString(StringBuilder builder, WriteOptions wm)
        {
            if (Lang == null) Lang = "en-US";
            if (ErrorText == null)
            {
                switch (Error)
                {
                    case StreamErrorKind.BadFormat: Text = "the entity has sent XML that cannot be processed"; break;
                    case StreamErrorKind.BadNamespace: Text = "the entity has sent a namespace prefix that is unsupported, or has sent no namespace prefix on an element that requires such a prefix"; break;
                    case StreamErrorKind.Conflict: Text = "the server is closing the active stream for this entity because a new stream has been initiated that conflicts with the existing stream."; break;
                    case StreamErrorKind.ConnectionTimeout: Text = "the entity has not generated any traffic over the stream for some period of time"; break;
                    case StreamErrorKind.HostGone: Text = "the value of the 'to' attribute provided by the initiating entity in the stream header corresponds to a hostname that is no longer hosted by the server."; break;
                    case StreamErrorKind.HostUnknown: Text = "the value of the 'to' attribute provided by the initiating entity in the stream header does not correspond to a hostname that is hosted by the server."; break;
                    case StreamErrorKind.ImproperAddressing: Text = "a stanza sent between two servers lacks a 'to' or 'from' attribute (or the attribute has no value)."; break;
                    case StreamErrorKind.InternalServerError: Text = "the server has experienced a misconfiguration or an otherwise-undefined internal error that prevents it from servicing the stream."; break;
                    case StreamErrorKind.InvalidFrom: Text = "the JID or hostname provided in a 'from' address does not match an authorized JID or validated domain negotiated between servers via SASL or dialback, or between a client and a server via authentication and resource binding."; break;
                    case StreamErrorKind.InvalidId: Text = "the stream ID or dialback ID is invalid or does not match an ID previously provided."; break;
                    case StreamErrorKind.InvalidNamespace: Text = "the streams namespace name is something other than \"http://etherx.jabber.org/streams\" or the dialback namespace name is something other than \"jabber:server:dialback\""; break;
                    case StreamErrorKind.InvalidXml: Text = "the entity has sent invalid XML over the stream to a server that performs validation"; break;
                    case StreamErrorKind.NotAuthorized: Text = "the entity has attempted to send data before the stream has been authenticated, or otherwise is not authorized to perform an action related to stream negotiation; the receiving entity MUST NOT process the offending stanza before sending the stream error."; break;
                    case StreamErrorKind.PolicyViolation: Text = "the entity has violated some local service policy"; break;
                    case StreamErrorKind.RemoteConnectionFailed: Text = "the server is unable to properly connect to a remote entity that is required for authentication or authorization."; break;
                    case StreamErrorKind.ResourceConstraint: Text = "the server lacks the system resources necessary to service the stream."; break;
                    case StreamErrorKind.RestrictedXml: Text = "the entity has attempted to send restricted XML features such as a comment, processing instruction, DTD, entity reference, or unescaped character."; break;
                    case StreamErrorKind.SeeOtherHost: Text = "the server will not provide service to the initiating entity but is redirecting traffic to another host"; break;
                    case StreamErrorKind.SystemShutdown: Text = "the server is being shut down and all active streams are being closed"; break;
                    case StreamErrorKind.UndefinedCondition: Text = "the error condition is not one of those defined by the other conditions in this list; this error condition SHOULD be used only in conjunction with an application-specific condition."; break;
                    case StreamErrorKind.UnsupportedEncoding: Text = "the initiating entity has encoded the stream in an encoding that is not supported by the server."; break;
                    case StreamErrorKind.UnsupportedStanzaType: Text = "the initiating entity has sent a first-level child of the stream that is not supported by the server."; break;
                    case StreamErrorKind.UnsupportedVersion: Text = "the value of the 'version' attribute provided by the initiating entity in the stream header specifies a version of XMPP that is not supported by the server"; break;
                    case StreamErrorKind.XmlNotWellFormed: Text = "the initiating entity has sent XML that is not well-formed as defined by [XML]."; break;
                    default:
                        Text = "unknown error"; break;
                }
            }
            if (Error != StreamErrorKind.Unknown)
            {
                for (int i = Elements.Count - 1; i >= 0; i--)
                {
                    Element el = Elements[i];
                    if (el.NamespaceURI == NamespaceURI)
                    {
                        Elements.RemoveAt(i);
                    }
                }
                UnknownElement wel = new UnknownElement();
                wel.Name = Mapping[Error];
                wel.Attributes.Add(new Attribute { Name = "xmlns", Value = Namespaces.XMPPStreamNamespace });
                Elements.Add(wel);
                wel = new UnknownElement();
                wel.Name = "text";
                wel.Text = Text;
                wel.Attributes.Add(new Attribute { Name = "xmlns", Value = Namespaces.XMPPStreamNamespace });
                wel.Attributes.Add(new Attribute { Prefix = "xml", Name = "lang", Value = Lang });
                Elements.Add(wel);
            }

            base.ToString(builder, wm);
        }
    }

}
