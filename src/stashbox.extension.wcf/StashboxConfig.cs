using Stashbox.Lifetime;
using Stashbox.Utils;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Stashbox.Extension.Wcf
{
    public static class StashboxConfig
    {
        /// <summary>
        ///
        /// </summary>
        public static ServiceRegistrationLifetime? DefaultServiceLifetime { get; set; }

        /// <summary>
        ///
        /// </summary>
        public static bool EnablePerSericeOperationLifetime { get; set; }

        /// <summary>
        ///
        /// </summary>
        public static Assembly[] ServiceAssemblies { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="configureAction"></param>
        public static IStashboxContainer RegisterStashbox(Action<IStashboxContainer> configureAction)
        {
            var container = new StashboxContainer(config => config.WithCircularDependencyTracking()
                                                                  .WithDisposableTransientTracking()
                                                                  .WithUnknownTypeResolution());

            ConfigureStashboxServiceComponents(container);
            configureAction.Invoke(container);

            return container;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="container"></param>
        public static void RegisterStashbox(IStashboxContainer container)
        {
            ConfigureStashboxServiceComponents(container);
        }

        private static void ConfigureStashboxServiceComponents(IStashboxContainer container)
        {
            container.Register<StashboxInstanceProvider>();

            container.Register<IScopeProvider, StashboxPerServiceInstanceScopeProvider>(context => context
                     .WhenDependantIs<StashboxInstanceProvider>());

            container.Register<IScopeProvider, StashboxPerServiceOperationScopeProvider>(context => context
                     .WhenDependantIs<StashboxDependencyInjectionParameterInspector>());

            StashboxServiceHostFactoryBase.SetContainer(container);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="container"></param>
        /// <param name="servicesAssemblies"></param>
        public static void RegisterWcfServices(this IStashboxContainer container, params Assembly[] servicesAssemblies)
        {
            Shield.EnsureNotNull(container, nameof(container));

            if (servicesAssemblies == null || servicesAssemblies.Length == 0)
            {
                servicesAssemblies = ServiceAssemblies;
            }

            var serviceTypes = (from assembly in servicesAssemblies
                                where !assembly.IsDynamic
                                from type in GetExportedTypes(assembly)
                                where !type.IsAbstract
                                where !type.IsInterface
                                where !type.IsGenericTypeDefinition
                                where ServiceMetadataProvider.IsWcfServiceType(type)
                                select type).ToList();

            StashboxServiceValidationHandler.ValidateConcurrencyMode(serviceTypes);

            foreach (Type serviceType in serviceTypes)
            {
                ILifetime lifetimeScope = ServiceMetadataProvider.GetLifetimeScope(serviceType);
                container.Register(serviceType, context => context
                         .WithLifetime(lifetimeScope));
            }
        }

        private static Type[] GetExportedTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (NotSupportedException)
            {
                // A type load exception would typically happen on an Anonymously Hosted DynamicMethods
                // Assembly and it would be safe to skip this exception.
                return Type.EmptyTypes;
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Return the types that could be loaded. Types can contain null values.
                return ex.Types.Where(type => type != null).ToArray();
            }
            catch (Exception ex)
            {
                // Throw a more descriptive message containing the name of the assembly.
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to load types from assembly {0}. {1}", assembly.FullName, ex.Message), ex);
            }
        }
    }
}
