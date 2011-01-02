using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemObjects.InternetPack.XMPP.Elements
{

    public enum IQType { get, set, result, error }
    public enum IQErrorType { cancel, @continue, modify, auth, wait }
    public enum IQPredefinedError
    {
        BadRequest,
        Conflict,
        FeatureNotImplemented,
        Forbidden,
        Gone,
        InternalServerError,
        ItemNotFound,
        JidMalformed,
        NotAcceptable,
        NotAllowed,
        NotAuthorized,
        PaymentRequired,
        RecipientUnavailable,
        Redirect,
        RegistrationRequired,
        RemoteServerNotFound,
        RemoteServerTimeout,
        ResourceConstraint,
        ServiceUnavailable,
        SubscriptionRequired,
        UndefinedCondition,
        UnexpectedRequest
    }

    public class IQError : Element
    {
        public override ElementType Type
        {
            get { return ElementType.IQError; }
        }
        protected override void Init()
        {
            base.Init();
            Name = "error";
        }
        public IQErrorType ErrorType
        {
            get
            {
                string val = GetAttributeByName(null, "type");
                switch (val)
                {
                    case "continue": return IQErrorType.@continue;
                    case "modify": return IQErrorType.modify;
                    case "auth": return IQErrorType.auth;
                    case "wait": return IQErrorType.wait;
                    default:   return IQErrorType.cancel;
                }
            }
            set
            {
                switch (value)
                {
                    case IQErrorType.auth: AddOrReplaceAttribute(null, "type", "auth"); break;
                    case IQErrorType.@continue: AddOrReplaceAttribute(null, "type", "continue"); break;
                    case IQErrorType.modify: AddOrReplaceAttribute(null, "type", "modify"); break;
                    case IQErrorType.wait: AddOrReplaceAttribute(null, "type", "wait"); break;
                    default:
                        AddOrReplaceAttribute(null, "type", "cancel");break;
                }
            }
        }

        public IQPredefinedError Error
        {
            get
            {
                foreach (var el in Elements)
                {
                    if (el.NamespaceURI == Namespaces.StanzasNamespace && el.Text != "text")
                    {
                        switch (el.Name)
                        {
                            case "bad-request": return IQPredefinedError.BadRequest;
                            case "conflict": return IQPredefinedError.Conflict;
                            case "feature-not-implemented": return IQPredefinedError.FeatureNotImplemented;
                            case "forbidden": return IQPredefinedError.Forbidden;
                            case "gone": return IQPredefinedError.Gone;
                            case "internal-server-error": return IQPredefinedError.InternalServerError;
                            case "item-not-found": return IQPredefinedError.ItemNotFound;
                            case "jid-malformed": return IQPredefinedError.JidMalformed;
                            case "not-acceptable": return IQPredefinedError.NotAcceptable;
                            case "not-allowed": return IQPredefinedError.NotAllowed;
                            case "not-authorized": return IQPredefinedError.NotAuthorized;
                            case "payment-required": return IQPredefinedError.PaymentRequired;
                            case "recipient-unavailable": return IQPredefinedError.RecipientUnavailable;
                            case "redirect": return IQPredefinedError.Redirect;
                            case "registration-required": return IQPredefinedError.RegistrationRequired;
                            case "remote-server-not-found": return IQPredefinedError.RemoteServerNotFound;
                            case "remote-server-timeout": return IQPredefinedError.RemoteServerTimeout;
                            case "resource-constraint": return IQPredefinedError.ResourceConstraint;
                            case "service-unavailable": return IQPredefinedError.ServiceUnavailable;
                            case "subscription-required": return IQPredefinedError.SubscriptionRequired;
                            case "undefined-condition": return IQPredefinedError.UndefinedCondition;
                            case "unexpected-request": return IQPredefinedError.UnexpectedRequest;
                        }
                    }
                }
                return IQPredefinedError.ItemNotFound;
            }
            set
            {
                foreach (var el in Elements)
                {
                    if (el.NamespaceURI == Namespaces.StanzasNamespace && el.Text != "text")
                    {
                        Elements.Remove(el);
                        break;
                    }
                }
                var newel = new UnknownElement();
                newel.NamespaceURI = Namespaces.StanzasNamespace;
                switch (value)
                {
                    case IQPredefinedError.BadRequest: newel.Name = "bad-request"; break;
                    case IQPredefinedError.Conflict: newel.Name = "conflict"; break;
                    case IQPredefinedError.FeatureNotImplemented: newel.Name = "feature-not-implemented"; break;
                    case IQPredefinedError.Forbidden: newel.Name = "forbidden"; break;
                    case IQPredefinedError.Gone: newel.Name = "gone"; break;
                    case IQPredefinedError.InternalServerError: newel.Name = "internal-server-error"; break;
                    case IQPredefinedError.ItemNotFound: newel.Name = "item-not-found"; break;
                    case IQPredefinedError.JidMalformed: newel.Name = "jid-malformed"; break;
                    case IQPredefinedError.NotAcceptable: newel.Name = "not-acceptable"; break;
                    case IQPredefinedError.NotAllowed: newel.Name = "not-allowed"; break;
                    case IQPredefinedError.NotAuthorized: newel.Name = "not-authorized"; break;
                    case IQPredefinedError.PaymentRequired: newel.Name = "payment-required"; break;
                    case IQPredefinedError.RecipientUnavailable: newel.Name = "recipient-unavailable"; break;
                    case IQPredefinedError.Redirect: newel.Name = "redirect"; break;
                    case IQPredefinedError.RegistrationRequired: newel.Name = "registration-required"; break;
                    case IQPredefinedError.RemoteServerNotFound: newel.Name = "remote-server-not-found"; break;
                    case IQPredefinedError.RemoteServerTimeout: newel.Name = "remote-server-timeout"; break;
                    case IQPredefinedError.ResourceConstraint: newel.Name = "resource-constraint"; break;
                    case IQPredefinedError.ServiceUnavailable: newel.Name = "service-unavailable"; break;
                    case IQPredefinedError.SubscriptionRequired: newel.Name = "subscription-required"; break;
                    case IQPredefinedError.UndefinedCondition: newel.Name = "undefined-condition"; break;
                    case IQPredefinedError.UnexpectedRequest: 
                    default:
                        newel.Name = "unexpected-request"; break;
                }
                Elements.Insert(0, newel);
            }
        }
    }
    
    public class IQ : Stanza
    {
        public Element First
        {
            get
            {
                if (Elements.Count == 0)
                    return null;
                return Elements[0];
            }
        }

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

        public IQError Error
        {
            get
            {
                if (IQType == XMPP.Elements.IQType.error)
                {
                    return Elements.Where(a => a.Name == "error").FirstOrDefault() as IQError;
                }
                return null;
            }
        }

        public override ElementType Type
        {
            get { return ElementType.IQ; }
        }
    }
}
