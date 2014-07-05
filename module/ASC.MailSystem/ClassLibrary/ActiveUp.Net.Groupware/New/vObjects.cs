using System;
using System.IO;
using System.Collections;

namespace CompanyName.vObjects
{
    /// <summary>
    /// Summary description for vObjects.
    /// </summary>
    public class vCalendar: vObject
    {
        private ArrayList _Entities;
        public vCalendar(): base("VCalendar")
        {
            this.
            _Entities=new ArrayList();
        }
        public override String ToStringForm()
        {
            String ts;
            ts="BEGIN:"+ObjectName+"\r\n";
            ts=ts+base.ToStringForm();                        // write all object's properties
            foreach(vObject ob in _Entities)            // write all nested objects
                ts=ts+ob.ToStringForm();
            ts=ts+"END:"+ObjectName;
            return ts;
        }

        public override void ParseFromStream(StreamReader sr)
        {
            String tkn="";
            while(true)
            {
                tkn=Parser.GetToken(sr);
                if(tkn=="END")
                {
                    tkn=Parser.GetToken(sr);
                    if(tkn!=":")throw new InvalidDocumentStructureExeption();
                    tkn=Parser.GetToken(sr);
                    if(tkn!="VCALENDAR")throw new InvalidDocumentStructureExeption();
                    return ;
                }

                vObjectProperty pr=null;
                switch(tkn)
                {
                    case "BEGIN":
                    {
                        tkn=Parser.GetToken(sr);
                        if(tkn!=":")throw new InvalidDocumentStructureExeption();
                        tkn=Parser.GetToken(sr);
                        vObject ent=null;
                        if(tkn=="VEVENT")
                            ent=new vEvent();
                        if(tkn=="VTODO")
                            ent=new vTodo();
                        if((tkn!="VEVENT")&&(tkn!="VTODO"))throw new InvalidDocumentStructureExeption();
                        ent.ParseFromStream(sr);
                        AddEntity(ent);
                        break;
                    }
                    case "GEO":
                        pr=new GeographicPosition(this);
                        break;
                    case "PRODID":
                        pr=new ProductID(this);
                        break;
                    case "TZ":
                        pr=new TZone(this);
                        break;
                    case "VERSION":
                        pr=new SpecificationVersion(this);
                        break;
                    default:
                        if(tkn.Substring(0,2)=="X-")  // Extention property
                        {
                            pr=new ExtensionProperty(tkn,this);
                        }
                        else  // Comment this if optional properties support is not required
                        {
                            pr=new OptionalProperty(tkn,this);
                        }
                        break;
                }
                if(pr!=null)
                {
                    pr.ParseFromStream(sr);
                    SetProperty(pr);
                }
            }
        }


        #region Working with NESTED OBJECTS (e.g. vTodo or vEvent)
        public void AddvTodo(vTodo ent)
        {
            _Entities.Add(ent);
        }
        public void AddvEvent(vEvent ent)
        {
            _Entities.Add(ent);
        }
        public void AddEntity(vObject ent)
        {
            _Entities.Add(ent);
        }
        public void RemoveEntity(int n)
        {
            if(n>=_Entities.Count)throw new InvalidEntityNumberException();
            _Entities.RemoveAt(n);
        }
        public vObject GetEntity(int n)
        {
            if(n>=_Entities.Count)throw new InvalidEntityNumberException();
            return (vObject)_Entities[n];
        }
        #endregion
        
        #region Working with GEOGRAPHIC POSITION property
        public void SetGeo(GeographicCoordinates newgeo)
        {
            vObjectProperty pr=GetProperty("GEO");
            if(pr==null)SetProperty(new GeographicPosition(this,newgeo));
            else ((GeographicPosition)pr).SetCoord(newgeo.longitude,newgeo.latitude);
        }
        public GeographicCoordinates GetGeo()
        {
            vObjectProperty pr=GetProperty("GEO");
            if(pr==null)throw new ReferenceToNonExistentPropertyException();
            return (GeographicCoordinates)pr.Value;
        }
        public GeographicPosition GetPropGeo()
        {
            return (GeographicPosition)GetProperty("GEO");
        }
        public void RemoveGeo()
        {
            RemoveProperty("GEO");
        }
        #endregion

        #region Working with PRODUCT IDENTIFIER property
        public void SetProdID(TextValue prid)
        {
            vObjectProperty pr=GetProperty("PRODID");
            if(pr==null)
                SetProperty(new ProductID(this,prid));
            else
                ((ProductID)pr).SetProdID(prid);    
        }
        public void SetProdID(String prid)
        {
            SetProdID(new TextValue(prid));
        }
        public TextValue GetProdID()
        {
            vObjectProperty pr=GetProperty("PRODID");
            return (TextValue)pr.Value;
        }
        public ProductID GetPropProdID()
        {
            return (ProductID)GetProperty("PRODID");
        }
        public void RemoveProdID()
        {
            RemoveProperty("PRODID");
        }
        #endregion

        #region Working with TIME ZONE property
        public void SetTZ(TimeZoneValue tz)
        {
            vObjectProperty pr=GetProperty("TZ");
            if(pr==null)
                SetProperty(new TZone(this,tz));
            else
                ((TZone)pr).SetTZ(tz);
        }
        public TimeZoneValue GetTZ()
        {
            vObjectProperty pr=GetProperty("TZ");
            return (TimeZoneValue)pr.Value;
        }
        public TZone GetPropTZ()
        {
            return (TZone)GetProperty("TZ");
        }
        public void RemoveTZ()
        {
            RemoveProperty("TZ");
        }
        #endregion
        
        #region Working with Version property
        public void SetVersion()
        {
            vObjectProperty pr=GetProperty("VERSION");
            if(pr==null)
                SetProperty(new SpecificationVersion(this));
        }
        public VersionValue GetVersion()
        {
            vObjectProperty pr=GetProperty("VERSION");
            return (VersionValue)pr.Value;
        }
        public SpecificationVersion GetPropVersion()
        {
            return (SpecificationVersion)GetProperty("VERSION");
        }
        public void RemoveVersion()
        {
            RemoveProperty("VERSION");
        }
        #endregion

    }

    public class vEvent: vObject
    {
        public vEvent():base("VEVENT")
        {
        }
        public override String ToStringForm()
        {
            String ts;
            ts="BEGIN:"+ObjectName+'\n';
            ts=ts+base.ToStringForm();
            ts=ts+"END:"+ObjectName+'\n';
            return ts;
        }

        public override void ParseFromStream(StreamReader sr)
        {
            String tkn="";
            while(true)
            {
                tkn=Parser.GetToken(sr);
                if(tkn=="END")
                {
                    tkn=Parser.GetToken(sr);
                    if(tkn!=":")throw new InvalidDocumentStructureExeption();
                    tkn=Parser.GetToken(sr);
                    if(tkn!="VEVENT")throw new InvalidDocumentStructureExeption();
                    return ;
                }

                vObjectProperty pr=null;
                switch(tkn)
                {
                    case "BEGIN":
                        throw new InvalidDocumentStructureExeption();
                        break;
                    case "DCREATED":
                        pr=new DateTimeCreated(this);
                        break;
                    case "COMPLETED":
                        pr=new DateTimeCompleted(this);
                        break;
                    case "DESCRIPTION":
                        pr=new Description(this);
                        break;
                    case "LAST-MODIFIED":
                        pr=new LastModified(this);
                        break;
                    case "CATEGORIES":
                        pr=new Categories(this);
                        break;
                    case "DUE":
                        pr=new DueDateTime(this);
                        break;
                    case "DTEND":
                        pr=new EndDateTime(this);
                        break;
                    case "PRIORITY":
                        pr=new Priority(this);
                        break;
                    case "DTSTART":
                        pr=new StartDateTime(this);
                        break;
                    case "SUMMARY":
                        pr=new Summary(this);
                        break;
                    case "STATUS":
                        pr=new Status(this);
                        break;
                    default:
                        if(tkn.Substring(0,2)=="X-")  // Extention property
                        {
                            pr=new ExtensionProperty(tkn,this);
                        }
                        else  // Comment this if optional properties support is not required
                        {
                            pr=new OptionalProperty(tkn,this);
                        }
                        break;
                }
                if(pr!=null)
                {
                    pr.ParseFromStream(sr);
                    SetProperty(pr);
                }
            }
        }


        #region Working with Date/Time Created property
        public void SetDCreated(ISODateTime vl)
        {
            vObjectProperty pr=GetProperty("DCREATED");
            if(pr==null)
                SetProperty(new DateTimeCreated(this,vl));
            else
                ((DateTimeCreated)pr).SetDCreate(vl);
        }
        public ISODateTime GetDCreated()
        {
            return (ISODateTime)(GetProperty("DCREATED").Value);
        }
        public void RemoveDCreated()
        {
            RemoveProperty("DCREATED");
        }
        #endregion
        
        #region Working with Date/Time Completed property
        public void SetDCompleted(ISODateTime dt)
        {
            vObjectProperty pr=GetProperty("COMPLETED");
            if(pr==null)
                SetProperty(new DateTimeCompleted(this,dt));
            else
                ((DateTimeCompleted)pr).SetDCompleted(dt);
        }
        public void RemoveDCompleted()
        {
            RemoveProperty("COMPLETED");
        }
        public ISODateTime GetDCompleted()
        {
            return ((ISODateTime)GetProperty("COMPLETED").Value);
        }
        #endregion
        
        #region Working with Description property
        public void SetDescription(Description ds)
        {
            RemoveProperty("DESCRIPTION");
            SetProperty(ds);
        }
        public void SetDescription(String dscr)
        {
            vObjectProperty pr=GetProperty("DESCRIPTION");
            if(pr==null)
                SetProperty(new Description(this,dscr));
            else
                ((Description)pr).SetDescription(dscr);
        }
        public TextValue GetDescription()
        {
            return ((TextValue)(GetProperty("DESCRIPTION").Value));
        }
        public Description GetPropDescription()
        {
            return (Description)GetProperty("DESCRIPTION");
        }
        public void RemoveDescription()
        {
            RemoveProperty("DESCRIPTION");
        }
        #endregion

        #region Working with LastModified property
        public void SetLastModified(ISODateTime Time)
        {
            SetProperty(new LastModified(this,Time));
        }
        public ISODateTime GetLastModified()
        {
            vObjectProperty pr=GetProperty("LAST-MODIFIED");
            return (ISODateTime)pr.Value;
        }
        public void RemoveLastModified()
        {
            RemoveProperty("LAST-MODIFIED");
        }
        #endregion

        #region Working with CATEGORIES
        public void SetCategories(CategoriesValue cts)
        {
            if(cts.catlist.Count==0)
            {
                RemoveCategories();
                return;
            }
            vObjectProperty pr=GetProperty("CATEGORIES");
            if(pr==null)
                SetProperty(new Categories(this,cts));
            else
                ((Categories)pr).SetCategories(cts);
        }
        public void SetCategory(String ct)
        {
            vObjectProperty pr=GetProperty("CATEGORIES");
            if(pr==null)
                SetProperty(new Categories(this,ct));
            else
                ((CategoriesValue)(pr.Value)).SetNewCategory(ct);
        }
        public CategoriesValue GetCategories()
        {
            vObjectProperty pr=GetProperty("CATEGORIES");
            return (CategoriesValue)(pr.Value);
        }
        public void RemoveCategory(String ct)
        {
            vObjectProperty pr1=GetProperty("CATEGORIES");
            Categories pr=(Categories)pr1;
            pr.RemoveCategory(ct);
            if(pr.empty())RemoveProperty("CATEGORIES");
        }
        public void RemoveCategories()
        {
            RemoveProperty("CATEGORIES");
        }
        #endregion
        
        #region Working with Due Date/Time property
        public void SetDue(ISODateTime vl)
        {
            vObjectProperty pr=GetProperty("DUE");
            if(pr==null)
                SetProperty(new DueDateTime(this,vl));
            else
                ((DueDateTime)pr).SetDue(vl);
        }
        public ISODateTime GetDue()
        {
            return (ISODateTime)(GetProperty("DUE").Value);
        }
        public void RemoveDue()
        {
            RemoveProperty("DUE");
        }
        #endregion

        #region Working with End Date/Time property
        public void SetDTEnd(ISODateTime vl)
        {
            vObjectProperty pr=GetProperty("DTEND");
            if(pr==null)
                SetProperty(new EndDateTime(this,vl));
            else
                ((EndDateTime)pr).SetDTEnd(vl);
        }
        public ISODateTime GetDTEnd()
        {
            return (ISODateTime)(GetProperty("DTEND").Value);
        }
        public void RemoveDTEnd()
        {
            RemoveProperty("DTEND");
        }
        #endregion
        
        #region Working with Priority property
        public void SetPriority(int p)
        {
            vObjectProperty pr=GetProperty("PRIORITY");
            if(pr==null)
                SetProperty(new Priority(this,p));
            else
                ((Priority)pr).SetPriority(p);
        }
        public int GetPriority()
        {
            return ((PriorityValue)(GetProperty("PRIORITY").Value)).priority;
        }
        public void RemovePriority()
        {
            RemoveProperty("PRIORITY");
        }
        #endregion

        #region Working with Start Date/Time property
        public void SetDTStart(ISODateTime vl)
        {
            vObjectProperty pr=GetProperty("DTSTART");
            if(pr==null)
                SetProperty(new StartDateTime(this,vl));
            else
                ((StartDateTime)pr).SetDTStart(vl);
        }
        public ISODateTime GetDTStart()
        {
            return (ISODateTime)(GetProperty("DTSTART").Value);
        }
        public void RemoveDTStart()
        {
            RemoveProperty("DTSTART");
        }
        #endregion

        #region Working with Summary-description property
        public void SetSummary(String st)
        {
            vObjectProperty pr=GetProperty("SUMMARY");
            if(pr==null)
                SetProperty(new Summary(this,st));
            else
                ((Summary)pr).SetSummary(st);
        }
        public String GetSummary()
        {
            return ((TextValue)(((Summary)GetProperty("SUMMARY")).Value)).txt;
        }
        public Summary GetPropSummary()
        {
            return (Summary)GetProperty("SUMMARY");
        }
        public void RemoveSummary()
        {
            RemoveProperty("SUMMARY");
        }
        #endregion
        
        #region Working with Status property
        public void SetStatus(StatusValue.PosVal vl)
        {
            vObjectProperty pr=GetProperty("STATUS");
            if(pr==null)
                SetProperty(new Status(this,vl));
            else
                ((Status)pr).SetStatus(vl);
        }
        public StatusValue.PosVal GetStatus()
        {
            return ((StatusValue)(((Status)GetProperty("STATUS")).Value)).vl;
        }
        public void RemoveStatus()
        {
            RemoveProperty("STATUS");
        }
        #endregion

    }
    public class vTodo: vObject
    {
        public vTodo():base("VTODO")
        {
        }
        public override String ToStringForm()
        {
            String ts;
            ts="BEGIN:"+ObjectName+'\n';
            ts=ts+base.ToStringForm();
            ts=ts+"END:"+ObjectName+'\n';
            return ts;
        }

        public override void ParseFromStream(StreamReader sr)
        {
            String tkn="";
            while(true)
            {
                tkn=Parser.GetToken(sr);
                if(tkn=="END")
                {
                    tkn=Parser.GetToken(sr);
                    if(tkn!=":")throw new InvalidDocumentStructureExeption();
                    tkn=Parser.GetToken(sr);
                    if(tkn!="VTODO")throw new InvalidDocumentStructureExeption();
                    return ;
                }

                vObjectProperty pr=null;
                switch(tkn)
                {
                    case "BEGIN":
                        throw new InvalidDocumentStructureExeption();
                        break;
                    case "DCREATED":
                        pr=new DateTimeCreated(this);
                        break;
                    case "COMPLETED":
                        pr=new DateTimeCompleted(this);
                        break;
                    case "DESCRIPTION":
                        pr=new Description(this);
                        break;
                    case "LAST-MODIFIED":
                        pr=new LastModified(this);
                        break;
                    case "CATEGORIES":
                        pr=new Categories(this);
                        break;
                    case "DUE":
                        pr=new DueDateTime(this);
                        break;
                    case "DTEND":
                        pr=new EndDateTime(this);
                        break;
                    case "PRIORITY":
                        pr=new Priority(this);
                        break;
                    case "DTSTART":
                        pr=new StartDateTime(this);
                        break;
                    case "SUMMARY":
                        pr=new Summary(this);
                        break;
                    case "STATUS":
                        pr=new Status(this);
                        break;
                    default:
                        if(tkn.Substring(0,2)=="X-")  // Extention property
                        {
                            pr=new ExtensionProperty(tkn,this);
                        }
                        else  // Comment this if optional properties support is not required
                        {
                            pr=new OptionalProperty(tkn,this);
                        }
                        break;
                }
                if(pr!=null)
                {
                    pr.ParseFromStream(sr);
                    SetProperty(pr);
                }
            }
        }

        #region Working with Date/Time Created property
        public void SetDCreated(ISODateTime vl)
        {
            vObjectProperty pr=GetProperty("DCREATED");
            if(pr==null)
                SetProperty(new DateTimeCreated(this,vl));
            else
                ((DateTimeCreated)pr).SetDCreate(vl);
        }
        public ISODateTime GetDCreated()
        {
            return (ISODateTime)(GetProperty("DCREATED").Value);
        }
        public void RemoveDCreated()
        {
            RemoveProperty("DCREATED");
        }
        #endregion
        
        #region Working with Date/Time Completed property
        public void SetDCompleted(ISODateTime dt)
        {
            vObjectProperty pr=GetProperty("COMPLETED");
            if(pr==null)
                SetProperty(new DateTimeCompleted(this,dt));
            else
                ((DateTimeCompleted)pr).SetDCompleted(dt);
        }
        public void RemoveDCompleted()
        {
            RemoveProperty("COMPLETED");
        }
        public ISODateTime GetDCompleted()
        {
            return ((ISODateTime)GetProperty("COMPLETED").Value);
        }
        #endregion
        
        #region Working with Description property
        public void SetDescription(Description ds)
        {
            RemoveProperty("DESCRIPTION");
            SetProperty(ds);
        }
        public void SetDescription(String dscr)
        {
            vObjectProperty pr=GetProperty("DESCRIPTION");
            if(pr==null)
                SetProperty(new Description(this,dscr));
            else
                ((Description)pr).SetDescription(dscr);
        }
        public TextValue GetDescription()
        {
            return ((TextValue)(GetProperty("DESCRIPTION").Value));
        }
        public Description GetPropDescription()
        {
            return (Description)GetProperty("DESCRIPTION");
        }
        public void RemoveDescription()
        {
            RemoveProperty("DESCRIPTION");
        }
        #endregion

        #region Working with LastModified property
        public void SetLastModified(ISODateTime Time)
        {
            SetProperty(new LastModified(this,Time));
        }
        public ISODateTime GetLastModified()
        {
            vObjectProperty pr=GetProperty("LAST-MODIFIED");
            return (ISODateTime)pr.Value;
        }
        public void RemoveLastModified()
        {
            RemoveProperty("LAST-MODIFIED");
        }
        #endregion

        #region Working with CATEGORIES
        public void SetCategories(CategoriesValue cts)
        {
            if(cts.catlist.Count==0)
            {
                RemoveCategories();
                return;
            }
            vObjectProperty pr=GetProperty("CATEGORIES");
            if(pr==null)
                SetProperty(new Categories(this,cts));
            else
                ((Categories)pr).SetCategories(cts);
        }
        public void SetCategory(String ct)
        {
            vObjectProperty pr=GetProperty("CATEGORIES");
            if(pr==null)
                SetProperty(new Categories(this,ct));
            else
                ((CategoriesValue)(pr.Value)).SetNewCategory(ct);
        }
        public CategoriesValue GetCategories()
        {
            vObjectProperty pr=GetProperty("CATEGORIES");
            return (CategoriesValue)(pr.Value);
        }
        public void RemoveCategory(String ct)
        {
            vObjectProperty pr1=GetProperty("CATEGORIES");
            Categories pr=(Categories)pr1;
            pr.RemoveCategory(ct);
            if(pr.empty())RemoveProperty("CATEGORIES");
        }
        public void RemoveCategories()
        {
            RemoveProperty("CATEGORIES");
        }
        #endregion
        
        #region Working with Due Date/Time property
        public void SetDue(ISODateTime vl)
        {
            vObjectProperty pr=GetProperty("DUE");
            if(pr==null)
                SetProperty(new DueDateTime(this,vl));
            else
                ((DueDateTime)pr).SetDue(vl);
        }
        public ISODateTime GetDue()
        {
            return (ISODateTime)(GetProperty("DUE").Value);
        }
        public void RemoveDue()
        {
            RemoveProperty("DUE");
        }
        #endregion

        #region Working with End Date/Time property
        public void SetDTEnd(ISODateTime vl)
        {
            vObjectProperty pr=GetProperty("DTEND");
            if(pr==null)
                SetProperty(new EndDateTime(this,vl));
            else
                ((EndDateTime)pr).SetDTEnd(vl);
        }
        public ISODateTime GetDTEnd()
        {
            return (ISODateTime)(GetProperty("DTEND").Value);
        }
        public void RemoveDTEnd()
        {
            RemoveProperty("DTEND");
        }
        #endregion
        
        #region Working with Priority property
        public void SetPriority(int p)
        {
            vObjectProperty pr=GetProperty("PRIORITY");
            if(pr==null)
                SetProperty(new Priority(this,p));
            else
                ((Priority)pr).SetPriority(p);
        }
        public int GetPriority()
        {
            return ((PriorityValue)(GetProperty("PRIORITY").Value)).priority;
        }
        public void RemovePriority()
        {
            RemoveProperty("PRIORITY");
        }
        #endregion

        #region Working with Start Date/Time property
        public void SetDTStart(ISODateTime vl)
        {
            vObjectProperty pr=GetProperty("DTSTART");
            if(pr==null)
                SetProperty(new StartDateTime(this,vl));
            else
                ((StartDateTime)pr).SetDTStart(vl);
        }
        public ISODateTime GetDTStart()
        {
            return (ISODateTime)(GetProperty("DTSTART").Value);
        }
        public void RemoveDTStart()
        {
            RemoveProperty("DTSTART");
        }
        #endregion

        #region Working with Summary-description property
        public void SetSummary(String st)
        {
            vObjectProperty pr=GetProperty("SUMMARY");
            if(pr==null)
                SetProperty(new Summary(this,st));
            else
                ((Summary)pr).SetSummary(st);
        }
        public String GetSummary()
        {
            return ((TextValue)(((Summary)GetProperty("SUMMARY")).Value)).txt;
        }
        public Summary GetPropSummary()
        {
            return (Summary)GetProperty("SUMMARY");
        }
        public void RemoveSummary()
        {
            RemoveProperty("SUMMARY");
        }
        #endregion
        
        #region Working with Status property
        public void SetStatus(StatusValue.PosVal vl)
        {
            vObjectProperty pr=GetProperty("STATUS");
            if(pr==null)
                SetProperty(new Status(this,vl));
            else
                ((Status)pr).SetStatus(vl);
        }
        public StatusValue.PosVal GetStatus()
        {
            return ((StatusValue)(((Status)GetProperty("STATUS")).Value)).vl;
        }
        public void RemoveStatus()
        {
            RemoveProperty("STATUS");
        }
        #endregion
    }

    public class vCard: vObject
    {
        public vCard():base("VCARD")
        {
            _crds=new ArrayList();
        }
        public override String ToStringForm()
        {
            String ts;
            ts="BEGIN:"+ObjectName+"\r\n";
            ts=ts+base.ToStringForm();
            for(int i=0;i<_crds.Count;i++)
                ts=ts+GetNested(i).ToStringForm();
            ts=ts+"END:"+ObjectName+"\r\n";
            return ts;
        }

        public override void ParseFromStream(StreamReader sr)
        {
            String tkn="";
            while(true)
            {
                tkn=Parser.GetTokenPnt(sr);
                if(tkn=="END")
                {
                    tkn=Parser.GetToken(sr);
                    if(tkn!=":")throw new InvalidDocumentStructureExeption();
                    tkn=Parser.GetToken(sr);
                    if(tkn!="VCARD")throw new InvalidDocumentStructureExeption();
                    return ;
                }

                vObjectProperty pr=null;
                switch(tkn)
                {
                    case "BEGIN":
                    {
                        tkn=Parser.GetToken(sr);
                        if(tkn!=":")throw new InvalidDocumentStructureExeption();
                        tkn=Parser.GetToken(sr);
                        vCard ent=null;
                        if(tkn=="VCARD")
                            ent=new vCard();
                        if(tkn!="VCARD")throw new InvalidDocumentStructureExeption();
                        ent.ParseFromStream(sr);
                        AddNested(ent);
                        break;
                    }
                    case "N":
                        pr=new NameProperty(this);
                        break;
                    case "GEO":
                        pr=new GeographicPosition(this);
                        break;
                    case "TZ":
                        pr=new TZone(this);
                        break;
                    case "VERSION":
                        pr=new SpecificationVersion(this);
                        break;
                    default:
                        if(tkn[tkn.Length-1]=='.')
                        { 
                            String cn=tkn.Substring(0,tkn.Length-1);
                            tkn=Parser.GetToken(sr);
                            pr=new OptionalProperty(tkn,this);
                            pr.ParseFromStream(sr);
                            SetPrToComposite(cn,pr);
                            pr=null;
                            break;
                        }
                        if(tkn.Substring(0,2)=="X-")  // Extention property
                        {
                            pr=new ExtensionProperty(tkn,this);
                        }
                        else  // Comment this if optional properties support is not required
                        {
                            pr=new OptionalProperty(tkn,this);
                        }
                        break;
                }
                if(pr!=null)
                {
                    pr.ParseFromStream(sr);
                    SetProperty(pr);
                }
            }
        }

        #region Working with NESTED CARDS
        private ArrayList _crds;
        public void AddNested(vCard nc)
        {
            _crds.Add(nc);
        }
        public vCard GetNested(int n)
        {
            return (vCard)_crds[n];
        }
        public void RemoveNested(int n)
        {
            if(n<_crds.Count)
                _crds.RemoveAt(n);
        }
        #endregion

        #region Working with PropertyCompositors
        public void SetPrToComposite(String cn,vObjectProperty newpr)
        {
            vObjectProperty pr=GetProperty(cn);
            if(pr==null)SetProperty(pr=new PropertyComposite(cn,this));
            ((PropertyComposite)pr).SetNewProperty(newpr);
        }
        public vObjectProperty GetFromComposite(String cn,String pn)
        {
            PropertyComposite pc=GetComposite(cn);
            return pc.GetProperty(pn);
        }
        public PropertyComposite GetComposite(String cn)
        {
            return (PropertyComposite)GetProperty(cn);
        }
        public void RemoveComposite(String cn)
        {
            RemoveProperty(cn);
        }
        #endregion

        #region Working with NAME property
        public void SetName(String nm)
        {
            vObjectProperty pr=GetProperty("N");
            if(pr==null)
                SetProperty(new NameProperty(this,nm));
            else
                ((NameProperty)pr).SetName(nm);
        }
        public String GetName()
        {
            return ((TextValue)GetProperty("N").Value).txt;
        }
        public NameProperty GetPropName()
        {
            return (NameProperty)GetProperty("N");
        }
        public void RemoveName()
        {
            RemoveProperty("N");
        }
        #endregion

        #region Working with GEOGRAPHIC POSITION property
        public void SetGeo(GeographicCoordinates newgeo)
        {
            vObjectProperty pr=GetProperty("GEO");
            if(pr==null)SetProperty(new GeographicPosition(this,newgeo));
            else ((GeographicPosition)pr).SetCoord(newgeo.longitude,newgeo.latitude);
        }
        public GeographicCoordinates GetGeo()
        {
            vObjectProperty pr=GetProperty("GEO");
            if(pr==null)throw new ReferenceToNonExistentPropertyException();
            return (GeographicCoordinates)pr.Value;
        }
        public GeographicPosition GetPropGeo()
        {
            return (GeographicPosition)GetProperty("GEO");
        }
        public void RemoveGeo()
        {
            RemoveProperty("GEO");
        }
        #endregion

        #region Working with TIME ZONE property
        public void SetTZ(TimeZoneValue tz)
        {
            vObjectProperty pr=GetProperty("TZ");
            if(pr==null)
                SetProperty(new TZone(this,tz));
            else
                ((TZone)pr).SetTZ(tz);
        }
        public TimeZoneValue GetTZ()
        {
            vObjectProperty pr=GetProperty("TZ");
            return (TimeZoneValue)pr.Value;
        }
        public TZone GetPropTZ()
        {
            return (TZone)GetProperty("TZ");
        }
        public void RemoveTZ()
        {
            RemoveProperty("TZ");
        }
        #endregion

        #region Working with Version property
        public void SetVersion()
        {
            vObjectProperty pr=GetProperty("VERSION");
            if(pr==null)
                SetProperty(new SpecificationVersion(this,2,1));
        }
        public VersionValue GetVersion()
        {
            vObjectProperty pr=GetProperty("VERSION");
            return (VersionValue)pr.Value;
        }
        public SpecificationVersion GetPropVersion()
        {
            return (SpecificationVersion)GetProperty("VERSION");
        }
        public void RemoveVersion()
        {
            RemoveProperty("VERSION");
        }
        #endregion

    }
}
