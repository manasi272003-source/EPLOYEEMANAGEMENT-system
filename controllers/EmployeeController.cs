using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
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

        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Employees");

        // ====================================================
        // HEADER
        // ====================================================

        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Age";
        worksheet.Cell(1, 4).Value = "Status";
        worksheet.Cell(1, 5).Value = "Photo";

        var headerRange = worksheet.Range("A1:E1");

        headerRange.Style.Font.Bold = true;
        headerRange.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        // ====================================================
        // EMPLOYEE DATA
        // ====================================================

        int row = 2;

        foreach (var employee in employees)
        {
            worksheet.Cell(row, 1).Value = employee.Id;
            worksheet.Cell(row, 2).Value = employee.Name;
            worksheet.Cell(row, 3).Value = employee.Age;
            worksheet.Cell(row, 4).Value = employee.Status;

            // ====================================================
            // PHOTO
            // ====================================================

            if (!string.IsNullOrWhiteSpace(employee.Photo))
            {
                try
                {
                    string photo = employee.Photo.Trim();

                    // Remove Data URL prefix
                    if (photo.StartsWith("data:image/",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        int commaIndex = photo.IndexOf(',');

                        if (commaIndex >= 0)
                        {
                            photo = photo.Substring(
                                commaIndex + 1
                            );
                        }
                    }

                    // Remove whitespace
                    photo = photo.Replace(
                        "\r",
                        ""
                    ).Replace(
                        "\n",
                        ""
                    ).Trim();

                    // Convert Base64 → image bytes
                    byte[] imageBytes =
                        Convert.FromBase64String(photo);

                    using var imageStream =
                        new MemoryStream(imageBytes);

                    // Add actual image to Excel
                    var picture =
                        worksheet.AddPicture(imageStream)
                                 .MoveTo(
                                     worksheet.Cell(row, 5)
                                 );

                    picture.Width = 80;
                    picture.Height = 80;
                }
                catch (Exception photoException)
                {
                    Console.WriteLine(
                        $"Photo export failed for employee {employee.Id}: " +
                        photoException.Message
                    );
                }
            }

            row++;
        }

        // ====================================================
        // FORMAT
        // ====================================================

        worksheet.Column(1).Width = 10;
        worksheet.Column(2).Width = 25;
        worksheet.Column(3).Width = 10;
        worksheet.Column(4).Width = 15;
        worksheet.Column(5).Width = 15;

        // Give rows enough height for photos
        for (int i = 2; i < row; i++)
        {
            worksheet.Row(i).Height = 65;
        }

        // Center ID, Age, Status and Photo
        worksheet.Column(1).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        worksheet.Column(3).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        worksheet.Column(4).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        worksheet.Column(5).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        worksheet.Column(1).Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        worksheet.Column(2).Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        worksheet.Column(3).Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        worksheet.Column(4).Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        worksheet.Column(5).Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        // Add filter
        worksheet.Range(
            1,
            1,
            Math.Max(1, row - 1),
            5
        ).SetAutoFilter();

        // Freeze header
        worksheet.SheetView.FreezeRows(1);

        // ====================================================
        // RETURN EXCEL FILE
        // ====================================================

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        byte[] bytes = stream.ToArray();

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "employees.xlsx"
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