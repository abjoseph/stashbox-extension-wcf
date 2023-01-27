using System.ServiceModel;

namespace Stashbox.Extension.Wcf
{
    internal class StashboxPerServiceInstanceScopeProvider : IScopeProvider
    {
        private InstanceContext InstanceContext => OperationContext?.InstanceContext;
        private OperationContext OperationContext => OperationContext.Current;

        private IDependencyResolver _container;

        public StashboxPerServiceInstanceScopeProvider(IDependencyResolver container)
        {
            this._container = container;
        }

        public IScopeExtension GetOrCreateScope()
        {
            var scopeExtension = InstanceContext?.Extensions.Find<PerServiceInstanceScopeExtension>();
            if (scopeExtension != null || InstanceContext == null)
                return scopeExtension;

            scopeExtension = new PerServiceInstanceScopeExtension(_container.BeginScope());
            InstanceContext.Extensions.Add(scopeExtension);

            return scopeExtension;
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _container.Dispose();
                    _container = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
