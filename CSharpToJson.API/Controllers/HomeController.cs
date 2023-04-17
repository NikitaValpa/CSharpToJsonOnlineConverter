using System.ComponentModel.DataAnnotations;
using CSharpToJson.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CSharpToJson.Application.Logic.SharpCodeModel.Requests;
using CSharpToJson.Domain.Models;
using Mediator;

namespace CSharpToJson.API.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMediator _mediator;

        public HomeController(ILogger<HomeController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new JsonCodeViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Convert([FromForm, Required] CSharpCodeModel model,
            CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var res = await _mediator.Send(new ConvertCSharpToJsonRequest
                {
                    Request = model
                }, cancellationToken);

                return View("Index", res);
            }

            var errors = string.Join("\n", ModelState.Values
                .SelectMany(kv => kv.Errors)
                .Select(e => e.ErrorMessage));

            _logger.LogWarning("Validation errors:\n {errors}", errors);

            return View("Index", new JsonCodeViewModel { Errors = errors });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}