namespace Stashbox.Extension.Wcf
{
    public interface IScopeExtension
    {
        IDependencyResolver Scope { get; }

        void TerminateScope();
    }
}