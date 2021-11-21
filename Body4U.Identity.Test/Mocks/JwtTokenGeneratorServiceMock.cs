namespace Body4U.Identity.Test.Mocks
{
    using Body4U.Common;
    using Body4U.Identity.Data.Models;
    using Body4U.Identity.Services;
    using Moq;

    using static Body4U.Common.Constants.Fakes.Identity;

    public class JwtTokenGeneratorServiceMock
    {
        public static IJwtTokenGeneratorService Instance
        {
            get
            {
                var mock = new Mock<IJwtTokenGeneratorService>();
                mock.Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>())).ReturnsAsync(Result<string>.SuccessWith(FakeToken));

                return mock.Object;
            }
        }
    }
}
