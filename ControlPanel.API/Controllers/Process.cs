using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ControlPanel.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using SerialComms.Manager;

namespace ControlPanel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[EnableCors("AllowAll")]
    public class ProcessController : ControllerBase
    {
        ISerialComms _serialComms;
        public ProcessController(ISerialComms serialComms)
        {
            _serialComms = serialComms;
        }

        [HttpPost]
        [EnableCors("MyPolicy")]
        public IActionResult ProcessExecute([FromBody]ProcessDto processDto)
        {
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            string myArgs = null;
            if (processDto != null)
            {
                if (processDto.Id == "0" || processDto.Id == "9")
                {
                    myArgs = processDto.Status;
                }
                else
                {
                    myArgs = processDto.Status + processDto.Id;
                }

            }
            _serialComms.Send(myArgs);
            return Ok();
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("running successfully...");
        }

        //api/process/StatusInquiry
        [HttpGet("StatusInquiry")]
        public IActionResult StatusInquiry()
        {
            //call this function to inquiry status, 
            //then call GetStatus function to read status
            _serialComms.Send("status");
            return Ok();

        }
        //api/process/GetStatus
        [HttpGet("GetStatus")]
        public IActionResult GetStatus()
        {
            return Ok(_serialComms.GetStatus());

        }

    }
}
