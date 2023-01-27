using Stashbox.Registration;
using Stashbox.Utils;
using System;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Stashbox.Extension.Wcf
{
    public class ServiceMetadata
    {
        public Type ServiceType { get; }

        public Func<object> ServiceActivator { get; }

        public ServiceRegistrationLifetime ServiceLifetime { get; }

        internal IServiceRegistration RegistrationInformation { get; }

        public ServiceBehaviorAttribute ServiceBehaviorConfiguration { get; }

        public AspNetCompatibilityRequirementsAttribute AspNetCompatibilityRequirementsAttribute { get; }

        public string ServiceName =>
            ServiceBehaviorConfiguration?.Name;

        public ConcurrencyMode? ConcurrencyMode =>
            ServiceBehaviorConfiguration?.ConcurrencyMode;

        public InstanceContextMode? InstanceContextMode =>
            ServiceBehaviorConfiguration?.InstanceContextMode;

        public AspNetCompatibilityRequirementsMode? AspNetCompatibilityRequirementsMode
            => AspNetCompatibilityRequirementsAttribute?.RequirementsMode;

        public bool IsSingletonConfiguration
        {
            get
            {
                var isSingletonContextMode = InstanceContextMode.HasValue && (InstanceContextMode.Value == System.ServiceModel.InstanceContextMode.Single);
                var isSingletonRegistrationLifetime = (ServiceLifetime == ServiceRegistrationLifetime.Singleton);

                CheckIfRegistrationLifetimeMismatch(isSingletonContextMode, isSingletonRegistrationLifetime);
                CheckIfContextModeMismatch(isSingletonContextMode, isSingletonRegistrationLifetime);

                return isSingletonContextMode && isSingletonRegistrationLifetime;
            }
        }

        public ServiceMetadata(Type serviceType,
                               Func<object> serviceActivator,
                               ServiceRegistrationLifetime serviceRegistrationLifetime,
                               IServiceRegistration registrationInformation,
                               ServiceBehaviorAttribute serviceBehaviorConfig,
                               AspNetCompatibilityRequirementsAttribute compatibilityRequirements)
        {
            Shield.EnsureNotNull(serviceType, nameof(serviceType));
            Shield.EnsureNotNull(registrationInformation, nameof(registrationInformation));

            ServiceType = serviceType;
            ServiceActivator = serviceActivator ?? (() => null);
            ServiceLifetime = serviceRegistrationLifetime;
            RegistrationInformation = registrationInformation;
            ServiceBehaviorConfiguration = serviceBehaviorConfig;
            AspNetCompatibilityRequirementsAttribute = compatibilityRequirements;
        }

        private void CheckIfRegistrationLifetimeMismatch(bool isSingletonContextMode, bool isSingletonRegistrationLifetime)
        {
            string nonSingletonRegistrationLiftimeError =
                $"The given service type {ServiceType.FullName} has been configured with a registration lifetime of 'Singleton' however, " +
                "causing WCF to hold on to this instance indefinitely, while {1} has been registered " +
                "with the {2} lifestyle in the container. Please make sure that {1} is registered " +
                "as Singleton as well, or mark {0} with " +
                "[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)] instead.";

            if (isSingletonContextMode && !isSingletonRegistrationLifetime)
                throw new InvalidOperationException(nonSingletonRegistrationLiftimeError);
        }

        private void CheckIfContextModeMismatch(bool isSingletonContextMode, bool isSingletonRegistrationLifetime)
        {
            string nonSingletonContextModeError =
                $"The given service type {ServiceType.FullName} has been configured with a registration lifetime of 'Singleton' however, " +
                "causing WCF to hold on to this instance indefinitely, while {1} has been registered " +
                "with the {2} lifestyle in the container. Please make sure that {1} is registered " +
                "as Singleton as well, or mark {0} with " +
                "[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)] instead.";

            if (!isSingletonContextMode && isSingletonRegistrationLifetime)
                throw new InvalidOperationException(nonSingletonContextModeError);
        }
    }
}
