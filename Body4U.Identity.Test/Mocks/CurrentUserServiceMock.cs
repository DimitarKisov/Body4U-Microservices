namespace Body4U.Identity.Test.Mocks
{
    using Body4U.Common.Services.Identity;
    using Moq;

    using static Body4U.Common.Constants.Fakes.Identity;

    public class CurrentUserServiceMock
    {
        public static ICurrentUserService Instance 
        {
            get
            {
                var mock = new Mock<ICurrentUserService>();

                mock
                    .Setup(x => x.UserId)
                    .Returns(FakeUserId);

                mock.Setup(x => x.IsAdmin)
                    .Returns(true);

                return mock.Object;
            }
        }
    }
}
