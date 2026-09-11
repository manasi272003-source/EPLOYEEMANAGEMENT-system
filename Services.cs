#nullable disable

using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace MANASI
{
    public class Services
    {
        private string GetConnectionString()
        {
            string server =
                Environment.GetEnvironmentVariable("DB_SERVER")
                ?? "manasi-employee-db-manasi272003-e1b2.f.aivencloud.com";

            string port =
                Environment.GetEnvironmentVariable("DB_PORT")
                ?? "28290";

            string database =
                Environment.GetEnvironmentVariable("DB_NAME")
                ?? "manasidb";

            string user =
                Environment.GetEnvironmentVariable("DB_USER")
                ?? "avnadmin";

            string password =
                Environment.GetEnvironmentVariable("DB_PASSWORD")
                ?? "";

            if (string.IsNullOrWhiteSpace(server) ||
                string.IsNullOrWhiteSpace(port) ||
                string.IsNullOrWhiteSpace(database) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new Exception(
                    "Database environment variables are not configured correctly."
                );
            }

            Console.WriteLine("========================================");
            Console.WriteLine("DATABASE CONFIGURATION");
            Console.WriteLine("Server: " + server);
            Console.WriteLine("Port: " + port);
            Console.WriteLine("Database: " + database);
            Console.WriteLine("User: " + user);
         Console.WriteLine("Password: [HIDDEN]");
            Console.WriteLine("========================================");

            return
                $"Server={server};" +
                $"Port={port};" +
                $"Database={database};" +
                $"User ID={user};" +
                $"Password={password};" +
                "SslMode=Required;" +
                "Connection Timeout=30;";
        }

        public List<Employee> GetEmployees()
        {
            List<Employee> employees = new List<Employee>();

            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(GetConnectionString()))
                {
                    connection.Open();

                    string query = @"
                        SELECT Id, Name, Age, Status, Photo
                        FROM employee1
                        ORDER BY Id ASC;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    using (MySqlDataReader reader =
                        command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new Employee
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"]?.ToString() ?? "",
                                Age = Convert.ToInt32(reader["Age"]),
                                Status = reader["Status"]?.ToString() ?? "Active",
                                Photo = reader["Photo"]?.ToString() ?? ""
                            });
                        }
                    }
                }

                return employees;
            }
            catch (Exception ex)
            {
                Console.WriteLine("========================================");
                Console.WriteLine("DATABASE ERROR - GetEmployees");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("========================================");

                throw new Exception(
                    "Unable to load employees. " + ex.Message,
                    ex
                );
            }
        }

        public Employee GetEmployeeById(int id)
        {
            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(GetConnectionString()))
                {
                    connection.Open();

                    string query = @"
                        SELECT Id, Name, Age, Status, Photo
                        FROM employee1
                        WHERE Id = @Id;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Employee
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"]?.ToString() ?? "",
                                    Age = Convert.ToInt32(reader["Age"]),
                                    Status = reader["Status"]?.ToString() ?? "Active",
                                    Photo = reader["Photo"]?.ToString() ?? ""
                                };
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DATABASE ERROR - GetEmployeeById");
                Console.WriteLine(ex.ToString());

                throw new Exception(
                    "Unable to load employee. " + ex.Message,
                    ex
                );
            }
        }

        public Employee CreateEmployee(
            string name,
            int age,
            string status,
            string photo)
        {
            ValidateEmployee(name, age, status);

            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(GetConnectionString()))
                {
                    connection.Open();

                    string checkQuery = @"
                        SELECT COUNT(*)
                        FROM employee1
                        WHERE LOWER(Name) = LOWER(@Name);
                    ";

                    using (MySqlCommand checkCommand =
                        new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue(
                            "@Name",
                            name.Trim()
                        );

                        int count = Convert.ToInt32(
                            checkCommand.ExecuteScalar()
                        );

                        if (count > 0)
                        {
                            throw new Exception(
                                "An employee with this name already exists."
                            );
                        }
                    }

                    string insertQuery = @"
                        INSERT INTO employee1
                            (Name, Age, Status, Photo)
                        VALUES
                            (@Name, @Age, @Status, @Photo);

                        SELECT LAST_INSERT_ID();
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@Name",
                            name.Trim()
                        );

                        command.Parameters.AddWithValue("@Age", age);

                        command.Parameters.AddWithValue(
                            "@Status",
                            string.IsNullOrWhiteSpace(status)
                                ? "Active"
                                : status.Trim()
                        );

                        command.Parameters.AddWithValue(
                            "@Photo",
                            string.IsNullOrEmpty(photo)
                                ? (object)DBNull.Value
                                : photo
                        );

                        int newId = Convert.ToInt32(
                            command.ExecuteScalar()
                        );

                        return new Employee
                        {
                            Id = newId,
                            Name = name.Trim(),
                            Age = age,
                            Status = string.IsNullOrWhiteSpace(status)
                                ? "Active"
                                : status.Trim(),
                            Photo = photo ?? ""
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DATABASE ERROR - CreateEmployee");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public Employee UpdateEmployee(
            int id,
            string name,
            int age,
            string status,
            string photo)
        {
            ValidateEmployee(name, age, status);

            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(GetConnectionString()))
                {
                    connection.Open();

                    string existsQuery = @"
                        SELECT COUNT(*)
                        FROM employee1
                        WHERE Id = @Id;
                    ";

                    using (MySqlCommand existsCommand =
                        new MySqlCommand(existsQuery, connection))
                    {
                        existsCommand.Parameters.AddWithValue("@Id", id);

                        if (Convert.ToInt32(
                            existsCommand.ExecuteScalar()) == 0)
                        {
                            throw new Exception(
                                "Employee with this ID does not exist."
                            );
                        }
                    }

                    string duplicateQuery = @"
                        SELECT COUNT(*)
                        FROM employee1
                        WHERE LOWER(Name) = LOWER(@Name)
                        AND Id <> @Id;
                    ";

                    using (MySqlCommand duplicateCommand =
                        new MySqlCommand(duplicateQuery, connection))
                    {
                        duplicateCommand.Parameters.AddWithValue(
                            "@Name",
                            name.Trim()
                        );

                        duplicateCommand.Parameters.AddWithValue(
                            "@Id",
                            id
                        );

                        if (Convert.ToInt32(
                            duplicateCommand.ExecuteScalar()) > 0)
                        {
                            throw new Exception(
                                "Another employee with this name already exists."
                            );
                        }
                    }

                    string updateQuery;

                    if (photo != null)
                    {
                        updateQuery = @"
                            UPDATE employee1
                            SET
                                Name = @Name,
                                Age = @Age,
                                Status = @Status,
                                Photo = @Photo
                            WHERE Id = @Id;
                        ";
                    }
                    else
                    {
                        updateQuery = @"
                            UPDATE employee1
                            SET
                                Name = @Name,
                                Age = @Age,
                                Status = @Status
                            WHERE Id = @Id;
                        ";
                    }

                    using (MySqlCommand command =
                        new MySqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@Name", name.Trim());
                        command.Parameters.AddWithValue("@Age", age);

                        command.Parameters.AddWithValue(
                            "@Status",
                            string.IsNullOrWhiteSpace(status)
                                ? "Active"
                                : status.Trim()
                        );

                        if (photo != null)
                        {
                            command.Parameters.AddWithValue(
                                "@Photo",
                                string.IsNullOrEmpty(photo)
                                    ? (object)DBNull.Value
                                    : photo
                            );
                        }

                        command.ExecuteNonQuery();
                    }

                    return GetEmployeeById(id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DATABASE ERROR - UpdateEmployee");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public bool UpdateEmployeeStatus(
            int id,
            string status)
        {
            if (status != "Active" && status != "Inactive")
            {
                throw new Exception(
                    "Status must be either Active or Inactive."
                );
            }

            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(GetConnectionString()))
                {
                    connection.Open();

                    string query = @"
                        UPDATE employee1
                        SET Status = @Status
                        WHERE Id = @Id;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@Status", status);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "DATABASE ERROR - UpdateEmployeeStatus"
                );

                Console.WriteLine(ex.ToString());

                throw new Exception(
                    "Unable to update employee status. " + ex.Message,
                    ex
                );
            }
        }

        public bool DeleteEmployee(int id)
        {
            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(GetConnectionString()))
                {
                    connection.Open();

                    string query = @"
                        DELETE FROM employee1
                        WHERE Id = @Id;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DATABASE ERROR - DeleteEmployee");
                Console.WriteLine(ex.ToString());

                throw new Exception(
                    "Unable to delete employee. " + ex.Message,
                    ex
                );
            }
        }

        public List<Employee> SearchEmployees(string search)
        {
            List<Employee> employees = new List<Employee>();

            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(GetConnectionString()))
                {
                    connection.Open();

                    string query = @"
                        SELECT Id, Name, Age, Status, Photo
                        FROM employee1
                        WHERE
                            Name LIKE @Search
                            OR CAST(Id AS CHAR) LIKE @Search
                        ORDER BY Id ASC;
                    ";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@Search",
                            "%" + (search ?? "").Trim() + "%"
                        );

                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                employees.Add(new Employee
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"]?.ToString() ?? "",
                                    Age = Convert.ToInt32(reader["Age"]),
                                    Status = reader["Status"]?.ToString()
                                        ?? "Active",
                                    Photo = reader["Photo"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }

                return employees;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DATABASE ERROR - SearchEmployees");
                Console.WriteLine(ex.ToString());

                throw new Exception(
                    "Unable to search employees. " + ex.Message,
                    ex
                );
            }
        }

        public Employee ImportEmployee(Employee employee)
        {
            if (employee == null)
            {
                throw new Exception(
                    "Employee data cannot be null."
                );
            }

            ValidateEmployee(
                employee.Name,
                employee.Age,
                employee.Status
            );

            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(GetConnectionString()))
                {
                    connection.Open();

                    string idQuery = @"
                        SELECT COUNT(*)
                        FROM employee1
                        WHERE Id = @Id;
                    ";

                    bool idExists;

                    using (MySqlCommand idCommand =
                        new MySqlCommand(idQuery, connection))
                    {
                        idCommand.Parameters.AddWithValue(
                            "@Id",
                            employee.Id
                        );

                        idExists =
                            Convert.ToInt32(
                                idCommand.ExecuteScalar()
                            ) > 0;
                    }

                    if (idExists)
                    {
                        string duplicateNameQuery = @"
                            SELECT COUNT(*)
                            FROM employee1
                            WHERE LOWER(Name) = LOWER(@Name)
                            AND Id <> @Id;
                        ";

                        using (MySqlCommand duplicateCommand =
                            new MySqlCommand(
                                duplicateNameQuery,
                                connection))
                        {
                            duplicateCommand.Parameters.AddWithValue(
                                "@Name",
                                employee.Name.Trim()
                            );

                            duplicateCommand.Parameters.AddWithValue(
                                "@Id",
                                employee.Id
                            );

                            if (Convert.ToInt32(
                                duplicateCommand.ExecuteScalar()) > 0)
                            {
                                throw new Exception(
                                    "Another employee with this name already exists."
                                );
                            }
                        }

                        string updateQuery = @"
                            UPDATE employee1
                            SET
                                Name = @Name,
                                Age = @Age,
                                Status = @Status,
                                Photo = @Photo
                            WHERE Id = @Id;
                        ";

                        using (MySqlCommand command =
                            new MySqlCommand(
                                updateQuery,
                                connection))
                        {
                            command.Parameters.AddWithValue(
                                "@Id",
                                employee.Id
                            );

                            command.Parameters.AddWithValue(
                                "@Name",
                                employee.Name.Trim()
                            );

                            command.Parameters.AddWithValue(
                                "@Age",
                                employee.Age
                            );

                            command.Parameters.AddWithValue(
                                "@Status",
                                string.IsNullOrWhiteSpace(
                                    employee.Status)
                                    ? "Active"
                                    : employee.Status.Trim()
                            );

                            command.Parameters.AddWithValue(
                                "@Photo",
                                string.IsNullOrEmpty(employee.Photo)
                                    ? (object)DBNull.Value
                                    : employee.Photo
                            );

                            command.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string duplicateNameQuery = @"
                            SELECT COUNT(*)
                            FROM employee1
                            WHERE LOWER(Name) = LOWER(@Name);
                        ";

                        using (MySqlCommand duplicateCommand =
                            new MySqlCommand(
                                duplicateNameQuery,
                                connection))
                        {
                            duplicateCommand.Parameters.AddWithValue(
                                "@Name",
                                employee.Name.Trim()
                            );

                            if (Convert.ToInt32(
                                duplicateCommand.ExecuteScalar()) > 0)
                            {
                                throw new Exception(
                                    "An employee with this name already exists."
                                );
                            }
                        }

                        string insertQuery = @"
                            INSERT INTO employee1
                                (Id, Name, Age, Status, Photo)
                            VALUES
                                (@Id, @Name, @Age, @Status, @Photo);
                        ";

                        using (MySqlCommand command =
                            new MySqlCommand(
                                insertQuery,
                                connection))
                        {
                            command.Parameters.AddWithValue(
                                "@Id",
                                employee.Id
                            );

                            command.Parameters.AddWithValue(
                                "@Name",
                                employee.Name.Trim()
                            );

                            command.Parameters.AddWithValue(
                                "@Age",
                                employee.Age
                            );

                            command.Parameters.AddWithValue(
                                "@Status",
                                string.IsNullOrWhiteSpace(
                                    employee.Status)
                                    ? "Active"
                                    : employee.Status.Trim()
                            );

                            command.Parameters.AddWithValue(
                                "@Photo",
                                string.IsNullOrEmpty(employee.Photo)
                                    ? (object)DBNull.Value
                                    : employee.Photo
                            );

                            command.ExecuteNonQuery();
                        }
                    }

                    return GetEmployeeById(employee.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DATABASE ERROR - ImportEmployee");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private void ValidateEmployee(
            string name,
            int age,
            string status)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception(
                    "Employee name is required."
                );
            }

            if (name.Trim().Length > 100)
            {
                throw new Exception(
                    "Employee name cannot exceed 100 characters."
                );
            }

            if (age < 1 || age > 100)
            {
                throw new Exception(
                    "Employee age must be between 1 and 100."
                );
            }

            if (!string.IsNullOrWhiteSpace(status) &&
                status != "Active" &&
                status != "Inactive")
            {
                throw new Exception(
                    "Status must be either Active or Inactive."
                );
            }
        }
    }
}