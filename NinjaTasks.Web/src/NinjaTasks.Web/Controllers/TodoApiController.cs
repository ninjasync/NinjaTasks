using System;
using System.Collections.Generic;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTasks.Web.Zools;

namespace NinjaTasks.Web.Controllers
{
    // TODO: protect from overposting http://www.asp.net/mvc/overview/getting-started/getting-started-with-ef-using-mvc/implementing-basic-crud-functionality-with-the-entity-framework-in-asp-net-mvc-application#overpost
    [Route("api/todo")]
    [Produces("application/json")]
    [Authorize]
    public class TodoApiController : Controller
    {
        [FromServices]
        public ITodoStorage Storage { get; set; }

        #region Lists

        [HttpGet]
        [Route("lists")]
        public IEnumerable<TodoListWithCount> GetTodoLists()
        {
            return Storage.GetLists();
        }

        [HttpGet]
        [Route("lists/{id:Guid}")]
        public IActionResult GetTodoList(Guid id)
        {
            return Storage.GetLists(id.ToString()).FirstOr404();
        }

        [HttpPatch]
        [Route("lists/{id:Guid}")]
        public IActionResult SaveTodoList(Guid id, [FromBody] TodoList value)
        {
            if (value == null || value.Id != id.ToString())
                return HttpBadRequest();
            Storage.Save(value);
            return new NoContentResult();
        }

        [HttpPost]
        [Route("lists")]
        public IActionResult SaveTodoList([FromBody] TodoList value)
        {
            Storage.Save(value);
            return CreatedAtRoute("GetTodoList", new { controller = "Todo", id = value.Id }, value);
        }

        [HttpDelete]
        [Route("lists/{id:Guid}")]
        public void DeleteList(Guid id)
        {
            Storage.Delete(SelectionMode.SelectSpecified, new TrackableId(TrackableType.List, id.ToString()));
        }
        #endregion

        #region Tasks
        [HttpGet]
        [Route("lists/{listsId:Guid}/tasks")]
        public IEnumerable<TodoTask> GetTodoTasksForList(Guid listsId)
        {
            return Storage.GetTasks(new TodoList {Id=listsId.ToString()});
        }

        [HttpGet]
        [Route("tasks/{taskId:Guid}")]
        public IActionResult GetTodoTasks(Guid taskId)
        {
            return Storage.GetTasks(ids: taskId.ToString()).FirstOr404();
        }

        [HttpPatch]
        [Route("tasks/{id:Guid}")]
        public IActionResult SaveTodoTask(Guid id, [FromBody] TodoTask value)
        {
            if (value == null || value.Id != id.ToString())
                return HttpBadRequest();
            Storage.Save(value);
            return new NoContentResult();
        }

        [HttpPost]
        [Route("tasks")]
        public IActionResult SaveTodoTask([FromBody] TodoList value)
        {
            Storage.Save(value);
            return CreatedAtRoute("GetTodoTasks", new {controller = "Todo", id = value.Id}, value);
        }

        [HttpDelete]
        [Route("tasks/{id:Guid}")]
        public void DeleteTask(Guid id)
        {
            Storage.Delete(SelectionMode.SelectSpecified, new TrackableId(TrackableType.Task, id.ToString()));
        }
        #endregion
    }
}
