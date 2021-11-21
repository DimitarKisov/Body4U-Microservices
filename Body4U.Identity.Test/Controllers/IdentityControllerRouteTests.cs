namespace Body4U.Identity.Test.Controllers
{
    using Body4U.Identity.Controllers;
    using Body4U.Identity.Models.Requests;
    using MyTested.AspNetCore.Mvc;
    using Xunit;

    public class IdentityControllerRouteTests
    {
        [Fact]
        public void RegisterShouldBeRoutedCorrectly()
            => MyRouting
                .Configuration()
                .ShouldMap(request => request
                    .WithLocation("/Identity/Register")
                    .WithMethod(HttpMethod.Post))
                .To<IdentityController>(x => x.Register(With.Any<RegisterUserRequestModel>()));

        [Fact]
        public void LoginShouldBeRouterCorrectly()
            => MyRouting
                .Configuration()
                .ShouldMap(request => request
                    .WithLocation("/Identity/Login")
                    .WithMethod(HttpMethod.Post))
                .To<IdentityController>(x => x.Login(With.Any<LoginUserRequestModel>()));
    }
}
