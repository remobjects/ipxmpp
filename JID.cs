using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemObjects.InternetPack.XMPP
{
    public struct JID
    {
        public static JID FromJID(string value)
        {
            JID res = new JID();
            res.value = value;
            return res;
        }

        public JID(string domain)
            : this(null, domain, null)
        {

        }


        public JID(string name, string domain)
            : this(name, domain, "")
        {

        }

        public JID(string name, string domain, string resource)
        {
            this.value = null;
            SetValue(name, domain, resource);
        }

        private void SetValue(string name, string domain, string resource)
        {
            if (String.IsNullOrEmpty(name))
            {
                this.value = domain;
            }
            else if (String.IsNullOrEmpty(resource))
                this.value = name + "@" + domain;
            else
                this.value = name + "@" + domain + "/" + resource;

        }


        private string value;
        public string Value { get { return value; } set { this.value = value; } }

        public bool IsSet { get { return value != null; } }

        public string Name
        {
            get
            {
                int n = value.IndexOf("@");
                if (n == -1)
                    return null;
                return value.Substring(0, n);
            }
            set
            {
                SetValue(value, Domain, Resource);
            }
        }

        public string Domain
        {
            get
            {
                int n = value.IndexOf("@");
                if (n == -1)
                {
                    return value; // resource isn't valid without a user
                }
                else
                {
                    string s = value.Substring(n + 1);
                    n = s.IndexOf('/');
                    if (n == -1)
                        return s;
                    return s.Substring(0, n);
                }
            }
            set
            {
                SetValue(Name, Domain, Resource);
            }
        }

        public string NameAndDomain
        {
            get
            {
                return Name + "@" + Domain;
            }
        }

        public string Resource
        {
            get
            {
                int n = value.IndexOf('/');
                if (n == -1)
                    return null;
                return value.Substring(n + 1);
            }
            set
            {
                SetValue(Name, Domain, value);
            }
        }

        public override string ToString()
        {
            return value;
        }
    }
}
