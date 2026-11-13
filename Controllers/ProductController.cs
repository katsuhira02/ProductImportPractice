using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductImportPractice.Models;
using ProductImportPractice.Services;

namespace ProductImportPractice.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _repository;
        private readonly IProductImportService _importService;

        public ProductController(IProductRepository repository, IProductImportService importService)
        {
            _repository = repository;
            _importService = importService;
        }

        public IActionResult Index()
        {
            var products = _repository.GetAll();
            return View(products);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            _repository.Remove(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Clear()
        {
            _repository.Clear();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _repository.GetAll().FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (product.Price <= 0)
            {
                ModelState.AddModelError("Price", AppConstants.Messages.PriceMustBePositive);
            }

            if (product.Stock < 0)
            {
                ModelState.AddModelError("Stock", AppConstants.Messages.NegativeStock);
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            _repository.Update(product);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Export()
        {
            var products = _repository.GetAll();
            var content = _importService.Export(products);
            return File(content, "text/csv", $"products_export_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Error = AppConstants.Messages.SelectFile;
                return View();
            }

            if (!file.FileName.EndsWith(AppConstants.Csv.Extension, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = AppConstants.Messages.OnlyCsvAllowed;
                return View();
            }

            using var stream = file.OpenReadStream();
            var result = _importService.Import(stream);

            return View(AppConstants.Views.ImportResult, result);
        }
    }
}
