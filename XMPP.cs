using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using RemObjects.InternetPack.XMPP.Elements;
using RemObjects.InternetPack.DNS;
using System.Threading;

namespace RemObjects.InternetPack.XMPP
{

    public class XMPPException : Exception { public XMPPException(string s) : base(s) { } }
    public class TlsRequiredException : XMPPException { public TlsRequiredException(string s) : base(s) { } }

    public class SaslFailureException : XMPPException
    {
        public SaslFailureException(string s) : base(s) { }
        public SaslFailureException(SaslFailure aFailure)
            : base(aFailure.StringError)
        {
            fFailure = aFailure;
        }

        private SaslFailure fFailure;
        public SaslFailure Failure { get { return fFailure; } }
    }
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(Message aMessage)
            : base()
        {
            fMessage = aMessage;
        }

        private Message fMessage;
        public Message Message { get { return fMessage; } }
    }
    public class PresenceEventArgs : EventArgs
    {
        public PresenceEventArgs(Presence aPresence)
            : base()
        {
            fPresence = aPresence;
        }

        private Presence fPresence;
        public Presence Presence { get { return fPresence; } }
    }
    
    public class IQEventArgs : EventArgs
    {
        public IQEventArgs(IQ aIQ)
        {
            fIQ = aIQ;
        }

        private IQ fIQ;
        public IQ IQ { get { return fIQ; } }

        private bool fSkipReply;
        public bool SkipReply { get { return fSkipReply; } set { fSkipReply = value; } }
    }

    public class StreamErrorArgs : EventArgs
    {
        public StreamErrorArgs(StreamError error)
        {
            fError = error;
        }
        private StreamError fError;

        public StreamError Error { get { return fError; } }
    }
    public class StreamFeaturesArgs : EventArgs
    {
        public StreamFeaturesArgs(StreamFeatures features)
        {
            fFeatures = features;
        }
        private StreamFeatures fFeatures;

        public StreamFeatures Features { get { return fFeatures; } }
    }

    public class ErrorArgs : EventArgs
    {
        public ErrorArgs(Exception error)
        {
            fError = error;
        }
        private Exception fError;

        public Exception Error { get { return fError; } }
    }
    public class AuthenticationFailedArgs : EventArgs
    {
        public AuthenticationFailedArgs(SaslFailure failure)
        {
            fFailure = failure;
        }

        private SaslFailure fFailure;
        public SaslFailure Failure { get { return fFailure; } }
    }

    public enum State { Disconnected, Resolving, Connecting, Connected, InitializingTLS, Authenticating, Authenticated, BindingResource, CreatingSession, Active, Disconnecting }
    public enum InitTLSMode { None, IfAvailable, Always }
    public class XMPPClient: Client
    {
        #region Properties
        private TimeSpan fTimeout;
        public TimeSpan Timeout { get { return fTimeout; } set { fTimeout = value; } }
        
        private State fState;
        [Browsable(false)]
        public State State { get { return fState; }}
        public string Username { get; set; }
        public string Password { get; set; }

        public InitTLSMode InitTLS { get; set; }
        public string Domain { get; set; }

        [DefaultValue(true)]
        public bool ResolveDomain { get; set; }
        
        [DefaultValue(true)]
        public bool Authenticate { get; set; }

        [DefaultValue(null)]
        public string Resource { get; set; }
        [DefaultValue(true)]
        public bool BindResource { get; set; }
        
        [DefaultValue(true)]
        public bool CreateSession { get; set; }
        #endregion

        [DefaultValue(true)]
        public bool SendPresenceAndPriority { get; set; }
        [DefaultValue(1)]
        public int Priority { get; set; }

        #region Events
        public event EventHandler Disconnected;
        public event EventHandler Connecting;
        public event EventHandler Connected;
        public event EventHandler InitializingTLS;
        public event EventHandler InitializedTLS;
        public event EventHandler Authenticating;
        public event EventHandler Authenticated;
        public event EventHandler<AuthenticationFailedArgs> AuthenticationFailed;
        public event EventHandler Active;
        public event EventHandler CreatingSession; 
        public event EventHandler<StreamErrorArgs> StreamError;
        public event EventHandler<StreamFeaturesArgs> StreamFeatures;
        public event EventHandler<ErrorArgs> Error;
        public event EventHandler<IQEventArgs> IQ;
        public event EventHandler<MessageEventArgs> Message;
        public event EventHandler<PresenceEventArgs> Presence;

        protected void OnDisconnected() {
            if (Disconnected != null)
                Disconnected(this, EventArgs.Empty);
        }
        protected void OnConnecting() {
            if (Connecting != null)
                Connecting(this, EventArgs.Empty);
        }
        protected void OnConnected() {
            if (Connected != null)
                Connected(this, EventArgs.Empty);
        }
        protected void OnInitializingTLS() {
            if (InitializingTLS != null)
                InitializingTLS(this, EventArgs.Empty);
        }
        protected void OnInitializedTLS() {
            if (InitializedTLS != null)
                InitializedTLS(this, EventArgs.Empty);
        }
        protected void OnAuthenticating() {
            if (Authenticating != null)
                Authenticating(this, EventArgs.Empty);
        }
        protected void OnAuthenticated() {
            if (Authenticated != null) 
                Authenticated(this, EventArgs.Empty);
        }
        protected void OnAuthenticationFailed(SaslFailure aFailure)
        {
            if (AuthenticationFailed != null) {
                AuthenticationFailedArgs args = new AuthenticationFailedArgs(aFailure);
                AuthenticationFailed(this, args);
            }
        }
        protected void OnError(Exception error)
        {
            if (Error != null)
            {
                ErrorArgs args = new ErrorArgs(error);
                Error(this, args);
            }
        }
        protected void OnActive () {
            if (Active != null) {
                Active(this, EventArgs.Empty);
            }
        }
        protected void OnCreateSession()
        {
            if (CreatingSession != null)
            {
                CreatingSession(this, EventArgs.Empty);
            }
        }
        protected void OnStreamError(StreamError anError){
            if (StreamError != null) {
                StreamErrorArgs args = new StreamErrorArgs(anError);
                StreamError(this, args);
            }
        }
        protected void OnStreamFeatures(StreamFeatures aFeatures)
        {
            if (StreamFeatures != null)
            {
                StreamFeaturesArgs args = new StreamFeaturesArgs(aFeatures);
                StreamFeatures(this, args);
            }
        }
        protected bool OnIQ(IQ anIQ)
        {
            if (IQ != null)
            {
                IQEventArgs args = new IQEventArgs(anIQ);
                IQ(this, args);
                return args.SkipReply;
            }
            return false;
        }
        protected void OnMessage(Message aMessage)
        {
            if (Message != null)
            {
                MessageEventArgs args = new MessageEventArgs(aMessage);
                Message(this, args);
            }
        }
        protected void OnPresence(Presence aPresence)
        {
            if (Presence != null)
            {
                PresenceEventArgs args = new PresenceEventArgs(aPresence);
                Presence(this, args);
            }
        }
        
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (fTimer != null)
                    fTimer.Dispose();
                if (fConnection != null)
                    fConnection.Dispose();
            }
            base.Dispose(disposing);
        }
        public XMPPClient()
        {
            LoadDefaults();
        }
        private void LoadDefaults()
        {
            InitTLS = InitTLSMode.IfAvailable;
            Port = 5222;
            Priority = 1;
            SendPresenceAndPriority = true;
            BindResource = true;
            Authenticate = true;
            ResolveDomain = true;
            CreateSession = true;
            fTimeout = new TimeSpan(0, 0, 15);
        }

        private int fCounter;
        private class IQReply {
            public Action<IQ> Callback;
            public DateTime Timeout;
        }
        private Dictionary<int, IQReply> fIQReplies = new Dictionary<int, IQReply>();

        private Stream fRootElement;
        private StringBuilder sb = new StringBuilder();

        private Connection fConnection;
        public Connection Connection { get { return fConnection; } }


        private LinkedList<QueuedItem> fItems = new LinkedList<QueuedItem>();
        private volatile bool fInSending;
        private class QueuedItem
        {
            public Element Element {get; set;}
            public WriteMode Mode { get; set; }
            public Action Done { get; set; }
        }


        private Element fServerRoot;
        private List<Element> fServerElementStack = new List<Element>();
        private AsyncXmlParser parser;
        private Mechanisms fServerMechanisms;
        private System.Threading.Timer fTimer;
        private volatile Action fTimeoutCallback;

        
        private void BeginSend(Element element, WriteMode wm, Action done) 
        {
            lock (fItems) {
                
                    fItems.AddLast(new QueuedItem
                    {
                        Element = element,
                        Mode = wm,
                        Done = done
                    });
                if (!fInSending)
                    DoSendItem(element, wm, done);
            }
        }

        private void DoSendItem(Element element, WriteMode mode, Action done)
        {
            fInSending = true;
            sb.Length = 0;
            element.ToString(sb, new WriteOptions { Mode = mode, StreamPrefix = "stream" });
            byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
            try
            {
                Connection cn = fConnection;
                if (cn != null)
                    cn.BeginWrite(data, 0, data.Length, new AsyncCallback(ItemSent), done);
            }
            catch
            {
            }
        }

        private void ItemSent(IAsyncResult ar)
        {
            try
            {
                fConnection.EndWrite(ar);
                if (ar.AsyncState != null) {
                    ((Action)ar.AsyncState)();
                }
                lock (fItems)
                {
                    fItems.RemoveFirst();
                    if (fItems.Count > 0)
                    {
                        var el = fItems.First.Value;
                        DoSendItem(el.Element, el.Mode, el.Done);
                    }
                    else
                    {
                        fInSending = false;
                    }
                }
            }
            catch
            {
            }
        }

        public void Open()
        {
            if (fState != XMPP.State.Disconnected) throw new InvalidOperationException();
            if (fConnection != null) return;


            fConnection = NewConnection(this.BindingV4);
            fConnection.AsyncDisconnect += new EventHandler(ServerDisconnected);
            if (ResolveDomain)
            {
                fState = XMPP.State.Resolving;
                DNSClient cl = new DNSClient();
                cl.BeginRequest(DNSClass.Internet, DNSType.SRV, "_xmpp-client._tcp."+ Domain, false, a =>
                {
                    if (a == null || a.Type != DNSType.SRV)
                    {
                        OnError(new System.Net.Sockets.SocketException((int)System.Net.Sockets.SocketError.HostNotFound));
                        fState = XMPP.State.Disconnected;

                        OnDisconnected();
                        return;
                    }
                    this.HostName = ((DNSSRVRecord) a).TargetName;
                    this.Port = ((DNSSRVRecord)a).Port;

                    
                    BeginConnect();
                });

            }
            else
            {
                BeginConnect();
            }
        }

        private void ServerDisconnected(object sender, EventArgs e)
        {
            OnDisconnected();
            fState = XMPP.State.Disconnected;
            EndTimeout();
            Connection cn = fConnection;
            fConnection = null;
            if (cn != null) cn.Dispose();
        }

        private void BeginConnect()
        {
            fState = XMPP.State.Connecting;
            try
            {
                if (this.HostAddress == null)
                {
                    string s = HostName;
                    

                    System.Net.Dns.BeginGetHostEntry(s, a =>
                    {
                        try
                        {
                            var res = System.Net.Dns.EndGetHostEntry(a);
                            if (res == null || res.AddressList.Length < 1)
                            {
                                OnError(new System.Net.Sockets.SocketException((int)System.Net.Sockets.SocketError.HostNotFound));
                                fState = XMPP.State.Disconnected;
                                OnDisconnected();
                                return;
                            }
                            HostAddress = res.AddressList[0];
                            BeginConnect();
                        }
                        catch (Exception e)
                        {
                            OnError(e);
                            fState = XMPP.State.Disconnected;
                            OnDisconnected();
                            return;
                        }
                    }, null);
                    return;
                }
                OnConnecting();

                fConnection.BeginConnect(HostAddress, Port, a =>
                {
                    try
                    {
                        fConnection.EndConnect(a);
                        BeginStream();
                    }
                    catch (Exception e)
                    {
                        OnError(e);
                        fState = XMPP.State.Disconnected;
                        OnDisconnected();
                        return;
                    }
                }, null);
            }
            catch (Exception e)
            {
                OnError(e);
                fState = XMPP.State.Disconnected;
                OnDisconnected();
                return;
            }
        }


        private void BeginStream()
        {
            OnConnected();
            parser = new AsyncXmlParser();
            parser.Origin = fConnection;
            fState = XMPP.State.Connected;
            fServerRoot = null;
            parser.ReadXmlElementAsync(new Action<XmlParserResult>(GotData), false);

            fRootElement = new ClientStream();
            fRootElement.To = new JID(Domain);

            BeginSend(fRootElement, WriteMode.Open, null);
            BeginTimeout();
        }

        private void BeginTimeout()
        {
            BeginTimeout(() =>
            {
                OnError(new TimeoutException());
                Close();
            });
        }

        private void GotData(XmlParserResult data)
        {
            bool lStopReading = false;
            switch (data.Type)
            {
                case XmlParserResultType.Error:
                    SendStreamError(new StreamError { Error = ((XmlErrorResult)data).Error });
                    return;
                case XmlParserResultType.Node:
                    XmlNodeResult nd = (XmlNodeResult)data;
                    switch (nd.NodeType)
                    {
                        case XmlNodeType.XmlInfo: break; // who cares about that?
                        case XmlNodeType.Single:
                            if (fServerElementStack.Count == 0)
                            {
                                SendStreamError(new StreamError { Error = StreamErrorKind.InvalidXml });
                                return;
                            }
                            Element el = CreateNode(nd, fServerElementStack[fServerElementStack.Count - 1]);
                            if (this.fServerElementStack.Count <= 1)
                                lStopReading = GotFirstLevelElement(el);
                            break;
                        case XmlNodeType.Open:
                            el = CreateNode(nd, fServerElementStack.Count == 0 ? null : fServerElementStack[fServerElementStack.Count - 1]);
                            this.fServerElementStack.Add(el);
                            if (this.fServerElementStack.Count == 1)
                                GotRootLevelElement(el);

                            break;
                        case XmlNodeType.Close:
                            if (fServerElementStack.Count == 0 || !fServerElementStack[fServerElementStack.Count - 1].Matches(nd.Prefix, nd.Name))
                            {
                                SendStreamError(new StreamError { Error = StreamErrorKind.InvalidXml });
                                return;
                            }
                            el = fServerElementStack[fServerElementStack.Count - 1];
                            fServerElementStack.RemoveAt(fServerElementStack.Count - 1);
                            if (fServerElementStack.Count == 0)
                            {
                                Close();
                                break;
                            }
                            if (fServerElementStack.Count == 1)
                            {
                                lStopReading = GotFirstLevelElement(el);
                            }
                            break;
                    }
                    break;
                case XmlParserResultType.Text:
                    if (fServerElementStack.Count == 0)
                    {
                        // discard
                    }
                    else
                    {
                        fServerElementStack[fServerElementStack.Count - 1].Text += ((XmlTextResult)data).Text;
                    }
                    break;
            }
            if (!lStopReading)
                parser.ReadXmlElementAsync(new Action<XmlParserResult>(GotData), fServerElementStack.Count > 0);
        }

        private void SendStreamError(Elements.StreamError streamError)
        {
            BeginSend(streamError, WriteMode.None, new Action(Close));
        }

        private bool GotFirstLevelElement(Element el)
        {
            switch (el.Type)
            {
                case ElementType.Presence:
                    GotPresence((RemObjects.InternetPack.XMPP.Elements.Presence)el);
                    break;
                case ElementType.Message:
                    GotMessage((Elements.Message)el);
                    break;
                case ElementType.IQ:
                    GotIQ((Elements.IQ)el);
                    break;
                case ElementType.StreamError:
                    OnStreamError((Elements.StreamError)el);
                    Close();
                    break;
                case ElementType.SASLAbort:
                case ElementType.SASLAuth:
                case ElementType.SASLChallenge:
                case ElementType.SASLFailure:
                case ElementType.SASLResponse:
                case ElementType.SASLSuccess:
                    if (fState == State.Authenticating)
                        AuthReply(el);
                    break;
                case ElementType.StartTLSFailure:
                case ElementType.StartTLSProceed:
                    if (fState == State.InitializingTLS)
                        return TLSReply(el);
                    break;
                case ElementType.StreamFeatures:
                    if (fState == State.Connected || fState == XMPP.State.Authenticated)
                        GotFeatures((Elements.StreamFeatures)el);
                    break;
                    
            }
            return false;
        }

        private void GotFeatures(StreamFeatures features)
        {
            EndTimeout();
            OnStreamFeatures(features);
            if (fState == XMPP.State.Authenticated)
            {
                if (BindResource)
                {
                    fState = XMPP.State.BindingResource;

                    SendResourceBinding();
                }
                else
                {
                    fState = XMPP.State.Active; // finally!
                        
                    OnActive();
                }
                return;
            }
            fServerMechanisms = features.AuthenticationMechanisms;
            var starttls = features.StartTLS;
            if (!(fConnection is SslConnection))
            {
                switch (InitTLS)
                {
                    case InitTLSMode.Always:
                        if (starttls == null)
                        {
                            OnError(new TlsRequiredException("TLS required but not supported by server"));
                            Close();
                            return;
                        }
                        StartTLS();
                        break;
                    case InitTLSMode.IfAvailable:
                        StartTLS();
                        break;
                    default:
                        StartAuth();
                        break;
                }
            }
            else
                StartAuth();
        }

        private void StartTLS() 
        {
            fState = State.InitializingTLS;
            OnInitializingTLS();
            BeginTimeout();
            BeginSend(new StartTLS(), WriteMode.None, null);
        }
        
        private bool TLSReply(Element el) 
        {
            EndTimeout();
            if (el.Type == ElementType.StartTLSProceed)
            {
                SslOptions.TargetHostName = Domain;
                var newconn = SslOptions.CreateClientConnection(fConnection);
                BeginTimeout();
                if (((SslConnection)newconn).BeginInitializeClientConnection((a) =>
                {
                    try
                    {

                        ((SslConnection)newconn).EndInitializeClientConnection(a);
                        RestartConnection(newconn);
                    }
                    catch (Exception e)
                    {
                        OnError(e);
                        Close();
                    }
                }, null) == null)
                {
                    RestartConnection(newconn);
                }
                return true;
            }
            else
            {
                OnError(new TlsRequiredException("Server did not accept starttls request"));
                Close();
                return false;
            }
        }

        private void RestartConnection(InternetPack.Connection newconn)
        {
            OnInitializedTLS();
            fConnection = newconn;
            fState = XMPP.State.Connected;
            fServerElementStack.Clear();
            fServerMechanisms = null;
            fServerRoot = null;

            parser.Origin = fConnection;
            parser.ReadXmlElementAsync(new Action<XmlParserResult>(GotData), fServerElementStack.Count > 0);
            BeginSend(fRootElement, WriteMode.Open, null);
            BeginTimeout();
        }

        private void StartAuth() 
        {
            fState = XMPP.State.Authenticating;
            OnAuthenticating();
            if (fServerMechanisms == null || !fServerMechanisms.Items.Any(a=>String.Equals(a, "PLAIN", StringComparison.InvariantCultureIgnoreCase))) 
            {
                OnError(new XMPPException("PLAIN authentication not supported"));
                Close();
                return;
            }
            SaslAuth auth = new SaslAuth();
            auth.AddOrReplaceAttribute(null, "mechanism", "PLAIN");
            auth.AddOrReplaceAttribute(Namespaces.GTalkAuth, "client-uses-full-bind-result", "true");
            auth.Text = Convert.ToBase64String(Encoding.UTF8.GetBytes("\0"+Username+"\0"+Password));
            BeginTimeout();
            BeginSend(auth, WriteMode.None, null);
        }

        private void AuthReply(Element el)
        {
            EndTimeout();
            switch (el.Type)
            {
                case ElementType.SASLSuccess:

                    fState = XMPP.State.Authenticated;
                    fServerElementStack.Clear();
                    fServerRoot = null;
                    OnAuthenticated();
                    BeginTimeout();
                    BeginSend(fRootElement, WriteMode.Open, null);

                    break;
                case ElementType.SASLFailure:
                    OnError(new SaslFailureException((SaslFailure)el));
                    Close();
                    break;
                default:
                    OnError(new SaslFailureException("Invalid SASL reply: " + el.ToString()));
                    Close();
                    break;
            }
        }



        private void SendResourceBinding()
        {
            fState = XMPP.State.BindingResource;
            var iq = new IQ();
            iq.IQType = IQType.set;
            iq.Elements.Add(new RemObjects.InternetPack.XMPP.Elements.IQTypes.Bind {
                Resource = this.Resource });

            SendIQ(iq, a =>
            {
                if (a == null)
                {
                    OnError(new TimeoutException());
                    Close();
                    return;
                }
                if (a.IQType != IQType.result)
                {
                    OnError(new XMPPException("Error binding resource: "+a));
                    Close();
                    return;
                }
                if (CreateSession)
                {
                    SendCreateSession();
                }
                else
                {
                    fState = XMPP.State.Active;
                    OnActive();
                }
            });
            
        }

        private void SendCreateSession()
        {
            fState = XMPP.State.CreatingSession;
            OnCreateSession();
            var iq = new IQ();
            iq.To = new JID(Domain);
            iq.IQType = IQType.set;
            iq.Elements.Add(new UnknownElement {
                NamespaceURI = Namespaces.SessionNamespace,
                Name = "sesion"});
            SendIQ(iq, a => {
                if (a == null)
                {
                    OnError(new TimeoutException());
                    Close();
                    return;
                }
                if (a.IQType != IQType.result)
                {
                    OnError(new XMPPException("Error creating session: " + a));
                    Close();
                    return;
                }
                fState = XMPP.State.Active;
                OnActive();

                if (SendPresenceAndPriority)
                {
                    var pres = new Presence();
                    pres.Priority = Priority;
                    SendPresence(pres);
                }
            });
        }

        public void SendPresence(Presence presence)
        {
            BeginSend(presence, WriteMode.None, null);
        }

        public void SendMessage(Message msg)
        {
            BeginSend(msg, WriteMode.None, null);
        }

        public void SendIQ(IQ packet, Action<IQ> reply)
        {
            lock (fIQReplies)
            {
                packet.ID = "n" + fCounter.ToString();
                fIQReplies.Add(fCounter, new IQReply
                {
                    Callback = reply,
                    Timeout = DateTime.UtcNow.Add(fTimeout)
                });
                fCounter++;
                if (fIQReplies.Count == 1)
                    BeginTimeout(new Action(IQTimeoutCheck), true);
            }
            
            BeginSend(packet, WriteMode.None, null);
        }

        private void IQTimeoutCheck() 
        {
            LinkedList<KeyValuePair<int, IQReply>> lToRemove=  null;
            lock (fIQReplies)
            {
                DateTime n = DateTime.UtcNow;
                foreach (var el in fIQReplies)
                {
                    if (n > el.Value.Timeout)
                    {
                        if (lToRemove == null) lToRemove = new LinkedList<KeyValuePair<int,IQReply>>();
                        lToRemove.AddLast(el);
                    }
                }
                if (lToRemove != null)
                    foreach (var el in lToRemove)
                    {
                        fIQReplies.Remove(el.Key);
                    }
            }
            if (lToRemove != null)
            foreach (var el in lToRemove){
                if (el.Value.Callback != null)
                    el.Value.Callback(null);
            }
        }

        private void GotIQ(Elements.IQ iq)
        {
            switch (iq.IQType)
            {
                case IQType.error:
                case IQType.result:
                    int no;
                    if (iq.ID.StartsWith("n") && int.TryParse(iq.ID.Substring(1), out no))
                    {
                        IQReply repl = null;
                            
                        lock (fIQReplies)
                        {
                            if (fIQReplies.TryGetValue(no, out repl))
                            {
                                fIQReplies.Remove(no);
                                
                            }
                        }
                        if (repl.Callback != null)
                            repl.Callback(iq);
                    }
                    break; // else it's not one of ours
                default:
                    // TODO: any known iq items here
                    if (OnIQ(iq)) return;

                    Elements.IQ replyiq = new IQ();
                    replyiq.ID = iq.ID;
                    replyiq.IQType = IQType.error;

                    break;
            }
        }

        private void GotMessage(Elements.Message message)
        {
            OnMessage(message);
        }

        private void GotPresence(Elements.Presence presence)
        {
            OnPresence(presence);
        }


        private Element CreateNode(XmlNodeResult nd, Element parent)
        {
            string ens;
            if (nd.Prefix != null)
            {
                RemObjects.InternetPack.XMPP.Elements.Attribute at = nd.Attribute.FirstOrDefault(a => a.Prefix == "xmlns" && a.Name == nd.Prefix);
                if (at == null)
                {
                    Element el = parent;
                    ens = string.Empty;
                    while (el != null)
                    {
                        RemObjects.InternetPack.XMPP.Elements.Attribute els = el.Attributes.Where(a => a.Prefix == "xmlns" && a.Name == nd.Prefix).FirstOrDefault();
                        if (els != null)
                        {
                            ens = els.Value;
                            break;
                        }
                        el = el.Parent;
                    }
                }
                else
                    ens = at.Value;
            }
            else
            {
                RemObjects.InternetPack.XMPP.Elements.Attribute at = nd.Attribute.FirstOrDefault(a => a.Prefix == null && a.Name == "xmlns");
                if (at == null) 
                    ens = string.Empty;
                else
                    ens = at.Value;
            }
            Element res = null;
            switch (ens)
            {
                case Namespaces.ClientStreamNamespace:
                case Namespaces.ServerStreamNamespace:
                case "":
                    if (ens == null && parent != null && parent.Type == ElementType.IQ && nd.Name == "error")
                        res = new IQError();
                    else
                        switch (nd.Name)
                        {
                            case "iq":
                                res = new IQ();
                                break;
                            case "presence":
                                res = new Presence();
                                break;
                            case "message":
                                res = new Message();
                                break;
                        }
                    break;
                case Namespaces.StreamNamespace:
                    switch (nd.Name)
                    {
                        case "stream":

                            RemObjects.InternetPack.XMPP.Elements.Attribute att = nd.Attribute.FirstOrDefault(a => a.Prefix == null && a.Name == "xmlns");
                            if (att == null || att.Value == Namespaces.ClientStreamNamespace)
                                res = new ClientStream();
                            else
                                res = new ServerStream();
                            break;
                        case "features":
                            res = new StreamFeatures();
                            break;
                        case "error":
                            res = new StreamError();
                            break;
                    }
                    break;
                case Namespaces.StartTLSNamespace:
                    switch (nd.Name)
                    {
                        case "starttls":
                            res = new StartTLS();
                            break;
                        case "failure":
                            res = new StartTLSFailure();
                            break;
                        case "proceed":
                            res = new StartTLSProceed();
                            break;  
                    }

                    break;
                case Namespaces.SaslNamespace:
                    switch (nd.Name) {
                        case "mechanisms":
                            res = new Mechanisms();
                            break;
                        case "auth":
                            res = new SaslAuth();
                            break;
                        case "challenge":
                            res = new SaslChallenge();
                            break;
                        case "response":
                            res = new SaslResponse();
                            break;
                        case "abort":
                            res = new SaslAbort();
                            break;
                        case "success":
                            res = new SaslSuccess();
                            break;
                        case "failure":
                            res = new SaslFailure();
                            break;
                    }
                    break;
            }
            if (res == null)
            {
                res = new UnknownElement();
            }
            else
                res.Attributes.Clear(); // default ones shouldn't be here during the reading process
            if (parent != null)
            {
                res.Parent = parent;
                if (parent != fServerRoot)
                    parent.Elements.Add(res);
            }
            res.Prefix = nd.Prefix;
            res.Name = nd.Name;
            foreach (var el in nd.Attribute)
                res.Attributes.Add(el);
            return res;
        }

        private void GotRootLevelElement(Element el)
        {
            if (fServerRoot != null)
            {
                SendStreamError(new StreamError { Error = StreamErrorKind.InvalidXml });
                Close();
                return;
            }
            fServerRoot = el;
            if (fServerRoot.NamespaceURI != Namespaces.StreamNamespace || fServerRoot.GetAttributeByName(null, "xmlns") != Namespaces.ClientStreamNamespace) {
                SendStreamError(new StreamError { Error = StreamErrorKind.BadNamespace });
                return;
            }
        }


        private void TimerCallback(object o)
        {
            Action act = fTimeoutCallback;
            
            if (act != null) act();
        }

        private void BeginTimeout(Action act) { BeginTimeout(act, false); }
        private void BeginTimeout(Action act, bool aRepeat)
        {
            lock (this)
            {
                fTimeoutCallback = act;
                if (aRepeat)
                {
                    if (fTimer == null)
                    {
                        fTimer = new System.Threading.Timer(new System.Threading.TimerCallback(TimerCallback), null, (long)fTimeout.TotalMilliseconds / 2, (long)fTimeout.TotalMilliseconds / 2);
                    }
                    else
                    {
                        fTimer.Change((long)fTimeout.TotalMilliseconds / 2, (long)fTimeout.TotalMilliseconds / 2);
                    }
                }
                else
                {
                    if (fTimer == null)
                    {
                        fTimer = new System.Threading.Timer(new System.Threading.TimerCallback(TimerCallback), null, (long)fTimeout.TotalMilliseconds, System.Threading.Timeout.Infinite);
                    }
                    else
                    {
                        fTimer.Change((long)fTimeout.TotalMilliseconds, System.Threading.Timeout.Infinite);
                    }
                }
            }
        }

        private void EndTimeout()
        {
            fTimeoutCallback = null;
            fTimer.Change(0, System.Threading.Timeout.Infinite);
        }

        public void Close()
        {
            if (fConnection == null) return;
            fState = XMPP.State.Disconnecting;
            BeginTimeout(() =>
            {
                try
                {
                    Connection cn = fConnection;
                    if (cn != null) fConnection.Abort();
                }
                catch { } // ignore any errors that might occur from the socket
                fConnection = null;
                fState = XMPP.State.Disconnected;
                OnDisconnected();
            });
            BeginSend(fRootElement, WriteMode.Close, () => {
                fConnection.Close(true);
                fConnection = null;
                fState = XMPP.State.Disconnected;
                OnDisconnected();
            });
        }

    }
}
