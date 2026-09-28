using InsuranceApp.Domain.Clients;

namespace InsuranceApp.UnitTests.Domain.Clients;

public sealed class ClientTests
{
    private const string OriginalIdentificationNumber = "1234567890123";
    private const string OriginalName = "Original Client";
    private const string OriginalEmail = "original@example.com";
    private const string OriginalPhone = "+40 700 000 000";
    private const string OriginalAddress = "Original Street 10";

    [Theory]
    [InlineData(ClientType.Individual)]
    [InlineData(ClientType.Company)]
    public void Constructor_WhenInputIsValid_PreservesIdentityAndNormalizesDetails(ClientType type)
    {
        // Arrange
        var id = Guid.NewGuid();
        var identificationNumber = "0012345678901";
        var name = "Example Client";
        var email = "Client@example.com";
        var phone = "+40 700 000 000";
        var primaryAddress = "Example Street 10";

        // Act
        var client = CreateClient(
            id: id,
            type: type,
            identificationNumber: $"  {identificationNumber}  ",
            name: $"  {name}  ",
            email: $"  {email}  ",
            phone: $"  {phone}  ",
            primaryAddress: $"  {primaryAddress}  "
        );

        // Assert
        Assert.Equal(id, client.Id);
        Assert.Equal(type, client.Type);
        Assert.Equal(identificationNumber, client.IdentificationNumber);
        Assert.Equal(name, client.Name);
        Assert.Equal(email, client.Email);
        Assert.Equal(phone, client.Phone);
        Assert.Equal(primaryAddress, client.PrimaryAddress);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public void Constructor_WhenPrimaryAddressIsAbsent_StoresNull(string? primaryAddress)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var client = CreateClient(id, primaryAddress: primaryAddress);

        // Assert
        Assert.Null(client.PrimaryAddress);
    }

    [Fact]
    public void Constructor_WhenIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => CreateClient(id)
        );

        Assert.Equal("id", exception.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(999)]
    public void Constructor_WhenTypeIsUndefined_ThrowsArgumentOutOfRangeException(int value)
    {
        // Arrange
        var id = Guid.NewGuid();
        var type = (ClientType)value;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateClient(id, type: type)
        );

        Assert.Equal("type", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Constructor_WhenIdentificationNumberIsMissing_ThrowsArgumentException(string? identificationNumber)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.ThrowsAny<ArgumentException>(
            () => CreateClient(id, identificationNumber: identificationNumber!)
        );

        Assert.Equal("identificationNumber", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Constructor_WhenNameIsMissing_ThrowsArgumentException(string? name)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.ThrowsAny<ArgumentException>(
            () => CreateClient(id, name: name!)
        );

        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Constructor_WhenEmailIsMissing_ThrowsArgumentException(string? email)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.ThrowsAny<ArgumentException>(
            () => CreateClient(id, email: email!)
        );

        Assert.Equal("email", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Constructor_WhenPhoneIsMissing_ThrowsArgumentException(string? phone)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.ThrowsAny<ArgumentException>(
            () => CreateClient(id, phone: phone!)
        );

        Assert.Equal("phone", exception.ParamName);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    [InlineData("Example Client <client@example.com>")]
    [InlineData("first@example.com, second@example.com")]
    public void Constructor_WhenEmailIsInvalid_ThrowsArgumentException(string email)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => CreateClient(id, email: email)
        );

        Assert.Equal("email", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenNameIsAtLengthLimit_AcceptsValue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = new string('A', 200);

        // Act
        var client = CreateClient(id, name: $"  {name}  ");

        // Assert
        Assert.Equal(name, client.Name);
    }

    [Fact]
    public void Constructor_WhenNameExceedsLengthLimit_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = new string('A', 201);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => CreateClient(id, name: $"  {name}  ")
        );

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenIdentificationNumberIsAtLengthLimit_AcceptsValue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var identificationNumber = new string('7', 50);

        // Act
        var client = CreateClient(id, identificationNumber: $"  {identificationNumber}  ");

        // Assert
        Assert.Equal(identificationNumber, client.IdentificationNumber);
    }

    [Fact]
    public void Constructor_WhenIdentificationNumberExceedsLengthLimit_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var identificationNumber = new string('7', 51);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => CreateClient(id, identificationNumber: $"  {identificationNumber}  ")
        );

        Assert.Equal("identificationNumber", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenPhoneIsAtLengthLimit_AcceptsValue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var phone = new string('7', 30);

        // Act
        var client = CreateClient(id, phone: $"  {phone}  ");

        // Assert
        Assert.Equal(phone, client.Phone);
    }

    [Fact]
    public void Constructor_WhenPhoneExceedsLengthLimit_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var phone = new string('7', 31);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => CreateClient(id, phone: $"  {phone}  ")
        );

        Assert.Equal("phone", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenPrimaryAddressIsAtLengthLimit_AcceptsValue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var primaryAddress = new string('A', 500);

        // Act
        var client = CreateClient(id, primaryAddress: $"  {primaryAddress}  ");

        // Assert
        Assert.Equal(primaryAddress, client.PrimaryAddress);
    }

    [Fact]
    public void Constructor_WhenPrimaryAddressExceedsLengthLimit_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var primaryAddress = new string('A', 501);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => CreateClient(id, primaryAddress: $"  {primaryAddress}  ")
        );

        Assert.Equal("primaryAddress", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenEmailHas256Characters_AcceptsValue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = CreateMaxLengthEmail();

        // Act
        var client = CreateClient(id, email: $"  {email}  ");

        // Assert
        Assert.Equal(email, client.Email);
        Assert.Equal(256, client.Email.Length);
    }

    [Fact]
    public void Constructor_WhenEmailExceeds256Characters_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = CreateEmailExceedingMaxLength();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => CreateClient(id, email: $"  {email}  ")
        );

        Assert.Equal("email", exception.ParamName);
    }

    [Theory]
    [InlineData(ClientType.Individual)]
    [InlineData(ClientType.Company)]
    public void UpdateDetails_WhenInputIsValid_UpdatesDetailsAndPreservesIdentity(ClientType type)
    {
        // Arrange
        var id = Guid.NewGuid();
        var client = CreateClient(id, type: type);

        var updatedName = "Updated Client";
        var updatedEmail = "updated@example.com";
        var updatedPhone = "+40 700 000 001";
        var updatedAddress = "Updated Street 20";

        // Act
        client.UpdateDetails(
            name: $"  {updatedName}  ",
            email: $"  {updatedEmail}  ",
            phone: $"  {updatedPhone}  ",
            primaryAddress: $"  {updatedAddress}  "
        );

        // Assert
        Assert.Equal(id, client.Id);
        Assert.Equal(type, client.Type);
        Assert.Equal(OriginalIdentificationNumber, client.IdentificationNumber);

        Assert.Equal(updatedName, client.Name);
        Assert.Equal(updatedEmail, client.Email);
        Assert.Equal(updatedPhone, client.Phone);
        Assert.Equal(updatedAddress, client.PrimaryAddress);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public void UpdateDetails_WhenPrimaryAddressIsAbsent_ClearsExistingAddress(string? primaryAddress)
    {
        // Arrange
        var id = Guid.NewGuid();
        var client = CreateClient(id);

        // Act
        client.UpdateDetails(
            name: client.Name,
            email: client.Email,
            phone: client.Phone,
            primaryAddress: primaryAddress
        );

        // Assert
        Assert.Null(client.PrimaryAddress);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("email")]
    [InlineData("phone")]
    [InlineData("primaryAddress")]
    public void UpdateDetails_WhenAnyFieldIsInvalid_LeavesAllExistingValuesUnchanged(string invalidField)
    {
        // Arrange
        var id = Guid.NewGuid();
        var client = CreateClient(id);

        var name = invalidField == "name" ? " " : "Updated Client";
        var email = invalidField == "email" ? "invalid-email" : "updated@example.com";
        var phone = invalidField == "phone" ? " " : "+40 700 000 001";
        var primaryAddress = invalidField == "primaryAddress"
            ? new string('A', 501)
            : "Updated Street 20";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => client.UpdateDetails(name, email, phone, primaryAddress)
        );

        Assert.Equal(invalidField, exception.ParamName);

        Assert.Equal(id, client.Id);
        Assert.Equal(ClientType.Individual, client.Type);
        Assert.Equal(OriginalIdentificationNumber, client.IdentificationNumber);
        Assert.Equal(OriginalName, client.Name);
        Assert.Equal(OriginalEmail, client.Email);
        Assert.Equal(OriginalPhone, client.Phone);
        Assert.Equal(OriginalAddress, client.PrimaryAddress);
    }

    private static Client CreateClient(
        Guid id,
        ClientType type = ClientType.Individual,
        string identificationNumber = OriginalIdentificationNumber,
        string name = OriginalName,
        string email = OriginalEmail,
        string phone = OriginalPhone,
        string? primaryAddress = OriginalAddress
    )
    {
        return new(
            id: id,
            type: type,
            identificationNumber: identificationNumber,
            name: name,
            email: email,
            phone: phone,
            primaryAddress: primaryAddress
        );
    }

    private static string CreateEmail(
        int localPartLength,
        params int[] domainLabelLengths
    )
    {
        var localPart = new string('a', localPartLength);

        var domainLabels = domainLabelLengths
            .Select((length, index) => new string((char)('b' + index), length)
        );

        return $"{localPart}@{string.Join(".", domainLabels)}";
    }

    private static string CreateMaxLengthEmail()
    {
        return CreateEmail(64, 63, 63, 63);
    }

    private static string CreateEmailExceedingMaxLength()
    {
        return CreateEmail(64, 63, 63, 62, 1);
    }
}
