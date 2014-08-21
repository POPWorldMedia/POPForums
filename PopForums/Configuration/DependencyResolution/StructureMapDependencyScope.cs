// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StructureMapDependencyScope.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;
using Microsoft.Practices.ServiceLocation;
using StructureMap;

namespace PopForums.Configuration.DependencyResolution
{
	/// <summary>
	/// The structure map dependency scope.
	/// </summary>
	public class StructureMapDependencyScope : ServiceLocatorImplBase, IDependencyScope
	{
		private const string NestedContainerKey = "Nested.Container.Key";

		public StructureMapDependencyScope(IContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}
			Container = container;
		}

		public IContainer Container { get; set; }
		public IContainer CurrentNestedContainer
		{
			get { return (IContainer) HttpContext.Items[NestedContainerKey]; }
			set { HttpContext.Items[NestedContainerKey] = value; }
		}

		private HttpContextBase HttpContext
		{
			get
			{
				var context = Container.TryGetInstance<HttpContextBase>();
				return context ?? new HttpContextWrapper(System.Web.HttpContext.Current);
			}
		}

		public void CreateNestedContainer()
		{
			if (CurrentNestedContainer != null)
			{
				return;
			}
			CurrentNestedContainer = Container.GetNestedContainer();
		}

		public void Dispose()
		{
			if (CurrentNestedContainer != null)
			{
				CurrentNestedContainer.Dispose();
			}

			Container.Dispose();
		}

		public void DisposeNestedContainer()
		{
			if (CurrentNestedContainer != null)
			{
				CurrentNestedContainer.Dispose();
			}
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return DoGetAllInstances(serviceType);
		}

		protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
		{
			return (CurrentNestedContainer ?? Container).GetAllInstances(serviceType).Cast<object>();
		}

		protected override object DoGetInstance(Type serviceType, string key)
		{
			var container = (CurrentNestedContainer ?? Container);

			if (string.IsNullOrEmpty(key))
			{
				return serviceType.IsAbstract || serviceType.IsInterface
					? container.TryGetInstance(serviceType)
					: container.GetInstance(serviceType);
			}

			return container.GetInstance(serviceType, key);
		}
	}
}