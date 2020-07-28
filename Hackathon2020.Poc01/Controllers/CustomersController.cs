using Hackathon2020.Poc01.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Hackathon2020.Poc01.Controllers
{
    public class CustomersController : ODataController
    {
        private readonly DbContext _db;

        public CustomersController(DbContext db)
        {
            _db = db;
        }

        [EnableQuery]
        public IQueryable<Customer> Get()
        {
            return _db.Set<Customer>();
        }

        public ActionResult Get([FromBody] int key)
        {
            return Ok(_db.Set<Customer>().Find(key));
        }
    }
}
