# Book-Store-Assignment-2 cancel order and e-mail
E-mail Service codes changed:
1. BookStore.Business.Entities : Delivery Status, add two new status, now has five [Submitted, Delivered, Failed, PickedUp, Delivering]

2.BookStore.Business.Components.Interfaces: IDeliveryNotificationProvider

3.BookStore.Business.Components: DeliveryNotificationProvider

4.DeliveryCo.Services.Interfaces: IDeliveryNotificationServer -> DeliveryInfoStatus + new operations

5.BookStore.Services: DeliveryNotificationService -> Interface Implementation + GetDeliveryStatus

6.BookStore.Business.Components: Order provider has manged the function about sending Order Error Message (Line 97 - Line 114)

Customer Cancel Oder and refund codes changed:

1.OrderProvider.cs: add new functions so that customer can get refund

	-> Cancel Order
	-> TransferFundsFromBookStore
	->SendRefundConfirmation

2. IOrderProvider.cs: add "Cancel order" method interface

3.BookStore.Services.Interfaces (IOrderService.cs) and BookStore.Services (OrderService.cs): add "cancel order service"
