using Stashbox.Utils;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Stashbox.Extension.Wcf
{
    public abstract class StashboxServiceHostFactoryBase : ServiceHostFactory
    {
        protected internal static IStashboxContainer Container { get; private set; }

        protected static IDictionary<Type, Action<ServiceHostBase>> OnHostCreatedActions { get; }

        static StashboxServiceHostFactoryBase()
        {
            OnHostCreatedActions = new Dictionary<Type, Action<ServiceHostBase>>();
        }

        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            var serviceHostBase = base.CreateServiceHost(constructorString, baseAddresses);

            if (OnHostCreatedActions.ContainsKey(serviceHostBase.Description.ServiceType))
                OnHostCreatedActions[serviceHostBase.Description.ServiceType].Invoke(serviceHostBase);

            return serviceHostBase;
        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var serviceHost = default(ServiceHost);
            var serviceMetaData = serviceType.GetMetaData();

            if (serviceMetaData.IsSingletonConfiguration)
                serviceHost = CreateSingletonServiceHost(serviceMetaData, baseAddresses);
            else
            {
                serviceHost = CreateDefaultServiceHost(serviceMetaData, baseAddresses);
                serviceHost.Opening += (sender, args) => serviceHost.AddServiceBehavior(new StashboxDependencyInjectionServiceBehavior(Container.Resolve<Func<StashboxInstanceProvider>>()));
            }

            if (StashboxConfig.EnablePerSericeOperationLifetime)
                serviceHost.AddOperationBehavior(new StashboxDependencyInjectionOperationBehavior(Container.Resolve<Func<StashboxDependencyInjectionParameterInspector>>()));

            return serviceHost;
        }

        protected abstract ServiceHost CreateDefaultServiceHost(ServiceMetadata serviceMetadata, Uri[] baseAddresses);

        protected abstract ServiceHost CreateSingletonServiceHost(ServiceMetadata serviceMetadata, Uri[] baseAddresses);

        public static void RegisterOnHostCreatedAction(Type serviceType, Action<ServiceHostBase> onHostCreatedAction)
        {
            Shield.EnsureTrue(!OnHostCreatedActions.ContainsKey(serviceType), $"Cannot register multiple actions for a specific service. Service Type: {serviceType.FullName}");
            Shield.EnsureNotNull(onHostCreatedAction, nameof(onHostCreatedAction));

            OnHostCreatedActions.Add(serviceType, onHostCreatedAction);
        }

        public static void SetContainer(IStashboxContainer container)
        {
            Shield.EnsureTrue(Container == null, $"{nameof(Container)} is already initialized.");
            Shield.EnsureNotNull(container, nameof(container));

            Container = container;
        }
    }
}
