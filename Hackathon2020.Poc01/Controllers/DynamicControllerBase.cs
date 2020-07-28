using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
//using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Hackathon2020.Poc01.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DynamicControllerBase<TEntity> : ODataController where TEntity : class//ControllerBase where TEntity : class
    {
        private readonly DbContext _db;

        public DynamicControllerBase(DbContext db)
        {
            _db = db;
        }

        [EnableQuery]
        public IQueryable<TEntity> Get()
        {
            var values = _db.Set<TEntity>();
            return values;
        }

        public ActionResult Get(int key)
        {
            return Ok(_db.Set<TEntity>().Find(key));
        }

        public ActionResult Post([FromBody] TEntity data)
        {
            var added = _db.Set<TEntity>().Add(data);
            _db.SaveChanges();
            return Ok(added);
        }
    }
}
