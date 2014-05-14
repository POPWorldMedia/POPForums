using System;
using System.Threading;

namespace PopForums.WebJob
{
	class Program
	{
		static void Main(string[] args)
		{
			while (true)
			{
				Console.WriteLine("Party's over here y'all!");
				Thread.Sleep(5000);	
			}
		}
	}
}
