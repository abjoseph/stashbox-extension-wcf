﻿using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;

namespace Stashbox.Extension.Wcf
{
    class StashboxServiceHost : ServiceHost
    {
        private readonly bool _isSingleConfiguration;

        public StashboxServiceHost(object singletonInstance, params Uri[] baseAddresses) : base(singletonInstance, baseAddresses)
        {
            _isSingleConfiguration = true;
        }

        public StashboxServiceHost(Type serviceType, params Uri[] baseAddresses) : base(serviceType, baseAddresses) { }

        protected StashboxServiceHost() { }

        protected override void OnClosing()
        {
            CleanUpPerServinceInstanceLifetimeComponents();

            CleanUpPerServiceOperationLifetimeComponents();

            base.OnClosing();
        }

        private void CleanUpPerServinceInstanceLifetimeComponents()
        {
            if (_isSingleConfiguration)
                return;

            Description.Behaviors.Remove<StashboxDependencyInjectionServiceBehavior>()?.Dispose();

            foreach (var endpoinDispatcher in ChannelDispatchers.OfType<ChannelDispatcher>().SelectMany(cd => cd.Endpoints))
            {
                (endpoinDispatcher.DispatchRuntime.InstanceProvider as StashboxInstanceProvider)?.Dispose();
                endpoinDispatcher.DispatchRuntime.InstanceProvider = null;
            }
        }

        private void CleanUpPerServiceOperationLifetimeComponents()
        {
            if (!StashboxConfig.EnablePerSericeOperationLifetime)
                return;

            RemoveDependencyInjectionOperationBehavior();

            RemoveDependencyInjectionParameterInspectors();
        }

        private void RemoveDependencyInjectionOperationBehavior()
        {
            var operationDescriptions =
                Description.Endpoints.Select(ep => ep.Contract).Distinct()
                    .SelectMany(cd => cd.Operations);

            foreach (var op in operationDescriptions)
            {
                op.Behaviors.Remove<StashboxDependencyInjectionOperationBehavior>()?.Dispose();
            }
        }

        private void RemoveDependencyInjectionParameterInspectors()
        {
            var dispatchOperations =
                ChannelDispatchers.OfType<ChannelDispatcher>()
                    .SelectMany(cd => cd.Endpoints.SelectMany(ep => ep.DispatchRuntime.Operations));

            foreach (var dop in dispatchOperations)
            {
                var paramInspector =
                    dop.ParameterInspectors.OfType<StashboxDependencyInjectionParameterInspector>().FirstOrDefault();
                if (paramInspector != null)
                    dop.ParameterInspectors.Remove(paramInspector);
            }
        }
    }
}