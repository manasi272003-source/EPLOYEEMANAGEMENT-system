const API_URL = "/api/employees";


// GET EMPLOYEES
async function loadEmployees() {

    const response = await fetch(API_URL);

    const employees = await response.json();

    const table = document.getElementById("employeeTable");

    table.innerHTML = "";

    employees.forEach(employee => {

        table.innerHTML += `
            <tr>
                <td>${employee.id}</td>
                <td>${employee.name}</td>
                <td>${employee.age}</td>
                <td>
                    <button class="delete-btn"
                        onclick="deleteEmployee(${employee.id})">
                        Delete
                    </button>
                </td>
            </tr>
        `;

    });
}


// ADD EMPLOYEE
async function addEmployee() {

    const name = document.getElementById("name").value;

    const age = document.getElementById("age").value;

    if (name === "" || age === "") {
        alert("Please enter name and age");
        return;
    }

    await fetch(API_URL, {

        method: "POST",

        headers: {
            "Content-Type": "application/json"
        },

        body: JSON.stringify({
            name: name,
            age: parseInt(age)
        })

    });

    document.getElementById("name").value = "";

    document.getElementById("age").value = "";

    loadEmployees();
}


// DELETE EMPLOYEE
async function deleteEmployee(id) {

    await fetch(`${API_URL}/${id}`, {
        method: "DELETE"
    });

    loadEmployees();
}


// LOAD DATA WHEN WEBSITE OPENS
loadEmployees();