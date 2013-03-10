using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Ninject;

namespace PopForums.Configuration
{
	public class NinjectDependencyResolver : IDependencyResolver
	{
		public NinjectDependencyResolver(IKernel kernel)
		{
			if (kernel == null)
				throw new ArgumentNullException("kernel", "Can't set Ninject Kernel for DependencyResolver. Make sure the Kernel has been initialized, and that you don't have an app start up ordering issue.");
			_kernel = kernel;
		}

		private readonly IKernel _kernel;

		public object GetService(Type serviceType)
		{
			return _kernel.TryGet(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return _kernel.GetAll(serviceType);
		}
	}
}