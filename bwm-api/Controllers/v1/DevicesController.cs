using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Produces("application/json")]
public class DevicesController : ControllerBase
{
    private const string dbServer = "192.168.50.225";
    private const string dbName = "bwm";
    private const string dbCollection = "devices";

    [HttpGet]
    [Route("api/v1/[controller]")]
    public ActionResult GetOne()
    {
        var client = new MongoClient("mongodb://" + dbServer);
        var database = client.GetDatabase(dbName);
        var collection = database.GetCollection<Device>(dbCollection).AsQueryable();

        var result = collection.ToList();

        return Ok(result);
    }

    [HttpGet]
    [Route("api/v1/[controller]/{id}")]
    public ActionResult GetOne(Guid id)
    {
        var client = new MongoClient("mongodb://" + dbServer);
        var database = client.GetDatabase(dbName);
        var collection = database.GetCollection<Device>(dbCollection).AsQueryable();

        var result = collection.Where(x => x.Id == id);

        return Ok(result);
    }

    [HttpGet]
    [Route("api/v1/[controller]/{id}/counters")]
    public ActionResult GetDeviceCounters(Guid id, int take = 25, int skip = 0)
    {
        var client = new MongoClient("mongodb://" + dbServer);
        var database = client.GetDatabase(dbName);
        var collection = database.GetCollection<Device>(dbCollection).AsQueryable();
        var site = collection.FirstOrDefault(x => x.Id == id);

        var bandwidths = database.GetCollection<Bandwidth>(site.Site).AsQueryable();

        var result = bandwidths
            .OrderByDescending(x => x.Timestamp)
            .Take(take)
            .Skip(skip)
            .ToList();

        return Ok(result);
    }

    [HttpGet]
    [Route("api/v1/[controller]/{id}/bw")]
    public ActionResult GetDeviceCurrentThroughput(Guid id)
    {
        var client = new MongoClient("mongodb://" + dbServer);
        var database = client.GetDatabase(dbName);
        var collection = database.GetCollection<Device>(dbCollection).AsQueryable();
        var site = collection.FirstOrDefault(x => x.Id == id);

        var bandwidths = database.GetCollection<Bandwidth>(site.Site).AsQueryable();

        var results = bandwidths
            .OrderByDescending(x => x.Timestamp)
            .Take(2)
            .ToArray();

        return Ok(CalculateThroughput(site, results));
    }

    [HttpGet]
    [Route("api/v1/[controller]/bw")]
    public ActionResult GetAllDevicesCurrentThroughput()
    {
        var throughputs = new List<Throughput>();

        var devices = new MongoClient("mongodb://" + dbServer)
            .GetDatabase(dbName)
            .GetCollection<Device>(dbCollection)
            .AsQueryable()
            .ToList();

        foreach(var d in devices)
        {
            try
            {
                var query = new MongoClient("mongodb://" + dbServer)
                    .GetDatabase(dbName)
                    .GetCollection<Bandwidth>(d.Site)
                    .AsQueryable();

                var counters = query
                .OrderByDescending(x => x.Timestamp)
                .Take(2)
                .ToArray();

                throughputs.Add(CalculateThroughput(d, counters));
            }
            catch { }
        }

        return Ok(throughputs);
    }

    private static Throughput CalculateThroughput(Device device, Bandwidth[] counters)
    {
        var throughput = new Throughput
        {
            Id = Guid.NewGuid(),
            DeviceIPAddress = device.IPAddress,
            Site = device.Site,
            Timestamp = counters[0].Timestamp,
        };

        var time = (counters[0].Timestamp.Ticks - counters[1].Timestamp.Ticks) / 10000000; //dt

        var x0ingress = (counters[0].X0Ingress - counters[1].X0Ingress) * 8; //Octets to bits
        var x1ingress = (counters[0].X1Ingress - counters[1].X1Ingress) * 8; //Octets to bits
        var x2ingress = (counters[0].X2Ingress - counters[1].X2Ingress) * 8; //Octets to bits

        var x0egress = (counters[0].X0Egress - counters[1].X0Egress) * 8; //Octets to bits
        var x1egress = (counters[0].X1Egress - counters[1].X1Egress) * 8; //Octets to bits
        var x2egress = (counters[0].X2Egress - counters[1].X2Egress) * 8; //Octets to bits

        throughput.X0IngressThroughput = x0ingress / time; //bits / sec
        throughput.X1IngressThroughput = x1ingress / time; //bits / sec
        throughput.X2IngressThroughput = x2ingress / time; //bits / sec

        throughput.X0EgressThroughput = x0egress / time; //bits / sec
        throughput.X1EgressThroughput = x1egress / time; //bits / sec
        throughput.X2EgressThroughput = x2egress / time; //bits / sec

        return throughput;
    }
}
