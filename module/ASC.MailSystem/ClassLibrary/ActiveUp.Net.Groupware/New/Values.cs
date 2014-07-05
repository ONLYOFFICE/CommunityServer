using System;
using System.IO;
using System.Collections;
using System.Globalization;
//using System.Globalization.NumberFormatInfo;

namespace CompanyName.vObjects
{

    public class ISOTimeDuration: PropertyValueType
    {
        public long y,m,w,d,h,mn,s;
        public ISOTimeDuration(): base("ISO8601 basic representation for duration of time")
        {
        }
        public ISOTimeDuration(String td): base("ISO8601 basic representation for duration of time")
        {
            SetFromString(td);
        }
        public ISOTimeDuration(long dy,long dm,long dw,long dd,long dh,long dmn,long ds): base("ISO8601 basic representation for duration of time")
        {
            y=dy;
            m=dm;
            w=dw;
            d=dd;
            h=dh;
            mn=dmn;
            s=ds;
        }
        private long GetConcDuration(String ss,String s)
        {
            ss=ss.ToUpper();
            if(s.IndexOf(ss)==-1)return 0;
            int i=s.IndexOf(ss)-1;
            while((s[i]>='0')&&(s[i]<='9'))i--;
            s=s.Substring(i+1,s.IndexOf(ss)-i-1);
            return Convert.ToInt32(s);
        }
        public override void SetFromString(String td)
        {
            if(td.IndexOf("T")==-1)td=td+"T";
            String t=td.Substring(td.IndexOf("T"),td.Length-td.IndexOf("T"));
            td=td.Substring(0,td.IndexOf("T"));
            y=GetConcDuration("y",td);
            m=GetConcDuration("m",td);
            w=GetConcDuration("w",td);
            d=GetConcDuration("d",td);
            h=GetConcDuration("h",t);
            mn=GetConcDuration("m",t);
            s=GetConcDuration("s",t);
        }
        private String td(long v,String suf)
        {
            String ts="";
            if(v==0)return ts;
            ts=v.ToString()+suf;
            return ts;
        }
        public override String ToStringForm()
        {
            String TT="";
            if((h!=0)||(mn!=0)||(s!=0))TT="T";
            return "P"+td(y,"Y")+td(m,"M")+td(w,"W")+td(d,"D")+TT+td(h,"H")+td(mn,"M")+td(s,"S");
        }

    }


    public class ISODateTime: PropertyValueType
    {
        private DateTime dt;
        public DateTime date
        {
            get{return dt;}
        }
        public ISODateTime(): base("ISO8601 formated date and time")
        {
        }
        public ISODateTime(String sd): base("ISO8601 formated date and time")
        {
            SetFromString(sd);
        }
        public override void SetFromString(String sd)
        {
            DateTimeFormatInfo dtinf=new DateTimeFormatInfo();
            if(sd.IndexOf('Z')!=-1)sd=sd.Remove(sd.IndexOf('Z'),1);
            dt=DateTime.ParseExact(sd,"yyyyMMdd'T'HHmmss",dtinf);
        }
        public void SetDate(DateTime sd)
        {
            dt=sd;
        }
        public override String ToStringForm()
        {
            return dt.ToString("yyyyMMdd'T'HHmmss");
        }

    }


    public class TextValue: PropertyValueType
    {
        private String vl;
        public TextValue(): base("Text")
        {
        }
        public TextValue(String val): base("Text")
        {
            vl=val;
        }
        public override void SetFromString(String val)
        {
            vl=val;
        }
        public String txt
        {
            get{return vl;}
        }
        public override String ToStringForm()
        {
            return vl;
        }

    }


    public class GeographicCoordinates: PropertyValueType
    {
        private double _longitude;
        private double _latitude;
        public GeographicCoordinates(): base("Geographic Position")
        {
        }
        public GeographicCoordinates(double lon,double lat): base("Geographic Position")
        {
            latitude=lat;
            longitude=lon;
        }
        public double longitude
        {
            get{return _longitude;}
            set
            {
                if((value>180)||(value<-180))throw new InvalidGeographicPositionException();
                _longitude=value;
            }
        }
        public double latitude
        {
            get{return _latitude;}
            set
            {
                if((value>90)||(value<-90))throw new InvalidGeographicPositionException();
                _latitude=value;
            }
        }
        public override String ToStringForm()
        {
            NumberFormatInfo ni=new NumberFormatInfo();
            ni.CurrencyDecimalSeparator=".";
            return Convert.ToString(_longitude,ni)+","+Convert.ToString(_latitude,ni);
        }
        public override void SetFromString(String vl)
        {
            String lo=vl.Substring(0,vl.IndexOf(","));
            String la=vl.Remove(0,vl.IndexOf(",")+1);
            NumberFormatInfo ni=new NumberFormatInfo();
            ni.CurrencyDecimalSeparator=".";
            _longitude=Convert.ToDouble(lo,ni);
            _latitude=Convert.ToDouble(la,ni);
        }

    }


    public class TimeZoneValue: PropertyValueType
    {
        private int _nh;
        private int _nm;
        public TimeZoneValue(): base("Time Zone")
        {
            _nh=0;
            _nm=0;
        }
        public TimeZoneValue(int nh,int nm): base("Time Zone")
        {
            _nh=nh;
            _nm=nm;
        }
        public int nh
        {
            get{return _nh;}
            set{_nh=value;}
        }
        public int nm
        {
            get{return _nm;}
            set{_nm=value;}
        }
        public override String ToStringForm()
        {
            String ts="";
            int tnh=Math.Abs(_nh);
            if(nh<0)ts="-";else ts="+";
            ts=ts+(char)(tnh/10+'0')+(char)(tnh%10+'0');
            if(_nm!=0)
            {
                ts=ts+":";
                ts=ts+(char)(_nm/10+'0')+(char)(_nm%10+'0');
            }
            return ts;
        }
        public override void SetFromString(String tz)
        {
            if(tz.IndexOf(":")==-1)
            {
                _nm=0;
                _nh=Convert.ToInt32(tz);
                return;
            }
            String ho=tz.Substring(0,tz.IndexOf(":"));
            String mi=tz.Remove(0,tz.IndexOf(":")+1);
            _nh=Convert.ToInt32(ho);
            _nm=Convert.ToInt32(mi);
        }

    }


    public class VersionValue: PropertyValueType
    {
        private int _major,_minor;
        public VersionValue(): base("VERSION")
        {
            _major=1;
            _minor=0;
        }
        public VersionValue(int ma,int mi): base("VERSION")
        {
            _major=ma;
            _minor=mi;
        }
        public int major
        {
            get{return _major;}
        }
        public int minor
        {
            get{return _minor;}
        }
        public override String ToStringForm()
        {
            String ts="";
            ts=_major.ToString()+"."+_minor.ToString();
            return ts;
        }
        public override void SetFromString(String tx)
        {
            String ma=tx.Substring(0,tx.IndexOf("."));
            String mi=tx.Remove(0,tx.IndexOf(".")+1);
            _major=Convert.ToInt32(ma);
            _minor=Convert.ToInt32(mi);
        }

    }

    
    public class CategoriesValue: PropertyValueType
    {
        private ArrayList _lst;
        public CategoriesValue(): base("Categories")
        {
            _lst=new ArrayList();
        }
        public CategoriesValue(String ct): base("Categories")
        {
            SetFromString(ct);
        }

        public override void SetFromString(String ct)
        {
            _lst=new ArrayList();
            while(ct.IndexOf(";")!=-1)
            {
                _lst.Add(ct.Substring(0,ct.IndexOf(";")));
                ct=ct.Remove(0,ct.IndexOf(";")+1);
            }
            _lst.Add(ct);
        }

        public void SetNewCategory(String cn)
        {
            _lst.Add(cn);
        }
        public void RemoveCategory(String cn)
        {
            _lst.Remove(cn);
        }
        public ArrayList catlist
        {
            get{return _lst;}
        }
        public override String ToStringForm()
        {
            String ts=(String)_lst[0];
            for(int i=1;i<_lst.Count;i++)ts=ts+";"+(String)_lst[i];
            return ts;
        }
    }

    
    public class PriorityValue: PropertyValueType
    {
        private int _pr;
        public PriorityValue(): base("Priority")
        {
            _pr=0;
        }
        public PriorityValue(int pr): base("Priority")
        {
            _pr=pr;
        }
        public override void SetFromString(String tx)
        {
            _pr=Convert.ToInt32(tx);
        }

        public int priority
        {
            get{return _pr;}
            set{_pr=value;}
        }
        public override String ToStringForm()
        {
            return _pr.ToString();
        }
    }
    
    
    public class StatusValue: PropertyValueType
    {
        public enum PosVal {Accepted,NeedsAction,Sent,Tentative,Confirmed,Declined,Completed,Delegated};
        public PosVal vl;
        public StatusValue(): base("Status")
        {
            vl=PosVal.NeedsAction;
        }
        public StatusValue(PosVal pv): base("Status")
        {
            vl=pv;
        }
        public override String ToStringForm()
        {
            switch (vl)
            {
                case PosVal.Accepted:return "ACCEPTED";
                case PosVal.NeedsAction:return "NEEDS ACTION";
                case PosVal.Sent:return "SENT";
                case PosVal.Tentative:return "TENTATIVE";
                case PosVal.Confirmed:return "CONFIRMED";
                case PosVal.Declined:return "DECLINED";
                case PosVal.Completed:return "COMPLETED";
                case PosVal.Delegated:return "DELEGATED";
            }
            return null;
        }
        public override void SetFromString(String tx)
        {
            switch (tx)
            {
                case "ACCEPTED": vl=PosVal.Accepted;
                    break;
                case "NEEDS ACTION": vl=PosVal.NeedsAction;
                    break;
                case "SENT": vl=PosVal.Sent;
                    break;
                case "TENTATIVE": vl=PosVal.Tentative;
                    break;
                case "CONFIRMED": vl=PosVal.Confirmed;
                    break;
                case "DECLINED": vl=PosVal.Declined;
                    break;
                case "COMPLETED": vl=PosVal.Completed;
                    break;
                case "DELEGATED": vl=PosVal.Delegated;
                    break;
            }
        }

    }


}
