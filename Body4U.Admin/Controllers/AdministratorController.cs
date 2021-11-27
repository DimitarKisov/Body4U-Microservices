namespace Body4U.Admin.Controllers
{
    using Body4U.Common.Infrastructure;
    using Microsoft.AspNetCore.Mvc;

    [AuthorizeAdministrator]
    [Route("[controller]")]
    public class AdministratorController : Controller
    {
    }
}
