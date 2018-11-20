using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class History
{
    public string DeviceIPAddress { get; set; }
    public string Site { get; set; }
    public Throughput Current { get; set; }
    public Throughput Last5Minutes { get; set; }
    public Throughput Last15Minutes { get; set; }
}
