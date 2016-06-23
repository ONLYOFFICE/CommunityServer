/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace ASC.Web.Core.Helpers
{
    public enum PropertyTagId
    {
        GpsVer = 0x0000,
        GpsLatitudeRef = 0x0001,
        GpsLatitude = 0x0002,
        GpsLongitudeRef = 0x0003,
        GpsLongitude = 0x0004,
        GpsAltitudeRef = 0x0005,
        GpsAltitude = 0x0006,
        GpsGpsTime = 0x0007,
        GpsGpsSatellites = 0x0008,
        GpsGpsStatus = 0x0009,
        GpsGpsMeasureMode = 0x000A,
        GpsGpsDop = 0x000B,
        GpsSpeedRef = 0x000C,
        GpsSpeed = 0x000D,
        GpsTrackRef = 0x000E,
        GpsTrack = 0x000F,
        GpsImgDirRef = 0x0010,
        GpsImgDir = 0x0011,
        GpsMapDatum = 0x0012,
        GpsDestLatRef = 0x0013,
        GpsDestLat = 0x0014,
        GpsDestLongRef = 0x0015,
        GpsDestLong = 0x0016,
        GpsDestBearRef = 0x0017,
        GpsDestBear = 0x0018,
        GpsDestDistRef = 0x0019,
        GpsDestDist = 0x001A,
        NewSubfileType = 0x00FE,
        SubfileType = 0x00FF,
        ImageWidth = 0x0100,
        ImageHeight = 0x0101,
        BitsPerSample = 0x0102,
        Compression = 0x0103,
        PhotometricInterp = 0x0106,
        ThreshHolding = 0x0107,
        CellWidth = 0x0108,
        CellHeight = 0x0109,
        FillOrder = 0x010A,
        DocumentName = 0x010D,
        ImageDescription = 0x010E,
        EquipMake = 0x010F,
        EquipModel = 0x0110,
        StripOffsets = 0x0111,
        Orientation = 0x0112,
        SamplesPerPixel = 0x0115,
        RowsPerStrip = 0x0116,
        StripBytesCount = 0x0117,
        MinSampleValue = 0x0118,
        MaxSampleValue = 0x0119,
        XResolution = 0x011A,
        YResolution = 0x011B,
        PlanarConfig = 0x011C,
        PageName = 0x011D,
        XPosition = 0x011E,
        YPosition = 0x011F,
        FreeOffset = 0x0120,
        FreeByteCounts = 0x0121,
        GrayResponseUnit = 0x0122,
        GrayResponseCurve = 0x0123,
        T4Option = 0x0124,
        T6Option = 0x0125,
        ResolutionUnit = 0x0128,
        PageNumber = 0x0129,
        TransferFunction = 0x012D,
        SoftwareUsed = 0x0131,
        DateTime = 0x0132,
        Artist = 0x013B,
        HostComputer = 0x013C,
        Predictor = 0x013D,
        WhitePoint = 0x013E,
        PrimaryChromaticities = 0x013F,
        ColorMap = 0x0140,
        HalftoneHints = 0x0141,
        TileWidth = 0x0142,
        TileLength = 0x0143,
        TileOffset = 0x0144,
        TileByteCounts = 0x0145,
        InkSet = 0x014C,
        InkNames = 0x014D,
        NumberOfInks = 0x014E,
        DotRange = 0x0150,
        TargetPrinter = 0x0151,
        ExtraSamples = 0x0152,
        SampleFormat = 0x0153,
        SMinSampleValue = 0x0154,
        SMaxSampleValue = 0x0155,
        TransferRange = 0x0156,
        JPEGProc = 0x0200,
        JPEGInterFormat = 0x0201,
        JPEGInterLength = 0x0202,
        JPEGRestartInterval = 0x0203,
        JPEGLosslessPredictors = 0x0205,
        JPEGPointTransforms = 0x0206,
        JPEGQTables = 0x0207,
        JPEGDCTables = 0x0208,
        JPEGACTables = 0x0209,
        YCbCrCoefficients = 0x0211,
        YCbCrSubsampling = 0x0212,
        YCbCrPositioning = 0x0213,
        REFBlackWhite = 0x0214,
        Gamma = 0x0301,
        ICCProfileDescriptor = 0x0302,
        SRGBRenderingIntent = 0x0303,
        ImageTitle = 0x0320,
        ResolutionXUnit = 0x5001,
        ResolutionYUnit = 0x5002,
        ResolutionXLengthUnit = 0x5003,
        ResolutionYLengthUnit = 0x5004,
        PrintFlags = 0x5005,
        PrintFlagsVersion = 0x5006,
        PrintFlagsCrop = 0x5007,
        PrintFlagsBleedWidth = 0x5008,
        PrintFlagsBleedWidthScale = 0x5009,
        HalftoneLPI = 0x500A,
        HalftoneLPIUnit = 0x500B,
        HalftoneDegree = 0x500C,
        HalftoneShape = 0x500D,
        HalftoneMisc = 0x500E,
        HalftoneScreen = 0x500F,
        JPEGQuality = 0x5010,
        GridSize = 0x5011,
        ThumbnailFormat = 0x5012,
        ThumbnailWidth = 0x5013,
        ThumbnailHeight = 0x5014,
        ThumbnailColorDepth = 0x5015,
        ThumbnailPlanes = 0x5016,
        ThumbnailRawBytes = 0x5017,
        ThumbnailSize = 0x5018,
        ThumbnailCompressedSize = 0x5019,
        ColorTransferFunction = 0x501A,
        ThumbnailData = 0x501B,
        ThumbnailImageWidth = 0x5020,
        ThumbnailImageHeight = 0x5021,
        ThumbnailBitsPerSample = 0x5022,
        ThumbnailCompression = 0x5023,
        ThumbnailPhotometricInterp = 0x5024,
        ThumbnailImageDescription = 0x5025,
        ThumbnailEquipMake = 0x5026,
        ThumbnailEquipModel = 0x5027,
        ThumbnailStripOffsets = 0x5028,
        ThumbnailOrientation = 0x5029,
        ThumbnailSamplesPerPixel = 0x502A,
        ThumbnailRowsPerStrip = 0x502B,
        ThumbnailStripBytesCount = 0x502C,
        ThumbnailResolutionX = 0x502D,
        ThumbnailResolutionY = 0x502E,
        ThumbnailPlanarConfig = 0x502F,
        ThumbnailResolutionUnit = 0x5030,
        ThumbnailTransferFunction = 0x5031,
        ThumbnailSoftwareUsed = 0x5032,
        ThumbnailDateTime = 0x5033,
        ThumbnailArtist = 0x5034,
        ThumbnailWhitePoint = 0x5035,
        ThumbnailPrimaryChromaticities = 0x5036,
        ThumbnailYCbCrCoefficients = 0x5037,
        ThumbnailYCbCrSubsampling = 0x5038,
        ThumbnailYCbCrPositioning = 0x5039,
        ThumbnailRefBlackWhite = 0x503A,
        ThumbnailCopyRight = 0x503B,
        LuminanceTable = 0x5090,
        ChrominanceTable = 0x5091,
        FrameDelay = 0x5100,
        LoopCount = 0x5101,
        GlobalPalette = 0x5102,
        IndexBackground = 0x5103,
        IndexTransparent = 0x5104,
        PixelUnit = 0x5110,
        PixelPerUnitX = 0x5111,
        PixelPerUnitY = 0x5112,
        PaletteHistogram = 0x5113,
        Copyright = 0x8298,
        ExifExposureTime = 0x829A,
        ExifFNumber = 0x829D,
        ExifIFD = 0x8769,
        ICCProfile = 0x8773,
        ExifExposureProg = 0x8822,
        ExifSpectralSense = 0x8824,
        GpsIFD = 0x8825,
        ExifISOSpeed = 0x8827,
        ExifOECF = 0x8828,
        ExifVer = 0x9000,
        ExifDTOrig = 0x9003,
        ExifDTDigitized = 0x9004,
        ExifCompConfig = 0x9101,
        ExifCompBPP = 0x9102,
        ExifShutterSpeed = 0x9201,
        ExifAperture = 0x9202,
        ExifBrightness = 0x9203,
        ExifExposureBias = 0x9204,
        ExifMaxAperture = 0x9205,
        ExifSubjectDist = 0x9206,
        ExifMeteringMode = 0x9207,
        ExifLightSource = 0x9208,
        ExifFlash = 0x9209,
        ExifFocalLength = 0x920A,
        ExifMakerNote = 0x927C,
        ExifUserComment = 0x9286,
        ExifDTSubsec = 0x9290,
        ExifDTOrigSS = 0x9291,
        ExifDTDigSS = 0x9292,
        ExifFPXVer = 0xA000,
        ExifColorSpace = 0xA001,
        ExifPixXDim = 0xA002,
        ExifPixYDim = 0xA003,
        ExifRelatedWav = 0xA004,
        ExifInterop = 0xA005,
        ExifFlashEnergy = 0xA20B,
        ExifSpatialFR = 0xA20C,
        ExifFocalXRes = 0xA20E,
        ExifFocalYRes = 0xA20F,
        ExifFocalResUnit = 0xA210,
        ExifSubjectLoc = 0xA214,
        ExifExposureIndex = 0xA215,
        ExifSensingMethod = 0xA217,
        ExifFileSource = 0xA300,
        ExifSceneType = 0xA301,
        ExifCfaPattern = 0xA302
    }

    public enum PropertyTagType
    {
        PixelFormat4bppIndexed = 0,
        Byte = 1,
        ASCII = 2,
        Short = 3,
        Long = 4,
        Rational = 5,
        Undefined = 7,
        SLONG = 9,
        SRational = 10
    }

    public class Fraction
    {
        #region Class constructor

        public Fraction(int numerator, int denumerator)
        {
            Numerator = numerator;
            Denumerator = denumerator;
        }

        public Fraction(uint numerator, uint denumerator)
        {
            Numerator = Convert.ToInt32(numerator);
            Denumerator = Convert.ToInt32(denumerator);
        }

        public Fraction(int numerator)
        {
            Numerator = numerator;
            Denumerator = 1;
        }

        #endregion

        #region Numerator

        private int numerator;
        public int Numerator
        {
            get
            {
                return numerator;
            }
            set
            {
                numerator = value;
            }
        }

        #endregion

        #region Denumerator

        private int denumerator;
        public int Denumerator
        {
            get
            {
                return denumerator;
            }
            set
            {
                denumerator = value;
            }
        }

        #endregion

        #region ToString override

        public override string ToString()
        {
            if (Denumerator == 1)
                return String.Format("{0}", Numerator);
            else
                return String.Format("{0}/{1}", Numerator, Denumerator);
        }


        #endregion
    }

    public class PropertyTagValue
    {
        public static object GetValueObject(PropertyItem property)
        {
            if (property == null)
                return null;
            switch ((PropertyTagType)property.Type)
            {
                //ASCII
                case PropertyTagType.ASCII:
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    return encoding.GetString(property.Value, 0, property.Len - 1);
                //BYTE
                case PropertyTagType.Byte:
                    if (property.Len == 1)
                        return property.Value[0];
                    else
                        return property.Value;
                //LONG
                case PropertyTagType.Long:
                    uint[] resultLong = new uint[property.Len / 4];
                    for (int i = 0; i < resultLong.Length; i++)
                        resultLong[i] = BitConverter.ToUInt32(property.Value, i * 4);
                    if (resultLong.Length == 1)
                        return resultLong[0];
                    else
                        return resultLong;
                //SHORT
                case PropertyTagType.Short:
                    ushort[] resultShort = new ushort[property.Len / 2];
                    for (int i = 0; i < resultShort.Length; i++)
                        resultShort[i] = BitConverter.ToUInt16(property.Value, i * 2);
                    if (resultShort.Length == 1)
                        return resultShort[0];
                    else
                        return resultShort;
                //SLONG
                case PropertyTagType.SLONG:
                    int[] resultSLong = new int[property.Len / 4];
                    for (int i = 0; i < resultSLong.Length; i++)
                        resultSLong[i] = BitConverter.ToInt32(property.Value, i * 4);
                    if (resultSLong.Length == 1)
                        return resultSLong[0];
                    else
                        return resultSLong;
                //RATIONAL
                case PropertyTagType.Rational:
                    Fraction[] resultRational = new Fraction[property.Len / 8];
                    uint uNumerator;
                    uint uDenumerator;
                    for (int i = 0; i < resultRational.Length; i++)
                    {
                        uNumerator = BitConverter.ToUInt32(property.Value, i * 8);
                        uDenumerator = BitConverter.ToUInt32(property.Value, i * 8 + 4);
                        resultRational[i] = new Fraction(uNumerator, uDenumerator);
                    }
                    if (resultRational.Length == 1)
                        return resultRational[0];
                    else
                        return resultRational;
                //SRATIONAL
                case PropertyTagType.SRational:
                    Fraction[] resultSRational = new Fraction[property.Len / 8];
                    int sNumerator;
                    int sDenumerator;
                    for (int i = 0; i < resultSRational.Length; i++)
                    {
                        sNumerator = BitConverter.ToInt32(property.Value, i * 8);
                        sDenumerator = BitConverter.ToInt32(property.Value, i * 8 + 4);
                        resultSRational[i] = new Fraction(sNumerator, sDenumerator);
                    }
                    if (resultSRational.Length == 1)
                        return resultSRational[0];
                    else
                        return resultSRational;
                //UNDEFINE
                default:
                    if (property.Len == 1)
                        return property.Value[0];
                    else
                        return property.Value;
            }
        }
    }    

    public class EXIFReader
    {
        #region EXIFReader constructors

        public EXIFReader(Image image)
        {
            this.image = image;
        }

        public EXIFReader(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                this.image = Image.FromStream(fs);
            }
        }

        public EXIFReader(Stream stream)
        {
            this.image = Image.FromStream(stream);
        }

        #endregion

        #region Image

        private Image image;

        #endregion

        #region EXIF property indexers

        public object this[int Id]
        {
            get
            {
                try
                {
                    PropertyItem property = image.GetPropertyItem(Id);
                    return PropertyTagValue.GetValueObject(property);
                }
                catch
                {
                    return null;
                }
            }
        }

        public object this[PropertyTagId TagId]
        {
            get
            {
                try
                {
                    PropertyItem property = image.GetPropertyItem((int)TagId);
                    return PropertyTagValue.GetValueObject(property);
                }
                catch
                {
                    return null;
                }
            }
        }

        #endregion
    }
}


