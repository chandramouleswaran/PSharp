﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Microsoft.PSharp.Timers
{
	/// <summary>
	/// Wrapper class for a system timer.
	/// </summary>
	public class TimerProduction : Machine
	{
		#region fields

		/// <summary>
		/// Specified if periodic timeout events are desired.
		/// </summary>
		private bool IsPeriodic;

		/// <summary>
		/// Specify the periodicity of timeout events.
		/// </summary>
		private int period;

		/// <summary>
		/// Machine to which eTimeout events are dispatched.
		/// </summary>
		private MachineId client;

		/// <summary>
		/// System timer to generate Elapsed timeout events in production mode.
		/// </summary>
		private System.Timers.Timer timer;

		/// <summary>
		/// Flag to prevent timeout events being sent after stopping the timer.
		/// </summary>
		private volatile bool IsTimerEnabled = false;

		/// <summary>
		/// Used to synchronize the Elapsed event handler with timer stoppage.
		/// </summary>
		private readonly Object tlock = new object();

		#endregion

		#region states
		[Start]
		[OnEntry(nameof(InitializeTimer))]
		[OnEventDoAction(typeof(HaltTimer), nameof(DisposeTimer))]
		private class Init : MachineState { }

		#endregion

		#region handlers
		private void InitializeTimer()
		{
			InitTimer e = (this.ReceivedEvent as InitTimer);
			this.client = e.client;
			this.IsPeriodic = e.IsPeriodic;
			this.period = e.period;

			this.IsTimerEnabled = true;
			this.timer = new System.Timers.Timer(period);

			if (!IsPeriodic)
			{
				this.timer.AutoReset = false;
			}

			this.timer.Elapsed += ElapsedEventHandler;
			this.timer.Start();
		}
		#endregion

		#region private methods
		/// <summary>
		/// Handler for the Elapsed event generated by the system timer.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void ElapsedEventHandler(Object source, ElapsedEventArgs e)
		{
			lock (this.tlock)
			{
				if (this.IsTimerEnabled)
				{
					Runtime.SendEvent(this.client, new eTimeout(this.Id));
				}
			}
		}

		private void DisposeTimer()
		{
			lock (this.tlock)
			{
				MachineId ReceivedClient = (this.ReceivedEvent as HaltTimer).client;

				// The client attempting to stop this timer must be the one who created it.
				this.Assert(ReceivedClient == this.client);

				this.IsTimerEnabled = false;
				this.timer.Stop();
				this.timer.Dispose();
				this.Raise(new Halt());
			}
		}
		#endregion

	}
}