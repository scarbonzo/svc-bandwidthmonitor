using MongoDB.Driver;
using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceProcess;

public partial class Service1 : ServiceBase
{
    //MongoDB Info
    private const string dbServer = "192.168.50.225";
    private const string dbName = "bwm";

    //Service Timer Info
    private System.Timers.Timer m_mainTimer;
    private int interval = 15 * 1000; //How often to run in milliseconds (seconds * 1000)

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

    public void OnDebug()
    {
        //CreateDevices();
        //Manually kick off the service when debugging
        OnStart(null);
    }

    protected override void OnStop()
    {
    }

    private void Routine()
    {
        var devices = GetDevicesFromDb();
        var results = new List<Bandwidth>();

        foreach (var d in devices)
        {
            results.Add(GetBandwidth(d.Site, d.IPAddress));
        }

        foreach (var r in results)
        {
            WriteBandwidthToDb(r);
        }
    }

    private static Bandwidth GetBandwidth(string Site, string IP)
    {
        try
        {
            var community = new OctetString("public");

            var param = new AgentParameters(community)
            {
                Version = SnmpVersion.Ver1
            };

            var agent = new IpAddress(IP);

            using (var target = new UdpTarget((IPAddress)agent, 161, 2000, 1))
            {
                var pdu = new Pdu(PduType.Get);
                pdu.VbList.Add(".1.3.6.1.2.1.2.2.1.5.1");
                pdu.VbList.Add(".1.3.6.1.2.1.2.2.1.10.1");
                pdu.VbList.Add(".1.3.6.1.2.1.2.2.1.16.1");
                pdu.VbList.Add(".1.3.6.1.2.1.2.2.1.5.2");
                pdu.VbList.Add(".1.3.6.1.2.1.2.2.1.10.2");
                pdu.VbList.Add(".1.3.6.1.2.1.2.2.1.16.2");
                pdu.VbList.Add(".1.3.6.1.2.1.2.2.1.5.3");
                pdu.VbList.Add(".1.3.6.1.2.1.2.2.1.10.3");
                pdu.VbList.Add(".1.3.6.1.2.1.2.2.1.16.3");

                var result = (SnmpV1Packet)target.Request(pdu, param);

                if (result != null)
                {
                    // ErrorStatus other then 0 is an error returned by 
                    // the Agent - see SnmpConstants for error definitions
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        // agent reported an error with the request
                        Console.WriteLine("Error in SNMP reply. Error {0} index {1}",
                            result.Pdu.ErrorStatus,
                            result.Pdu.ErrorIndex);
                    }
                    else
                    {
                        var bw = new Bandwidth
                        {
                            Id = Guid.NewGuid(),
                            DeviceIPAddress = IP,
                            Site = Site,
                            Timestamp = DateTime.Now,
                            X0Speed = Convert.ToDouble(result.Pdu.VbList[0].Value.ToString()),
                            X0Ingress = Convert.ToDouble(result.Pdu.VbList[1].Value.ToString()),
                            X0Egress = Convert.ToDouble(result.Pdu.VbList[2].Value.ToString()),
                            X1Speed = Convert.ToDouble(result.Pdu.VbList[3].Value.ToString()),
                            X1Ingress = Convert.ToDouble(result.Pdu.VbList[4].Value.ToString()),
                            X1Egress = Convert.ToDouble(result.Pdu.VbList[5].Value.ToString()),
                            X2Speed = Convert.ToDouble(result.Pdu.VbList[6].Value.ToString()),
                            X2Ingress = Convert.ToDouble(result.Pdu.VbList[7].Value.ToString()),
                            X2Egress = Convert.ToDouble(result.Pdu.VbList[8].Value.ToString())
                        };

                        return bw;
                    }
                }
                else
                {
                    Console.WriteLine("No response received from SNMP agent.");
                }
                target.Close();
                return null;
            }
        }
        catch { return null; }
    }

    private static List<Device> GetDevicesFromDb()
    {
        var _devices = new List<Device>();

        try
        {
            var client = new MongoClient("mongodb://" + dbServer);
            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<Device>("devices").AsQueryable();

            _devices.AddRange(collection.ToList());
        }
        catch { }

        
        return _devices;
    }

    private static void WriteBandwidthToDb(Bandwidth Bandwidth)
    {
        try
        {
            var client = new MongoClient("mongodb://" + dbServer);
            var database = client.GetDatabase(dbName);

            database.GetCollection<Bandwidth>(Bandwidth.Site).InsertOne(Bandwidth);
        }
        catch { }
    }

    private static void CreateDevices()
    {
        var client = new MongoClient("mongodb://" + dbServer);
        var database = client.GetDatabase(dbName);
        
        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.100.2",
            Site = "Edison"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.2.1",
            Site = "New Brunswick"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.3.1",
            Site = "Perth Amboy"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.4.1",
            Site = "Somerville"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.5.1",
            Site = "Newton"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.6.1",
            Site = "Trenton"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.7.1",
            Site = "Belvidere"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.8.1",
            Site = "Paterson"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.9.1",
            Site = "Morristown"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.10.1",
            Site = "Flemington"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.11.1",
            Site = "Jersey City"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.12.1",
            Site = "Elizabeth"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.13.1",
            Site = "Freehold"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.14.1",
            Site = "Toms River"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.15.1",
            Site = "Newark"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.17.1",
            Site = "Atlantic City"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.18.1",
            Site = "Cape May"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.19.1",
            Site = "Hackensack"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.20.1",
            Site = "Camden"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.21.1",
            Site = "Vineland"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.22.1",
            Site = "Mt. Holly"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.23.1",
            Site = "Woodbury"
        });

        database.GetCollection<Device>("devices").InsertOne(new Device
        {
            Id = Guid.NewGuid(),
            IPAddress = "192.168.99.1",
            Site = "Cedar Knolls"
        });

    }
}
