using System.ServiceProcess;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main()
    {
#if DEBUG
        using (var s = new Service1())
        {
            s.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
        }
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);
#endif
    }
}