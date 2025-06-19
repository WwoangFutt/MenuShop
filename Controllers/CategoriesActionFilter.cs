using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using MenuShop.Data;

namespace MenuShop.Controllers
{
    public class CategoriesActionFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _context;

        public CategoriesActionFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Load categories for sidebar
            var categories = await _context.Categories.ToListAsync();
            
            if (context.Controller is Controller controller)
            {
                controller.ViewBag.Categories = categories;
            }

            await next();
        }
    }
} 