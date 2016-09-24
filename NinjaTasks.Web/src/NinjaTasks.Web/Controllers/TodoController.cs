using System;
using System.Linq;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;

namespace NinjaTasks.Web.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private readonly ITodoStorage _storage;

        public TodoController(ITodoStorage storage)
        {
            _storage = storage;
        }

        public IActionResult Lists(string id)
        {
            if (id == null)
                return HttpBadRequest();

            var lists = _storage.GetLists().ToArray();
            var selectedList = lists.FirstOrDefault(l => l.Id == id);

            if (selectedList == null)
                return HttpNotFound();

            var tasks = _storage.GetTasks(selectedList);
            var view = View(lists);
            view.ViewData["SelectedList"] = lists.FirstOrDefault()?.Id;
            view.ViewData["Tasks"] = tasks;
            
            return view;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            var lists = _storage.GetLists().ToArray();
            var first = lists.OrderBy(l => l.Description).FirstOrDefault();
            if(first == null) 
                throw  new ArgumentException();
                
            return RedirectToAction("Lists", new {id=first.Id});
        }

        public IActionResult Angular()
        {
            return new ViewResult();
        }
    }
}
