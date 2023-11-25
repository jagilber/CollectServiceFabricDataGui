using CollectSFData;
using CollectSFData.Common;
using CollectSFDataGui.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Net.Http.Json;
using Newtonsoft.Json.Serialization;

namespace CollectSFDataGui.Server.Controllers
{
    // https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-5.0
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class Controller : ControllerBase
    {
        private static Collector _collector;

        private static ConfigurationOptions _config;
        private static ILogger<Controller> _logger;
        //private static List<LogMessage> _logMessages;
        private static List<string> _logMessages = MessageLogger.Messages;

        static Controller()
        {
            _collector = new Collector(false);
            //_logMessages = new List<LogMessage>();
            // to subscribe to log messages
            Log.MessageLogged += Log_MessageLogged;
            _config = _collector.Config;
        }

        public Controller(ILogger<Controller> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("/api")]
        public IEnumerable<JsonResult> Get()
        {
            ConfigurationOptions ConfigurationOptions = _config.Clone();
            string jsonString = JsonSerializer.Serialize(ConfigurationOptions);

            _logger.LogInformation($"Get:enter:jsonString:{jsonString}");
            return new List<JsonResult>() { CreateJsonResult(new ConfigurationOptions()) }.AsEnumerable();
        }

        [HttpGet]
        [Route("/api/index")]
        public IEnumerable<ConfigurationOptions> Index()
        {
            return new List<ConfigurationOptions>() { _config.Clone() }.AsEnumerable();
        }

        [HttpGet]
        [Route("/api/logMessages")]
        public ActionResult GetLogMessages()
        {
            return CreateJsonResult(_logMessages.ToArray());
        }

        [HttpGet]
        [Route("/api/logMessages/clear")]
        public ActionResult ClearLogMessages()
        {
            _logMessages.Clear();
            return CreateJsonResult(true);
        }

        [HttpGet]
        [Route("/api/logMessages/last")]
        public ActionResult GetLastLogMessage()
        {
            if (_logMessages.Count > 0)
            {
                return CreateJsonResult(_logMessages.Last());
            }
            else
            {
                return CreateJsonResult(new LogMessage());
            }
        }

        private static string CreateJson<T>(T value)
        {
            string jsonString = JsonSerializer.Serialize(value, JsonHelpers.GetJsonSerializerOptions());
            _logger.LogInformation($"CreateJson:returning jsonString:{jsonString}");
            return jsonString;
        }

        private static JsonResult CreateJsonResult<T>(T value)
        {
            string jsonString = JsonSerializer.Serialize(value, JsonHelpers.GetJsonSerializerOptions());
            _logger.LogInformation($"CreateJsonResult:enter:jsonString:{jsonString}");
            JsonResult jsonResult = new JsonResult(value, JsonHelpers.GetJsonSerializerOptions()) { };
            jsonResult.ContentType = "application/json;charset=utf-8";

            _logger.LogInformation($"CreateJsonResult:exit:jsonResult:{jsonResult.ToString()}");
            return jsonResult;
        }


        private static void Log_MessageLogged(object sender, LogMessage args)
        {
            _logger.LogInformation($"Controller:CSFDMessage:{args.Message}");
            //if (args.IsError)
            //{
                _logMessages.Add(args.Message);
            //}
        }
    }
}