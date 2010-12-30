using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemObjects.InternetPack.XMPP.Elements
{

    public class Mechanisms : Element
    {
        protected override void Init()
        {
            base.Init();
            this.NamespaceURI = Namespaces.SaslNamespace;
            this.Name = "mechanisms";
        }

        public IEnumerable<string> Items
        {
            get
            {
                return Elements.Where(a => a.NamespaceURI == Namespaces.SaslNamespace && a.Name == "mechanism").Select(a => a.Text);
            }
        }

        public override ElementType Type
        {
            get { return ElementType.Mechanism; }
        }
    }

    public class SaslAuth : Element
    {
        protected override void Init()
        {
            base.Init();
            NamespaceURI = Namespaces.SaslNamespace;
            Name = "auth";
        }

        public override ElementType Type
        {
            get { return ElementType.SASLAuth; }
        }
    }
    public class SaslChallenge : Element
    {
        protected override void Init()
        {
            base.Init();
            NamespaceURI = Namespaces.SaslNamespace;
            Name = "challenge";
        }

        public override ElementType Type
        {
            get { return ElementType.SASLChallenge; }
        }
    }
    public class SaslResponse : Element
    {
        protected override void Init()
        {
            base.Init();
            NamespaceURI = Namespaces.SaslNamespace;
            Name = "response";
        }

        public override ElementType Type
        {
            get { return ElementType.SASLResponse; }
        }
    }

    public enum SaslFailureType
    {
        Aborted,
        IncorrectEncoding,
        InvalidAutzid,
        InvalidMechanism,
        MechanismTooWeak,
        NotAuthorized,
        TemporaryAuthFailure,
        Unknown
    }

    public class SaslFailure : Element
    {
        protected override void Init()
        {
            base.Init();
            NamespaceURI = Namespaces.SaslNamespace;
            Name = "failure";
        }

        public SaslFailureType FailureType
        {
            get
            {
                if (Elements.Count < 1) return SaslFailureType.Unknown;
                switch (Elements[0].Name)
                {
                    case "aborted": return SaslFailureType.Aborted;
                    case "incorrect-encoding": return SaslFailureType.IncorrectEncoding;
                    case "invalid-authzid": return SaslFailureType.InvalidAutzid;
                    case "invalid-mechanism": return SaslFailureType.InvalidMechanism;
                    case "mechanism-too-weak": return SaslFailureType.MechanismTooWeak;
                    case "not-authorized": return SaslFailureType.NotAuthorized;
                    case "temporary-auth-failure": return SaslFailureType.TemporaryAuthFailure;
                       
                }
                return SaslFailureType.Unknown;
            }
        }

        public override ElementType Type
        {
            get { return ElementType.SASLFailure; }
        }
    }
    public class SaslSuccess : Element
    {
        protected override void Init()
        {
            base.Init();
            NamespaceURI = Namespaces.SaslNamespace;
            Name = "success";
        }

        public override ElementType Type
        {
            get { return ElementType.SASLSuccess; }
        }
    }
    public class SaslAbort : Element
    {
        protected override void Init()
        {
            base.Init();
            NamespaceURI = Namespaces.SaslNamespace;
            Name = "abort";
        }

        public override ElementType Type
        {
            get { return ElementType.SASLAbort; }
        }
    }
}
