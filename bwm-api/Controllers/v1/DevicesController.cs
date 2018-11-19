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
    [Route("api/v1/[controller]/{id}/bw")]
    public ActionResult GetDeviceBandwidths(Guid id, int take = 25, int skip = 0)
    {
        var client = new MongoClient("mongodb://" + dbServer);
        var database = client.GetDatabase(dbName);
        var collection = database.GetCollection<Device>(dbCollection).AsQueryable();
        var site = collection.FirstOrDefault(x => x.Id == id);

        var bandwidths = database.GetCollection<Bandwidth>(site.Site).AsQueryable();

        var result = bandwidths
            .Take(take)
            .Skip(skip)
            .ToList();

        return Ok(result);
    }
}
