using System;

namespace CompanyName.vObjects
{
    /// <summary>
    /// Summary description for Properties.
    /// </summary>
    public class Description: vObjectProperty
    {
        public Description(vObject owner): base("DESCRIPTION",owner)
        {
            _val=new TextValue();
        }
        public Description(vObject owner,String descr): base("DESCRIPTION",owner)
        {
            _val=new TextValue(descr);
        }
        public void SetDescription(String descr)
        {
            ((TextValue)_val).SetFromString(descr);
        }
        
    }
    public class GeographicPosition: vObjectProperty
    {
        public GeographicPosition(vObject owner): base("GEO",owner)
        {
            _val=new GeographicCoordinates();
        }
        public GeographicPosition(vObject owner,GeographicCoordinates crd): base("GEO",owner)
        {
            _val=crd;
        }
        public void SetCoord(double longtitude,double latitude)
        {
            ((GeographicCoordinates)_val).latitude=latitude;
            ((GeographicCoordinates)_val).longitude=longtitude;
        }

    }
    public class ProductID: vObjectProperty
    {
        public ProductID(vObject owner): base("PRODID",owner)
        {
            _val=new TextValue();
        }
        public ProductID(vObject owner,TextValue prid): base("PRODID",owner)
        {
            _val=prid;
        }
        public void SetProdID(String prid)
        {
            ((TextValue)_val).SetFromString(prid);
        }
        public void SetProdID(TextValue prid)
        {
            _val=prid;
        }
    }
    public class DateTimeCreated: vObjectProperty
    {
        public DateTimeCreated(vObject owner): base("DCREATED",owner)
        {
            _val=new ISODateTime();
        }
        public DateTimeCreated(vObject owner,ISODateTime vl): base("DCREATED",owner)
        {
            _val=vl;
        }
        public DateTimeCreated(vObject owner,String sd): base("DCREATED",owner)
        {
            _val=new ISODateTime(sd);
        }
        public void SetDCreate(ISODateTime vl)
        {
            _val=vl;
        }
    }
    public class DateTimeCompleted: vObjectProperty
    {
        public DateTimeCompleted(vObject owner):base("COMPLETED",owner)
        {
            _val=new ISODateTime();
        }
        public DateTimeCompleted(vObject owner,ISODateTime dt):base("COMPLETED",owner)
        {
            _val=dt;
        }
        public DateTimeCompleted(vObject owner,String ds):base("COMPLETED",owner)
        {
            _val=new ISODateTime(ds);
        }
        public void SetDCompleted(ISODateTime ct)
        {
            _val=ct;
        }
    }
    public class LastModified: vObjectProperty
    {
        public LastModified(vObject owner): base("LAST-MODIFIED",owner)
        {
            _val=new ISODateTime();
        }
        public LastModified(vObject owner, ISODateTime Time): base("LAST-MODIFIED",owner)
        {
            _val=Time;
        }
        public void SetLastModified(ISODateTime Time)
        {
            _val=Time;
        }
    }
    public class TZone: vObjectProperty
    {
        public TZone(vObject owner): base("TZ",owner)
        {
            _val=new TimeZoneValue();
        }
        public TZone(vObject owner,TimeZoneValue tzv): base("TZ",owner)
        {
            _val=tzv;
        }
        public void SetTZ(TimeZoneValue tzv)
        {
            ((TimeZoneValue)_val).nh=tzv.nh;
            ((TimeZoneValue)_val).nm=tzv.nm;
        }
        public void SetTZ(int nh,int nm)
        {
            ((TimeZoneValue)_val).nh=nh;
            ((TimeZoneValue)_val).nm=nm;
        }
    }
    public class SpecificationVersion: vObjectProperty
    {
        public SpecificationVersion(vObject owner): base("VERSION",owner)
        {
            _val=new VersionValue();
        }
        public SpecificationVersion(vObject owner,int ma,int mi): base("VERSION",owner)
        {
            _val=new VersionValue(ma,mi);
        }
    }
    public class Categories: vObjectProperty
    {
        public Categories(vObject owner): base("CATEGORIES",owner)
        {
            _val=new CategoriesValue();
        }
        public Categories(vObject owner,CategoriesValue cv): base("CATEGORIES",owner)
        {
            _val=cv;
        }
        public Categories(vObject owner,String cv): base("CATEGORIES",owner)
        {
            _val=new CategoriesValue();
            ((CategoriesValue)_val).SetNewCategory(cv);
        }
        public void SetCategory(String ct)
        {
            ((CategoriesValue)_val).SetNewCategory(ct);
        }
        public void SetCategories(CategoriesValue ct)
        {
            _val=ct;
        }
        public void RemoveCategory(String ct)
        {
            ((CategoriesValue)_val).RemoveCategory(ct);
        }
        public bool empty()
        {
            if(((CategoriesValue)_val).catlist.Count==0)return true;
            else return false; 
        }
    }
    public class DueDateTime: vObjectProperty
    {
        public DueDateTime(vObject owner):base("DUE",owner)
        {
            _val=new ISODateTime();
        }
        public DueDateTime(vObject owner,ISODateTime dt):base("DUE",owner)
        {
            _val=dt;
        }
        public DueDateTime(vObject owner,String ds):base("DUE",owner)
        {
            _val=new ISODateTime(ds);
        }
        public void SetDue(ISODateTime ct)
        {
            _val=ct;
        }
    }

    public class EndDateTime: vObjectProperty
    {
        public EndDateTime(vObject owner):base("DTEND",owner)
        {
            _val=new ISODateTime();
        }
        public EndDateTime(vObject owner,ISODateTime dt):base("DTEND",owner)
        {
            _val=dt;
        }
        public EndDateTime(vObject owner,String ds):base("DTEND",owner)
        {
            _val=new ISODateTime(ds);
        }
        public void SetDTEnd(ISODateTime ct)
        {
            _val=ct;
        }
    }
    
    public class StartDateTime: vObjectProperty
    {
        public StartDateTime(vObject owner):base("DTSTART",owner)
        {
            _val=new ISODateTime();
        }
        public StartDateTime(vObject owner,ISODateTime dt):base("DTSTART",owner)
        {
            _val=dt;
        }
        public StartDateTime(vObject owner,String ds):base("DTSTART",owner)
        {
            _val=new ISODateTime(ds);
        }
        public void SetDTStart(ISODateTime ct)
        {
            _val=ct;
        }
    }

    public class Priority: vObjectProperty
    {
        public Priority(vObject owner):base("PRIORITY",owner)
        {
            _val=new PriorityValue();
        }
        public Priority(vObject owner,int pr):base("PRIORITY",owner)
        {
            _val=new PriorityValue(pr);
        }
        public Priority(vObject owner,PriorityValue vl):base("PRIORITY",owner)
        {
            _val=vl;
        }
        public void SetPriority(int pr)
        {
            ((PriorityValue)_val).priority=pr;
        }
        public void SetPriority(PriorityValue pr)
        {
            _val=pr;
        }
    }

    public class Summary: vObjectProperty
    {
        public Summary(vObject owner): base("SUMMARY",owner)
        {
            _val=new TextValue();
        }
        public Summary(vObject owner,String st): base("SUMMARY",owner)
        {
            _val=new TextValue(st);
        }
        public void SetSummary(String st)
        {
            ((TextValue)_val).SetFromString(st);
        }
    }
    public class ExtensionProperty: vObjectProperty
    {
        public ExtensionProperty(String pn,vObject owner): base(pn,owner)
        {
            _val=new TextValue();
        }
        public void SetValue(String tx)
        {
            ((TextValue)(_val)).SetFromString(tx);
        }
        public void SetParam(String pn,String pv)
        {
            PropertyParameter par=new PropertyParameter(pn,this);
            par.ParameterValue=pv;
            SetPar(par);
        }
        public void RemoveParam(String pn)
        {
            RemovePar(pn);
        }
        public String GetParamVal(String pn)
        {
            return GetPar(pn).ParameterValue;
        }
    }
    public class OptionalProperty: vObjectProperty
    {
        public OptionalProperty(String pn,vObject owner): base(pn,owner)
        {
            _val=new TextValue();
        }
        public void SetValue(String tx)
        {
            ((TextValue)(_val)).SetFromString(tx);
        }
        public void SetParam(String pn,String pv)
        {
            PropertyParameter par=new PropertyParameter(pn,this);
            par.ParameterValue=pv;
            SetPar(par);
        }
        public void RemoveParam(String pn)
        {
            RemovePar(pn);
        }
        public String GetParamVal(String pn)
        {
            return GetPar(pn).ParameterValue;
        }
    }
    public class Status: vObjectProperty
    {
        public Status(vObject owner): base("STATUS",owner)
        {
            _val=new StatusValue();
        }
        public Status(vObject owner,StatusValue.PosVal vl): base("STATUS",owner)
        {
            _val=new StatusValue(vl);
        }
        public void SetStatus(StatusValue.PosVal vl)
        {
            ((StatusValue)(_val)).vl=vl;
        }
    }
    public class NameProperty: vObjectProperty
    {
        public NameProperty(vObject owner): base("N",owner)
        {
            _val=new TextValue();
        }
        public NameProperty(vObject owner,String nm): base("N",owner)
        {
            _val=new TextValue(nm);
        }
        public void SetName(String nm)
        {
            ((TextValue)_val).SetFromString(nm);
        }
    }
}
