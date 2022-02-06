using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppGyga
{
	public class ForkJoinSort<T> where T : IComparable<T>
	{


		public async Task Sort(T[] a)
		{
			var arrs = Divide(a);

			List<Task> tasks = new List<Task>();
			foreach (var arr in arrs)
			{
				var tmp = arr;
				tasks.Add(Task.Run(() => { Array.Sort(tmp); }));
			}


			await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);

			var list = new List<Arr>();
			for (int i = 0; i < arrs.Count; i++)
			{
				list.Add(new Arr()
				{
					a = arrs[i],
					ptr = 0
				});
			}
			Merge(a, list);


		}
		private class Arr
		{
			public T[] a;
			public int ptr;
		}

		private static void Merge(T[] destArr, List<Arr> arrs)
		{
			T minValue;
			Arr min;

			for (int i = 0; i < destArr.Length; i++)
			{
				var firstArr = arrs.First();
				minValue = firstArr.a[firstArr.ptr];
				min = firstArr;

				for (int j = 1; j < arrs.Count; j++)
				{
					if (arrs[j].a[arrs[j].ptr].CompareTo(minValue) < 0)
					{
						minValue = arrs[j].a[arrs[j].ptr];
						min = arrs[j];
					}
				}

				destArr[i] = minValue;
				min.ptr++;

				if (min.ptr >= min.a.Length)
				{
					arrs.Remove(min);
				}
			}
		}

		private List<T[]> Divide(T[] a)
		{
			List<T[]> arrs = new List<T[]>();
			int totalWorkers = Environment.ProcessorCount;
			int divisionSize = a.Length / totalWorkers;

			for (int i = 0; i < totalWorkers - 1; i++)
			{
				var arr = new T[divisionSize];
				Array.Copy(a, divisionSize * i, arr, 0, arr.Length);
				arrs.Add(arr);
			}
			var ar = new T[a.Length - (divisionSize * arrs.Count)];
			Array.Copy(a, (divisionSize * arrs.Count), ar, 0, ar.Length);
			arrs.Add(ar);

			return arrs;

		}
	}
}
