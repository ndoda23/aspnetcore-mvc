using BulkyBook.Business.Services.IServices;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using BulkyBookWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IAuditLogService _auditLogService;

        [BindProperty]
        public OrderHeader OrderHeader { get; set; }

        public OrderController(IOrderService orderService, IAuditLogService auditLogService)
        {
            _orderService = orderService;
            _auditLogService = auditLogService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int orderId)
        {
            OrderHeader = await _orderService.GetOrderHeaderAsync(orderId, includeDetails: true, includeUser: true);
            return View(OrderHeader);
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public async Task<IActionResult> UpdateOrderDetails()
        {
            var orderHeaderFromDb = await _orderService.GetOrderHeaderAsync(OrderHeader.Id);
            orderHeaderFromDb.Name = OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderHeader.City;
            orderHeaderFromDb.State = OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderHeader.Carrier) && orderHeaderFromDb.OrderStatus == SD.StatusShipped)
            {
                orderHeaderFromDb.Carrier = OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderHeader.TrackingNumber) && orderHeaderFromDb.OrderStatus == SD.StatusShipped)
            {
                orderHeaderFromDb.TrackingNumber = OrderHeader.TrackingNumber;
            }

            await _orderService.UpdateOrderAsync(orderHeaderFromDb);

            await _auditLogService.LogAsync(User, SD.AuditUpdated, SD.EntityOrder,
                orderHeaderFromDb.Id.ToString(), null, "Shipping details updated");

            TempData["Success"] = "Order Details Updated Successfully.";

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });

        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public async Task<IActionResult> UpdateOrderStatus(string status)
        {
            var orderHeader = await _orderService.GetOrderHeaderAsync(OrderHeader.Id);
            
            if(orderHeader == null)
            {
                TempData["error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            string successMessage;

            switch(status)
            {
                case SD.StatusInProcess:
                    await _orderService.UpdateOrderStatusAsync(OrderHeader.Id, status);
                    successMessage = "Order processing started successfully.";
                    break;
                case SD.StatusCancelled:
                    await _orderService.UpdateOrderStatusAsync(OrderHeader.Id, status);
                    successMessage = "Order cancelled successfully.";
                    break;
                case SD.StatusReFunded:
                    await _orderService.UpdateOrderStatusAsync(OrderHeader.Id, status);
                    successMessage = "Order refunded successfully.";
                    break;
                case SD.StatusShipped:
                   

                    if(string.IsNullOrEmpty(OrderHeader.Carrier) || string.IsNullOrEmpty(OrderHeader.TrackingNumber))
                    {
                        TempData["error"] = "Please provide both carrier and tracking number.";
                        return RedirectToAction(nameof(Details), new { orderId = OrderHeader.Id });
                    }

                    await _orderService.UpdateOrderStatusAsync(
                        OrderHeader.Id,SD.StatusShipped,OrderHeader.Carrier,OrderHeader.TrackingNumber);
                    successMessage = "Order shipped successfully.";
                    break;

                default:
                    TempData["error"] = "Invalid status";
                    return RedirectToAction(nameof(Details), new { orderId = OrderHeader.Id });

            }

            await _auditLogService.LogAsync(User, SD.AuditStatusChanged, SD.EntityOrder,
                  OrderHeader.Id.ToString(), orderHeader.OrderStatus, status);

            TempData["Success"] = successMessage;

            return RedirectToAction(nameof(Details), new { orderId = OrderHeader.Id });

        }



        #region API CALLS

        public async Task<IActionResult> GetAll(string status)
        {

            string? userId = null;

            if (!User.IsInRole(SD.RoleAdmin) && !User.IsInRole(SD.RoleEmployee))
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
            }

            var orders = await _orderService.GetAllOrderAsync(userId,status);
            return Json(new { data = orders });
        }


        #endregion
    }
}
