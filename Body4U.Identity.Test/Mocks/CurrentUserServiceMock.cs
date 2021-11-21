namespace Body4U.Identity.Test.Mocks
{
    using Body4U.Common.Services.Identity;
    using Moq;

    public class CurrentUserServiceMock
    {
        public static ICurrentUserService Instance 
        {
            get
            {
                var mock = new Mock<ICurrentUserService>();

                mock
                    .Setup(x => x.UserId)
                    .Returns("123");

                mock.Setup(x => x.IsAdmin)
                    .Returns(true);

                return mock.Object;
            }
        }
    }
}
