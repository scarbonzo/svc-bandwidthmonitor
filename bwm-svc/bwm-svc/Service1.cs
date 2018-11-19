using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

public partial class Service1 : ServiceBase
{
    //MongoDB Info
    private const string dbServer = "192.168.50.225";
    private const string dbName = "bwm";

    //Service Timer Info
    private System.Timers.Timer m_mainTimer;
    private int interval = 30 * 60000; //How often to run in milliseconds (minutes * 60000)

    public Service1()
    {
        InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
        //Create the Main timer
        m_mainTimer = new System.Timers.Timer
        {
            //Set the timer interval
            Interval = interval
        };
        //Dictate what to do when the event fires
        m_mainTimer.Elapsed += m_mainTimer_Elapsed;
        //Something to do with something, I forgot since it's been a while
        m_mainTimer.AutoReset = true;

#if DEBUG
#else
            m_mainTimer.Start(); //Start timer only in Release
#endif
        //Run 1st Tick Manually
        Console.Beep();
        Routine();
    }

    void m_mainTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        //Each interval run the UpdateUsers() function
        Routine();
    }

    private void Routine()
    {
    }

    public void OnDebug()
    {
        //Manually kick off the service when debugging
        OnStart(null);
    }

    protected override void OnStop()
    {
    }
}
