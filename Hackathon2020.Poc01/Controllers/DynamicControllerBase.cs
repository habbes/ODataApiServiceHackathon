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

        public ActionResult Get([FromODataUri] int key)
        {
            var entity = _db.Set<TEntity>().Find(key);
            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        public ActionResult Delete([FromODataUri] int key)
        {
            var entity = _db.Set<TEntity>().Find(key);
            if (entity == null)
            {
                return NotFound();
            }

            _db.Set<TEntity>().Remove(entity);
            _db.SaveChanges();
            return Ok(entity);
        }

        public ActionResult Post([FromBody] TEntity data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var added = _db.Set<TEntity>().Add(data);
            _db.SaveChanges();
            return Ok(added);
        }

        public ActionResult Patch([FromODataUri] int key, Delta<TEntity> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = _db.Set<TEntity>().Find(key);
            if (entity == null)
            {
                return NotFound();
            }

            patch.Patch(entity);
            _db.SaveChanges();

            return Ok(entity);
        }

        public ActionResult Put([FromODataUri] int key, TEntity update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _db.Entry(update).State = EntityState.Modified;

            _db.SaveChanges();

            return Ok(update);
        }
    }
}
