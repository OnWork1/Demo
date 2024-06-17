using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Timers;
using log4net.Appender;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Logging
{
	class DisposableBufferingForwardingAppender : BufferingForwardingAppender, IDisposable
	{
		// Constants

		// Delegates

		// Events

		// Private Fields
		Timer timer;

		// Constructors
		public DisposableBufferingForwardingAppender()
		{
			timer = new Timer()
			{ 
				AutoReset = true,
				Enabled = true,
				Interval = 5000
			};
			timer.Elapsed += DoFlush;
			AppDomain.CurrentDomain.DomainUnload += DoFlush;
			AppDomain.CurrentDomain.ProcessExit += DoFlush;
			AppDomain.CurrentDomain.UnhandledException += DoFlush;
			//jeste musim porobit na tym, zatim, kdyz vypnu aplikaci ve VisualStudiu, tak se nedeje nic a o logy prichazim :(
		}

		public DisposableBufferingForwardingAppender(double interval)
			:this()
		{
			this.Interval = interval;
		}

		// Private Properties

		// Protected Properties

		// Public Properties

		double Interval
		{
			get { return this.timer.Interval; }
			set
			{
				if (value < 1)
				{
					this.timer.Enabled = false;
				}
				else
				{
					this.timer.Interval = value;
					this.timer.Enabled = true;
				}
			}
		}

		// Private Methods
		private void DoFlush(object sender, System.EventArgs eventArgs)
		{
			this.Flush();
		}

		private void ResetTimer()
		{
			if (this.timer.Enabled)
			{
				this.timer.Stop();
				this.timer.Start();
			}
		}

		// Protected Methods
		//protected override bool PreAppendCheck()
		//{
		//    //this.ResetTimer();
		//    return base.PreAppendCheck();
		//}

		protected override void Append(log4net.Core.LoggingEvent loggingEvent)
		{
			this.ResetTimer();
			base.Append(loggingEvent);
		}

		// Public Methods

		// Event Handlers


		public void Dispose()
		{
			try
			{
				this.Flush();
			}
			catch (Exception ex) { throw ex; }
			try
			{
				this.OnClose();
			}
			catch (Exception ex) { throw ex; }
		}

		~DisposableBufferingForwardingAppender()
		{
			try
			{
				this.Flush();
			}
			catch (Exception ex) { throw ex; }
		}
	}
}
