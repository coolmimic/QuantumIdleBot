using Microsoft.AspNetCore.Mvc;

namespace QuantumIdleMobile.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }

        public IActionResult Login()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }

        public IActionResult Dashboard()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }

        public IActionResult Orders()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }

        public IActionResult Settings()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }

        public IActionResult Register()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }

        public IActionResult Telegram()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }

        public IActionResult Schemes()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }

        public IActionResult History()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }

        public IActionResult Odds()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }

        public IActionResult SchemeEdit(int? id)
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            ViewData["SchemeId"] = id;
            return View();
        }

        public IActionResult OddsConfig(int? schemeId)
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            ViewData["SchemeId"] = schemeId;
            return View();
        }

        public IActionResult DrawRuleConfig(int? schemeId, int? type)
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            ViewData["SchemeId"] = schemeId;
            ViewData["RuleType"] = type;
            return View();
        }

        public IActionResult Logs()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }

        public IActionResult AdminCard()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }

        public IActionResult Activate()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }

        public IActionResult ResetPassword()
        {
            ViewData["ApiBaseUrl"] = _configuration["ApiBaseUrl"] ?? "http://localhost:5000/api";
            return View();
        }
    }
}

