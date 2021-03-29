﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ToyStore.Models;
using ToyStore.Service;

namespace ToyStore.Controllers
{
    public class CartController : Controller
    {
        private IProductService _productService;
        private ICustomerService _customerService;
        private IOrderService _orderService;
        private IOrderDetailService _orderDetailService;
        private ICartService _cartService;
        public CartController(IProductService productService, ICustomerService customerService, IOrderService orderService, IOrderDetailService orderDetailService, ICartService cartService)
        {
            _productService = productService;
            _customerService = customerService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _cartService = cartService;
        }
        // GET: Cart
        public List<Cart> GetCart()
        {
            Member member = Session["Member"] as Member;
            if (member != null)
            {
                if (_cartService.CheckCartMember(member.ID))
                {
                    List<Cart> listCart = _cartService.GetCart(member.ID);
                    Session["Cart"] = listCart;
                    return listCart;
                }
            }
            else
            {
                List<Cart> listCart = Session["Cart"] as List<Cart>;
                //Check null session Cart
                if (listCart == null)
                {
                    //Initialization listCart
                    listCart = new List<Cart>();
                    Session["Cart"] = listCart;
                    return listCart;
                }
                return listCart;
            }
            return null;
        }
        public ActionResult AddItemCart(int ID, string strURL)
        {
            //Check product already exists in DB
            Product product = _productService.GetByID(ID);
            if (product == null)
            {
                //product does not exist
                Response.StatusCode = 404;
                return null;
            }
            //Get cart
            List<Cart> listCart = GetCart();

            //If member
            Member member = Session["Member"] as Member;
            if (member != null)
            {
                //Case 1: If product already exists in Member Cart
                if (_cartService.CheckProductInCart(ID, member.ID))
                {
                    _cartService.AddQuantityProductCartMember(ID, member.ID);
                }
                else
                {
                    //Case 2: If product does not exist in Member Cart
                    //Get product
                    Product productAdd = _productService.GetByID(ID);
                    Cart itemCart = new Cart();
                    itemCart.ID = productAdd.ID;
                    itemCart.Price = (decimal)productAdd.PromotionPrice;
                    itemCart.Name = productAdd.Name;
                    itemCart.Quantity = 1;
                    itemCart.Total = itemCart.Price * itemCart.Quantity;
                    itemCart.Image = productAdd.Image1;
                    _cartService.AddCartIntoMember(itemCart, member.ID);
                }
                List<Cart> carts = _cartService.GetCart(member.ID);
                Session["Cart"] = carts;
                return Redirect(strURL);
            }
            else
            {
                if (listCart != null)
                {
                    //Case 1: If product already exists in session Cart
                    Cart itemCart = listCart.SingleOrDefault(n => n.ID == ID);
                    if (itemCart != null)
                    {
                        //Check inventory before letting customers make a purchase
                        if (product.Quantity < itemCart.Quantity)
                        {
                            return View("ThongBao");
                        }
                        itemCart.Quantity++;
                        itemCart.Total = itemCart.Quantity * itemCart.Price;
                        return Redirect(strURL);
                    }

                    //Case 2: If product does not exist in the Session Cart
                    Cart item = new Cart(ID);
                    listCart.Add(item);
                }
            }
            return Redirect(strURL);
        }
        public ActionResult CartPartial()
        {
            if (GetTotalQuanity() == 0)
            {
                ViewBag.TotalQuanity = 0;
                ViewBag.TotalPrice = 0;
                return PartialView();
            }
            ViewBag.TotalQuanity = GetTotalQuanity();
            ViewBag.TotalPrice = GetTotalPrice().ToString("#,##");
            return PartialView();
        }
        public double GetTotalQuanity()
        {
            List<Cart> listCart = Session["Cart"] as List<Cart>;
            if (listCart == null)
            {
                return 0;
            }
            return listCart.Sum(n => n.Quantity);
        }
        public decimal GetTotalPrice()
        {//Lấy giỏ hàng
            List<Cart> listCart = Session["Cart"] as List<Cart>;
            if (listCart == null)
            {
                return 0;
            }
            return listCart.Sum(n => n.Total);
        }
        public ActionResult Checkout()
        {
            ViewBag.TotalQuantity = GetTotalQuanity();
            return View();
        }
        [HttpGet]
        public ActionResult EditCart(int ID)
        {
            //Check null session cart
            if (Session["Cart"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            //Check whether the product exists in the database or not?
            Product product = _productService.GetByID(ID);
            if (product == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            //Get cart
            List<Cart> listCart = GetCart();
            //Check if the product exists in the shopping cart
            Cart item = listCart.SingleOrDefault(n => n.ProductID == ID);
            if (item == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Cart = listCart;
            return Json(new
            {
                data = item,
                status = true
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult EditCart(Cart cart)
        {
            //Check stock quantity
            Product product = _productService.GetByID(cart.ProductID);
            if (product.Quantity < cart.Quantity)
            {
                return View("ThongBao");
            }
            //Updated quantity in cart Session
            List<Cart> listCart = GetCart();
            //Get products from within listCart to update
            Cart itemCartUpdate = listCart.Find(n => n.ProductID == cart.ProductID);
            itemCartUpdate.Quantity = cart.Quantity;
            itemCartUpdate.Total = itemCartUpdate.Quantity * itemCartUpdate.Price;

            Member member = Session["Member"] as Member;
            if (member != null)
            {
                //Update Cart Quantity Member
                _cartService.UpdateQuantityCartMember(cart.Quantity, cart.ProductID, member.ID);
                Session["Cart"] = listCart;
            }

            return RedirectToAction("Checkout");
        }
        [HttpGet]
        public ActionResult RemoveItemCart(int ID)
        {
            //Check null session Cart
            if (Session["Cart"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            //Check whether the product exists in the database or not?
            Product product = _productService.GetByID(ID);
            if (product == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            //Get cart
            List<Cart> listCart = GetCart();
            //Check if the product exists in the shopping cart
            Cart item = listCart.SingleOrDefault(n => n.ProductID == ID);
            if (item == null)
            {
                return RedirectToAction("Index", "Home");
            }
            //Remove item cart
            listCart.Remove(item);
            Member member = Session["Member"] as Member;
            _cartService.RemoveCart(ID, member.ID);
            List<Cart> carts = _cartService.GetCart(member.ID);
            Session["Cart"] = carts;
            return RedirectToAction("Checkout");
        }
        [HttpGet]
        public ActionResult AddOrder(Customer customer)
        {
            //Check null session cart
            if (Session["Cart"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            Customer customercheck = new Customer();
            bool status = false;
            //Is Customer
            Customer customerNew = new Customer();
            if (Session["Member"] == null)
            {
                //Insert customer into DB
                customerNew = customer;
                customerNew.MemberCategoryID = 1;
                _customerService.AddCustomer(customerNew);
            }
            else
            {
                //Is member
                Member member = Session["Member"] as Member;
                customercheck = _customerService.GetAll().FirstOrDefault(x => x.FullName.Contains(member.FullName));
                if (customercheck != null)
                {
                    status = true;
                }
                else
                {
                    customerNew.FullName = member.FullName;
                    customerNew.Address = member.Address;
                    customerNew.Email = member.Email;
                    customerNew.PhoneNumber = member.PhoneNumber;
                    customerNew.MemberCategoryID = member.MemberCategoryID;
                    _customerService.AddCustomer(customerNew);
                }
            }
            //Add order
            Order order = new Order();
            if (status)
            {
                order.CustomerID = customercheck.ID;
            }
            else
            {
                order.CustomerID = customerNew.ID;
            }
            order.DateOrder = DateTime.Now;
            order.DateShip = DateTime.Now;
            order.IsPaid = false;
            order.IsDelete = false;
            order.IsDelete = false;
            order.Offer = 0;
            _orderService.AddOrder(order);
            //Add order detail
            List<Cart> listCart = GetCart();
            foreach (Cart item in listCart)
            {
                OrderDetail orderDetail = new OrderDetail();
                orderDetail.OrderID = order.ID;
                orderDetail.ProductID = item.ID;
                orderDetail.Quantity = item.Quantity;
                orderDetail.Price = item.Price;
                _orderDetailService.AddOrderDetail(orderDetail);
            }
            Session["Cart"] = null;
            if (status)
            {
                SentMail("Đặt hàng thành công", customercheck.Email, "lapankhuongnguyen@gmail.com", "Garena009", "<p style=\"font-size:20px\">Cảm ơn bạn đã đặt hàng<br/>Mã đơn hàng của bạn là: " + order.ID + "<br/>Nhập mã đơn hàng vào ô tìm kiếm để xem thông tin và theo dõi đơn hàng của bạn</p>");
            }
            else
            {
                SentMail("Đặt hàng thành công", customerNew.Email, "lapankhuongnguyen@gmail.com", "Garena009", "<p style=\"font-size:20px\">Cảm ơn bạn đã đặt hàng<br/>Mã đơn hàng của bạn là: " + order.ID + "<br/>Nhập mã đơn hàng vào ô tìm kiếm để xem thông tin và theo dõi đơn hàng của bạn</p>");
            }
            return RedirectToAction("Message");
        }
        [HttpGet]
        public ActionResult Message()
        {
            return View();
        }
        public void SentMail(string Title, string ToEmail, string FromEmail, string Password, string Content)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(ToEmail);
            mail.From = new MailAddress(ToEmail);
            mail.Subject = Title;
            mail.Body = Content;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(FromEmail, Password);
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }
    }
}