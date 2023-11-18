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
using System.Threading.Tasks;

namespace CollectSFDataGui.Server.Controllers
{
    // https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-5.0
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class CollectionController : ControllerBase
    {
        private static Collector _collector;

        private static ConfigurationOptions _config;
        private static ILogger<CollectionController> _logger;
        private static List<LogMessage> _logMessages;
        private static Task _task;
        static CollectionController()
        {
            _collector = new Collector(false);
            _logMessages = new List<LogMessage>();
            // to subscribe to log messages
            Log.MessageLogged += Log_MessageLogged;
            _config = _collector.Config;
        }

        public CollectionController(ILogger<CollectionController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("/api/collection/cancel")]
        public ActionResult CancelCollection()
        {
            // todo: cancel collection
            _collector.Close();
            return new JsonResult(true) { };
        }

        [HttpGet]
        [Route("/api/collection/status")]
        public ActionResult CollectionStatus()
        {
            // todo: cancel collection
            if (_task != null)
            {
                return CreateJsonResult(_task.Status);
            }
            else
            {
                return CreateJsonResult(TaskStatus.Canceled.ToString());
            }
            // var jsonString = JsonSerializer.Serialize(_collector.Instance.Totals, JsonHelpers.GetJsonSerializerOptions());
            // _logger.LogInformation($"CollectionStatus:jsonString:{jsonString}");
            // return CreateJsonResult(jsonString);

        }

        [HttpGet]
        [Route("/api/collection/start")]
        public ActionResult StartCollection()
        {
            bool success = true;
            try
            {
                _logger.LogInformation($"StartCollection:enter");
                // get current confguration from configuration controller
                var logger = default(ILogger<ConfigurationController>);
                ConfigurationController configurationController = new ConfigurationController(logger);
                ActionResult configurationActionResult = configurationController.GetConfiguration();
                JsonResult configurationJsonResult = (JsonResult)configurationActionResult;
                ConfigurationOptions configurationOptions = (ConfigurationOptions)configurationJsonResult.Value;
                _logger.LogInformation($"StartCollection:configurationOptions:{configurationOptions}");
                //_collector = new Collector(false);
                _task = new Task(() => _collector.Collect(configurationOptions));
                _task.Start();

                return CreateJsonResult(success);
            }
            catch (Exception ex)
            {
                _logger.LogError($"StartCollection:exception:{ex.Message}");
                success = false;

                return CreateJsonResult(success);
            }
        }
        private static JsonResult CreateJsonResult<T>(T value)
        {
            string jsonString = JsonSerializer.Serialize(value, JsonHelpers.GetJsonSerializerOptions());
            _logger.LogInformation($"CreateJsonResult:enter:jsonString:{jsonString}");
            JsonResult jsonResult = new JsonResult(value, JsonHelpers.GetJsonSerializerOptions()) { };
            jsonResult.ContentType = "application/json;charset=utf-8";
            return jsonResult;
        }

        private static void Log_MessageLogged(object sender, LogMessage args)
        {
            _logger.LogInformation($"CollectionController:CSFDMessage:{args.Message}");
//            if (args.IsError)
//            {
                _logMessages.Add(args);
//            }
        }
    }
}