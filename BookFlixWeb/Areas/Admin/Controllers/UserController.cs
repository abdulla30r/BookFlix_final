﻿using Microsoft.AspNetCore.Mvc;

namespace BookFlixWeb.Areas.Admin.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
