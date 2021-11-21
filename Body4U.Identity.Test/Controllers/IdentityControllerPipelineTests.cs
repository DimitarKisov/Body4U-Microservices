namespace Body4U.Identity.Test.Controllers
{
    using Body4U.Identity.Controllers;
    using Body4U.Identity.Models.Requests;
    using Body4U.Identity.Models.Responses;
    using MyTested.AspNetCore.Mvc;
    using Xunit;

    using static Body4U.Common.Constants.Fakes.Identity;

    public class IdentityControllerPipelineTests
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

        [Fact]
        public void MyProfileShouldReturnDataSuccessfully()
            => MyPipeline
                .Configuration()
                .ShouldMap(request => request
                    .WithLocation("/Identity/MyProfile")
                    .WithMethod(HttpMethod.Post))
                .To<IdentityController>(x => x
                    .MyProfile())
                .Which()
                .ShouldReturn()
                .Ok(new MyProfileResponseModel { Id = FakeUserId});
    }
}
