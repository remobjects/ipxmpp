using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RemObjects.InternetPack.XMPP.Elements
{
    public enum XmlParserResultType { Node, Text, Error }

    public abstract class XmlParserResult 
    {
        public abstract XmlParserResultType Type { get; }
        
        
        public static XmlParserResult Create(string text)
        {
            return new XmlTextResult(text);
        }

        public static XmlParserResult Create(StreamErrorKind error)
        {
            return new XmlErrorResult(error);
        }

        public static XmlParserResult Create(XmlNodeType currentNodeType, string name, List<Attribute> attributes)
        {
            int n = name.IndexOf(':');
            if (n == -1)
                return new XmlNodeResult(currentNodeType, null, name, attributes.ToArray());
            return new XmlNodeResult(currentNodeType, name.Substring(0, n), name.Substring(n + 1), attributes.ToArray());
        }
    }

    public class XmlTextResult: XmlParserResult
    {
        private string text;
        public string Text { get { return text; }}
        public override XmlParserResultType Type
        {
            get { return XmlParserResultType.Text; }
        }

        public XmlTextResult(string text)
        {
            this.text = text;
        }
    }

    public class XmlErrorResult : XmlParserResult
    {
        private StreamErrorKind error;
        public StreamErrorKind Error { get { return error; } }

        public XmlErrorResult(StreamErrorKind error)
        {
            this.error = error;
        }
        public override XmlParserResultType Type
        {
            get { return XmlParserResultType.Error; }
        }
    }

    class XmlNodeResult : XmlParserResult
    {
        private XmlNodeType nodeType;
        private string prefix;
        private string name;
        private Attribute[] attribute;

        private XmlNodeType NodeType { get { return nodeType; } }
        private string Prefix { get { return prefix; } }
        private string Name { get { return name; } }
        private Attribute[] Attribute { get { return attribute; } }



        public XmlNodeResult(XmlNodeType currentNodeType, string prefix, string name, Attribute[] attribute)
        {
            this.nodeType = currentNodeType;
            this.prefix = prefix;
            this.name = name;
            this.attribute = attribute;
        }
        public override XmlParserResultType Type
        {
            get { return XmlParserResultType.Node; }
        }
    }

    public enum XmlNodeType { Open, Close, Single, XmlInfo }

    public class AsyncXmlParser
    {
        public Connection Origin { get; set; }

        
        public void ReadXmlElementAsync(Action<XmlParserResult> result, bool allowText)
        {
            if (state != AsyncState.None) throw new InvalidOperationException("Already reading");
            if (allowText)
                state = AsyncState.AllowText;
            gotResultCallback = result;
            Read();
        }
   
        private Action<XmlParserResult> gotResultCallback;
        private AsyncState state;
        private byte[] readBuffer = new byte[1024];
        private int readBufferStart = 0;
        private int readBufferEnd = 0;
        private MemoryStream currentData = new MemoryStream();

        private bool BeginReadUntil(Func<byte, bool> read, bool clearData)
        {
            if (clearData)
                currentData.SetLength(0);
            do
            {
                while (readBufferStart < readBufferEnd)
                {
                    if (read(readBuffer[readBufferStart]))
                    {
                        return true;
                    }
                    else
                    {
                        currentData.WriteByte(readBuffer[readBufferStart]);
                        readBufferStart++;
                    }
                }
                if (Origin.Available > 0)
                {
                    readBufferStart = 0;
                    readBufferEnd = Origin.ReceiveWhatsAvailable(readBuffer, 0, readBuffer.Length);
                }
                else break;
            } while (true);
            try
            {
                readBufferStart = 0;
                Origin.BeginRead(readBuffer, 0, readBuffer.Length, new AsyncCallback(GotReadUntil), read);
            }
            catch { }
            return false;
        }

        private int ReadByte()
        {
            if (readBufferEnd - readBufferStart <= 0) return -1;
            return readBuffer[readBufferStart ++];
        }

        private void GotReadUntil(IAsyncResult ar)
        {
            try {
                readBufferEnd = Origin.EndRead(ar);
                if (readBufferEnd == 0) return;
            } catch {}
            if (BeginReadUntil(ar.AsyncState as Func<byte, bool>, false))
                Read();
        }

        private enum AsyncState
        {
            None,
            AllowText,
            GotAllowText,
            GotStartByte,
            GotSecondByte,
            GotName,
            SkipWhitespace,
            AfterWhitespaceInElement,
            WantClosingTag,
            ReadAttributeName,
            AfterWhitespaceInAttributeBeforeEqual,
            AfterWhitespaceInAttributeAfterEqual,
            ReadStringDoubleQuote,
            ReadStringSingleQuote,
        }

        private XmlNodeType currentNodeType;
        private string name;
        private string currattname;
        private List<Attribute> attributes = new List<Attribute>();

        private void Read()
        {
            try
            {
                switch (state)
                {
                    case AsyncState.AllowText:
                        state = AsyncState.GotAllowText;
                        if (BeginReadUntil(a => a == '<', true))
                        {
                            goto case AsyncState.GotAllowText;
                        }
                        return;
                    case AsyncState.GotAllowText:
                        if (this.currentData.Length > 0)
                        {
                            state = AsyncState.None;
                            gotResultCallback(XmlParserResult.Create(UnEscape(Encoding.UTF8.GetString(this.currentData.GetBuffer(), 0, (int)currentData.Length))));
                            return;
                        }

                        goto case AsyncState.GotStartByte;
                    case AsyncState.None:
                        state = AsyncState.GotStartByte;
                        if (BeginReadUntil(a => true, false)) // read until we've got something
                            goto case AsyncState.GotStartByte;
                        return;
                    case AsyncState.GotStartByte:
                        ReadByte();
                        state = AsyncState.GotSecondByte;
                        if (BeginReadUntil(a => true, false))
                            goto case AsyncState.GotSecondByte;
                        return;
                    case AsyncState.GotSecondByte:
                        currentData.SetLength(0);
                        int n = ReadByte();
                        switch (n)
                        {
                            case '?':
                                currentNodeType = XmlNodeType.XmlInfo;
                                break;
                            case '/':
                                currentNodeType = XmlNodeType.Close;
                                break;
                            default:
                                if (ValidXmlNameChar((char)n, true))
                                    currentData.WriteByte((byte)n);
                                state = AsyncState.None;
                                gotResultCallback(XmlParserResult.Create(StreamErrorKind.InvalidXml));
                                return;
                        }
                        state = AsyncState.GotName;
                        if (BeginReadUntil(a => !ValidXmlNameChar((char)a, currentData.Length == 0), false))
                            goto case AsyncState.GotName;
                        return;
                    case AsyncState.GotName:
                        name = Encoding.UTF8.GetString(this.currentData.GetBuffer(), 0, (int)currentData.Length);
                        attributes.Clear();
                        goto case AsyncState.SkipWhitespace;
                    case AsyncState.SkipWhitespace:
                        state = AsyncState.AfterWhitespaceInElement;
                        if (BeginReadUntil(a => !(a == '\r' || a == '\n' || a == ' ' || a == '\t'), true))
                            goto case AsyncState.AfterWhitespaceInElement;
                        return;
                    case AsyncState.AfterWhitespaceInElement:
                        n = ReadByte();
                        if (n == '/')
                        {
                            if (currentNodeType != XmlNodeType.Open)
                            {
                                state = AsyncState.None;
                                gotResultCallback(XmlParserResult.Create(StreamErrorKind.InvalidXml));
                                return;
                            }
                            currentNodeType = XmlNodeType.Single;
                            state = AsyncState.WantClosingTag;
                            if (BeginReadUntil(a => true, false))
                                goto case AsyncState.WantClosingTag;
                            return;
                        }
                        else if (n == '?')
                        {
                            if (currentNodeType != XmlNodeType.XmlInfo)
                            {
                                state = AsyncState.None;
                                gotResultCallback(XmlParserResult.Create(StreamErrorKind.InvalidXml));
                                return;
                            }
                            state = AsyncState.WantClosingTag;
                            if (BeginReadUntil(a => true, false))
                                goto case AsyncState.WantClosingTag;
                            return;
                        }
                        else if (n == '>')
                        {
                            if (currentNodeType == XmlNodeType.XmlInfo)
                            {
                                state = AsyncState.None;
                                gotResultCallback(XmlParserResult.Create(StreamErrorKind.InvalidXml));
                                return;
                            }
                            state = AsyncState.None;
                            gotResultCallback(XmlParserResult.Create(currentNodeType, name, attributes));
                            return;
                        }
                        // attribute:
                        if (ValidXmlNameChar((char)n, true))
                        {
                            state = AsyncState.ReadAttributeName;
                            if (BeginReadUntil(a => ValidXmlNameChar((char)a, false), true))
                                goto case AsyncState.ReadAttributeName;
                        }
                        else
                        {
                            state = AsyncState.None;
                            gotResultCallback(XmlParserResult.Create(StreamErrorKind.InvalidXml));
                        }
                        return;
                    case AsyncState.WantClosingTag:
                        if (ReadByte() != '>')
                        {
                            state = AsyncState.None;
                            gotResultCallback(XmlParserResult.Create(StreamErrorKind.InvalidXml));
                            return;
                        }
                        state = AsyncState.None;
                        gotResultCallback(XmlParserResult.Create(currentNodeType, name, attributes));
                        return;
                    case AsyncState.ReadAttributeName:
                        currattname = Encoding.UTF8.GetString(this.currentData.GetBuffer(), 0, (int)currentData.Length);
                        state = AsyncState.AfterWhitespaceInAttributeBeforeEqual;
                        if (BeginReadUntil(a => !(a == '\r' || a == '\n' || a == ' ' || a == '\t'), true))
                            goto case AsyncState.AfterWhitespaceInAttributeBeforeEqual;
                        return;
                    case AsyncState.AfterWhitespaceInAttributeBeforeEqual:
                        if (ReadByte() != '=')
                        {
                            state = AsyncState.None;
                            gotResultCallback(XmlParserResult.Create(StreamErrorKind.InvalidXml));
                            return;
                        }
                        state = AsyncState.AfterWhitespaceInAttributeAfterEqual;
                        if (BeginReadUntil(a => !(a == '\r' || a == '\n' || a == ' ' || a == '\t'), true))
                            goto case AsyncState.AfterWhitespaceInAttributeAfterEqual;
                        return;
                    case AsyncState.AfterWhitespaceInAttributeAfterEqual:
                        n = ReadByte();
                        if (n == '"')
                        {
                            state = AsyncState.ReadStringDoubleQuote;
                            if (BeginReadUntil(a => a == '"', true))
                                goto case AsyncState.ReadStringDoubleQuote;
                        }
                        else if (n == '\'')
                        {
                            state = AsyncState.ReadStringSingleQuote;
                            if (BeginReadUntil(a => a == '"', true))
                                goto case AsyncState.ReadStringSingleQuote;
                        }
                        else
                        {
                            state = AsyncState.None;
                            gotResultCallback(XmlParserResult.Create(StreamErrorKind.InvalidXml));
                        }
                        return;
                    case AsyncState.ReadStringDoubleQuote:
                    case AsyncState.ReadStringSingleQuote:
                        ReadByte();    // we don't do anything with this
                        attributes.Add(ParseAttribute(currattname, Encoding.UTF8.GetString(this.currentData.GetBuffer(), 0, (int)currentData.Length)));
                        goto case AsyncState.SkipWhitespace;
                    default: 
                         state = AsyncState.None;
                        gotResultCallback(XmlParserResult.Create(StreamErrorKind.InvalidXml)); // shouldn't happen
                        return;
                }
            }
            catch
            {
                state = AsyncState.None;
                gotResultCallback(XmlParserResult.Create(StreamErrorKind.InvalidXml));
                return;
            }

            
        }

        private Attribute ParseAttribute(string currattname, string value)
        {
            int n = currattname.IndexOf(':');
            if (n == -1)
                return new Attribute
                {
                    Name = currattname,
                    Value = UnEscape(value)
                };
            return new Attribute
            {
                Name = currattname.Substring(n + 1),
                Prefix = currattname.Substring(0, n),
                Value = UnEscape(value)
            };
        }

        private StringBuilder sb;

        private string UnEscape(string value)
        {
            if (value.IndexOf('&') == -1) return value;
            if (sb == null) sb = new StringBuilder(); else sb.Length = 0;
            for (int i = 0; i < value.Length; i++) {
                if (value[i] == '&')
                {
                    int n = value.IndexOf(';', i);
                    if (n >= 0)
                    {
                        string data = value.Substring(i + 1, n - i - 1);
                        i = n;
                        if ((data.StartsWith("x") || data.StartsWith("X")) && data.Length == 3)
                        {
                            if (!Int32.TryParse(data.Substring(1), System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.InvariantInfo, out n))
                            {
                                sb.Append(data);
                                sb.Append(';');
                            }
                            else
                                sb.Append((char)n);
                        }
                        else if ((data.StartsWith("u") || data.StartsWith("U")) && data.Length == 5)
                        {
                            if (!Int32.TryParse(data.Substring(1), System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.InvariantInfo, out n))
                            {
                                sb.Append(data);
                                sb.Append(';');
                            }
                            else
                                sb.Append((char)n);
                        }
                        else
                        {
                            switch (data)
                            {
                                case "lt": sb.Append("<"); break;
                                case "gt": sb.Append(">"); break;
                                case "amp": sb.Append("&"); break;
                                case "apos": sb.Append("'"); break;
                                case "quot": sb.Append("\""); break;
                                default:
                                    sb.Append(data);
                                    sb.Append(';');
                                    break;
                            }
                        }
                        
                    }
                    else
                        sb.Append(value[i]);
                }
                else
                    sb.Append(value[i]);
            }
            return sb.ToString();
        }

        private bool ValidXmlNameChar(char n, bool start)
        {
            if ((n >= 'a' && n <= 'z') ||
                (n >= 'A' && n <= 'A')) return true;
            if (!start && n == '.' || (n >= '0' && n <= '9') || n == ':' || n == '-' || n == '.')
                return true;
            return false;
        }
    }
}
