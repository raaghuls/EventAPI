using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventAPI.Infrastructure;
using EventAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventAPI.Controllers
{
    [Produces("text/json","text/xml")]
   // [EnableCors("MSPolicy")]
    [Route("api/[controller]")] //Route Prefix
    [ApiController]
    public class EventsController : ControllerBase
    {
        //THREE RETURN TYPES
        //[HttpGet]
        //public string GetMessage()
        //{
        //    return "Hello";
        //}

        //[HttpGet]
        //public IActionResult GetMessage()
        //{
        //    return Created("","Hello");
        //}

        //PREFERRED APPROACH
        //[HttpGet]
        //public ActionResult<string> GetMessage()
        //{
        //    if(Request.Headers["XYZ"].Contains("abc"))
        //    {
        //        return Created("", "Done");
        //    }
        //    else
        //    {
        //        return "Hello";
        //    }

        //}

        private EventDbContext db;
        public EventsController(EventDbContext dbContext)
        {
            db = dbContext;
        }

        //GET /api/events
        [HttpGet(Name ="GetAll")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<List<EventInfo>> GetEvents()
        {
            var events = db.Events.ToList();
            return Ok(events); //status code 200
        }


        //  //POST /api/events
        //  [HttpPost]
        //  public ActionResult<List<EventInfo>> AddEvent([FromBody] EventInfo eventInfo)
        //  {
        //     var result = db.Events.Add(eventInfo);
        //      db.SaveChanges();

        //      //return Created("", result.Entity); //status code 201
        //     // return CreatedAtAction(nameof(GetEvent), new { id = result.Entity.Id}, result.Entity); //status code 201
        //      return CreatedAtRoute("GetById", new { id = result.Entity.Id }, result.Entity); //status code 201

        ////   return CreatedAtAction(nameof(GetEvent), new { id = result.Entity.Id }, result.Entity); //status code 201
        //  }


        //USING ASYNC
        //POST /api/events
        [Authorize]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<EventInfo>> AddEventAsync([FromBody] EventInfo eventInfo)
        {
            if(ModelState.IsValid)
            {
                var result = await db.Events.AddAsync(eventInfo);
                await db.SaveChangesAsync();
                return CreatedAtRoute("GetById", new { id = result.Entity.Id }, result.Entity); //status code 201
            }
            else
            {
                return BadRequest(ModelState);
            }
         }

        //GET /api/events/id
        //[HttpGet("{id}")]
        //For CreatedAtRoute
        //[HttpGet("{id}",Name ="GetById")]
        //public ActionResult<EventInfo> GetEvent([FromRoute] int id)
        //{
        //    var eventInfo = db.Events.Find(id);
        //    return eventInfo; //status code 200
        //}

        //USING ASYNC
        [HttpGet("{id}", Name = "GetById")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<EventInfo>> GetEventAsync([FromRoute] int id)
        {
           // throw new NullReferenceException();
            var eventInfo = await db.Events.FindAsync(id);
            if(eventInfo != null)
                return Ok(eventInfo); //status code 200
            else
            {
               return NotFound("Item does not exists");
            }
        }



    }
}