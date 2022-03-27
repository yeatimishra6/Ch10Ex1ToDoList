using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class HomeController : Microsoft.AspNetCore.Mvc.Controller
    {
        private ToDoContext context;
        public HomeController(ToDoContext ctx) => context = ctx;
     
        public IActionResult Index(string id)
        {
            ToDoViewModel Model = new ToDoViewModel();
            // load current filters and data needed for filter drop downs in ViewBag
            var filters = new Filters(id); // creates new filter object by passing the Id parameter to its constructor 
            Model.Filters = filters;
            Model.Categories = context.Categories.ToList();
            Model.Statuses = context.Statuses.ToList();
            Model.DueFilters = Filters.DueFilterValues;

            // get ToDo objects from database based on current filters
            IQueryable<ToDo> query = context.ToDos
                .Include(t => t.Category).Include(t => t.Status);
            if (filters.HasCategory) {
                query = query.Where(t => t.CategoryId == filters.CategoryId);
            }
            if (filters.HasStatus) {
                query = query.Where(t => t.StatusId == filters.StatusId);
            }
            if (filters.HasDue) {
                var today = DateTime.Today;
                if (filters.IsPast)
                    query = query.Where(t => t.DueDate < today);
                else if (filters.IsFuture)
                    query = query.Where(t => t.DueDate > today);
                else if (filters.IsToday)
                    query = query.Where(t => t.DueDate == today);
            }
            var tasks = query.OrderBy(t => t.DueDate).ToList();
            Model.Tasks = tasks;
            return View(Model);
            

        }

        public IActionResult Add()
        {
            ToDoViewModel Model = new ToDoViewModel();
            Model.Categories = context.Categories.ToList();
            Model.Statuses = context.Statuses.ToList();
            return View(Model);
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        public IActionResult Add(ToDoViewModel model)
        {
            if (ModelState.IsValid)
            {
                context.ToDos.Add(model.CurrentTask);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                model.Categories = context.Categories.ToList();
                model.Statuses = context.Statuses.ToList();
                return View(model);
            }
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        public IActionResult Filter(string[] filter)
        {
            string id = string.Join('-', filter);
            return RedirectToAction("Index", new { ID = id });
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        public IActionResult Edit([FromRoute]string id, ToDo selected)
        {
            if (selected.StatusId == null) {
                context.ToDos.Remove(selected);
            }
            else {
                string newStatusId = selected.StatusId;
                selected = context.ToDos.Find(selected.Id);
                selected.StatusId = newStatusId;
                context.ToDos.Update(selected);
            }
            context.SaveChanges();

            return RedirectToAction("Index", new { ID = id });
        }
    }
}