using System.ServiceModel;

namespace Stashbox.Extension.Wcf
{
    internal class StashboxPerServiceOperationScopeProvider : IScopeProvider
    {
        private OperationContext OperationContext => OperationContext.Current;

        private IDependencyResolver _container;

        public StashboxPerServiceOperationScopeProvider(IDependencyResolver container)
        {
            this._container = container;
        }

        public IScopeExtension GetOrCreateScope()
        {
            var scopeExtension = OperationContext?.Extensions.Find<PerServiceOperationScopeExtension>();
            if (scopeExtension != null || OperationContext == null)
                return scopeExtension;

            scopeExtension = new PerServiceOperationScopeExtension(_container.BeginScope());
            OperationContext.Extensions.Add(scopeExtension);

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
