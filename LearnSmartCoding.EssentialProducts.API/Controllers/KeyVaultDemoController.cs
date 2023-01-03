﻿using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace LearnSmartCoding.EssentialProducts.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyVaultDemoController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ILogger<KeyVaultDemoController> Logger { get; }

        public KeyVaultDemoController(ILogger<KeyVaultDemoController> logger, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            Logger = logger;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
        }


        [HttpGet("GetCustomSecret", Name = "GetCustomSecret")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCustomSecretAsync()
        {
            var model = new KeyVaultModel();
            try
            {
                // read secret1 from Azure Key Vault
                string kvUri = configuration.GetSection("KeyVaultURL").Value;
                model.KeyVaultURL = kvUri;

                SecretClient client =
                    new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
                //pass secret name and version
                var secret = await client.GetSecretAsync(configuration.GetSection("SecretName").Value,
                  configuration.GetSection("SecreteVersion").Value);
                model.SecretValue = secret.Value.Value;
                model.SecretName = configuration.GetSection("SecretName").Value;
            }
            catch (Exception ex)
            {
                // Never return exception messages/details to the caller. The following code is only for demonration purpose.
                model.ErrorMessage = ex.Message;
            }

            return Ok(model);
        }


        [HttpGet("GetCustomSecretByKVReference", Name = "GetCustomSecretByKVReference")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public  IActionResult GetCustomSecretByKVReference()
        {
            var model = new KeyVaultModel()
            {
                KeyVaultURL = configuration.GetSection("KeyVaultURL").Value,
                SecretName = "SecretNumber",
                SecretValue = configuration.GetSection("SecretNumber").Value
            };
            return Ok(model);
        }

        [HttpGet("GetReadKey", Name = "GetReadKey")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult GetReadKey()
        {
            string keyName = "key-2";//"key-name";
            var model = new KeyVaultModel()
            {
                KeyVaultURL = "https://lsckv3.vault.azure.net"//configuration.GetSection("KeyVaultURL").Value
            };

            var client = new KeyClient(new Uri(model.KeyVaultURL), new DefaultAzureCredential());

            var key = client.GetKeyAsync(keyName).Result;
            model.KeyProperties = key.Value.Properties;

            return Ok(model);
        }

        [HttpGet("GetReadCertificate", Name = "GetReadCertificate")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult GetReadCertificate()
        {
            string certificateName = "lsc-cert";
            var model = new KeyVaultModel()
            {
                KeyVaultURL = "https://lsckv3.vault.azure.net"//configuration.GetSection("KeyVaultURL").Value
            };

            var kvUri = model.KeyVaultURL;
            var client = new CertificateClient(new Uri(kvUri), new DefaultAzureCredential());
            var certificate = client.GetCertificateAsync(certificateName).Result;
            model.CertificateValue = certificate.Value.SecretId;

            return Ok(model);
        }

    }

    public class KeyVaultModel
    {
        public string KeyVaultURL { get; set; }
        public string SecretName { get; set; }
        public string SecretValue { get; set; }
        public Uri CertificateValue { get; set; }
        public KeyProperties KeyProperties { get; set; }
        public string  ErrorMessage { get; set; }
    }
}
