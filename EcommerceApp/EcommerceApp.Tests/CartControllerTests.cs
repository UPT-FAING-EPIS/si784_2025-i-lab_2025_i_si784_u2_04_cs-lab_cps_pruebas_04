using EcommerceApp.Api.Controllers;
using EcommerceApp.Api.Models;
using EcommerceApp.Api.Services;
using Moq;

namespace EcommerceApp.Tests;

public class ControllerTests
{
    private CartController controller;

    private Mock<IDiscountService> discountServiceMock;

    private Mock<IPaymentService> paymentServiceMock;
    private Mock<ICartService> cartServiceMock;

    private Mock<IShipmentService> shipmentServiceMock;
    private Mock<ICard> cardMock;
    private Mock<IAddressInfo> addressInfoMock;
    private List<ICartItem> items;

    [SetUp]
    public void Setup()
    {
        // Inicializar primero todos los mocks
        cartServiceMock = new Mock<ICartService>();
        paymentServiceMock = new Mock<IPaymentService>();
        shipmentServiceMock = new Mock<IShipmentService>();
        discountServiceMock = new Mock<IDiscountService>();

        cardMock = new Mock<ICard>();
        addressInfoMock = new Mock<IAddressInfo>();

        // Configurar los ítems simulados
        var cartItemMock = new Mock<ICartItem>();
        cartItemMock.Setup(item => item.Price).Returns(10);

        items = new List<ICartItem> { cartItemMock.Object };
        cartServiceMock.Setup(c => c.Items()).Returns(items.AsEnumerable());

        // Dejar el descuento en 1:1 por defecto
        discountServiceMock.Setup(d => d.ApplyDiscount(It.IsAny<double>())).Returns<double>(total => total);

        // Inicializar el controller correctamente
        controller = new CartController(
            cartServiceMock.Object,
            paymentServiceMock.Object,
            shipmentServiceMock.Object,
            discountServiceMock.Object
        );
    }


    [Test]
    public void ShouldReturnCharged()
    {
        string expected = "charged";
        paymentServiceMock.Setup(p => p.Charge(It.IsAny<double>(), cardMock.Object)).Returns(true);

        // act
        var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

        // assert
        shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Once());

        Assert.That(expected, Is.EqualTo(result));
    }

    [Test]
    public void ShouldReturnNotCharged()
    {
        string expected = "not charged";
        paymentServiceMock.Setup(p => p.Charge(It.IsAny<double>(), cardMock.Object)).Returns(false);

        // act
        var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

        // assert
        shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Never());
        Assert.That(expected, Is.EqualTo(result));
    }    
      
    [TestCase(100, true, "charged")]
    [TestCase(100, false, "not charged")]
    [TestCase(200, true, "charged")]
    [TestCase(200, false, "not charged")]
    public void CheckOut_WithVariousTotalsAndChargeResults_ReturnsExpected(
        double total,
        bool chargeResult,
        string expected)
    {
        // Arrange
        cartServiceMock.Setup(c => c.Total()).Returns(total);
        discountServiceMock.Setup(d => d.ApplyDiscount(total)).Returns(total * 0.9); // 10% descuento
        paymentServiceMock.Setup(p => p.Charge(It.IsAny<double>(), cardMock.Object)).Returns(chargeResult);

        // Act
        var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

        // Assert
        if (chargeResult)
            shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Once());
        else
            shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Never());

        Assert.That(result, Is.EqualTo(expected));
    }

}