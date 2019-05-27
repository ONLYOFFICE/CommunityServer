namespace Ical.Net.Serialization
{
    internal interface IEncodingProvider
    {
        string Encode(string encoding, byte[] data);
        string DecodeString(string encoding, string value);
        byte[] DecodeData(string encoding, string value);
    }
}