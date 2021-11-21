namespace Body4U.Identity.Test.Controllers
{
    using Body4U.Identity.Controllers;
    using Body4U.Identity.Models.Requests;
    using MyTested.AspNetCore.Mvc;
    using Xunit;

    using static Body4U.Common.Constants.Fakes.Identity;

    public class IdentityControllerIntegrationTests
    {
        [Fact]
        public void LoginShouldReturnTokenWhenUserEntersValidCredentials()
            => MyPipeline
                .Configuration()
                .ShouldMap(request => request
                    .WithLocation("/Identity/Login")
                    .WithMethod(HttpMethod.Post)
                    .WithJsonBody(new
                    {
                        Email = FakeEmail,
                        Password = FakePassword
                    }))
                .To<IdentityController>(x => x
                    .Login(new LoginUserRequestModel
                    {
                        Email = FakeEmail,
                        Password = FakePassword
                    }))
                .Which()
                .ShouldReturn()
                .Ok(FakeToken);
    }
}
