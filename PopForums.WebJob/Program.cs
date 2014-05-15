using System;
using System.Threading;
using Ninject;
using PopForums.Data.Azure;
using PopForums.Web;

namespace PopForums.WebJob
{
	class Program
	{
		static void Main(string[] args)
		{
			PopForumsActivation.InitializeOutOfWeb();
			PopForumsActivation.Kernel.Load(new AzureInjectionModule());
			while (true)
			{
				foreach (var item in PopForumsActivation.ApplicationServices)
				{
					Console.WriteLine("{0}, IsRunning: {1}, {2}", item.Name, item.IsRunning, item.LastExecutionTime);
				}
				Thread.Sleep(60000);	
			}
		}
	}
}
