using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Hackathon2020.Poc01.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DynamicControllerBase<TEntity> : ControllerBase where TEntity : class
    {
        private readonly DbContext _db;

        public DynamicControllerBase(DbContext db)
        {
            _db = db;
        }

        [EnableQuery]
        public IQueryable<TEntity> Get()
        {
            return _db.Set<TEntity>();
        }

        public ActionResult Get([FromBody]int key)
        {
            return Ok(_db.Set<TEntity>().Find(key));
        }
    }
}
