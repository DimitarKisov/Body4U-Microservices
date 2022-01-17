namespace Body4U.Common.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Linq;
    using System.Net;

    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        protected const string PathSeparator = "/";
        protected const string Id = "{id}";
        protected const string TrainerId = "{trainerId}";
        protected const string Term = "{term}";

        protected ActionResult ProcessErrors(Result<object> result)
        {
            this.ModelState.Clear();

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                return this.BadRequest(result.Errors);
            }
            else if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                return this.Forbid();
            }
            else if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return this.NotFound(result.Errors);
            }
            else if (result.StatusCode == HttpStatusCode.Conflict)
            {
                return this.Conflict(result.Errors);
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                return this.Problem(result.Errors.First());
            }

            return this.BadRequest(result.Errors);
        }
    }
}
}
