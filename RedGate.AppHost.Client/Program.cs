using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CommandLine;
using RedGate.AppHost.Interfaces;

namespace RedGate.AppHost.Client
{
    internal static class Program
    {
        private static SafeChildProcessHandle s_SafeChildProcessHandle;

        [STAThread]
        private static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
#if DEBUG
                options.Debug = true;
#endif
                if (options.Debug)
                    ConsoleNativeMethods.AllocConsole();
                          
                MainInner(options.ChannelId, options.Assembly);
            }
            else
            {
                MessageBox.Show("This application is used by RedGate.AppHost and should not be started manually. See https://github.com/red-gate/RedGate.AppHost", "Red Gate", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private static void MainInner(string id, string assembly)
        {
            var entryPoint = LoadChildAssembly(assembly);
            var services = FindServices(assembly, entryPoint);
            InitializeRemoting(id, entryPoint, services);
            SignalReady(id);
            RunWpf();
        }

        private static IOutOfProcessEntryPoint LoadChildAssembly(string assembly)
        {
            var outOfProcAssembly = Assembly.LoadFile(assembly);

            var entryPoint = outOfProcAssembly.GetTypes().Single(x => x.GetInterfaces().Contains(typeof (IOutOfProcessEntryPoint)));

            return (IOutOfProcessEntryPoint) Activator.CreateInstance(entryPoint);
        }

        private static IOutOfProcessServices FindServices(string assembly, IOutOfProcessEntryPoint entryPoint)
        {
            var services = entryPoint as IOutOfProcessServices;
            if (services != null)
            {
                return services;
            }

            var outOfProcAssembly = Assembly.LoadFile(assembly);
            var serviceType = (from type in outOfProcAssembly.GetTypes()
                              where type.GetInterfaces().Contains(typeof(IOutOfProcessServices))
                                select type).FirstOrDefault();

            if (serviceType != null)
            {
                return (IOutOfProcessServices) Activator.CreateInstance(serviceType);
            }
            else
            {
                return null;
            }
    }

        private static void InitializeRemoting(string id, IOutOfProcessEntryPoint entryPoint, IOutOfProcessServices services)
        {
            Remoting.Remoting.RegisterChannels(true, id);

            s_SafeChildProcessHandle = new SafeChildProcessHandle(Dispatcher.CurrentDispatcher, entryPoint, services);
            Remoting.Remoting.RegisterService<SafeChildProcessHandle, ISafeChildProcessHandle>(s_SafeChildProcessHandle);
        }

        private static void SignalReady(string id)
        {
            using (EventWaitHandle signal = EventWaitHandle.OpenExisting(id))
            {
                signal.Set();
            }
        }

        private static void RunWpf()
        {
            Dispatcher.Run();
        }
    }
}
