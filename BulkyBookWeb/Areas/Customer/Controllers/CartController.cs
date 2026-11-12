using BulkyBook.Business.Services.IServices;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]

    public class CartController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IApplicationUserService _applicationUserService;

        private readonly IEmailSender _emailSender;


        public CartController(IOrderService orderService, IShoppingCartService shoppingCartService, IApplicationUserService applicationUserService,IEmailSender emailSender)
        {
            _orderService = orderService;
            _shoppingCartService = shoppingCartService;
            _applicationUserService = applicationUserService;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cartItems = await _shoppingCartService.GetUserCartItemsAsync(userId);

            var user = await _applicationUserService.GetUserByIdAsync(userId);
            ShoppingCartVM shoppingCartVM = new()
            {
                ShoppingCartList = cartItems,
                OrderHeader = new()
            };
            shoppingCartVM.OrderHeader.ApplicationUser = user;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;
            shoppingCartVM.OrderHeader.Name = user.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = user.PhoneNumber;
            shoppingCartVM.OrderHeader.StreetAddress = user.StreetAddress;
            shoppingCartVM.OrderHeader.City = user.City;
            shoppingCartVM.OrderHeader.State = user.State;
            shoppingCartVM.OrderHeader.PostalCode = user.PostalCode;

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPOST(ShoppingCartVM shoppingCartVM)
        {
            
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            

            var cartItems = await _shoppingCartService.GetUserCartItemsAsync(userId);
            shoppingCartVM.ShoppingCartList = cartItems;
            shoppingCartVM.OrderHeader.OrderDate = DateTime.UtcNow;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            } 

            shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            shoppingCartVM.OrderHeader.OrderDetails = shoppingCartVM.ShoppingCartList.Select(cart => new OrderDetails
            {
                ProductId = cart.ProductId,
                Price = cart.Price,
                Count = cart.Count,
            }).ToList();

            await _orderService.CreateOrderAsync(shoppingCartVM.OrderHeader);


            try
            {
                await _shoppingCartService.ClearCartAsync(userId);
                HttpContext.Session.SetInt32(SD.SessionCart, 0);
            }
            catch(Exception ex)
            {

            }

           var order = shoppingCartVM.OrderHeader;
           var user = await _applicationUserService.GetUserByIdAsync(userId);
           
           if (user?.Email != null)
           {
               string html = $@"
                   <h2>Thank you for your order!</h2>
                   <p>Your order has been placed successfully.</p>
                   <hr />
                   <p><strong>Order Number:</strong> {order.Id}</p>
                   <p><strong>Order Date:</strong> {order.OrderDate:MMMM dd, yyyy}</p>
                   <p><strong>Total Amount:</strong> {order.OrderTotal:C}</p>
                   <hr />
                   <p>Thank you for shopping with BulkyBook!</p>
                   <p>- The BulkyBook Team</p>";
           
               try
               {
                   await _emailSender.SendEmailAsync(user.Email,
                       $"Order Confirmation #{order.Id} - BulkyBook", html);
               }
               catch (Exception)
               {
               }
           }

        return RedirectToAction("OrderConfirmation", new { id = order.Id });          
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            return View(id);
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _shoppingCartService.GetCartByIdAsync(cartId);
            if (cart != null)
            {
                if (cart.Count == 1000)
                {
                    // do nothing
                }
                else
                {
                    cart.Count++;
                    await _shoppingCartService.UpdateCartAsync(cart);
                    await UpdateCartSessionAsync();

                }

            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _shoppingCartService.GetCartByIdAsync(cartId);
            if (cart != null)
            {
                cart.Count--;
                await _shoppingCartService.UpdateCartAsync(cart);
                await UpdateCartSessionAsync();

            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _shoppingCartService.GetCartByIdAsync(cartId);
            if (cart != null)
            {
                cart.Count=0;
                await _shoppingCartService.UpdateCartAsync(cart);
                await UpdateCartSessionAsync();

            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UpdateCart(int cartId,int count)
        {
            var cart = await _shoppingCartService.GetCartByIdAsync(cartId);
            if (cart == null)
            {
             return NotFound();
            }

            if (count <= 1)
            {
                cart.Count = 0;

            }
            else
            {
                if (count > 1000)
                {
                    cart.Count = 1000;
                }
                else
                {
                    cart.Count = count;

                }
            }
            await _shoppingCartService.UpdateCartAsync(cart);

            await UpdateCartSessionAsync();
            return Ok(new { success = true });
        }

        private async Task UpdateCartSessionAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var count = await _shoppingCartService.GetCartCountAsync(userId);
                HttpContext.Session.SetInt32(SD.SessionCart, count);
            }
           
        }

    }
}
