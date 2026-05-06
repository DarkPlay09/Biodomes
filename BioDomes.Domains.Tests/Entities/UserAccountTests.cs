using BioDomes.Domains.Entities;

namespace BioDomes.Domains.Tests.Entities;

[TestFixture]
public class UserAccountTests
{
    [Test]
    public void ShouldReadAndWriteProperties_WhenUsingUserAccount()
    {
        var account = new UserAccount
        {
            Id = 10,
            UserName = "ada",
            Email = "ada@biodomes.test",
            BirthDate = new DateOnly(1995, 5, 15),
            AvatarPath = "/avatars/ada.png",
            ResearchOrganisation = "HELMo",
            IsAdmin = true
        };

        Assert.Multiple(() =>
        {
            Assert.That(account.Id, Is.EqualTo(10));
            Assert.That(account.UserName, Is.EqualTo("ada"));
            Assert.That(account.Email, Is.EqualTo("ada@biodomes.test"));
            Assert.That(account.BirthDate, Is.EqualTo(new DateOnly(1995, 5, 15)));
            Assert.That(account.AvatarPath, Is.EqualTo("/avatars/ada.png"));
            Assert.That(account.ResearchOrganisation, Is.EqualTo("HELMo"));
            Assert.That(account.IsAdmin, Is.True);
        });
    }

    [Test]
    public void ShouldUseDefaultValues_WhenUserAccountIsCreated()
    {
        var account = new UserAccount();

        Assert.Multiple(() =>
        {
            Assert.That(account.Id, Is.EqualTo(0));
            Assert.That(account.UserName, Is.Empty);
            Assert.That(account.Email, Is.Empty);
            Assert.That(account.BirthDate, Is.EqualTo(default(DateOnly)));
            Assert.That(account.AvatarPath, Is.Null);
            Assert.That(account.ResearchOrganisation, Is.Null);
            Assert.That(account.IsAdmin, Is.False);
        });
    }
}