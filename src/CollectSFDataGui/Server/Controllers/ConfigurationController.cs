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
    public class ConfigurationController : ControllerBase
    {
        private static Collector _collector;

        private static ConfigurationOptions _config;
        private static ILogger<ConfigurationController> _logger;
        //private static List<LogMessage> _logMessages;
        private static List<string> _logMessages = MessageLogger.Messages;

        static ConfigurationController()
        {
            _collector = new Collector(false);
            //_logMessages = new List<LogMessage>();
            // to subscribe to log messages
            Log.MessageLogged += Log_MessageLogged;
            _config = _collector.Config;
        }
        public ConfigurationController(ILogger<ConfigurationController> logger = null)
        {
            if (logger != null)
            {
                _logger = logger;
            }
        }

        [HttpGet]
        [Route("/api/configuration")]
        public IEnumerable<string> GetPropertiesConfiguration()
        {
            ConfigurationProperties configurationProperties = _config.PropertyClone();
            string jsonString = JsonSerializer.Serialize(configurationProperties, JsonHelpers.GetJsonSerializerOptions());
            _logger.LogInformation($"Get:enter:jsonString:{jsonString}");

            JsonResult jsonResult = CreateJsonResult(configurationProperties);
            jsonResult.ContentType = "application/json;charset=utf-8";

            //return new List<JsonResult>() { jsonResult }.AsEnumerable();
            return new List<string>() { jsonString }.AsEnumerable();
        }

        [HttpGet]
        [Route("/api/configuration/save")]
        public ActionResult SaveConfiguration()
        {
            string jsonString = _config.SaveConfigFile();
            _logger.LogInformation($"Save:enter:jsonString:{jsonString}");

            JsonResult jsonResult = CreateJsonResult(jsonString);
            //jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet
            jsonResult.ContentType = "application/json;charset=utf-8";

            return jsonResult;
        }

        [HttpPost("/api/configuration/update")]
        public IActionResult ImportConfiguration([FromBody] object properties)
        {
            try
            {
                ConfigurationOptions configurationProperties = JsonSerializer.Deserialize<ConfigurationOptions>(properties.ToString(), JsonHelpers.GetJsonSerializerOptions());
                _config.MergeConfig(configurationProperties);
                bool validated = _config.Validate();
                ConfigurationOptions newConfigurationOptions = _config.Clone();

                JsonResult jsonResult = CreateJsonResult(newConfigurationOptions);
                jsonResult.ContentType = "application/json;charset=utf-8";
                string jsonString = JsonSerializer.Serialize(newConfigurationOptions, JsonHelpers.GetJsonSerializerOptions());

                if (validated)
                {
                    return Created($"/api/configuration/update", jsonString);
                }
                else
                {
                    //return Created($"/api/configuration/update", jsonString);
                    string jsonErrorString = JsonSerializer.Serialize(_logMessages, JsonHelpers.GetJsonSerializerOptions());
                    _logMessages.Clear();
                    return ValidationProblem($"failed validation:\r\n{jsonErrorString}", this.GetHashCode().ToString(), 400, "/api/configuration/update");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"exception:{e}");
                return BadRequest($"update/{properties} error:{e}");
            }
        }

        [HttpGet]
        [Route("/api/configuration/json")]
        public ActionResult GetConfiguration()
        {
            ConfigurationOptions ConfigurationOptions = _config.Clone();
            string jsonString = JsonSerializer.Serialize(ConfigurationOptions);
            _logger.LogInformation($"Get:enter:jsonString:{jsonString}");

            JsonResult jsonResult = CreateJsonResult(ConfigurationOptions);
            //jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet
            jsonResult.ContentType = "application/json;charset=utf-8";

            return jsonResult;
        }

        [HttpGet]
        [Route("/api/configuration/options")]
        public IEnumerable<string> GetOptionsConfiguration()
        {
            ConfigurationOptions configurationOptions = _config.Clone();
            string jsonString = JsonSerializer.Serialize(configurationOptions);
            _logger.LogInformation($"Get:enter:jsonString:{jsonString}");

            JsonResult jsonResult = CreateJsonResult(configurationOptions);
            jsonResult.ContentType = "application/json;charset=utf-8";

            //return new List<JsonResult>() { jsonResult }.AsEnumerable();
            return new List<string>() { jsonString }.AsEnumerable();
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
            _logger.LogInformation($"ConfigurationController:CSFDMessage:{args.Message}");
            //if (args.IsError)
            //{
            _logMessages.Add(args.Message);
            //}
        }
    }
}