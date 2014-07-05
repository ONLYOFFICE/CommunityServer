using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AppLimit.CloudComputing.SharpBox.Common.IO
{
    /// <summary>
    /// Defines the action which will done from the streamhelper after calling the callback
    /// </summary>
    internal enum StreamHelperResultCodes
    {
        /// <summary>
        /// go forward with stream transfer
        /// </summary>
        OK,

        /// <summary>
        /// abort stream transfer
        /// </summary>
        Aborted,

        /// <summary>
        /// Parameters are invalid
        /// </summary>
        InvalidParameter            
    }

    internal class StreamHelperResult
    {
        public StreamHelperResultCodes ResultCode;

        public long TransferedBytes;
    }

    internal class StreamHelperProgressEvent : EventArgs
    {
        /// <summary>
        /// Amount of bytes transfered in during this process
        /// </summary>
        public long ReadBytesTotal { get; set; }

        /// <summary>
        /// Amount of bytes which has to be transfered in this process
        /// </summary>
        public long TotalLength { get; set; }

        /// <summary>
        /// Amount of bytes transfered between this and the last event
        /// </summary>
        public long ReadBytesCurrentOperation { get; set; }        
                
        /// <summary>
        /// The transfer rate in KBits per Second related to bytes totally transfered (ReadBytesTotal)
        /// </summary>
        public long TransferRateTotal{ get; set; } 

        /// <summary>
        /// The transfer rate in KBits per Second related to the last 500ms 
        /// </summary>
        public long TransferRateCurrent { get; internal set; }        

        /// <summary>
        /// Overall progress in percent
        /// </summary>
        public int PercentageProgress
        {
            get 
            {
                if (TotalLength == -1)
                    return -1;
                else if (TotalLength == 0)
                    return 100;
                else
                    return (int)((100 * ReadBytesTotal) / TotalLength);
            }
        }
    }

    internal delegate StreamHelperResultCodes StreamHelperProgressCallback(object sender, StreamHelperProgressEvent e, params Object[] data);

    internal class StreamHelper
    {
        private static int _BufferSize = 4096;

        public static StreamHelperResult CopyStreamData(Object sender, Stream src, Stream trg, StreamHelperProgressCallback status, params Object[] data)
        {
            return CopyStreamData(sender, src, trg, -1, status, data);
        }

        public static StreamHelperResult CopyStreamData(Object sender, Stream src, Stream trg, long MaxSize, StreamHelperProgressCallback status, params Object[] data)
        {
            // validate parameter
            if (src == null || trg == null)
                return new StreamHelperResult() { ResultCode = StreamHelperResultCodes.InvalidParameter };

            if (src.CanRead == false || trg.CanWrite == false)
                return new StreamHelperResult() { ResultCode = StreamHelperResultCodes.InvalidParameter };

            // build the buffer as configured
            byte[] buffer = new byte[_BufferSize];

            // set the real buffer size
            int RealBufferSize = _BufferSize;

            // build the event for the status callback
            StreamHelperProgressEvent e = new StreamHelperProgressEvent();

            // copy the stream data
            int readBytes = 0;
            int readBytesTotal = 0;
            int readBytes500msFrame = 0;

            DateTime dtStart = DateTime.Now;
            DateTime dt500MsWatch = DateTime.Now;
            TimeSpan ts500MsWatch = new TimeSpan();

            // set the total length if possible
            try
            {
                e.TotalLength = src.Length;
            }
            catch (Exception)
            {
                if (MaxSize != -1)
                    e.TotalLength = MaxSize;
                else
                    e.TotalLength = -1;
            }

            if (MaxSize != -1 && e.TotalLength > MaxSize)
                e.TotalLength = MaxSize;

            do
            {
                // Read the bytes
                readBytes = src.Read(buffer, 0, RealBufferSize);

                // add
                readBytesTotal += readBytes;
                readBytes500msFrame += readBytes;

                // check for interuption
                if (readBytes <= 0)
                    break;

                // Write the bytes
                trg.Write(buffer, 0, readBytes);
                
                // notify state
                if (status != null)
                {
                    StreamHelperResultCodes action = StreamHelperResultCodes.OK;

                    // upadte the event                    
                    e.ReadBytesTotal = readBytesTotal;
                    e.ReadBytesCurrentOperation = readBytes;                    
                    
                    // call the callback
                    action = status(sender, e, data);                        

                    // result
                    if (action == StreamHelperResultCodes.Aborted)
                        return new StreamHelperResult() { ResultCode = StreamHelperResultCodes.Aborted, TransferedBytes = readBytesTotal };
                }          
      
                // stop measurement
                DateTime dtLocalStop = DateTime.Now;
                
                // set the 500 ms span 
                ts500MsWatch = dtLocalStop - dt500MsWatch;

                // check if we achieved 500 ms
                if (ts500MsWatch.TotalMilliseconds >= 500)
                {                    
                    // update the current transfer rate                    
                    e.TransferRateCurrent = readBytes500msFrame / Convert.ToInt64(ts500MsWatch.TotalMilliseconds);
                    // bits per millisecond == kbits per second
                    e.TransferRateCurrent *= 8;

                    // reset the bytes
                    readBytes500msFrame = 0;

                    // reset the timespan
                    ts500MsWatch = new TimeSpan();

                    // reset the start timer
                    dt500MsWatch = DateTime.Now;                   
                }

                // recalc the overall transfer rate
                TimeSpan consumedTimeAllOver = dtLocalStop - dtStart;

                if (Convert.ToInt64(consumedTimeAllOver.TotalMilliseconds) > 0)
                {
                    // bytes per millisecond
                    e.TransferRateTotal = readBytesTotal / Convert.ToInt64(consumedTimeAllOver.TotalMilliseconds);

                    // bits per millisecond == kbits per second
                    e.TransferRateTotal *= 8;                    
                }
                else
                    e.TransferRateTotal = -1;

                // check the max size 
                if (MaxSize != -1)
                {
                    if (readBytesTotal >= MaxSize)
                        break;

                    // check if we have to asjust the buffer size
                    if (MaxSize - readBytesTotal < RealBufferSize)
                    {
                        RealBufferSize = Convert.ToInt32(MaxSize - readBytesTotal);
                    }
                }

                // check if we have 
            } while (readBytes > 0);

            return new StreamHelperResult() { ResultCode = StreamHelperResultCodes.OK, TransferedBytes = readBytesTotal };
        }

        public static MemoryStream ToStream(String data)
        {
            // create the memory stream
            MemoryStream mStream = new MemoryStream();

            // write the data into
            StreamWriter sw = new StreamWriter(mStream);
            sw.Write(data);
            sw.Flush();

            // reset position
            mStream.Position = 0;

            // go ahead
            return mStream;
        }

        public static TimeSpan CalculateOperationTransferTime(StreamHelperProgressEvent e)
        {            
            // calc transfertime            
            if (e.TransferRateTotal != -1 && e.TransferRateTotal > 0)
            {
                long bytesPerSecond = (e.TransferRateTotal / 8) * 1000;

                if (bytesPerSecond > 0)
                {
                    long neededSeconds = (e.TotalLength - e.ReadBytesTotal) / bytesPerSecond;
                    return new TimeSpan(neededSeconds * TimeSpan.TicksPerSecond);
                }
                else
                    return new TimeSpan(long.MaxValue);
            }
            else
                return new TimeSpan(long.MaxValue);                       
        }
    }
}
