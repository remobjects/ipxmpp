using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemObjects.InternetPack.XMPP.Elements
{


    public class Message : Stanza
    {
        protected override void Init()
        {
            base.Init();

            Name = "message";
        }


        public override ElementType Type
        {
            get { return ElementType.Message; }
        }
    }
}
