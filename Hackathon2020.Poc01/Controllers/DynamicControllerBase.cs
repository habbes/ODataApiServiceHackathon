using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.UriParser;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hackathon2020.Poc01.Controllers
{
    public class DynamicControllerBase<TEntity> : ODataController where TEntity : class
    {
        private readonly DbContext _db;

        private Microsoft.AspNet.OData.Routing.ODataPath ODataPath { get => HttpContext.ODataFeature().Path; }

        public DynamicControllerBase(DbContext db)
        {
            _db = db;
        }

        public ActionResult Delete()
        {
            var segments = ODataPath.Segments;

            // only support patch /entitySet/key, we don't support nested paths at this time
            var keySegment = segments.OfType<KeySegment>().FirstOrDefault();
            if (keySegment == null)
            {
                return NotFound();
            }

            var entity = _db.Set<TEntity>().Find(keySegment.Keys.Select(kvp => kvp.Value).ToArray());
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
            return Ok(added.Entity);
        }

        public ActionResult Patch(Delta<TEntity> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var segments = ODataPath.Segments;
            
            // only support patch /entitySet/key, we don't support nested paths at this time
            var keySegment = segments.OfType<KeySegment>().FirstOrDefault();
            if (keySegment == null)
            {
                return NotFound();
            }

            var dbSet = _db.Set<TEntity>();
            var entity = dbSet.Find(keySegment.Keys.Select(kvp => kvp.Value).ToArray());

            if (entity == null)
            {
                return NotFound();
            }

            patch.Patch(entity);
            _db.SaveChanges();

            return Ok(entity);
        }

        public ActionResult Put(Delta<TEntity> update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var segments = ODataPath.Segments;
            var keySegment = segments.OfType<KeySegment>().FirstOrDefault();
            if (keySegment == null)
            {
                return NotFound();
            }

            var entity = _db.Set<TEntity>().Find(keySegment.Keys.Select(kvp => kvp.Value).ToArray());
            if (entity == null)
            {
                return NotFound();
            }

            update.Put(entity);

            // ensure the key in the url matches the key in the payload
            var type = typeof(TEntity);
            foreach (var kvp in keySegment.Keys)
            {
                type.GetProperty(kvp.Key).SetValue(entity, kvp.Value);
            }

            try
            {
                var updated = _db.Update(entity);
                _db.SaveChanges();
                return Ok(updated.Entity);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (ex.Message.Contains("not exist"))
                {
                    return NotFound();
                }

                throw ex;
            }   
        }

        [EnableQuery]
        public IActionResult GetNavigationProperty(string navigationProperty)
        {
            var segments = ODataPath.Segments;
            var keySegment = segments.OfType<KeySegment>().FirstOrDefault();
            if (keySegment == null)
            {
                return NotFound();
            }

            var entity = _db.Set<TEntity>().Find(keySegment.Keys.Select(kvp => kvp.Value).ToArray());
            if (entity == null)
            {
                return NotFound();
            }

            var relatedEntity = entity.GetType().GetProperty(navigationProperty).GetValue(entity);
            return Ok(relatedEntity);
        }

        /// <summary>
        /// This handles different types Get requests: i.e. /entityset, /entityset/key, /entityset/key/navigation /entityset/key/navigation/key
        /// </summary>
        /// <returns></returns>
        [EnableQuery]
        public IActionResult Get()
        {
            
            var odataFeature = HttpContext.ODataFeature();
            var odataPath = odataFeature.Path;
            var template = odataPath.PathTemplate;
            var segments = odataPath.Segments.Skip(1); // skip the first segment, expected to be the entity set segment

            var dbSet = _db.Set<TEntity>();

            // traverse the path to get the list nested properties to include in the db query
            var nestedPaths = new List<string>();
            foreach (var segment in segments.OfType<NavigationPropertySegment>())
            {
                nestedPaths.Add(segment.NavigationProperty.Name);
            }

            var pathToInclude = string.Join('.', nestedPaths);

            // current starts off as a DbQuery<T>
            // tell DB which nested properties to Include/Join in the query, e.g. Orders.OrderDetails
            // TODO: maybe we should avoid eager-loading for perf?
            object current = string.IsNullOrEmpty(pathToInclude) ? dbSet : dbSet.Include(pathToInclude);

            foreach (var segment in segments)
            {
                if (segment is KeySegment keySegment)
                {
                    // DbQuery<T> if we're filtering the dbSet or List<T> if we're filtering the values of a collection navigation property
                    var queryType = current.GetType();

                    var entityType = queryType.GetGenericArguments().First();

                    // Expression<Func<TEntity, bool>>
                    var exprType = typeof(Expression<>)
                        .MakeGenericType(typeof(Func<,>)
                        .MakeGenericType(entityType, typeof(bool)));

                    // filterPredicate = entity => entity.Key1 == value1 && entity.Key2 == value2 ...)
                    var filterParam = Expression.Parameter(entityType, "entity");
                    var filterConditions = keySegment.Keys.Select(kvp =>
                        Expression.Equal(
                            Expression.Property(filterParam, kvp.Key),
                            Expression.Constant(kvp.Value)));
                    var filterBody = filterConditions.Aggregate((left, right) => Expression.AndAlso(left, right));
                    var filterPredicate = Expression.Lambda(filterBody, filterParam);

                    // current = dbQuery.FirstOrDefault(filterPredicate);
                    var isQueryable = (current as IQueryable) != null;
                    var queryableType = isQueryable ? typeof(Queryable) : typeof(Enumerable);
                    var filterMethod = queryableType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .FirstOrDefault(m => m.Name == "FirstOrDefault" && m.GetParameters().Count() == 2)
                        .MakeGenericMethod(entityType);

                    if (isQueryable)
                    {
                        // we're filtering the db set
                        current = filterMethod.Invoke(null, new[] { current, filterPredicate });
                    }
                    else
                    {
                        // we're filtering a collection navigation property, the values have been eager-loaded
                        // TODO: eager-loading could be bad if we have a lot of values
                        current = filterMethod.Invoke(null, new[] { current, filterPredicate.Compile() });
                    }

                    if (current == null)
                    {
                        return NotFound();
                    }
                }
                else if (segment is NavigationPropertySegment navigationSegment)
                {
                    var propertyName = navigationSegment.NavigationProperty.Name;
                    //navigationSegment.NavigationProperty.
                    current = current.GetType().GetProperty(propertyName).GetValue(current);

                    // TODO: should a null property be considered 404?
                    if (current == null)
                    {
                        break;
                    }
                }
            }

            return Ok(current);
        }
    }
}
