using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Bandwidth
{
    public Guid Id { get; set; }
    public string DeviceIPAddress { get; set; }
    public string Site { get; set; }
    public DateTime Timestamp { get; set; }
    public double X0Speed { get; set; }
    public double X0Ingress { get; set; }
    public double X0Egress { get; set; }
    public double X1Speed { get; set; }
    public double X1Ingress { get; set; }
    public double X1Egress { get; set; }
    public double X2Speed { get; set; }
    public double X2Ingress { get; set; }
    public double X2Egress { get; set; }
}
