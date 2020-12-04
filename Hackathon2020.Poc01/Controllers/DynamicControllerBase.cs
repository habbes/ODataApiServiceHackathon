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
using Microsoft.AspNet.OData.Routing;
using DataLib;
using Microsoft.OData.Edm;

namespace Hackathon2020.Poc01.Controllers
{
    public class DynamicControllerBase<TEntity> : ODataController where TEntity : class
    {
        private IDataStore _db
        {
            get
            {
                // workaround to get the datastore from service container since
                // we can't use automatic DI from the constructor
                return (IDataStore)ControllerContext.HttpContext.RequestServices.GetService(typeof(IDataStore));
            }
        }

        private Microsoft.AspNet.OData.Routing.ODataPath ODataPath { get => HttpContext.ODataFeature().Path; }

        // Because new types are dynamically defined from this base class for each Singleton
        // We needed to define a default constructor, otherwise the TypeBuilder throws an error
        public DynamicControllerBase()
        {
        }

        // AspNet complains when we have two suitable constructors, so had
        // to leave this one out until we figure out how to get generate
        // Singleton controllers without a default constructor
        //public DynamicControllerBase(IDataStore db)
        //{
        //    _db = db;
        //}

        public ActionResult Delete()
        {
            var segments = ODataPath.Segments;

            TEntity entity = null;

            if (IsRootEntitySet())
            {
                // only support patch /entitySet/key, we don't support nested paths at this time
                var keySegment = segments.OfType<KeySegment>().FirstOrDefault();
                if (keySegment == null)
                {
                    return NotFound();
                }

                entity = _db.Set<TEntity>().FindByKey(keySegment.Keys.Select(kvp => kvp.Value).ToArray());
                if (entity == null)
                {
                    return NotFound();
                }

                _db.Set<TEntity>().Remove(entity);
            }
            else if (IsRootSingleton())
            {
                var wrapper = _db.Singleton<TEntity>(GetEdmSingleton().Name);
                entity = wrapper.Value;
                if (entity == null)
                {
                    return NotFound();
                }

                wrapper.Remove();
            }

            
            _db.SaveChanges();
            return Ok(entity);
        }

        public ActionResult Post([FromBody] TEntity data)
        {
            if (IsRootSingleton())
            {
                // POST not supported for singletons
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var added = _db.Set<TEntity>().Add(data);
            _db.SaveChanges();
            return Ok(added);
        }

        public ActionResult Patch(Delta<TEntity> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TEntity entity = null;

            if (IsRootEntitySet())
            {
                var segments = ODataPath.Segments;

                // only support patch /entitySet/key, we don't support nested paths at this time
                var keySegment = segments.OfType<KeySegment>().FirstOrDefault();
                if (keySegment == null)
                {
                    return NotFound();
                }

                var dbSet = _db.Set<TEntity>();
                entity = dbSet.FindByKey(keySegment.Keys.Select(kvp => kvp.Value).ToArray());

                if (entity == null)
                {
                    return NotFound();
                }

                patch.Patch(entity);
            }
            else if (IsRootSingleton())
            {
                var wrapper = _db.Singleton<TEntity>(GetEdmSingleton().Name);
                entity = wrapper.Value;

                if (entity == null)
                {
                    return NotFound();
                }

                patch.Patch(entity);
            }

            _db.SaveChanges();

            return Ok(entity);
        }

        public ActionResult Put(Delta<TEntity> update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TEntity entity = null;
            if (IsRootEntitySet())
            {
                var segments = ODataPath.Segments;
                var keySegment = segments.OfType<KeySegment>().FirstOrDefault();
                if (keySegment == null)
                {
                    return NotFound();
                }

                entity = _db.Set<TEntity>().FindByKey(keySegment.Keys.Select(kvp => kvp.Value).ToArray());
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

            }
            else if (IsRootSingleton())
            {
                var wrapper = _db.Singleton<TEntity>(GetEdmSingleton().Name);
                entity = wrapper.Value;
                if (entity == null)
                {
                    entity = wrapper.Set(update.GetInstance());
                }
                else
                {
                    update.Put(entity);
                }
            }

            
            try
            {
                //var updated = _db.Update(entity);
                _db.SaveChanges();
                return Ok(entity);
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

        public IActionResult CreateRef([FromBody] Uri link)
        {
            var odataFeature = HttpContext.ODataFeature();
            var requestContainer = odataFeature.RequestContainer;
            var urlHelper = odataFeature.UrlHelper;
            var pathHandler = (IODataPathHandler)requestContainer.GetService(typeof(IODataPathHandler));


            var segments = ODataPath.Segments;
            
            var navLinkSegment = segments.OfType<NavigationPropertyLinkSegment>().First();
            var entityType = typeof(TEntity);

            object entity = null;

            if (IsRootEntitySet())
            {
                var dbSet = _db.Set<TEntity>().Include(navLinkSegment.NavigationProperty.Name);
                var keySegment = segments.OfType<KeySegment>().First();
                entity = GetEntityByKey(dbSet, keySegment);
            }
            else if (IsRootSingleton())
            {
                var wrapper = _db.Singleton<TEntity>(GetEdmSingleton().Name);
                entity = wrapper.Value;
            }

            if (entity == null)
            {
                return NotFound();
            }

            var navProperty = entityType.GetProperty(navLinkSegment.NavigationProperty.Name);
            var isList = typeof(IList<>).MakeGenericType(navProperty.PropertyType.GetGenericArguments()).IsAssignableFrom(navProperty.PropertyType);

            var relatedType = isList ? navProperty.PropertyType.GetGenericArguments()[0] : navProperty.PropertyType;

            var dbSetMethod = _db.GetType().GetMethod("Set").MakeGenericMethod(relatedType);
            var relatedDbSet = dbSetMethod.Invoke(_db, Array.Empty<object>());

            
            // extract link from url
            // request body looks like { "@odata.id": "http://serviceroot/People(1)" }
            string serviceRoot = urlHelper.CreateODataLink();
            var linkOdataPath = pathHandler.Parse(serviceRoot, link.AbsoluteUri, requestContainer);
            var relatedKeySegment = linkOdataPath.Segments.OfType<KeySegment>().First();
            var relatedEntity = GetEntityByKey(relatedDbSet, relatedKeySegment);
            if (relatedEntity == null)
            {
                return null;
            }

            if (isList)
            {
                // entity.NavProp.Add(relatedEntity)
                var relatedEntitiesList = navProperty.GetValue(entity);
                navProperty.PropertyType.GetMethod("Add").Invoke(relatedEntitiesList, new[] { relatedEntity });
            }
            else
            {
                // entity.NavProp = relatedEntity
                navProperty.SetValue(entity, relatedEntity);
            }

            _db.SaveChanges();
            
            return Ok();
        }

        public IActionResult DeleteRef()
        {
            var segments = ODataPath.Segments;
            var navLinkSegment = segments.OfType<NavigationPropertyLinkSegment>().First();

            object entity = null;

            if (IsRootEntitySet())
            {
                var keySegment = segments.OfType<KeySegment>().First();
                var dbSet = _db.Set<TEntity>().Include(navLinkSegment.NavigationProperty.Name);
                entity = GetEntityByKey(dbSet, keySegment);
            }
            else if (IsRootSingleton())
            {
                var wrapper = _db.Singleton<TEntity>(GetEdmSingleton().Name);
                entity = wrapper.Value;
            }

            if (entity == null)
            {
                return NotFound();
            }

            var entityType = typeof(TEntity);

            var navProperty = entityType.GetProperty(navLinkSegment.NavigationProperty.Name);
            var isList = typeof(IList<>)
                .MakeGenericType(navProperty.PropertyType.GetGenericArguments())
                .IsAssignableFrom(navProperty.PropertyType);

            if (isList)
            {
                // entity.NavProp.Remove(relatedEntity);
                var relatedKeySegment = segments.OfType<KeySegment>().Last();
                var relatedEntitiesList = navProperty.GetValue(entity);
                var relatedEntity = GetEntityByKey(relatedEntitiesList, relatedKeySegment);
                if (relatedEntity == null)
                {
                    return NotFound();
                }

                navProperty.PropertyType.GetMethod("Remove").Invoke(relatedEntitiesList, new[] { relatedEntity });
            }
            else
            {
                // entity.NavProp = null;
                navProperty.SetValue(entity, null);
            }

            _db.SaveChanges();

            return Ok();
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

            var entity = _db.Set<TEntity>().FindByKey(keySegment.Keys.Select(kvp => kvp.Value).ToArray());
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
            var segments = odataPath.Segments.Skip(1); // skip the first segment, expected to be the navigation source segment

            object current = null;
            if (IsRootEntitySet())
            {
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
                current = string.IsNullOrEmpty(pathToInclude) ? dbSet : dbSet.Include(pathToInclude);
            }
            else if (IsRootSingleton())
            {
                var wrapper = _db.Singleton<TEntity>(GetEdmSingleton().Name);
                current = wrapper.Value;
            }

            foreach (var segment in segments)
            {
                if (current == null)
                {
                    return NotFound();
                }

                if (segment is KeySegment keySegment)
                {
                    current = GetEntityByKey(current, keySegment);

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
                }
                else if (segment is PropertySegment propertySegment)
                {
                    var propertyName = propertySegment.Property.Name;
                    current = current.GetType().GetProperty(propertyName).GetValue(current);
                }
            }

            return Ok(current);
        }

        private object GetEntityByKey(object querySet, KeySegment keySegment)
        {
            var current = querySet;
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

            return current;
        }

        private bool IsRootEntitySet()
        {
            return ODataPath.Segments.First() is EntitySetSegment;
        }

        private bool IsRootSingleton()
        {
            return ODataPath.Segments.First() is SingletonSegment;
        }

        private IEdmSingleton GetEdmSingleton()
        {
            var segment = ODataPath.Segments.First() as SingletonSegment;
            return segment?.Singleton;
        }
    }
}
