using System.ServiceModel;

namespace Stashbox.Extension.Wcf
{
    public class PerServiceOperationScopeExtension : IExtension<OperationContext>, IScopeExtension
    {
        public IDependencyResolver Scope { get; }

        public PerServiceOperationScopeExtension(IDependencyResolver scope)
        {
            Scope = scope;
        }

        public void Attach(OperationContext owner)
        {
            // Method intentionally left empty.
        }

        public void Detach(OperationContext owner)
        {
            // Method intentionally left empty.
        }

        public void TerminateScope()
        {
            Scope.Dispose();
        }
    }
}
