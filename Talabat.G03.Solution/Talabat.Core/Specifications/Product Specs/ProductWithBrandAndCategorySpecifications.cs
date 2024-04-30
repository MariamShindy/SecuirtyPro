using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;

namespace Talabat.Core.Specifications.Product_Specs
{
	public class ProductWithBrandAndCategorySpecifications : BaseSpecifications<Product>
	{
        // This Constructor is Used in Creating an Object, This Object will be used for Getting all Products
        public ProductWithBrandAndCategorySpecifications(ProductSpecParams specParams)
            :base(P => 
					(string.IsNullOrEmpty(specParams.Search) || P.Name.ToLower().Contains(specParams.Search)) &&
					(!specParams.BrandId.HasValue || P.BrandId == specParams.BrandId.Value) &&
					(!specParams.CategoryId.HasValue || P.CategoryId == specParams.CategoryId.Value)
			)
		{
			Includes.Add(P => P.Brand);
			Includes.Add(P => P.Category);

			if (!string.IsNullOrEmpty(specParams.Sort))
			{
				switch (specParams.Sort)
				{
					case "priceAsc":
						//OrderBy = P => P.Price;
						AddOrderBy(P => P.Price);
						break;
					case "priceDesc":
						//OrderByDesc = P => P.Price;
						AddOrderByDesc(P => P.Price);
						break;
					default:
						AddOrderBy(P => P.Name);
						break;
				}
			}
			else
				AddOrderBy(P => P.Name);

			// totalProducts = 5 ~ 6
			// pageSize      = 2
			// pageIndex     = 1

			ApplyPagination(specParams.PageSize * (specParams.PageIndex - 1), specParams.PageSize);
		}

		// This Constructor is Used in Creating an Object, This Object will be Used for Getting a Specific Product with Id
		public ProductWithBrandAndCategorySpecifications(int id)
            :base(P => P.Id == id)
        {
			Includes.Add(P => P.Brand);
			Includes.Add(P => P.Category);
		}
	}
}
