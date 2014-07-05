using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ActiveUp.Net
{
	/// <summary>
	/// Summary description for TimedUdpClient.
	/// </summary>
	[System.Serializable]
	public class TimedUdpClient : UdpClient
	{
		/// <summary>
		/// The result of the receive (udp mode).
		/// </summary>
		private byte[] _returnReceive;

		/// <summary>
		/// Indicates if an error occurs.
		/// </summary>
		private bool _errorOccurs = false;

		/// <summary>
		///The main job of this thread is to receive data in udp.
		/// </summary>
		private Thread _threadReceive;

		/// <summary>
		/// Represents a network endpoint as an IP address and a port number.
		/// </summary>
		private IPEndPoint _remote;

		/// <summary>
		/// Protects integrity of the _returnReceive variable.
		/// </summary>
		Mutex _mutexReturnReceive = new Mutex(false);

		/// <summary>
		/// Protects integrity of the _errorOccurs variable.
		/// </summary>
		Mutex _mutexErrorOccurs = new Mutex(false);

		/// <summary>
		/// Timeout of the underlying UdpClient.
		/// </summary>
		private int _timeout = 2000; // 2 sec default

		/// <summary>
		/// Gets or sets the timeout of the underlying UdpClient.
		/// </summary>
		public int Timeout 
		{
			get { return _timeout; }
			set { _timeout = value; }
		}
		/// <summary>
		/// Receives data.
		/// </summary>
		/// <param name="remote">Server pointer.</param>
		/// <returns>The received data.</returns>
		public new byte[] Receive(ref IPEndPoint remote)
		{
			_remote = remote;

            _threadReceive = new Thread(new ThreadStart(StartReceive));
			_threadReceive.Start();

			Thread.Sleep(_timeout);
			
			_mutexErrorOccurs.WaitOne();
			if (_errorOccurs == true)
			{
				_mutexErrorOccurs.ReleaseMutex();
				_threadReceive.Abort();
				throw new Exception("Connection timed out");
			}
			else
				_mutexErrorOccurs.ReleaseMutex();
			
			return _returnReceive;
		}

		/// <summary>
		/// Receive thread.
		/// </summary>
		private void StartReceive()
		{
			_mutexErrorOccurs.WaitOne();
			_errorOccurs = true;
			_mutexErrorOccurs.ReleaseMutex();

			try
			{
				byte[] ret = base.Receive(ref _remote);
				_mutexReturnReceive.WaitOne();
				_returnReceive = ret;
				_mutexReturnReceive.ReleaseMutex();
				_errorOccurs = false;
				
			}

			catch (SocketException) 
			{
			}
			
			catch (ThreadAbortException)
			{
			}
		}
	}
}
