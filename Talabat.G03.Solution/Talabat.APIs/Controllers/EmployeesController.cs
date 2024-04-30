using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Specifications;
using Talabat.Core.Specifications.Employee_Specs;
using Talabat.Core.Specifications.Product_Specs;

namespace Talabat.APIs.Controllers
{

	public class EmployeesController : BaseApiController
	{
		private readonly IGenericRepository<Employee> _employeesRepo;

		public EmployeesController(IGenericRepository<Employee> employeesRepo)
        {
			_employeesRepo = employeesRepo;
		}


		[HttpGet] // GET: /api/employees
		public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
		{
			var spec = new EmployeeWithDepartmentSpecifications();
			var employees = await _employeesRepo.GetAllWithSpecAsync(spec);

			return Ok(employees);
		}

		[HttpGet("{id}")] // GET: /api/employee/1
		public async Task<ActionResult<Employee>> GetEmployee(int id)
		{
			var spec = new EmployeeWithDepartmentSpecifications(id);

			var employee = await _employeesRepo.GetEntityWithSpecAsync(spec);

			if (employee is null)
				return NotFound(new { Message = "Not Found", StatusCode = 404 });

			return Ok(employee);
		}
    }
}
