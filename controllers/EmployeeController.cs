using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MANASI;

namespace MANASI.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeeController : ControllerBase
    {
        private readonly Services _services;

        public EmployeeController(Services services)
        {
            _services = services;
        }

        [HttpGet]
        public IActionResult GetEmployees()
        {
            try
            {
                return Ok(_services.GetEmployees());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    message = "Unable to load employees.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult GetEmployeeById(int id)
        {
            try
            {
                var employee = _services.GetEmployeeById(id);

                if (employee == null)
                {
                    return NotFound(new
                    {
                        message = "Employee not found."
                    });
                }

                return Ok(employee);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    message = "Unable to load employee.",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public IActionResult CreateEmployee(
            [FromBody] Employee employee)
        {
            try
            {
                if (employee == null)
                {
                    return BadRequest(new
                    {
                        message = "Employee data is required."
                    });
                }

                var result = _services.CreateEmployee(
                    employee.Name,
                    employee.Age,
                    employee.Status,
                    employee.Photo
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return BadRequest(new
                {
                    message = "Unable to create employee.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id:int}")]
        public IActionResult UpdateEmployee(
            int id,
            [FromBody] Employee employee)
        {
            try
            {
                if (employee == null)
                {
                    return BadRequest(new
                    {
                        message = "Employee data is required."
                    });
                }

                var result = _services.UpdateEmployee(
                    id,
                    employee.Name,
                    employee.Age,
                    employee.Status,
                    employee.Photo
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return BadRequest(new
                {
                    message = "Unable to update employee.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id:int}/status")]
        public IActionResult UpdateEmployeeStatus(
            int id,
            [FromBody] Employee employee)
        {
            try
            {
                if (employee == null)
                {
                    return BadRequest(new
                    {
                        message = "Employee data is required."
                    });
                }

                bool result =
                    _services.UpdateEmployeeStatus(
                        id,
                        employee.Status
                    );

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Employee not found."
                    });
                }

                return Ok(new
                {
                    message = "Employee status updated successfully."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return BadRequest(new
                {
                    message = "Unable to update employee status.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("{id:int}")]
        public IActionResult DeleteEmployee(int id)
        {
            try
            {
                bool result = _services.DeleteEmployee(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Employee not found."
                    });
                }

                return Ok(new
                {
                    message = "Employee deleted successfully."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return BadRequest(new
                {
                    message = "Unable to delete employee.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("search")]
        public IActionResult SearchEmployees(
            [FromQuery] string search = "")
        {
            try
            {
                return Ok(_services.SearchEmployees(search));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    message = "Unable to search employees.",
                    error = ex.Message
                });
            }
        }
        [HttpGet("export")]
public IActionResult ExportEmployees()
{
    try
    {
        var employees = _services.GetEmployees();

        var csv = new StringBuilder();

        // CSV Header
        csv.AppendLine("ID,Name,Age,Status,Photo");

        foreach (var employee in employees)
        {
            csv.AppendLine(
                $"{EscapeCsv(employee.Id.ToString())}," +
                $"{EscapeCsv(employee.Name)}," +
                $"{EscapeCsv(employee.Age.ToString())}," +
                $"{EscapeCsv(employee.Status)}," +
                $"{EscapeCsv(employee.Photo)}"
            );
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());

        return File(
            bytes,
            "text/csv",
            "employees.csv"
        );
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());

        return StatusCode(500, new
        {
            message = "Unable to export employees.",
            error = ex.Message
        });
    }
}

private string EscapeCsv(string value)
{
    if (string.IsNullOrEmpty(value))
    {
        return "";
    }

    // Escape quotes
    value = value.Replace("\"", "\"\"");

    // Wrap fields containing special CSV characters
    if (value.Contains(",") ||
        value.Contains("\"") ||
        value.Contains("\n") ||
        value.Contains("\r"))
    {
        return $"\"{value}\"";
    }

    return value;
}

        [HttpPost("import")]
        public IActionResult ImportEmployee(
            [FromBody] Employee employee)
        {
            try
            {
                if (employee == null)
                {
                    return BadRequest(new
                    {
                        message = "Employee data is required."
                    });
                }

                var result = _services.ImportEmployee(employee);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return BadRequest(new
                {
                    message = "Unable to import employee.",
                    error = ex.Message
                });
            }
        }
    }
}