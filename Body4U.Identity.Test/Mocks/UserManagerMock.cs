namespace Body4U.Identity.Test.Mocks
{
    using Body4U.Identity.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Moq;

    using static Body4U.Common.Constants.Fakes.Identity;

    public class UserManagerMock
    {
        public static UserManager<ApplicationUser> Instance
        {
            get
            {
                var storeMock = new Mock<IUserStore<ApplicationUser>>();
                var umMock = new Mock<UserManager<ApplicationUser>>(storeMock.Object, null, null, null, null, null, null, null, null);
                umMock.Object.UserValidators.Add(new UserValidator<ApplicationUser>());
                umMock.Object.PasswordValidators.Add(new PasswordValidator<ApplicationUser>());

                umMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser { Email = FakeEmail });
                umMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser { Id = FakeUserId });
                umMock.Setup(x => x.IsEmailConfirmedAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(true);
                umMock.Setup(x => x.IsLockedOutAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(false);
                umMock.Setup(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(true);

                return umMock.Object;
            }
        }
    }
}
