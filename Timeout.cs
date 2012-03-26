using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RemObjects.InternetPack.XMPP
{
	class Timer: IDisposable // mono has issues with timers
	{
		private Action<Object> fCallback;

		public Timer(Action<Object> Callback)
		{
			fCallback = Callback;
		}
		private volatile bool fRepeat;
		private TimeSpan fDelay;
		private volatile bool fStop;
		private System.Threading.AutoResetEvent fWaitEvent = new System.Threading.AutoResetEvent(false);

		public void Dispose()
		{
			fStop = true;
			fWaitEvent.Set();
			new Thread(ThreadStart).Start();
		}

		public void Set(TimeSpan val, bool repeat)
		{
			fRepeat = repeat;
			fDelay = val;
			fWaitEvent.Set();
		}

		public void ThreadStart()
		{
			while (true)
			{
				var lTotalSec = fDelay;
				var lCB = fCallback;
				if (lTotalSec.TotalSeconds < 0)
				{
					fWaitEvent.WaitOne();
					continue;
				}
				else
				{
					if (fWaitEvent.WaitOne(lTotalSec))
						continue; // don't want that
				}
				if (fStop) return;
				if (fCallback == lCB)
				{
					lCB(null);
					if (!fRepeat)
					{
						fDelay = TimeSpan.FromMinutes(-1);
					}
				}
			}
		}
	}
}
