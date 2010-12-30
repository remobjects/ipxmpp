using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemObjects.InternetPack.DNS
{

    public enum DNSClass
    {
        Internet = 1
    }


    public enum DNSType
    {
        A = 1,
        AAAA = 28,
        AFSDB = 18,
        CERT = 37,
        CNAME = 5,
        DHCID = 49,
        DLV = 32769,
        DNAME = 39,
        DNSKEY = 48,
        DS = 43,
        HIP = 55,
        IPSECKEY = 45,
        KEY = 25,
        LOC = 29,
        MX = 15,
        NAPTR = 35,
        NS = 2,
        NSEC = 47,
        NSEC3 = 50,
        NSEC3PARAM = 51,
        PTR = 12,
        RRSIG = 46,
        SIG = 24,
        SOA = 6,
        SPF = 99,
        SRV = 33,
        SSHFP = 44,
        TA = 32768,
        TXT = 16,
        ANY = 255,
        AXFR = 252,
        IXFR = 251,
        OPT = 41,
        NEGATIVE = 248,
        TKEY = 249,
        TSIG = 250

    }

}
