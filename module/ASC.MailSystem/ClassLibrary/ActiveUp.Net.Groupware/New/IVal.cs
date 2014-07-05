using System;
using System.IO;
using System.Collections;

namespace CompanyName.vObjects
{
    /// <summary>
    /// acVal is abstract class for vCalendars Objects, Properties and PropertyParameters
    /// </summary>
    public abstract class ICommon
    {
        private Stream _acts;
        public abstract String ToStringForm();  // return objects string representation
        public Stream ActiveStream         // active stream
        {
            get{return _acts;}
            set{_acts=value;}
        }
        public void WriteByWriter(StreamWriter st)     // write object to stream
        {
            st.Write(ToStringForm());
            st.Close();
        }
        public abstract void ParseFromStream(StreamReader sr);
    };
    
    public class PropertyComposite: vObjectProperty
    {
        private ArrayList _Properties;
        private String _GrName;
        public vObject Owner;
        public PropertyComposite(String nm,vObject own): base(nm,own)
        {
            _GrName=nm;
            Owner=own;
            _Properties=new ArrayList();
        }
        public override String ToStringForm()
        {
            String ts="";
            for(int i=0;i<_Properties.Count;i++)
                ts=ts+_GrName+"."+((vObjectProperty)_Properties[i]).ToStringForm()+"\r\n";
            return ts.Substring(0,ts.Length-2); //without last "\r\n"
        }
        public void SetNewProperty(vObjectProperty pr)
        {
            _Properties.Add(pr);
        }
        public void RemoveProperty(String pn)
        {
            foreach(vObjectProperty pr in _Properties)
                if(pr.PropertyName==pn)
                {
                    _Properties.Remove(pr);
                    return;
                }
        }
        public vObjectProperty GetProperty(String pn)
        {
            foreach(vObjectProperty pr in _Properties)
                if(pr.PropertyName==pn)
                    return pr;
            return null;
        }
    }

    public abstract class vObject: ICommon
    {
        private Encoder _enc;
        private String _objname;
        private ArrayList _proplist;
        public vObject(String name)
        {
            _objname=name;
            _proplist=new ArrayList();
            _enc=new _7BitEncoder();
        }
        public String ObjectName
        {
            get{return _objname;}
        }
        public Encoder ObjectEncoding
        {
            get{return _enc;}
            set{_enc=value;}
        }
        protected vObjectProperty GetProperty(String pn)
        {
            foreach(vObjectProperty prop in _proplist)
                if(prop.PropertyName==pn)return prop;
            return null;
        }
        protected void SetProperty(vObjectProperty prop)
        {
            _proplist.Add(prop);
        }
        protected void RemoveProperty(String pn)
        {
            foreach(vObjectProperty prop in _proplist)
                if(prop.PropertyName==pn)
                {
                    _proplist.Remove(prop);
                    return;
                }
        }
        public void SetActiveStream(Stream st)
        {
            ActiveStream=st;
        }
        public void WriteToFile(String path)
        {
            Stream st=new FileStream(path,FileMode.Create);
            WriteToStream(st);
        }
        public void ReadFromFile(String path)
        {
            Stream st=new FileStream(path,FileMode.Open);
            ReadFromStream(st);
        }
        public void WriteToStream(Stream st)
        {
            ActiveStream=st;
            StreamWriter sw=new StreamWriter(st);
            WriteByWriter(sw);
        }
        public void ReadFromStream(Stream st)
        {
            StreamReader sr=new StreamReader(st);
            String tmptkn=Parser.GetToken(sr);
            tmptkn=Parser.GetToken(sr);
            tmptkn=Parser.GetToken(sr);
            ParseFromStream(sr);
        }

        #region Working with Extension properties
        private String GetExtensionPropertyName(String vn,String pn)
        {
            String fpn="X-";
            if(vn!="")fpn=fpn+vn+"-";
            fpn=fpn+pn;
            return fpn.ToUpper();
        }
        public void SetExtProperty(String vn,String pn,String pval)  // VendorName  and PropertyName
        {
            String fpn=GetExtensionPropertyName(vn,pn);
            vObjectProperty pr=new ExtensionProperty(fpn,this);
            ((ExtensionProperty)pr).SetValue(pval);
            SetProperty(pr);
        }
        public void SetExtProperty(ExtensionProperty ep)
        {
            _proplist.Add(ep);
        }
        public ExtensionProperty GetExtProperty(String vn,String pn)
        {
            return (ExtensionProperty)GetProperty(GetExtensionPropertyName(vn,pn));
        }
        public String GetExtPropertyValue(String vn,String pn)
        {
            return ((TextValue)GetExtProperty(vn,pn).Value).txt;
        }
        public void RemoveExtProperty(String vn,String pn)
        {
            String fpn=GetExtensionPropertyName(vn,pn);
            RemoveProperty(fpn);
        }
        #endregion
        
        #region Working with optional properties
        public void SetOptionalProperty(String pn,String vl)
        {
            vObjectProperty pr=GetProperty(pn);
            if(pr==null)SetProperty(new OptionalProperty(pn,this));
            else
                ((OptionalProperty)pr).SetValue(vl);
        }
        public void SetOptionalProperty(OptionalProperty pr)
        {
            _proplist.Add(pr);
        }
        public OptionalProperty GetOptionalProperty(String pn)
        {
            return (OptionalProperty)GetProperty(pn);
        }
        public String GetOptionalPropertyValue(String pn)
        {
            return ((TextValue)(GetOptionalProperty(pn).Value)).txt;
        }
        public void RemoveOptionalProperty(String pn)
        {
            RemoveProperty(pn);
        }
        #endregion
        
        public override String ToStringForm()
        {
            String ts="";
            foreach(vObjectProperty prop in _proplist)
                ts=ts+prop.ToStringForm()+"\r\n";
            return ts;
        }
    }

    public class vObjectProperty: ICommon
    {
        private vObject _propowner;
        private ArrayList _par;
        private String _propname;
        protected PropertyValueType _val;
        private Encoder _enc;
        public vObjectProperty(String prname,vObject owner)
        {
            _propowner=owner;
            _propname=prname;
            _par=new ArrayList();
            _enc=_propowner.ObjectEncoding;
        }
        protected void SetPar(PropertyParameter pr)
        {
            _par.Add(pr);
            pr.Apply();
        }
        protected PropertyParameter GetPar(String pn)
        {
            foreach(PropertyParameter par in _par)
                if(par.ParameterName==pn)return par;
            return null;
        }
        protected void RemovePar(String pn)
        {
            foreach(PropertyParameter par in _par)
                if(par.ParameterName==pn)
                {
                    _par.Remove(par);
                    return;
                }
        }
        
        #region Property parameter's which can be presented in every property
// Value type
        public void SetValueType(String vt)
        {
            PropertyParameter pr=GetPar("VALUE");
            if(pr==null)SetPar(new ValueParameter(this,vt));
            else
            ((ValueParameter)pr).SetValue(vt);
        }
        public String GetValueType()
        {
            return GetPar("VALUE").ParameterValue;
        }
        public void RemoveValueType()
        {
            RemovePar("VALUE");
        }
// Encoding
        public void SetEncoding(Encoder en)
        {
            PropertyParameter par=GetPar("ENCODING");
            if(par==null)
                SetPar(new EncodingParameter(this,en));
            else
                ((EncodingParameter)par).SetEncoding(en);
        }
        public EncodingParameter GetEncoding()
        {
            return (EncodingParameter)GetPar("ENCODING");
        }

        #endregion

        public override String ToStringForm()
        {
            String ts=_propname;
            foreach(PropertyParameter par in _par)
                ts=ts+";"+par.ToStringForm();
            ts=ts+":";
            ts=ts+enc.Folding(enc.Encode(_val.ToStringForm()));
            return ts;
        }
        
        public override void ParseFromStream(StreamReader sr)
        {
            String tkn=Parser.GetToken(sr);
            while(tkn==";")                                    // Reading property parameters
            {
                tkn=Parser.GetToken(sr);
                PropertyParameter pp;
                switch(tkn)
                {
                    case "ENCODING":pp=new EncodingParameter(this);
                        break;
                    case "VALUE":pp=new ValueParameter(this);
                        break;
//                    case "BASE64": pp=new parEncoding(this,new Base64Enc());
//                        break;
                    case "QUOTED-PRINTABLE": pp=new EncodingParameter(this,new QuotedPrintableEncoder());
                        break;
                    case "7-BIT": pp=new EncodingParameter(this,new _7BitEncoder());
                        break;
                    default:pp=new PropertyParameter(tkn,this);
                        break;
                }
                pp.ParseFromStream(sr);  // We will apply parameters value when we'll read it
                SetPar(pp);
                tkn=Parser.GetToken(sr);
            }
            if(tkn!=":")throw new InvalidDocumentStructureExeption();
            _val.ParseFromStream(sr,enc);
        }

        public Encoder enc
        {
            get{return _enc;}
            set{_enc=value;}
        }
        public vObject PropertyOwner
        {
            get{return _propowner;}
        }
        public String PropertyName
        {
            get{return _propname;}
        }
        public PropertyValueType Value
        {
            get{return _val;}
        }
    }
    
    public class PropertyParameter: ICommon
    {
        private String _prname;
        private String _prvalue;
        private vObjectProperty _paramowner;

        public PropertyParameter(String nm,vObjectProperty parent)
        {
            _prname=nm;
            _paramowner=parent;
        }
        public String ParameterName
        {
            get{return _prname;}
        }
        public String ParameterValue
        {
            set{_prvalue=value;}
            get{return _prvalue;}
        }
        public vObjectProperty ParameterOwner
        {
            get{return _paramowner;}
        }
        public virtual void Apply()
        {
        }
        
        public override String ToStringForm()
        {
            if(_prname=="")_prname=null;
            if(_prvalue=="")_prvalue=null;
            if((_prname!=null)&&(_prvalue!=null))return (String.Format(_prname+"="+_prvalue));     // <--------- use here strategy of decoding
            else return _prname+_prvalue;
        }
        public override void ParseFromStream(StreamReader sr)
        {
            if(sr.Peek()!=(int)'=')return;
            String tkn=Parser.GetToken(sr);
            _prvalue=Parser.GetToken(sr);
            Apply();
        }

    }

    abstract public class PropertyValueType
    {
        private String _descr;
        public abstract String ToStringForm();
        public PropertyValueType(String nm)
        {
            _descr=nm;
        }
        public String TypeDescription
        {
            get{return _descr;}
        }
        public abstract void SetFromString(String tx);  // every Value-type should be able to read value from string
        public void ParseFromStream(StreamReader sr,Encoder en)
        {
            SetFromString(en.ReadAndDecode(sr));
        }
    }
    
    public class Parser
    {
        public static String GetToken(StreamReader sr)
        {
            String ts="";
            if((sr.Peek()==(int)':')||(sr.Peek()==(int)';')||(sr.Peek()==(int)'='))
            {
                ts=ts+(char)sr.Read();
                return ts;
            }
            while(sr.Peek()>=0)
            {
                if((sr.Peek()==10)||(sr.Peek()==13))
                {
                    while((sr.Peek()==10)||(sr.Peek()==13))sr.Read();
                    return ts.ToUpper();
                }
                if(((sr.Peek()==(int)':')||(sr.Peek()==(int)';')||(sr.Peek()==(int)'='))&&(ts[ts.Length-1]!='\\'))return ts.ToUpper();
                ts=ts+(char)sr.Read();
            }
            return ts.ToUpper();
        }
        public static String GetTokenPnt(StreamReader sr)
        {
            String ts="";
            if((sr.Peek()==(int)':')||(sr.Peek()==(int)';')||(sr.Peek()==(int)'='))
            {
                ts=ts+(char)sr.Read();
                return ts;
            }
            while(sr.Peek()>=0)
            {
                if((sr.Peek()==10)||(sr.Peek()==13))
                {
                    while((sr.Peek()==10)||(sr.Peek()==13))sr.Read();
                    return ts.ToUpper();
                }
                if(sr.Peek()==(int)'.')
                {
                    sr.Read();
                    ts=ts+'.';
                    return ts.ToUpper();
                }
                if(((sr.Peek()==(int)':')||(sr.Peek()==(int)';')||(sr.Peek()==(int)'='))&&(ts[ts.Length-1]!='\\'))return ts.ToUpper();
                ts=ts+(char)sr.Read();
            }
            return ts.ToUpper();
        }
    }

}
