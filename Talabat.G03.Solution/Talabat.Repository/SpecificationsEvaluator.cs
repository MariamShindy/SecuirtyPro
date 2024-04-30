using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Specifications;

namespace Talabat.Repository
{
	internal static class SpecificationsEvaluator<TEntity> where TEntity : BaseEntity
	{
		public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecifications<TEntity> spec)
		{
			var query = inputQuery; // query = _dbContext.Set<Order>()

			if(spec.Criteria is not null) // O => O.PaymentIntentId == "pi_3ODt1kJPgoKaWy9t1lhoc1z8"
				query = query.Where(spec.Criteria);

			// query = _dbContext.Set<Order>().Where(O => O.PaymentIntentId == "pi_3ODt1kJPgoKaWy9t1lhoc1z8")

			if (spec.OrderBy is not null)
				query = query.OrderBy(spec.OrderBy);

			else if(spec.OrderByDesc is not null) 
				query = query.OrderByDescending(spec.OrderByDesc);

			// query = _dbContext.Set<Order>().Where(O => O.PaymentIntentId == "pi_3ODt1kJPgoKaWy9t1lhoc1z8")


			if(spec.IsPaginationEnabled)
				query = query.Skip(spec.Skip).Take(spec.Take);

			// query = _dbContext.Set<Order>().Where(O => O.PaymentIntentId == "pi_3ODt1kJPgoKaWy9t1lhoc1z8")

			// Includes

			query = spec.Includes.Aggregate(query, (currentQuery, includeExpression) => currentQuery.Include(includeExpression));

			// query = _dbContext.Set<Order>().Where(O => O.PaymentIntentId == "pi_3ODt1kJPgoKaWy9t1lhoc1z8")

			return query;
		}
	}
}
