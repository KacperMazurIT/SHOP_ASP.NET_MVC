﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using shop.Models.Data;
using shop.Models.ViewModels.Cart;

namespace shop.Controllers
{
    public class CartController : Controller
    {
        // GET: Card
        public ActionResult Index()
        {
            // inicjalizacja koszyka 
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // sprawdzamy czy nasz koszyk jest pusty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Twój koszyk jest pusty";
                return View();
            }

            // obliczenie wartosci podsumowania koszyka i przekazanie do ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            return View(cart);
        }

        public ActionResult CartPartial()
        {
            // inicjalizacja CartVM
            CartVM model = new CartVM();

            // inicjalizacja ilosc i cena
            int qty = 0;
            decimal price = 0;

            // sprawdzamy czy mamy dane koszyka zapisane w sesji
            if (Session["cart"] != null)
            {
                // pobieranie wartości z sesji
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                // ustawiamy ilosc i cena na 0 
                qty = 0;
                price = 0m;
            }

            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            // inicjalizacja CartVM List
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // inicjalizacja CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                // Pobieramy produkt
                ProductDTO product = db.Products.Find(id);

                // Sprawdzamy czy ten produkt jest juz w koszyku
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                // w zaleznosci od tego czy produkt jest w koszyku go dodajemy lub zwiekszamy ilosc
                if(productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                else
                {
                    productInCart.Quantity++;
                }

            }

            // pobieramy calkowite wartosc ilosci i ceny i dodajemy do modelu
            int qty = 0;
                decimal price = 0m;

                foreach (var item in cart)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;

                // zapis w sesji
                Session["cart"] = cart;


            return PartialView(model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            // Inicjalizacja listy CartVM

            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // pobieramy CartVM
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

            // zwiekszamy ilosc produktu
            model.Quantity++;

            // przygotowanie danych do JSONA
            var result = new { qty = model.Quantity, price = model.Price };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DecrementProduct(int productId)
        {
            // Inicjalizacja listy CartVM

            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // pobieramy CartVM
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

            // zmniejszamy ilosc produktu
            if (model.Quantity > 1)
            {
                model.Quantity--;
            }
            else
            {
                model.Quantity = 0;
                cart.Remove(model);
            }

            // przygotowanie danych do JSONA
            var result = new { qty = model.Quantity, price = model.Price };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public void RemoveProduct(int productId)
        {
            // Inicjalizacja listy CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // pobieramy cartVM
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

            // usuwamy produkt
            cart.Remove(model);
        }

    }
}