using Godot;
using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace Debug
{
	public static class TimeLogger
	{
		public class TimeScope : IDisposable
		{
			private string _name;
			private int _logIndex;
			private TimeSpan _startTime;
			
			public TimeScope(string name)
			{
				_name = name;
				_logIndex = PendingLogs.Count;
				PendingLogs.Add(null);
				
				if (NestLevel == 0)
				{
					Watch.Restart();
				}
				_startTime = Watch.Elapsed;
				NestLevel++;
			}
			
			void IDisposable.Dispose()
			{
				NestLevel--;
				TimeSpan endTime = Watch.Elapsed;
				if (NestLevel == 0)
				{
					Watch.Stop();
				}
				
				// Create log and insert into it's spot in the stack
				string log = $"{_name}: {(endTime - _startTime).TotalMilliseconds:0.00} ms";
				PendingLogs[_logIndex] = log.PadLeft(NestLevel + log.Length, Indent);

				if (_logIndex == 0)
				{
					foreach(string toLog in PendingLogs)
					{
						GD.Print(toLog);
					}
					PendingLogs.Clear();
				}
			}
		}
		
		private static Stopwatch Watch = new Stopwatch();
		private static int NestLevel = 0;
		
		private static List<string> PendingLogs = new List<string>();
		private static char Indent = '-';
	}
}
