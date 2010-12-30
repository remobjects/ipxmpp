using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace RemObjects.InternetPack.DNS
{
    public class DNSException : Exception
    {
        public DNSException(string s) : base(s) { }
    }
    public enum DNSQueryType
    {
        Standard,
        Inverse,
        StatusRequest,
        NotUsed,
        Notify,
        UpdateRequest
    }
    public enum DNSError
    {
        NoError,
        FormatError,
        ServerFailure,
        NameError,
        NotImplemented,
        Refused,
        YZDomain,
        YXRRSet,
        NXRRSet,
        NotAuthorative,
        NotZone
    }


    public class DNSQuestion
    {
        private DNSClass fClass;
        private bool fClearCache;
        private string fName;
        private DNSType fType;

        public DNSQuestion Clone()
        {
            DNSQuestion question = new DNSQuestion();
            question.fClass = this.fClass;
            question.fClearCache = this.fClearCache;
            question.fName = this.fName;
            question.fType = this.fType;
            return question;
        }

        public void ReadFrom(BinaryReader aSrc)
        {
            this.fName = DNSPacket.ParseName(aSrc, aSrc.BaseStream);
            this.fType = (DNSType)DNSPacket.N2H(aSrc.ReadUInt16());
            ushort num = DNSPacket.N2H(aSrc.ReadUInt16());
            if (0 != (num & 0x8000))
            {
                this.fClearCache = true;
            }
            this.fClass = ((DNSClass)num) & ((DNSClass)(-32769));
        }

        public void WriteTo(BinaryWriter aDest)
        {
            DNSPacket.GenerateName(aDest, this.fName);
            aDest.Write(DNSPacket.H2N((ushort)this.fType));
            if (this.fClearCache)
            {
                aDest.Write(DNSPacket.H2N((ushort)(this.fClass | ((DNSClass)0x8000))));
            }
            else
            {
                aDest.Write(DNSPacket.H2N((ushort)this.fClass));
            }
        }

        public DNSClass Class
        {
            get
            {
                return this.fClass;
            }
            set
            {
                this.fClass = value;
            }
        }

        public bool ClearCache
        {
            get
            {
                return this.fClearCache;
            }
            set
            {
                this.fClearCache = value;
            }
        }

        public string Name
        {
            get
            {
                return this.fName;
            }
            set
            {
                this.fName = value;
            }
        }

        public int Size
        {
            get
            {
                return (this.fName.Length + 6);
            }
        }

        public DNSType Type
        {
            get
            {
                return this.fType;
            }
            set
            {
                this.fType = value;
            }
        }
    }



    public class DNSPacket
    {
        private ushort fFlags;
        private ushort fSequenceID;
        private List<DNSResource> fAdditionalRecords = new List<DNSResource>();
        private List<DNSResource> fAnswers = new List<DNSResource>();
        private List<DNSResource> fAuthorityRecords = new List<DNSResource>();
        private List<DNSQuestion> fQuestions = new List<DNSQuestion>();


        public static int CompareAuthority(IEnumerable<DNSResource> aFrom, IEnumerable<DNSResource> aTo)
        {
            DNSResource[] array = new List<DNSResource>(aFrom).ToArray();
            Array.Sort<DNSResource>(array, delegate(DNSResource a, DNSResource b)
            {
                return ((int)a.Type).CompareTo((int)b.Type);
            });
            DNSResource[] resourceArray2 = new List<DNSResource>(aTo).ToArray();
            Array.Sort<DNSResource>(resourceArray2, delegate(DNSResource a, DNSResource b)
            {
                return ((int)a.Type).CompareTo((int)b.Type);
            });


            for (int i = 0; i < Math.Min(array.Length, resourceArray2.Length); i++)
            {
                DNSResource resource = array[i];
                DNSResource resource2 = resourceArray2[i];
                int num = ((int)resource.Type).CompareTo((int)resource2.Type);
                if (num != 0)
                {
                    return num;
                }
                num = CompareBytes(resource.BodyWriteTo(), resource2.BodyWriteTo());
                if (num != 0)
                {
                    return num;
                }
            }
            return array.Length.CompareTo(resourceArray2.Length);
        }

        private static int CompareBytes(byte[] aLeft, byte[] aRight)
        {
            int num;
            int num3 = Math.Min(aLeft.Length, aRight.Length) - 1;
            int index = 0;
            if (index <= num3)
            {
                num3++;
                do
                {
                    num = aLeft[index].CompareTo(aRight[index]);
                    if (num != 0)
                    {
                        return num;
                    }
                    index++;
                }
                while (index != num3);
            }
            if (aLeft.Length > aRight.Length)
            {
                return 1;
            }
            if (aLeft.Length < aRight.Length)
            {
                return -1;
            }
            return 0;
        }

        internal static void GenerateName(BinaryWriter aDest, string aName)
        {
            if (aName != null)
            {
                var lNames = aName.Split('@', '.');
                for (int i = 0; i < lNames.Length; i++)
                {
                    var lBytes = Encoding.UTF8.GetBytes(lNames[i]);
                    if (lBytes.Length == 0) continue; // not a valid name
                    if (lBytes.Length >= 0xc0) throw new DNSException("\"" + lNames[i] + "\" dns name part too long in dns domain name \"" + aName + "\"");
                    aDest.Write((byte)lBytes.Length);
                    aDest.Write(lBytes, 0, lBytes.Length);
                }
            } aDest.Write('\0');
        }

        internal static ushort H2N(ushort W)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (ushort)(((W & 0xff00) >> 8) | ((W & 0xff) << 8));
            }
            return W;
        }

        internal static uint H2N(uint W)
        {

            if (BitConverter.IsLittleEndian)
                return ((W & 0xFF) << 24) |
                   ((W & 0xff00) << 8) |
                   ((W & 0xFF0000) >> 8) |
                   ((W & 0xff000000) >> 24);

            return W;
        }

        public void MergeCopyFrom(DNSPacket aSource)
        {
            this.Questions.AddRange(aSource.Questions);
            this.Answers.AddRange(aSource.Answers);
            this.AuthorityRecords.AddRange(aSource.AuthorityRecords);
            this.AdditionalRecords.AddRange(aSource.AdditionalRecords);
        }

        internal static ushort N2H(ushort W)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (ushort)(((W & 0xff00) >> 8) | ((W & 0xff) << 8));
            }
            return W;
        }

        internal static uint N2H(uint W)
        {
            if (BitConverter.IsLittleEndian)
            {
                return ((uint)((((W & 0xff) << 0x18) | ((W & 0xff00) << 8)) | ((W & 0xff0000) >> 8))) | ((uint)((W & -16777216) >> 0x18));
            }
            return W;
        }

        internal static string ParseName(BinaryReader aSrc, Stream aMainStream)
        {
            StringBuilder builder = new StringBuilder(0x80);
            while (true)
            {
                if (aSrc.PeekChar() < 0)
                {
                    break;
                }
                byte num = aSrc.ReadByte();
                if (num == 0)
                {
                    break;
                }
                if (num >= 0xc0)
                {
                    num = (byte)(((num & -193) << 8) | aSrc.ReadByte());
                    builder.Append(ParseName(aMainStream, (int)num));
                    return builder.ToString();
                }
                byte[] bytes = aSrc.ReadBytes((int)num);
                builder.Append(Encoding.UTF8.GetString(bytes));
                builder.Append('.');
            }
            return builder.ToString();
        }

        private static string ParseName(Stream aStream, int aPosition)
        {
            long position = aStream.Position;
            aStream.Position = aPosition;
            StringBuilder builder = new StringBuilder();
            while (true)
            {
                int num2 = aStream.ReadByte();
                if (num2 <= 0)
                {
                    break;
                }
                if (builder.Length > 0)
                {
                    builder.Append('.');
                }
                if (num2 >= 0xc0)
                {
                    num2 = ((int)((((uint)num2) & -193) << 8)) | aStream.ReadByte();
                    builder.Append(ParseName(aStream, num2));
                    aStream.Position = position;
                    return builder.ToString();
                }
                byte[] buffer = new byte[num2];
                aStream.Read(buffer, 0, buffer.Length);
                builder.Append(Encoding.UTF8.GetString(buffer));
            }
            aStream.Position = position;
            return builder.ToString();
        }

        public void ReadFrom(BinaryReader aSrc)
        {
            this.fSequenceID = N2H(aSrc.ReadUInt16());
            this.fFlags = N2H(aSrc.ReadUInt16());
            ushort num = N2H(aSrc.ReadUInt16());
            ushort num2 = N2H(aSrc.ReadUInt16());
            ushort num3 = N2H(aSrc.ReadUInt16());
            ushort num4 = N2H(aSrc.ReadUInt16());
            int num6 = num - 1;
            int num5 = 0;
            if (num5 <= num6)
            {
                num6++;
                do
                {
                    DNSQuestion item = new DNSQuestion();
                    item.ReadFrom(aSrc);
                    this.fQuestions.Add(item);
                    num5++;
                }
                while (num5 != num6);
            }
            int num7 = num2 - 1;
            num5 = 0;
            if (num5 <= num7)
            {
                num7++;
                do
                {
                    this.fAnswers.Add(DNSResource.ReadFrom(aSrc));
                    num5++;
                }
                while (num5 != num7);
            }
            int num8 = num3 - 1;
            num5 = 0;
            if (num5 <= num8)
            {
                num8++;
                do
                {
                    this.fAuthorityRecords.Add(DNSResource.ReadFrom(aSrc));
                    num5++;
                }
                while (num5 != num8);
            }
            int num9 = num4 - 1;
            num5 = 0;
            if (num5 <= num9)
            {
                num9++;
                do
                {
                    this.fAdditionalRecords.Add(DNSResource.ReadFrom(aSrc));
                    num5++;
                }
                while (num5 != num9);
            }
        }

        public void ReadFrom(byte[] aSrc)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(aSrc));
            this.ReadFrom(reader);
            reader.Close();
        }

        public byte[] WriteTo()
        {
            BinaryWriter aDest = new BinaryWriter(new MemoryStream());
            this.WriteTo(aDest);
            aDest.Flush();
            byte[] buffer = (aDest.BaseStream as MemoryStream).ToArray();
            aDest.Close();
            return buffer;
        }

        public void WriteTo(BinaryWriter aDest)
        {
            aDest.Write(H2N(this.fSequenceID));
            aDest.Write(H2N(this.fFlags));
            aDest.Write(H2N((ushort)this.fQuestions.Count));
            aDest.Write(H2N((ushort)this.fAnswers.Count));
            aDest.Write(H2N((ushort)this.fAuthorityRecords.Count));
            aDest.Write(H2N((ushort)this.fAdditionalRecords.Count));
            if (this.fQuestions != null)
            {
                foreach (DNSQuestion question in this.fQuestions)
                {
                    question.WriteTo(aDest);
                }
            }
            if (this.fAnswers != null)
            {
                foreach (DNSResource resource in this.fAnswers)
                {
                    resource.WriteTo(aDest);
                }
            }
            if (this.fAuthorityRecords != null)
            {
                foreach (DNSResource resource in this.fAuthorityRecords)
                {
                    resource.WriteTo(aDest);
                }
            }
            if (this.fAdditionalRecords != null)
            {
                foreach (DNSResource resource in this.fAdditionalRecords)
                {
                    resource.WriteTo(aDest);
                }
            }
        }

        public List<DNSResource> AdditionalRecords
        {
            get
            {
                return this.fAdditionalRecords;
            }
        }

        public List<DNSResource> Answers
        {
            get
            {
                return this.fAnswers;
            }
        }

        public bool Authorative
        {
            get
            {
                return (0 != (this.fFlags & 0x400));
            }
            set
            {
                if (value)
                {
                    this.fFlags = (ushort)(this.fFlags | 0x400);
                }
                else
                {
                    this.fFlags = (ushort)(this.fFlags & 0xfffffbff);
                }
            }
        }

        public List<DNSResource> AuthorityRecords
        {
            get
            {
                return this.fAuthorityRecords;
            }
        }

        public DNSError Error
        {
            get
            {
                return (((DNSError)this.fFlags) & (DNSError.NotZone | DNSError.Refused));
            }
            set
            {
                this.fFlags = (ushort)((this.fFlags & 0xfffffff0) | ((uint)(((int)value) & 15)));
            }
        }

        public ushort Flags
        {
            get
            {
                return this.fFlags;
            }
            set
            {
                this.fFlags = value;
            }
        }

        public DNSQueryType QueryType
        {
            get
            {
                return (((DNSQueryType)(this.fFlags >> 11)) & ((DNSQueryType)15));
            }
            set
            {
                this.fFlags = (ushort)((fFlags & ~((1 << 14) | (1 << 13) | (1 << 12) | (1 << 11))) | ((((int)value) & 0xf) << 11));
            }
        }

        public List<DNSQuestion> Questions
        {
            get
            {
                return this.fQuestions;
            }
        }

        public bool RecursionAvailable
        {
            get
            {
                return (0 != (this.fFlags & 0x80));
            }
            set
            {
                if (value)
                {
                    this.fFlags = (ushort)(this.fFlags | 0x80);
                }
                else
                {
                    this.fFlags = (ushort)(this.fFlags & 0xffffff7f);
                }
            }
        }

        public bool RecursionDesired
        {
            get
            {
                return (0 != (this.fFlags & 0x100));
            }
            set
            {
                if (value)
                {
                    this.fFlags = (ushort)(this.fFlags | 0x100);
                }
                else
                {
                    this.fFlags = (ushort)(this.fFlags & 0xfffffeff);
                }
            }
        }

        public bool Response
        {
            get
            {
                return (0 != (this.fFlags & 0x8000));
            }
            set
            {
                if (value)
                {
                    this.fFlags = (ushort)(this.fFlags | 0x8000);
                }
                else
                {
                    this.fFlags = (ushort)(this.fFlags & 0xffff7fff);
                }
            }
        }

        public ushort SequenceID
        {
            get
            {
                return this.fSequenceID;
            }
            set
            {
                this.fSequenceID = value;
            }
        }

        public int Size
        {
            get
            {
                int num = 12;
                if (this.fQuestions != null)
                {
                    foreach (DNSQuestion question in this.fQuestions)
                    {
                        num += question.Size;
                    }
                }
                if (this.fAnswers != null)
                {
                    foreach (DNSResource resource in this.fAnswers)
                    {
                        num += resource.Size;
                    }
                }
                if (this.fAuthorityRecords != null)
                {
                    foreach (DNSResource resource in this.fAuthorityRecords)
                    {
                        num += resource.Size;
                    }
                }
                if (this.fAdditionalRecords != null)
                {
                    foreach (DNSResource resource in this.fAdditionalRecords)
                    {
                        num += resource.Size;
                    }
                }
                return num;
            }
        }

        public bool Truncated
        {
            get
            {
                return (0 != (this.fFlags & 0x200));
            }
            set
            {
                if (value)
                {
                    this.fFlags = (ushort)(this.fFlags | 0x200);
                }
                else
                {
                    this.fFlags = (ushort)(this.fFlags & 0xfffffdff);
                }
            }
        }
    }

    public abstract class DNSResource
    {
        private DNSClass fClass;
        private bool fClearCache;
        private string fName;
        private DateTime fParseTime;
        private ushort fTTL;

        public DNSResource()
        {
        }

        public DNSResource(string aName, [DefaultParameterValue(DNSClass.Internet)] DNSClass aClass, [DefaultParameterValue(false)] bool aClearCache, [DefaultParameterValue((ushort)120)] ushort aTTL)
        {
            this.fName = aName;
            this.fClass = aClass;
            this.fClearCache = aClearCache;
            this.fTTL = aTTL;
        }

        public abstract void BodyReadFrom(byte[] aData, Stream aMainStream);
        public abstract byte[] BodyWriteTo();
        public abstract DNSResource Clone();
        public override bool Equals(object obj)
        {
            if (!IntEquals(obj)) return false;
            var xl = BodyWriteTo();
            var yl = ((DNSResource)obj).BodyWriteTo();
            if (xl.Length != yl.Length) return false;
            for (int i = 0; i < xl.Length; i++)
                if (xl[i] != yl[i]) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool IntEquals(object obj)
        {
            if (!(obj is DNSResource))
            {
                return false;
            }
            DNSResource resource = obj as DNSResource;
            return (((resource.fClass == this.fClass) && (resource.fName == this.fName)) && (resource.Type == this.Type));
        }

        public static DNSResource ReadFrom(BinaryReader aSrc)
        {
            DNSResource resource;
            string str = DNSPacket.ParseName(aSrc, aSrc.BaseStream);
            DNSType aType = (DNSType)DNSPacket.N2H(aSrc.ReadUInt16());
            DNSClass class2 = (DNSClass)DNSPacket.N2H(aSrc.ReadUInt16());
            bool flag = false;
            if (0 != (((ushort)class2) & 0x8000))
            {
                flag = true;
            }
            class2 &= (DNSClass)(-32769);
            uint num = DNSPacket.N2H(aSrc.ReadUInt32());
            ushort num2 = DNSPacket.N2H(aSrc.ReadUInt16());
            byte[] aData = aSrc.ReadBytes((int)num2);
            DNSType type2 = aType;
            if (type2 == DNSType.A)
            {
                resource = new DNSARecord();
            }
            else if (type2 == DNSType.AAAA)
            {
                resource = new DNSAAAARecord();
            }
            else if (type2 == DNSType.PTR)
            {
                resource = new DNSPTRRecord();
            }
            else if (type2 == DNSType.NS)
            {
                resource = new DNSNSRecord();
            }
            else if (type2 == DNSType.TXT)
            {
                resource = new DNSTXTRecord();
            }
            else if (type2 == DNSType.CNAME)
            {
                resource = new DNSCNameRecord();
            }
            else if (type2 == DNSType.SRV)
            {
                resource = new DNSSRVRecord();
            }
            else
            {
                resource = new DNSUnknownRecord(aType);
            }
            resource.fName = str;
            resource.fParseTime = DateTime.UtcNow;
            resource.fClass = class2;
            resource.fClearCache = flag;
            resource.fTTL = (ushort)num;
            resource.BodyReadFrom(aData, aSrc.BaseStream);
            return resource;
        }

        public void WriteTo(BinaryWriter aDest)
        {
            DNSPacket.GenerateName(aDest, this.fName);
            aDest.Write(DNSPacket.H2N((ushort)this.Type));
            if (this.fClearCache)
            {
                aDest.Write(DNSPacket.H2N((ushort)(this.fClass | ((DNSClass)0x8000))));
            }
            else
            {
                aDest.Write(DNSPacket.H2N((ushort)this.fClass));
            }
            byte[] buffer = this.BodyWriteTo();
            aDest.Write(DNSPacket.H2N((uint)this.fTTL));
            aDest.Write(DNSPacket.H2N((ushort)buffer.Length));
            aDest.Write(buffer, 0, buffer.Length);
        }

        public DNSClass Class
        {
            get
            {
                return this.fClass;
            }
            set
            {
                this.fClass = value;
            }
        }

        public bool ClearCache
        {
            get
            {
                return this.fClearCache;
            }
            set
            {
                this.fClearCache = value;
            }
        }

        public string Name
        {
            get
            {
                return this.fName;
            }
            set
            {
                this.fName = value;
            }
        }

        public DateTime ParseTime
        {
            get
            {
                return this.fParseTime;
            }
            set
            {
                this.fParseTime = value;
            }
        }

        public virtual int Size
        {
            get
            {
                return (this.fName.Length + 8);
            }
        }

        public ushort TTL
        {
            get
            {
                return this.fTTL;
            }
            set
            {
                this.fTTL = value;
            }
        }

        public abstract DNSType Type { get; }
    }



}
