using Microsoft.AspNetCore.Mvc;

namespace QuantumIdleMobile.Controllers
{
    /// <summary>
    /// 管理员后台 MVC 控制器（用于视图路由）
    /// </summary>
    public class AdminController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
