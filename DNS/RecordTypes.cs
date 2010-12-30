using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;

namespace RemObjects.InternetPack.DNS
{
    public class  DNSARecord : DNSResource
    {
        private IPAddress fAddress;

        public DNSARecord()
        {
        }

        public DNSARecord(string aName, DNSClass aClass, bool aClearCache, ushort aTTL)
            : base(aName, aClass, aClearCache, aTTL)
        {
        }

        public override void BodyReadFrom(byte[] aData, Stream aMainStream)
        {
            this.fAddress = new IPAddress(aData);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override byte[] BodyWriteTo()
        {
            return this.fAddress.GetAddressBytes();
        }

        public override DNSResource Clone()
        {
            DNSARecord record = new DNSARecord();
            DNSResource resource = record;
            record.Name = this.Name;
            record.Class = this.Class;
            record.ClearCache = this.ClearCache;
            record.TTL = this.TTL;
            record.fAddress = this.fAddress;
            return resource;
        }

        public override bool Equals(object obj)
        {
            DNSARecord record = obj as DNSARecord;
            if (record != null)
            {
                return (this.IntEquals(obj) && (record.fAddress == this.fAddress));
            }
            return base.Equals(obj);
        }

        public IPAddress Address
        {
            get
            {
                return this.fAddress;
            }
            set
            {
                this.fAddress = value;
            }
        }

        public override int Size
        {
            get
            {
                return (base.Size + this.fAddress.GetAddressBytes().Length);
            }
        }

        public override DNSType Type
        {
            get
            {
                return DNSType.A;
            }
        }
    }

    public class  DNSAAAARecord : DNSResource
    {
        private IPAddress fAddress;

        public DNSAAAARecord()
        {
        }

        public DNSAAAARecord(string aName, DNSClass aClass, bool aClearCache, ushort aTTL)
            : base(aName, aClass, aClearCache, aTTL)
        {
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override void BodyReadFrom(byte[] aData, Stream aMainStream)
        {
            this.fAddress = new IPAddress(aData);
        }

        public override byte[] BodyWriteTo()
        {
            return this.fAddress.GetAddressBytes();
        }

        public override DNSResource Clone()
        {
            DNSAAAARecord record = new DNSAAAARecord();
            DNSResource resource = record;
            record.Name = this.Name;
            record.Class = this.Class;
            record.ClearCache = this.ClearCache;
            record.TTL = this.TTL;
            record.fAddress = this.fAddress;
            return resource;
        }

        public override bool Equals(object obj)
        {
            DNSAAAARecord record = obj as DNSAAAARecord;
            if (record != null)
            {
                return (this.IntEquals(obj) && (record.fAddress == this.fAddress));
            }
            return base.Equals(obj);
        }

        public IPAddress Address
        {
            get
            {
                return this.fAddress;
            }
            set
            {
                this.fAddress = value;
            }
        }

        public override int Size
        {
            get
            {
                return (base.Size + this.fAddress.GetAddressBytes().Length);
            }
        }

        public override DNSType Type
        {
            get
            {
                return DNSType.AAAA;
            }
        }
    }

    public class  DNSCNameRecord : DNSResource
    {
        private string fTargetName;

        public DNSCNameRecord()
        {
        }

        public DNSCNameRecord(string aName, DNSClass aClass, bool aClearCache, ushort aTTL)
            : base(aName, aClass, aClearCache, aTTL)
        {
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override void BodyReadFrom(byte[] aData, Stream aMainStream)
        {
            BinaryReader aSrc = new BinaryReader(new MemoryStream(aData));
            this.fTargetName = DNSPacket.ParseName(aSrc, aMainStream);
            aSrc.Close();
        }

        public override byte[] BodyWriteTo()
        {
            BinaryWriter aDest = new BinaryWriter(new MemoryStream());
            DNSPacket.GenerateName(aDest, this.fTargetName);
            aDest.Flush();
            byte[] buffer = (aDest.BaseStream as MemoryStream).ToArray();
            aDest.Close();
            return buffer;
        }

        public override DNSResource Clone()
        {
            DNSCNameRecord record = new DNSCNameRecord();
            DNSResource resource = record;
            record.Name = this.Name;
            record.Class = this.Class;
            record.ClearCache = this.ClearCache;
            record.TTL = this.TTL;
            record.fTargetName = this.fTargetName;
            return resource;
        }

        public override bool Equals(object obj)
        {
            DNSCNameRecord record = obj as DNSCNameRecord;
            if (record != null)
            {
                return (this.IntEquals(obj) && (record.fTargetName == this.fTargetName));
            }
            return base.Equals(obj);
        }

        public override int Size
        {
            get
            {
                return ((base.Size + 2) + this.Name.Length);
            }
        }

        public string TargetName
        {
            get
            {
                return this.fTargetName;
            }
            set
            {
                this.fTargetName = value;
            }
        }

        public override DNSType Type
        {
            get
            {
                return DNSType.CNAME;
            }
        }
    }


    public class  DNSNSRecord : DNSResource
    {
        private string fNSHost;

        public DNSNSRecord()
        {
        }

        public DNSNSRecord(string aName, DNSClass aClass, bool aClearCache, ushort aTTL)
            : base(aName, aClass, aClearCache, aTTL)
        {
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void BodyReadFrom(byte[] aData, Stream aMainStream)
        {
            BinaryReader aSrc = new BinaryReader(new MemoryStream(aData));
            this.fNSHost = DNSPacket.ParseName(aSrc, aMainStream);
            aSrc.Close();
        }

        public override byte[] BodyWriteTo()
        {
            BinaryWriter aDest = new BinaryWriter(new MemoryStream());
            DNSPacket.GenerateName(aDest, this.fNSHost);
            aDest.Flush();
            byte[] buffer = (aDest.BaseStream as MemoryStream).ToArray();
            aDest.Close();
            return buffer;
        }

        public override DNSResource Clone()
        {
            DNSNSRecord record = new DNSNSRecord();
            DNSResource resource = record;
            record.Name = this.Name;
            record.Class = this.Class;
            record.ClearCache = this.ClearCache;
            record.TTL = this.TTL;
            record.fNSHost = this.fNSHost;
            return resource;
        }

        public override bool Equals(object obj)
        {
            DNSNSRecord record = obj as DNSNSRecord;
            if (record != null)
            {
                return (this.IntEquals(obj) && (record.fNSHost == this.fNSHost));
            }
            return base.Equals(obj);
        }

        public string NSHost
        {
            get
            {
                return this.fNSHost;
            }
            set
            {
                this.fNSHost = value;
            }
        }

        public override int Size
        {
            get
            {
                return ((base.Size + 2) + this.Name.Length);
            }
        }

        public override DNSType Type
        {
            get
            {
                return DNSType.NS;
            }
        }
    }


    public class  DNSPTRRecord : DNSResource
    {
        private string fTargetName;

        public DNSPTRRecord()
        {
        }

        public DNSPTRRecord(string aName, DNSClass aClass, bool aClearCache, ushort aTTL)
            : base(aName, aClass, aClearCache, aTTL)
        {
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override void BodyReadFrom(byte[] aData, Stream aMainStream)
        {
            BinaryReader aSrc = new BinaryReader(new MemoryStream(aData));
            this.fTargetName = DNSPacket.ParseName(aSrc, aMainStream);
            aSrc.Close();
        }

        public override byte[] BodyWriteTo()
        {
            BinaryWriter aDest = new BinaryWriter(new MemoryStream());
            DNSPacket.GenerateName(aDest, this.fTargetName);
            aDest.Flush();
            byte[] buffer = (aDest.BaseStream as MemoryStream).ToArray();
            aDest.Close();
            return buffer;
        }

        public override DNSResource Clone()
        {
            DNSPTRRecord record = new DNSPTRRecord();
            DNSResource resource = record;
            record.Name = this.Name;
            record.Class = this.Class;
            record.ClearCache = this.ClearCache;
            record.TTL = this.TTL;
            record.fTargetName = this.fTargetName;
            return resource;
        }

        public override bool Equals(object obj)
        {
            DNSPTRRecord record = obj as DNSPTRRecord;
            if (record != null)
            {
                return (this.IntEquals(obj) && (record.fTargetName == this.fTargetName));
            }
            return base.Equals(obj);
        }

        public override int Size
        {
            get
            {
                return ((base.Size + 2) + this.Name.Length);
            }
        }

        public string TargetName
        {
            get
            {
                return this.fTargetName;
            }
            set
            {
                this.fTargetName = value;
            }
        }

        public override DNSType Type
        {
            get
            {
                return DNSType.PTR;
            }
        }
    }


    public class  DNSSRVRecord : DNSResource
    {
        private ushort fPort;
        private ushort fPriority;
        private string fTargetName;
        private ushort fWeight;

        public DNSSRVRecord()
        {
        }

        public DNSSRVRecord(string aName, DNSClass aClass, bool aClearCache, ushort aTTL)
            : base(aName, aClass, aClearCache, aTTL)
        {
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void BodyReadFrom(byte[] aData, Stream aMainStream)
        {
            BinaryReader aSrc = new BinaryReader(new MemoryStream(aData));
            this.fPriority = DNSPacket.N2H(aSrc.ReadUInt16());
            this.fWeight = DNSPacket.N2H(aSrc.ReadUInt16());
            this.fPort = DNSPacket.N2H(aSrc.ReadUInt16());
            this.fTargetName = DNSPacket.ParseName(aSrc, aMainStream);
            aSrc.Close();
        }

        public override byte[] BodyWriteTo()
        {
            BinaryWriter aDest = new BinaryWriter(new MemoryStream());
            aDest.Write(DNSPacket.H2N(this.fPriority));
            aDest.Write(DNSPacket.H2N(this.fWeight));
            aDest.Write(DNSPacket.H2N(this.fPort));
            DNSPacket.GenerateName(aDest, this.fTargetName);
            aDest.Flush();
            byte[] buffer = (aDest.BaseStream as MemoryStream).ToArray();
            aDest.Close();
            return buffer;
        }

        public override DNSResource Clone()
        {
            DNSSRVRecord record = new DNSSRVRecord();
            DNSResource resource = record;
            record.Name = this.Name;
            record.Class = this.Class;
            record.ClearCache = this.ClearCache;
            record.TTL = this.TTL;
            record.fPriority = this.fPriority;
            record.fWeight = this.fWeight;
            record.fPort = this.fPort;
            record.fTargetName = this.fTargetName;
            return resource;
        }

        public override bool Equals(object obj)
        {
            DNSSRVRecord record = obj as DNSSRVRecord;
            if (record != null)
            {
                return (this.IntEquals(obj) && (record.fTargetName == this.fTargetName));
            }
            return base.Equals(obj);
        }

        public ushort Port
        {
            get
            {
                return this.fPort;
            }
            set
            {
                this.fPort = value;
            }
        }

        public ushort Priority
        {
            get
            {
                return this.fPriority;
            }
            set
            {
                this.fPriority = value;
            }
        }

        public override int Size
        {
            get
            {
                return (((base.Size + 2) + this.fTargetName.Length) + 6);
            }
        }

        public string TargetName
        {
            get
            {
                return this.fTargetName;
            }
            set
            {
                this.fTargetName = value;
            }
        }

        public override DNSType Type
        {
            get
            {
                return DNSType.SRV;
            }
        }

        public ushort Weight
        {
            get
            {
                return this.fWeight;
            }
            set
            {
                this.fWeight = value;
            }
        }
    }


    public class  DNSTXTRecord : DNSResource
    {
        private byte[] fData;

        public DNSTXTRecord()
        {
        }

        public DNSTXTRecord(string aName, DNSClass aClass, bool aClearCache, ushort aTTL)
            : base(aName, aClass, aClearCache, aTTL)
        {
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void BodyReadFrom(byte[] aData, Stream aMainStream)
        {
            this.fData = aData;
        }

        public override byte[] BodyWriteTo()
        {
            return this.fData;
        }

        public override DNSResource Clone()
        {
            DNSTXTRecord record = new DNSTXTRecord();
            DNSResource resource = record;
            record.Name = this.Name;
            record.Class = this.Class;
            record.ClearCache = this.ClearCache;
            record.TTL = this.TTL;
            record.fData = this.fData;
            return resource;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public byte[] Data
        {
            get
            {
                return this.fData;
            }
            set
            {
                this.fData = value;
            }
        }

        public override int Size
        {
            get
            {
                return (base.Size + this.fData.Length);
            }
        }

        public override DNSType Type
        {
            get
            {
                return DNSType.TXT;
            }
        }
    }


    public class  DNSUnknownRecord : DNSResource
    {
        private byte[] fData;
        private DNSType fType;

        public DNSUnknownRecord()
        {
        }

        public DNSUnknownRecord(DNSType aType)
        {
            this.fType = aType;
        }

        public override void BodyReadFrom(byte[] aData, Stream aMainStream)
        {
            this.fData = aData;
        }

        public override byte[] BodyWriteTo()
        {
            return this.fData;
        }

        public override DNSResource Clone()
        {
            DNSUnknownRecord record = new DNSUnknownRecord();
            DNSResource resource = record;
            record.Name = this.Name;
            record.Class = this.Class;
            record.ClearCache = this.ClearCache;
            record.TTL = this.TTL;
            record.RealType = this.RealType;
            record.fData = this.fData;
            return resource;
        }

        public byte[] Data
        {
            get
            {
                return this.fData;
            }
            set
            {
                this.fData = value;
            }
        }

        public DNSType RealType
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

        public override int Size
        {
            get
            {
                return (base.Size + this.fData.Length);
            }
        }

        public override DNSType Type
        {
            get
            {
                return this.fType;
            }
        }
    }





}
