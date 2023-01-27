using Stashbox.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;

namespace Stashbox.Extension.Wcf
{
    public class StashboxOperationContext : IExtension<OperationContext>, IDisposable
    {
        private IScopeExtension _scopeExtension;

        public IDictionary Items { get; private set; }
        public IDependencyResolver Scope => _scopeExtension.Scope;
        private static OperationContext OperationContext => OperationContext.Current;
        public static StashboxOperationContext Current => OperationContext?.Extensions.Find<StashboxOperationContext>();

        public StashboxOperationContext(IScopeExtension scopeExtension)
        {
            Shield.EnsureNotNull(scopeExtension, nameof(scopeExtension));
            Shield.EnsureNotNull(scopeExtension.Scope, $"{nameof(scopeExtension)}.{scopeExtension.Scope}");

            _scopeExtension = scopeExtension;

            Items = new Hashtable();
        }

        public void SetItems(IDictionary<object, object> items) => Items = (IDictionary)(items);

        public void SetItems(Hashtable items) => Items = items;

        public void Attach(OperationContext owner) { }
        public void Detach(OperationContext owner) { }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;

            if (disposing)
            {
                OperationContext?.Extensions.Remove((PerServiceOperationScopeExtension)_scopeExtension);
                OperationContext?.Extensions.Remove(this);

                _scopeExtension.TerminateScope();
                _scopeExtension = null;
                Items.Clear();
                Items = null;
            }

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}