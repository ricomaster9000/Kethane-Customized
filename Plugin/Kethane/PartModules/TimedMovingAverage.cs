using System;
using System.Collections.Generic;

namespace Kethane.PartModules
{
	// Token: 0x02000035 RID: 53
	internal class TimedMovingAverage
	{
		// Token: 0x06000173 RID: 371 RVA: 0x00009984 File Offset: 0x00007B84
		public TimedMovingAverage(double interval, double initialValue = 0.0)
		{
			this.interval = interval;
			this.values.Enqueue(new TimedMovingAverage.TimedValue(interval, initialValue));
		}

		// Token: 0x06000174 RID: 372 RVA: 0x000099B0 File Offset: 0x00007BB0
		public void Update(double time, double value)
		{
			this.values.Enqueue(new TimedMovingAverage.TimedValue(time, value));
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x06000175 RID: 373 RVA: 0x000099C4 File Offset: 0x00007BC4
		public double Average
		{
			get
			{
				double num = 0.0;
				double num2 = 0.0;
				int num3 = this.values.Count;
				using (Queue<TimedMovingAverage.TimedValue>.Enumerator enumerator = this.values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						TimedMovingAverage.TimedValue timedValue = enumerator.Current;
						num3--;
						if (num + timedValue.Time > this.interval)
						{
							num2 += timedValue.Value * (this.interval - num);
							break;
						}
						num += timedValue.Time;
						num2 += timedValue.Value * timedValue.Time;
					}
					goto IL_A6;
				}
				IL_96:
				num3--;
				this.values.Dequeue();
				IL_A6:
				if (num3 <= 0)
				{
					return num2 / this.interval;
				}
				goto IL_96;
			}
		}

		// Token: 0x040000D0 RID: 208
		private readonly Queue<TimedMovingAverage.TimedValue> values = new Queue<TimedMovingAverage.TimedValue>();

		// Token: 0x040000D1 RID: 209
		private readonly double interval;

		// Token: 0x0200005A RID: 90
		private struct TimedValue
		{
			// Token: 0x06000223 RID: 547 RVA: 0x0000AADC File Offset: 0x00008CDC
			public TimedValue(double time, double value)
			{
				this.Time = time;
				this.Value = value;
			}

			// Token: 0x04000158 RID: 344
			public readonly double Time;

			// Token: 0x04000159 RID: 345
			public readonly double Value;
		}
	}
}
