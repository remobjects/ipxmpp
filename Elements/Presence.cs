using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemObjects.InternetPack.XMPP.Elements
{


    public class Presence : Stanza
    {
        protected override void Init()
        {
            base.Init();

            Name = "presence";
        }

        public int Priority
        {
            get
            {
                var el = FindElement(null, "priority");
                if (el == null) return 0;
                int res;
                if (Int32.TryParse(el.Text, out res))
                    return res;
                return 0;
            }
            set
            {
                var el = FindElement(null, "priority");
                if (el == null)
                {
                    el = new UnknownElement { Name = "priority" };
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
