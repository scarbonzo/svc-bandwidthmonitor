using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Throughput
{
    public Guid Id { get; set; }
    public string DeviceIPAddress { get; set; }
    public string Site { get; set; }
    public DateTime Timestamp { get; set; }
    public double X0IngressThroughput { get; set; }
    public double X1IngressThroughput { get; set; }
    public double X2IngressThroughput { get; set; }
    public double X0EgressThroughput { get; set; }
    public double X1EgressThroughput { get; set; }
    public double X2EgressThroughput { get; set; }
}
