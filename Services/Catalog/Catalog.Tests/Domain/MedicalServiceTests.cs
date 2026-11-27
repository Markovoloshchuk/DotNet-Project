using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;

namespace Catalog.Tests.Domain;

public sealed class MedicalServiceTests
{
    [Fact]
    public void Constructor_ValidData_CreatesActiveService()
    {
        // Arrange & Act
        var service = new MedicalService(
            categoryId: 1,
            serviceName: "  Abdominal Ultrasound  ",
            price: 850m,
            durationMinutes: 20,
            equipmentId: 2);

        // Assert
        Assert.Equal(1, service.CategoryId);
        Assert.Equal("Abdominal Ultrasound", service.ServiceName);
        Assert.Equal(850m, service.Price);
        Assert.Equal(20, service.DurationMinutes);
        Assert.Equal(2, service.EquipmentId);
        Assert.True(service.IsActive);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void Constructor_InvalidCategoryId_ThrowsDomainException(
        long categoryId)
    {
        Assert.Throws<DomainException>(() =>
            new MedicalService(
                categoryId,
                "Valid service",
                100m,
                30));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyServiceName_ThrowsDomainException(
        string serviceName)
    {
        Assert.Throws<DomainException>(() =>
            new MedicalService(
                1,
                serviceName,
                100m,
                30));
    }

    [Fact]
    public void Constructor_ServiceNameLongerThan150Characters_Throws()
    {
        var serviceName = new string('A', 151);

        Assert.Throws<DomainException>(() =>
            new MedicalService(
                1,
                serviceName,
                100m,
                30));
    }

    [Fact]
    public void Constructor_NegativePrice_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new MedicalService(
                1,
                "Valid service",
                -1m,
                30));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Constructor_InvalidDuration_ThrowsDomainException(
        int durationMinutes)
    {
        Assert.Throws<DomainException>(() =>
            new MedicalService(
                1,
                "Valid service",
                100m,
                durationMinutes));
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void Constructor_InvalidEquipmentId_ThrowsDomainException(
        long equipmentId)
    {
        Assert.Throws<DomainException>(() =>
            new MedicalService(
                1,
                "Valid service",
                100m,
                30,
                equipmentId));
    }

    [Fact]
    public void ChangePrice_NegativeValue_ThrowsDomainException()
    {
        var service = new MedicalService(
            1,
            "Valid service",
            100m,
            30);

        Assert.Throws<DomainException>(() =>
            service.ChangePrice(-50m));
    }

    [Fact]
    public void Deactivate_ChangesIsActiveToFalse()
    {
        var service = new MedicalService(
            1,
            "Valid service",
            100m,
            30);

        service.Deactivate();

        Assert.False(service.IsActive);
    }

    [Fact]
    public void Activate_ChangesIsActiveToTrue()
    {
        var service = new MedicalService(
            1,
            "Valid service",
            100m,
            30);

        service.Deactivate();
        service.Activate();

        Assert.True(service.IsActive);
    }
}