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

        public override ElementType Type
        {
            get { return ElementType.Presence; }
        }
    }

}
