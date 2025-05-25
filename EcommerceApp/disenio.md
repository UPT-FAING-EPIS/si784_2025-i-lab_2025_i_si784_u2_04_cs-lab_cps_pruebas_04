```mermaid
classDiagram

class ICartService {
  +Total() Double
  +Items() IEnumerable~ICartItem~
}

class IDiscountService {
  +ApplyDiscount() Double
}

class IPaymentService {
  +Charge() Boolean
}

class IShipmentService {
  +Ship() Void
}

class IAddressInfo {
  +Street String
  +Address String
  +City String
  +PostalCode String
  +PhoneNumber String
}

class ICard {
  +CardNumber String
  +Name String
  +ValidTo DateTime
}

class ICartItem {
  +ProductId String
  +Quantity Int
  +Price Double
}

class CartController {
  +CheckOut() String
}
