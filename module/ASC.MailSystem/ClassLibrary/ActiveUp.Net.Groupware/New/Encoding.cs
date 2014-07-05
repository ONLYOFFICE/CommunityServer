using System;
using System.IO;
using System.Text;

namespace CompanyName.vObjects
{
    using System.Text;
    /// <summary>
    /// Summary description for Encoding.
    /// </summary>
    public abstract class Encoder
    {
        private String _encoderName;
        public Encoder(String nm)
        {
            _encoderName=nm;
        }
        public String Folding(String tx)
        {
            String ts="";
            for(int i=0;i<tx.Length;i++)
            {
                if((i>1)&&((int)tx[i-2]==13)&&((int)tx[i-1]==10))ts=ts+(char)9;
                ts=ts+tx[i];
            }
            return ts;
        }
        public String Unfolding(String tx)
        {
            String ts="";
            for(int i=0;i<tx.Length;i++)
            {
                if((i>1)&&((int)tx[i-2]==13)&&((int)tx[i-1]==10))continue;
                ts=ts+tx[i];
            }
            return ts;
        }
        public String SetEscapes(String tx)
        {
            String ts="";
            for(int i=0;i<tx.Length;i++)
            {
                if(tx[i]==';')ts=ts+'\\';
                ts=ts+tx[i];
            }
            return ts;
        }
        public String RemoveEscapes(String tx)
        {
            String ts="";
            for(int i=0;i<tx.Length-1;i++)
            {
                if((tx[i]=='\\')&&(tx[i+1]==';'))i++;
                ts=ts+tx[i];
            }
            return ts;
        }
        public abstract String Encode(String tx);
        public abstract String Decode(String tx);
        public String ReadAndDecode(StreamReader sr)
        {
            String ts=sr.ReadLine();
            while((sr.Peek()==9)||(sr.Peek()==32))
            {
                sr.Read();
                ts=ts+'\r'+'\n'+sr.ReadLine();
            }
            return Decode(ts);
        }

        public String EncoderName
        {
            get
            {
                return _encoderName;
            }
        }
    }

    public class _7BitEncoder: Encoder
    {
        public _7BitEncoder() : base("7-Bit")
        {
        }
        public override String Encode(String tx)
        {
            for(int i=0;i<tx.Length;i++)
                if((int)tx[i]>127)throw new InvalidCharacterException();
            return tx;                                        
        }
        public override String Decode(String tx)
        {
            return tx;
        }

    }
    public class QuotedPrintableEncoder : Encoder    
    {
        public QuotedPrintableEncoder() : base("QUOTED-PRINTABLE")
        {
        }
        private static String EncodeTable="0123456789ABCDEF";

        private String _ByteRepresentation(byte t)
        {
            String ts="=";
            ts=ts+EncodeTable[t/16]+EncodeTable[t%16];
            return ts;
            
        }
        private char _ToASCII(int h,int l)
        {
            int th=0,tl=0;
            if((h>=(int)'0')&&(h<=(int)'9'))th=h-(int)'0';
            if((h>=(int)'a')&&(h<=(int)'f'))th=h-(int)'a'+10;
            if((h>=(int)'A')&&(h<=(int)'F'))th=h-(int)'A'+10;
            if((l>=(int)'0')&&(l<=(int)'9'))tl=l-(int)'0';
            if((l>=(int)'a')&&(l<=(int)'f'))tl=l-(int)'a'+10;
            if((l>=(int)'A')&&(l<=(int)'F'))tl=l-(int)'A'+10;
            byte[] bt=new byte[1];
            bt[0]=Convert.ToByte(th*16+tl);
            Encoding AE = Encoding.GetEncoding(1253);
            return AE.GetChars(bt,0,1)[0];
        }
        public override String Encode(String tx)
        {
            Encoding AE = Encoding.GetEncoding(1253);
            byte[] ba = AE.GetBytes(tx);

            String ts="";
            int kl=0,wid=70;
            byte ch;
            for(int i=0;i<ba.Length;i++)
            {
                ch=ba[i];
                if((ch==13)&&(ba[i+1])==10){ts=ts+(char)13+(char)10;i++;kl=0;continue;}
                if(kl>wid)
                {
                    kl=0;
                    ts=ts+"="+(char)13+(char)10;
                }
                if(((i+2<ba.Length)&&(ba[i+1]==13)&&(ba[i+2]==10))&&((ch==32)||(ch==9)))
                {
                    ts=ts+_ByteRepresentation(ba[i]);
                    kl+=3;
                    continue;
                }
                if((ch==13)&&(ba[i+1])!=10){ts=ts+_ByteRepresentation(ba[i]);kl+=3;continue;}
                if((ch>=32)&&(ch<=126)&&(ch!=61)){ts=ts+(char)ba[i];kl++;continue;}
                ts=ts+_ByteRepresentation(ba[i]);
                kl+=3;
            }
            return ts;
        }
        public override String Decode(String tx)
        {
            Encoding AE= Encoding.GetEncoding(1253);
            byte[] ba = AE.GetBytes(tx);
            String ts="";
            byte ch;
            
            for(int i=0;i<ba.Length;i++)
            {
                ch=ba[i];
                if(ch==61)
                {
                    if((ba[i+1]==13)&&(ba[i+2]==10))
                    {
                        i+=2;
                        continue;
                    }
                    ts=ts+_ToASCII(ba[i+1],ba[i+2]);
                    i+=2;
                }else
                    ts=ts+(char)ch;
            }
            return ts;
        }

    }
    public class Base64Encoder: Encoder
    {
        public int LineSize;
        public Base64Encoder(): base("BASE64")
        {
            LineSize=40;
        }
        public static String EncodeTable="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        public static String DecodeTable="|$$$}rstuvwxyz{$$$$$$$>?@ABCDEFGHIJKLMNOPQRSTUVW$$$$$$XYZ[\\]^_`abcdefghijklmnopq";

        private void _EncodePart(byte[] ini,int len,byte[] iout)
        {
            iout[0]=(byte)EncodeTable[ ini[0] >> 2 ];
            iout[1]=(byte)EncodeTable[ ((ini[0] & 0x03) << 4) | ((ini[1] & 0xf0) >> 4) ];
            iout[2]=(byte)(len >1?EncodeTable[((ini[1]&0x0f)<< 2)|((ini[2] & 0xc0) >> 6) ]:'=');
            iout[3]=(byte)(len >2?EncodeTable[ini[2]&0x3f ]:'=');
        }
        public override String Encode(String tx)
        {
            String ts="";
            Encoding AE = Encoding.GetEncoding(1253);
            byte[] ba = AE.GetBytes(tx);

            byte[] ini=new byte[3],iout=new byte[4];
            int i, len, bs=0,tp;

            tp=0;
            while( tp<ba.Length ) 
            {
                len = 0;
                for(i=0;i<3;i++)
                {
                    if(tp<ba.Length)
                    {
                        ini[i]=ba[tp];
                        len++;
                    }
                    else ini[i]=0;
                    tp++;
                }
                if(len>0) 
                {
                    _EncodePart(ini,len,iout);
                    for(i=0;i<4;i++) 
                    {
                        ts=ts+(char)iout[i];
                    }
                    bs++;
                }
                if(bs>=(LineSize/4))
                {
                    if(bs!=0) 
                    {
                        ts=ts+"\r\n";
                    }
                    bs=0;
                }
            }
            return ts;
        }

        void _DecodePart(byte[] ini,byte[] iout)
        {   
            iout[0]=(byte)(ini[0] << 2 | ini[1] >> 4);
            iout[1]=(byte)(ini[1] << 4 | ini[2] >> 2);
            iout[2]=(byte)(((ini[2] << 6) & 0xc0) | ini[3]);
        }


        public override String Decode(String tx)
        {
            String ts="";
            while(tx.IndexOf("\r")!=-1)tx=tx.Remove(tx.IndexOf("\r"),1);
            while(tx.IndexOf("\n")!=-1)tx=tx.Remove(tx.IndexOf("\n"),1);
            Encoding AE = Encoding.GetEncoding(1253);
            byte[] ba = AE.GetBytes(tx);

            byte[] ini=new byte[4], iout=new byte[3];
            byte v;
            int i,len,tp=0;

            while(tp<ba.Length) 
            {
                for(len=0,i=0;(i<4)&&(tp<ba.Length);i++) 
                {
                    v=0;
                    while((tp<ba.Length)&&(v==0)) 
                    {
                        v=ba[tp];tp++;
                        v=(byte)((v<43||v>122)?0:DecodeTable[ v - 43 ]);
                        if(v!=0) 
                            v=(byte)((v==(int)'$')?0:v-61);
                    }
                    if(tp<ba.Length) 
                    {
                        len++;
                        if(v!=0) 
                        {
                            ini[i]=(byte)(v - 1);
                        }
                    }
                    else 
                    {
                        ini[i] = 0;
                    }
                }
                if(len!=0) 
                {
                    _DecodePart(ini,iout);
                    for( i = 0; i < len - 1; i++ ) 
                    {
                        ts=ts+AE.GetChars(iout,i,1)[0];
                    }
                }
            }
            return ts;
        }


    }
}
