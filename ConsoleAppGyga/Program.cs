// See https://aka.ms/new-console-template for more information


using ConsoleAppGyga;
using System.Diagnostics;

Console.WriteLine("Hello, World!");

var arrayP = GenRandomArray<int>(16_000_000);
Console.WriteLine($"Array.AsParallel().Sort -- start ");
Stopwatch approach7Stopwatch = new Stopwatch();
approach7Stopwatch.Start();
arrayP.AsParallel().OrderBy(x => x).ToArray();
approach7Stopwatch.Stop();
Console.WriteLine($"AsParallel.Sort - Is sorted? {IsSorted(arrayP)}. ElapsedMS={approach7Stopwatch.ElapsedMilliseconds}");

var arrayTmps = GenRandomArray<int>(16_000_000);
Console.WriteLine($"Array.Sort -- start ");
Stopwatch approach0Stopwatch = new Stopwatch();
approach0Stopwatch.Start();
Array.Sort(arrayTmps);
approach0Stopwatch.Stop();
Console.WriteLine($"Array.Sort - Is sorted? {IsSorted(arrayTmps)}. ElapsedMS={approach0Stopwatch.ElapsedMilliseconds}");

var arrayTmp = GenRandomArray<int>(16_000_000);
Console.WriteLine($"QuickSortIterative -- start ");
Stopwatch approachStopwatch = new Stopwatch();
approachStopwatch.Start();
InterativeSort.QuickSortIterative(arrayTmp, 0, arrayTmp.Length - 1);
approachStopwatch.Stop();
Console.WriteLine($"QuickSort.Iterative - Is sorted? {IsSorted(arrayTmp)}. ElapsedMS={approachStopwatch.ElapsedMilliseconds}");

var approach5Array = GenRandomArray<int>(16_000_000);
Console.WriteLine($"Array.Asynk().Sort -- start ");
Stopwatch approach5Stopwatch = new Stopwatch();
approach5Stopwatch.Start();
var trf = new ForkJoinSort<int>();
await trf.Sort(approach5Array);
approach5Stopwatch.Stop();
Console.WriteLine($"ForkJoinSort - Is sorted? {IsSorted(approach5Array)}. ElapsedMS={approach5Stopwatch.ElapsedMilliseconds}");



/*  ----------------------  */

//AsParallel.Sort - Is sorted? False.ElapsedMS= 3713
//Array.Sort - Is sorted? True.ElapsedMS= 1754
//QuickSort.Iterative - Is sorted? True.ElapsedMS= 4829
//ForkJoinSort - Is sorted? True.ElapsedMS= 2774
//Barrier.Sort - Is sorted? True.ElapsedMS= 1391

/*  ----------------------  */


var array = GenRandomArray<int>(16_000_000);
int[] auxArray = new int[array.Length];

int totalWorkers = Environment.ProcessorCount; // must be power of two

// Number of elements for each array, if the elements 
int partitionSize = array.Length / totalWorkers;
//     number is not divisible by the workers, the remainders 
//     will be added to the first worker (the main thread)
int remainder = array.Length % totalWorkers;

// number of iterations is determined by Log(workers), 
//     this is why the workers has to be power of 2
int iterations = (int)Math.Log(totalWorkers, 2);


Barrier barrier = new Barrier(totalWorkers, (b) =>
{
    //partitionSize <<= 1;
    Interlocked.Decrement(ref partitionSize);
    //Display(partitionSize);
    var temp = auxArray;
    auxArray = array;
    array = temp;
});


Action<object> workAction = (obj) =>
{
    int index = (int)obj;
    //calculate the partition boundary
    int low = index * partitionSize;
    if (index > 0)
        low += remainder;
    int high = (index + 1) * partitionSize - 1 + remainder;
    int dif =  high - low;
    //InterativeSort.QuickSortIterative(array, low, high);
    Array.Sort(array, low, dif);
    barrier.SignalAndWait();

    for (int j = 0; j < iterations; j++)
    {
        //we always remove the odd workers
        if (index % 2 == 1)
        {
            barrier.RemoveParticipant();
            break;
        }

        int newHigh = high + partitionSize / 2;
        index >>= 1; // update the index after removing the zombie workers
                     // if should be hunky-dory
                     // Interlocked.Increment(ref index);
        //Display(index);
        high = newHigh;
        barrier.SignalAndWait();
    }
};


Console.WriteLine($"Barrier.Sort -- start ");

Stopwatch approach1Stopwatch = new Stopwatch();
approach1Stopwatch.Start();

// worker tasks, -1 because the main thread will be used as a worker too
//int totalWorkers = Environment.ProcessorCount;
Task[] workers = new Task[totalWorkers - 1];
for (int i = 0; i < workers.Length; i++)
{
    workers[i] = Task.Factory.StartNew(obj => workAction(obj), i + 1);
}
workAction(0);

if (iterations % 2 != 0)
    Array.Copy(auxArray, array, array.Length); //here is Merge(array, auxArray, low, high, high + 1, newHigh);


approach1Stopwatch.Stop();
Console.WriteLine($"Barrier.Sort - Is sorted? {IsSorted(auxArray)}. ElapsedMS={approach1Stopwatch.ElapsedMilliseconds}");




T[] GenRandomArray<T>(int size = 10000)
{
	var a = new T[size];
	Random r = new Random();

	for (int i = 0; i < size; i++)
	{
		a[i] = (T)Convert.ChangeType(r.Next(Int32.MinValue, Int32.MaxValue), typeof(T));
	}

	return a;

}

bool IsSorted<T>(T[] a) where T : IComparable<T>
{
    if (!a.Any())
        return true;

    var prev = a.First();

    for (int i = 1; i < a.Length; i++)
    {
        if (a[i].CompareTo(prev) < 0)
            return false;

        prev = a[i];
    }

    return true;
}

void Display(int x) => Console.WriteLine($"{Convert.ToString(x, toBase: 2),8}");




