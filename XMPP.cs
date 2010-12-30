using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using RemObjects.InternetPack.XMPP.Elements;
using RemObjects.InternetPack.DNS;

namespace RemObjects.InternetPack.XMPP
{
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

    public enum State { Disconnected, Resolving, Connecting, Connected, InitializingTLS, Authenticating, Active, Disconnecting }
    public enum InitTLSMode { None, IfAvailable, Always }
    public class XMPPClient: Client
    {
#region Properties
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
        #endregion

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
        public event EventHandler<StreamErrorArgs> StreamError;
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
        protected void OnStreamError(StreamError anError){
            if (StreamError != null) {
                StreamErrorArgs args = new StreamErrorArgs(anError);
                StreamError(this, args);
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

        private void LoadDefaults()
        {
            InitTLS = InitTLSMode.IfAvailable;
            Port = 5222;
            BindResource = true;
            Authenticate = true;
            ResolveDomain = true;
        }

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

        private void BeginSend(Element element, WriteMode wm, Action done) 
        {
            lock (fItems) {
                if (fInSending)
                    fItems.AddLast(new QueuedItem
                    {
                        Element = element,
                        Mode = wm,
                        Done = done
                    });
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
                fConnection.BeginWrite(data, 0, data.Length, new AsyncCallback(ItemSent), done);
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
                    if (fItems.Count > 0)
                    {
                        var el = fItems.First.Value;
                        fItems.RemoveFirst();
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


            fConnection = SslOptions.Enabled ? SslOptions.CreateClientConnection(this.BindingV4) : ConnectionFactory.CreateClientConnection(this.BindingV4);

            if (ResolveDomain)
            {
                fState = XMPP.State.Resolving;
                DNSClient cl = new DNSClient();
                cl.BeginRequest(DNSClass.Internet, DNSType.SRV, Domain, false, a =>
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

        private void BeginConnect()
        {
            fState = XMPP.State.Connecting;
            try
            {
                if (this.HostName != null)
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

        private AsyncXmlParser parser;

        private void BeginStream()
        {
            OnConnected();
            parser = new AsyncXmlParser();
            parser.Origin = fConnection;
            fState = XMPP.State.Connected;
            parser.ReadXmlElementAsync(new Action<XmlParserResult>(GotData), false); 
                        
            fRootElement = new ClientStream();
            fRootElement.To = new JID(Domain);

            BeginSend(fRootElement, WriteMode.Open, null);
        }

        private void GotData(XmlParserResult data)
        {

            // TODO: Implement
            parser.ReadXmlElementAsync(new Action<XmlParserResult>(GotData), true);
        }

        private void BeginTimeout(Action act)
        {
            // TODO: Implement
        }

        public void Close()
        {
            if (fConnection == null) return;
            fState = XMPP.State.Disconnecting;
            BeginTimeout(() =>
            {
                try
                {
                    fConnection.Abort();
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
