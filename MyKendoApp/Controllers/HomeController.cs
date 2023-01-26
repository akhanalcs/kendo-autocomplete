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
        private readonly List<OrderViewModel> _appInMemoryOrderRepository;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _appInMemoryOrderRepository = new List<OrderViewModel>();
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
                Expression<Func<OrderViewModel, bool>> searchPredicate;

                if (!string.IsNullOrEmpty(orderIdHint))
                {
                    searchPredicate = o => o.OrderId.Contains(orderIdHint);
                }
                else
                {
                    searchPredicate = o => true;
                }

                // Get this from some Database call or service call. It's a simple example here.
                var orders = new List<OrderViewModel>
                {
                    new OrderViewModel{OrderId = "1234", OrderName = "MyOrder123", CustomerFirstName = "A", CustomerLastName = "B", OrderTotal = 100},
                    new OrderViewModel{OrderId = "1235", OrderName = "MyOrder456", CustomerFirstName = "A", CustomerLastName = "B", OrderTotal = 150},
                    new OrderViewModel{OrderId = "2345", OrderName = "MyOrder789", CustomerFirstName = "A", CustomerLastName = "B", OrderTotal = 200},
                    new OrderViewModel{OrderId = "2346", OrderName = "MyOrder101", CustomerFirstName = "A", CustomerLastName = "B", OrderTotal = 250}
                }.AsQueryable();

                var finalList = orders.Where(searchPredicate);

                return finalList.ToDataSourceResult(request);
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
            return Json(_appInMemoryOrderRepository.ToDataSourceResult(request));
        }

        // This is called using custom AJAX and not by the Kendo Grid's create call.
        public IActionResult AddOrderToAppRepo([FromBody] OrderViewModel orderData)
        {
            if (orderData != null && ModelState.IsValid)
            {
                var existingOrder = _appInMemoryOrderRepository.FirstOrDefault(o => o.OrderId == orderData.OrderId);
                if (existingOrder == null)
                {
                    var maxId = _appInMemoryOrderRepository.Max(o => o.OrderId);
                    orderData.OrderId = maxId + 1;
                    _appInMemoryOrderRepository.Add(orderData);
                }
                else
                {
                    existingOrder.OrderName = orderData.OrderName;
                    existingOrder.OrderDesc = orderData.OrderDesc;
                    existingOrder.CustomerLastName = orderData.CustomerLastName;
                    existingOrder.CustomerFirstName = orderData.CustomerFirstName;
                    existingOrder.OrderTotal = orderData.OrderTotal;
                }
            }

            return Json(new[] { orderData });
        }

        [AcceptVerbs("Post")]
        public IActionResult UpdateOrderRecord([DataSourceRequest] DataSourceRequest request, OrderViewModel orderData)
        {
            if (orderData != null && ModelState.IsValid)
            {
                var existingOrder = _appInMemoryOrderRepository.FirstOrDefault(o => o.OrderId == orderData.OrderId);
                if (existingOrder == null)
                {
                    var maxId = _appInMemoryOrderRepository.Max(o => o.OrderId);
                    orderData.OrderId = maxId + 1;
                    _appInMemoryOrderRepository.Add(orderData);
                }
                else
                {
                    existingOrder.OrderName = orderData.OrderName;
                    existingOrder.OrderDesc = orderData.OrderDesc;
                    existingOrder.CustomerLastName = orderData.CustomerLastName;
                    existingOrder.CustomerFirstName = orderData.CustomerFirstName;
                    existingOrder.OrderTotal = orderData.OrderTotal;
                }
            }

            return Json(new[] { orderData }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public IActionResult DeleteOrderRecord([DataSourceRequest] DataSourceRequest request, OrderViewModel orderData)
        {
            if (orderData != null)
            {
                var existingOrder = _appInMemoryOrderRepository.FirstOrDefault(o => o.OrderId == orderData.OrderId);
                if (existingOrder != null)
                {
                    _appInMemoryOrderRepository.Remove(existingOrder);
                }
            }

            return Json(new[] { orderData }.ToDataSourceResult(request, ModelState));
        }

        #endregion
    }
}