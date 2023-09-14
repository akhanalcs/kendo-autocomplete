using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using MyKendoApp.Models;
using System.Diagnostics;
using System.Linq.Expressions;

namespace MyKendoApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly List<Order> _ordersInAppDatabase;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _ordersInAppDatabase = new List<Order>()
            {
                new Order{OrderId = "0123", OrderName = "MyOrder012", CustomerFirstName = "SeedData", CustomerLastName = "B", OrderTotal = 100},
                new Order{OrderId = "0133", OrderName = "MyOrder013", CustomerFirstName = "SeedData", CustomerLastName = "B", OrderTotal = 150},
            };
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Kendo Autocomplete stuffs
        public DataSourceResult GetAllOrdersFromMasterDatabase([DataSourceRequest] DataSourceRequest request, string orderIdHint)
        {
            try
            {
                Expression<Func<Order, bool>> searchPredicate;

                if (!string.IsNullOrEmpty(orderIdHint))
                {
                    searchPredicate = o => o.OrderId.Contains(orderIdHint);
                }
                else
                {
                    searchPredicate = o => true;
                }

                // Get this from some Database call or service call. It's a simple example here.
                var ordersFromMasterDatabase = new List<Order>
                {
                    new Order{OrderId = "1234", OrderName = "MyOrder123", CustomerFirstName = "A", CustomerLastName = "B", OrderTotal = 100},
                    new Order{OrderId = "1235", OrderName = "MyOrder456", CustomerFirstName = "A", CustomerLastName = "B", OrderTotal = 150},
                    new Order{OrderId = "2345", OrderName = "MyOrder789", CustomerFirstName = "A", CustomerLastName = "B", OrderTotal = 200},
                    new Order{OrderId = "2346", OrderName = "MyOrder101", CustomerFirstName = "A", CustomerLastName = "B", OrderTotal = 250}
                }.AsQueryable();

                var orderViewModels = ordersFromMasterDatabase      // For a more practical app, this will be DbContext.DbSet
                                      .Where(searchPredicate)
                                      .Select(od => 
                                      new OrderViewModel
                                      { 
                                          OrderId = od.OrderId, 
                                          OrderName = od.OrderName, 
                                          CustomerFirstName = od.CustomerFirstName, 
                                          CustomerLastName = od.CustomerLastName, 
                                          OrderTotal = od.OrderTotal
                                      });

                return orderViewModels.ToDataSourceResult(request);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("GetAllOrdersFromMasterDatabase", ex.ToString());
                var error = new[] { "An error occured while getting all orders from master database. Review the error log for more information." };
                return error.ToDataSourceResult(request);
            }
        }

        public IActionResult GetOrderRecords([DataSourceRequest] DataSourceRequest request)
        {
            return Json(_ordersInAppDatabase.ToDataSourceResult(request));
        }

        // This is called using custom AJAX and not by the Kendo Grid's create call.
        public IActionResult UpsertOrderToAppDatabase([FromBody] OrderViewModel orderViewModelData)
        {
            if (orderViewModelData != null && ModelState.IsValid)
            {
                var orderData = new Order
                {
                    OrderId = orderViewModelData.OrderId,
                    OrderName = orderViewModelData.OrderName,
                    CustomerFirstName = orderViewModelData.CustomerFirstName,
                    CustomerLastName = orderViewModelData.CustomerLastName,
                    OrderTotal = orderViewModelData.OrderTotal
                };

                var existingOrder = _ordersInAppDatabase.FirstOrDefault(o => o.OrderId == orderData.OrderId);
                if (existingOrder == null)
                {
                    var maxId = _ordersInAppDatabase.Max(o => o.OrderId);
                    orderData.OrderId = maxId + 1;
                    orderViewModelData.OrderId = orderData.OrderId; // Set this so frontend knows we saved the data because we're returning orderViewModelData back
                    _ordersInAppDatabase.Add(orderData);
                }
                else
                {
                    existingOrder.OrderName = orderData.OrderName;
                    existingOrder.CustomerLastName = orderData.CustomerLastName;
                    existingOrder.CustomerFirstName = orderData.CustomerFirstName;
                    existingOrder.OrderTotal = orderData.OrderTotal;
                }
            }

            return Json(new[] { orderViewModelData });
        }

        [AcceptVerbs("Post")]
        public IActionResult UpdateOrderRecord([DataSourceRequest] DataSourceRequest request, OrderViewModel orderViewModelData)
        {
            if (orderViewModelData != null && ModelState.IsValid)
            {
                var orderData = new Order
                {
                    OrderId = orderViewModelData.OrderId,
                    OrderName = orderViewModelData.OrderName,
                    CustomerFirstName = orderViewModelData.CustomerFirstName,
                    CustomerLastName = orderViewModelData.CustomerLastName,
                    OrderTotal = orderViewModelData.OrderTotal
                };

                var existingOrder = _ordersInAppDatabase.FirstOrDefault(o => o.OrderId == orderData.OrderId);
                if (existingOrder == null)
                {
                    var maxId = _ordersInAppDatabase.Max(o => o.OrderId);
                    orderData.OrderId = maxId + 1;
                    orderViewModelData.OrderId = orderData.OrderId; // Set this so frontend knows we saved the data because we're returning orderViewModelData back
                    _ordersInAppDatabase.Add(orderData);
                }
                else
                {
                    existingOrder.OrderName = orderData.OrderName;
                    existingOrder.CustomerLastName = orderData.CustomerLastName;
                    existingOrder.CustomerFirstName = orderData.CustomerFirstName;
                    existingOrder.OrderTotal = orderData.OrderTotal;
                }
            }

            return Json(new[] { orderViewModelData }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public IActionResult DeleteOrderRecord([DataSourceRequest] DataSourceRequest request, OrderViewModel orderViewModelData)
        {
            if (orderViewModelData != null)
            {
                var existingOrder = _ordersInAppDatabase.FirstOrDefault(o => o.OrderId == orderViewModelData.OrderId);
                if (existingOrder != null)
                {
                    _ordersInAppDatabase.Remove(existingOrder);
                }
            }

            return Json(new[] { orderViewModelData }.ToDataSourceResult(request, ModelState));
        }

        #endregion
    }
}