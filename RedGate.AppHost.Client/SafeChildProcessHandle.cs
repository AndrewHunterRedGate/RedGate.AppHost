using System;
using System.Windows;
using System.Windows.Threading;
using RedGate.AppHost.Interfaces;
using RedGate.AppHost.Remoting.WPF;

namespace RedGate.AppHost.Client
{
    internal class SafeChildProcessHandle : MarshalByRefObject, ISafeChildProcessHandle
    {
        private readonly Dispatcher m_UiThreadDispatcher;
        private readonly IOutOfProcessEntryPoint m_EntryPoint;
        private readonly IOutOfProcessServices m_Services;

        public SafeChildProcessHandle(Dispatcher uiThreadDispatcher, IOutOfProcessEntryPoint entryPoint, IOutOfProcessServices services)
        {
            if (uiThreadDispatcher == null) 
                throw new ArgumentNullException("uiThreadDispatcher");
            
            if (entryPoint == null) 
                throw new ArgumentNullException("entryPoint");

            m_UiThreadDispatcher = uiThreadDispatcher;
            m_EntryPoint = entryPoint;
            m_Services = services;
        }

        public IRemoteElement CreateElement(IAppHostServices services)
        {
            Func<IRemoteElement> createRemoteElement = () => m_EntryPoint.CreateElement(services).ToRemotedElement();

            return (IRemoteElement)m_UiThreadDispatcher.Invoke(createRemoteElement);
        }

        public T GetService<T>(IAppHostServices services) where T:class
        {
            return m_Services.GetService<T>(services);
        }
    }
}