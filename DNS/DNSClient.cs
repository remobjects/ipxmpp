using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace RemObjects.InternetPack.DNS
{
    public class DNSClient
    {
        public DNSClient()
        {
        }

        private List<IPAddress> fAddresses = new List<IPAddress>();
        public List<IPAddress> Addresses { get { return fAddresses; } }

        public void LoadSystemAddresses()
        {

            foreach (var el in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                try
                {
                    fAddresses.AddRange(el.GetIPProperties().DnsAddresses.Where(a=>a.AddressFamily == AddressFamily.InterNetwork));
                }
                catch
                {
                }
            }
        }

        private TimeSpan fTimeout = new TimeSpan(0, 0, 10);
        public TimeSpan Timeout { get { return fTimeout; } set { fTimeout = value; } }
        private Random rnd = new Random();
        private class RequestInfo
        {
            public DNSClient Owner;
            public DNSPacket Input;
            public int Index;
            public Action<DNSPacket> Callback;

            public UdpClient cl;
            public Timer timer;
            public void Start()
            {
                byte[] bl = Input.WriteTo();
                if (cl == null)
                {
                    cl = new UdpClient();
                    cl.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
                    cl.BeginReceive(new AsyncCallback(GotData), null);
                }
                timer = new Timer(Owner.Timeout.TotalMilliseconds);
                timer.AutoReset = false;
                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                timer.Enabled = true;
                cl.BeginSend(bl, bl.Length, new IPEndPoint(Owner.fAddresses[Index], 53), new AsyncCallback(Sent), null);
            }

            void Sent(IAsyncResult ar)
            {
                try
                {
                    cl.EndSend(ar);
                }
                catch
                {
                }
            }

            void timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                lock (this)
                {
                    if (timer == null)
                    {
                        return;
                    }
                    timer.Dispose();
                    timer = null;
                }
                Index++;
                if (Index >= Owner.fAddresses.Count)
                {
                    cl.Close();
                    Callback(null);
                }
                else
                {
                    Start();
                }
            }

            private void GotData(IAsyncResult ar) 
            {
                lock (this)
                {
                    if (timer == null)
                    {
                        return;
                    }
                    try
                    {
                        DNSPacket pack = new DNSPacket();
                        IPEndPoint ipd = null;

                        pack.ReadFrom(cl.EndReceive(ar, ref ipd));
                        if (pack.SequenceID != this.Input.SequenceID) return; // bad wolf
                        var tm = timer;
                        timer = null;
                        tm.Dispose();
                        cl.Close();
                        Callback(pack);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void BeginRequest(DNSPacket input, Action<DNSPacket> action)
        {
            if (fAddresses.Count == 0)
            {
                LoadSystemAddresses();
                if (fAddresses == null) throw new DNSException("No addresses available");
            }
            RequestInfo info = new RequestInfo();
            info.Input = input;
            info.Index = 0;
            info.Owner = this;
            info.Callback = action;
            input.SequenceID = (ushort)rnd.Next(ushort.MaxValue);
           
            info.Start();
        }


        public void BeginRequest(DNSQuestion question, Action<DNSResource> action) 
        {
            DNSPacket req = new DNSPacket();
            req.QueryType = DNSQueryType.Standard;
            req.RecursionDesired = true;
            req.Questions.Add(question);
            BeginRequest(req, a =>
            {
                if (a == null || a.Answers.Count != 1)
                    action(null);
                else
                    action(a.Answers[0]);
            });
        }

        public void BeginRequest(DNSClass cl, DNSType type, string name, bool clearcache, Action<DNSResource> action)
        {
            DNSQuestion q= new DNSQuestion();
            q.Class = cl;
            q.ClearCache = clearcache;
            if (!name.EndsWith("."))
                name += "."; // domain names technically end with a .
            q.Name = name;
            q.Type = type;

            BeginRequest(q, action);
        }
    }
}
