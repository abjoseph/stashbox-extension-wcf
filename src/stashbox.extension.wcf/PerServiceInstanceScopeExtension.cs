using System.ServiceModel;

namespace Stashbox.Extension.Wcf
{
    public class PerServiceInstanceScopeExtension : IExtension<InstanceContext>, IScopeExtension
    {
        public IDependencyResolver Scope { get; }

        public PerServiceInstanceScopeExtension(IDependencyResolver scope)
        {
            Scope = scope;
        }

        public void Attach(InstanceContext owner)
        {
            // Method intentionally left empty.
        }

        public void Detach(InstanceContext owner)
        {
            // Method intentionally left empty.
        }

        public void TerminateScope()
        {
            Scope.Dispose();
        }
    }
}
