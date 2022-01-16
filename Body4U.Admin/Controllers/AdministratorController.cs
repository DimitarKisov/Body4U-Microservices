namespace Body4U.Admin.Controllers
{
    using Body4U.Common.Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Refit;
    using System.Collections.Generic;
    using System.Net;

    using static Body4U.Common.Constants.MessageConstants.Common;

    [AuthorizeAdministrator]
    [Route("[controller]")]
    public class AdministratorController : Controller
    {
        protected BadRequestObjectResult ProccessErrors(ApiException ex)
        {
            var errors = new List<string>();

            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                errors.Add(HttpStatusCode.NotFound.ToString());
                return this.BadRequest(errors);
            }

            if (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                errors.Add(HttpStatusCode.Unauthorized.ToString());
                return this.BadRequest(errors);
            }

            if (ex.ContentHeaders != null)
            {
                JsonConvert
                    .DeserializeObject<List<string>>(ex.Content)
                    .ForEach(error => errors.Add(error));

                return this.BadRequest(errors);
            }

            var vaex = ex as ValidationApiException;

            if (ex.HasContent && vaex != null)
            {
                foreach (var kvp in vaex.Content.Errors)
                {
                    foreach (var value in kvp.Value)
                    {
                        errors.Add(value);
                    }
                }
            }
            else
            {
                errors.Add(ServerError);
            }

            return this.BadRequest(errors);
        }
    }
}
