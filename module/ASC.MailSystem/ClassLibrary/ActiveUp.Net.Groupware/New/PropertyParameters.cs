using System;

namespace CompanyName.vObjects
{
    /// <summary>
    /// Summary description for PropertyParameters.
    /// </summary>
    public class EncodingParameter: PropertyParameter
    {
        public EncodingParameter(vObjectProperty owner):base("ENCODING",owner)
        {
        }
        public EncodingParameter(vObjectProperty owner,Encoder enc):base("ENCODING",owner)
        {
            ParameterValue=enc.EncoderName;
            ParameterOwner.enc=enc;
        }
        public void SetEncoding(Encoder enc)
        {
            ParameterValue=enc.EncoderName;
            ParameterOwner.enc=enc;
        }
        public override void Apply()
        {
            switch(ParameterValue)
            {
                case "QUOTED-PRINTABLE": ParameterOwner.enc=new QuotedPrintableEncoder();
                    break;
                case "BASE64": ParameterOwner.enc=new Base64Encoder();
                    break;
                case "7-BIT": ParameterOwner.enc=new _7BitEncoder();
                    break;
            }
        }

    }
    public class ValueParameter: PropertyParameter
    {
        public ValueParameter(vObjectProperty owner): base("VALUE",owner)
        {
        }
        public ValueParameter(vObjectProperty owner,String vt): base("VALUE",owner)
        {
            SetValue(vt);
        }
        public void SetValue(String vt)
        {
            vt.ToUpper();
            if((vt!="INLINE")&&(vt!="CONTENT-ID")&&(vt!="CID")&&(vt!="URL"))throw new InvalidValueTypeException();
            ParameterValue=vt;
        }
    }
}
