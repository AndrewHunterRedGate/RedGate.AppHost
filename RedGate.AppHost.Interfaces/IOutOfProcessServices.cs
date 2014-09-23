namespace RedGate.AppHost.Interfaces
{
    /// <summary>
    /// Assemblies may provide an object that implements this method in order to provide services to the host task
    /// </summary>
    public interface IOutOfProcessServices
    {
        /// <summary>
        /// Returns a service object of type TServiceType.
        /// </summary>
        TServiceType GetService<TServiceType>(IAppHostServices services)
            where TServiceType: class;
    }
}
