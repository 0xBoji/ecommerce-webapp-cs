using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce_webapp_cs.Controllers;

[ApiController]
public class ErrorController : ControllerBase
{
	[Route("/error")]

	[ApiExplorerSettings(IgnoreApi = true)]
	public IActionResult HandleError()
	{
		var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
		return Problem(detail: "500, please re-check your api method!");
	}
}
