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
		return Problem(detail: "Something wrong! If you delete category that has a discount, Please re-check!");
	}
}
