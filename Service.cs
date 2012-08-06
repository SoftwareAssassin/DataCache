///
/// Original Author: Software Assassin
/// 
/// GNU All-Permissive License:
/// Copying and distribution of this file, with or without modification,
/// are permitted in any medium without royalty provided the copyright
/// notice and this notice are preserved.  This file is offered as-is,
/// without any warranty.
/// 
/// Source code available at:
/// https://github.com/SoftwareAssassin/AssassinLibrary
/// 
/// Professional support available at:
/// http://www.softwareassassin.com
/// 

using System;
using Assassin;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Configuration;

namespace Assassin.DataCache
{
	partial class Service : BackgroundService
	{
		#region Static Members
		private static Service m_portal = null;
		public static Service Portal
		{
			get
			{
				return Service.m_portal;
			}
		}

		private static List<ClientConnection> m_clients = null;
		public static List<ClientConnection> Clients
		{
			get
			{
				return Service.m_clients;
			}
		}
		#endregion
		#region Members
		private bool connecting = false;
		#endregion

		public Service()
			: base()
		{
			InitializeComponent();
			this.Init();
		}
		~Service()
		{
			this.HandleDispose();
		}

		private void Init()
		{
			Service.m_portal = this;
			Service.m_clients = new List<ClientConnection>();
		}
		private void HandleDispose()
		{
			Service.m_clients.Clear();
			Service.m_clients = null;
		}

		private void HandleStart()
		{
			IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ConfigurationManager.AppSettings["listenIpAddress"])
				, System.Convert.ToInt32(ConfigurationManager.AppSettings["listenPort"])
				);
			Socket s = new Socket(ep.Address.AddressFamily
				, SocketType.Stream
				, ProtocolType.Tcp
				);

			s.Bind(ep);
			s.Listen(System.Convert.ToInt32(ConfigurationManager.AppSettings["connectionsBacklogSize"]));

			this.connecting = true;

			//wait for incoming connections
			while (this.State == BackgroundServiceState.Running)
			{
				ClientConnection cc = new ClientConnection(s.Accept());
				Thread t = new Thread(delegate() { cc.Execute(); });

				t.Start();
				Service.Clients.Add(cc);
			}

			this.connecting = false;
		}
		private void HandleStop()
		{
			//wait until we're no longer accepting connections
			while (this.connecting)
				Thread.Sleep(50);

			//destroy all active connections
			for (int i = 0; i < Service.Clients.Count; i++)
				Service.Clients[i].Stop();

			while (Service.Clients.Count > 0)
			{
				while (!Service.Clients[0].Stopped)
					Thread.Sleep(50);

				Service.Clients[0] = null;
				Service.Clients.RemoveAt(0);
			}
		}

		#region Event Handlers
		protected override void OnStart(string[] args)
		{
			base.OnStart(args);
			this.HandleStart();
		}
		protected override void OnStop()
		{
			this.HandleStop();
			base.OnStop();
		}
		protected override void OnPause()
		{
			this.HandleStop();
			base.OnPause();
		}
		#endregion
	}

	public class ClientConnection
	{
		#region Members
		private Socket m_connection = null;
		private int m_requests = 0;
		private BackgroundServiceState m_state = BackgroundServiceState.Initializing;
		private bool listening = false;
		#endregion
		#region Properties
		public Socket Connection
		{
			get
			{
				return this.m_connection;
			}
		}
		public int Requests
		{
			get
			{
				return this.m_requests;
			}
		}
		public BackgroundServiceState State
		{
			get
			{
				return this.m_state;
			}
		}
		public bool Stopped
		{
			get
			{
				return !this.listening;
			}
		}
		#endregion

		public ClientConnection(Socket s)
		{
			this.m_connection = s;
			this.Init();
		}
		~ClientConnection()
		{
			this.Dispose();
		}

		private void Init()
		{
			this.m_state = BackgroundServiceState.Idle;
		}
		private void Dispose()
		{
			//TODO
		}

		/// <summary>
		/// Get's called by Server to stop this thread
		/// </summary>
		public void Stop()
		{
			this.m_state = BackgroundServiceState.Idle;
		}

		/// <summary>
		/// Waits for incoming messages on this connection
		/// </summary>
		public void Execute()
		{
			this.m_state = BackgroundServiceState.Running;
			this.listening = true;

			while (this.State == BackgroundServiceState.Running)
			{
				byte[] buffer = null;
				this.Connection.Receive(buffer);
			}

			this.listening = false;
		}
	}
}
