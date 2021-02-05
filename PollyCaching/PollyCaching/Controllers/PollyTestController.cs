using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyCaching.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PollyTestController : ControllerBase
    {       
        private readonly ILogger<PollyTestController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IReadOnlyPolicyRegistry<string> _policyRegistry;
        private readonly string CACHE_POLICY_KEY = "myCachePolicy";

        public PollyTestController(ILogger<PollyTestController> logger, IHttpClientFactory httpClientFactory, IReadOnlyPolicyRegistry<string> policyRegistry)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _policyRegistry = policyRegistry ?? throw new ArgumentNullException(nameof(policyRegistry));
        }

        [HttpGet]
        [Route("TestCache")]
        public async Task<IActionResult> TestCache()
        {
            // Retrieve the cache policy from the registry
            var policy = _policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>(CACHE_POLICY_KEY);
            var httpClient = _httpClientFactory.CreateClient("PollyTest");

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
                    new Uri(httpClient.BaseAddress + "PollyTest/Test400"));

            Context context = new Context("Test400Key"); // This is the cache key

            HttpResponseMessage responseMessage = await policy.ExecuteAsync(async c => {
                return await httpClient.SendAsync(httpRequestMessage);
            }, context);


            var res = responseMessage.Content.ReadAsStringAsync();

            return Ok(res);
        }

        [HttpGet]
        [Route("Test400")]
        public async Task<IActionResult> Test400()
        {
            return Ok("Test 400 endpoint worked!");
        }
    }
}
