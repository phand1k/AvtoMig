using MainWebApplication.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace MainWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        public DataController(ApplicationDbContext context) {
         db = context;
        }
        private ApplicationDbContext db;
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var list = db.Cars.ToList();
            return Ok(list);
        }

        [HttpPost]
        public ActionResult Post([FromBody] string value)
        {
            // Добавить данные в базу данных
            return Ok();
        }
    }

}
