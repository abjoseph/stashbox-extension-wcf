using Stashbox.BuildUp;
using Stashbox.Lifetime;
using Stashbox.Registration;
using Stashbox.Resolution;
using Stashbox.Utils;
using System;
using System.Linq.Expressions;

namespace Stashbox.Extension.Wcf
{
    public sealed class PerServiceOperationLifetime : ScopedLifetimeBase
    {
        private volatile Expression _expression;

        private readonly object _lock = new object();

        /// <summary>
        /// Constructs a <see cref="PerServiceOperationLifetime"/>.
        /// </summary>
        public PerServiceOperationLifetime()
        {
            Shield.EnsureTrue(StashboxConfig.EnablePerSericeOperationLifetime, $"The {nameof(StashboxConfig)}.{nameof(StashboxConfig.EnablePerSericeOperationLifetime)} property must be set to true in order to use this lifetime.");
        }

        public override Expression GetExpression(IContainerContext containerContext, IServiceRegistration serviceRegistration, IObjectBuilder objectBuilder, ResolutionContext resolutionContext, Type resolveType)
        {
            if (this._expression != null) return this._expression;
            lock (this._lock)
            {
                if (this._expression != null) return this._expression;
                var factory = base.GetFactoryExpression(containerContext, serviceRegistration, objectBuilder, resolutionContext, resolveType);
                if (factory == null)
                    return null;

                var method = Constants.GetPerServiceOperationScopedValueMethod.MakeGenericMethod(resolveType);

                return this._expression = method.InvokeMethod(
                    resolutionContext.CurrentScopeParameter,
                    factory,
                    base.ScopeId.AsConstant());
            }
        }

        private static TValue CollectScopedInstance<TValue>(IResolutionScope scope, Func<IResolutionScope, object> factory, string scopeId)
            where TValue : class
        {
            var operationCtx = StashboxOperationContext.Current;
            if (operationCtx == null)
                return null;

            if (operationCtx.Items[scopeId] != null)
                return operationCtx.Items[scopeId] as TValue;

            TValue instance;
            var resolutionScope = operationCtx.Scope as IResolutionScope;
            if (resolutionScope != null)
            {
                instance = factory(resolutionScope) as TValue;
                operationCtx.Items[scopeId] = instance;
            }
            else
                instance = factory(scope) as TValue;

            return instance;
        }

        public override ILifetime Create() => new PerServiceOperationLifetime();
    }
}