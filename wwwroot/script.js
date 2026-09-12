// ============================================================
// PEOPLEFLOW - EMPLOYEE MANAGEMENT
// ============================================================

let employees = [];
let filteredEmployees = [];

let currentPage = 1;
const employeesPerPage = 10;

let editingEmployeeId = null;
let employeeToDelete = null;


// ============================================================
// PAGE LOAD
// ============================================================

document.addEventListener("DOMContentLoaded", function () {

    setupEvents();

    loadEmployees();

    showDashboard();

});


// ============================================================
// SETUP EVENTS
// ============================================================

function setupEvents() {

    const form = document.getElementById("employeeForm");

    if (form) {

        form.addEventListener("submit", function (event) {

            event.preventDefault();

            saveEmployee();

        });

    }


    const photoInput =
        document.getElementById("employeePhoto");

    if (photoInput) {

        photoInput.addEventListener(
            "change",
            previewEmployeePhoto
        );

    }


    const searchInput =
        document.getElementById("searchInput");

    if (searchInput) {

        searchInput.addEventListener(
            "input",
            function () {

                currentPage = 1;

                applyFilters();

            }
        );

    }


    const statusFilter =
        document.getElementById("statusFilter");

    if (statusFilter) {

        statusFilter.addEventListener(
            "change",
            function () {

                currentPage = 1;

                applyFilters();

            }
        );

    }


    const sortFilter =
        document.getElementById("sortFilter");

    if (sortFilter) {

        sortFilter.addEventListener(
            "change",
            function () {

                currentPage = 1;

                applyFilters();

            }
        );

    }

}


// ============================================================
// NAVIGATION
// ============================================================

function showDashboard() {

    const dashboard =
        document.getElementById("dashboardSection");

    const employeesSection =
        document.getElementById("employeesSection");

    const dashboardNav =
        document.getElementById("dashboardNav");

    const employeesNav =
        document.getElementById("employeesNav");

    if (dashboard) {
        dashboard.classList.remove("hidden");
    }

    if (employeesSection) {
        employeesSection.classList.add("hidden");
    }

    if (dashboardNav) {
        dashboardNav.classList.add("active");
    }

    if (employeesNav) {
        employeesNav.classList.remove("active");
    }

    setText("pageTitle", "Dashboard");

}


function showEmployees() {

    const dashboard =
        document.getElementById("dashboardSection");

    const employeesSection =
        document.getElementById("employeesSection");

    const dashboardNav =
        document.getElementById("dashboardNav");

    const employeesNav =
        document.getElementById("employeesNav");

    if (dashboard) {
        dashboard.classList.add("hidden");
    }

    if (employeesSection) {
        employeesSection.classList.remove("hidden");
    }

    if (dashboardNav) {
        dashboardNav.classList.remove("active");
    }

    if (employeesNav) {
        employeesNav.classList.add("active");
    }

    setText("pageTitle", "Employees");

}


// ============================================================
// LOAD EMPLOYEES
// ============================================================

async function loadEmployees() {

    try {

        const response =
            await fetch("/api/employees");

        if (!response.ok) {

            throw new Error(
                "Unable to load employees."
            );

        }


        const data =
            await response.json();


        employees =
            Array.isArray(data)
                ? data.map(employee => ({

                    id:
                        Number(employee.id),

                    name:
                        String(
                            employee.name || ""
                        ),

                    age:
                        Number(
                            employee.age || 0
                        ),

                    status:
                        employee.status === "Inactive"
                            ? "Inactive"
                            : "Active",

                    photo:
                        normalizePhoto(
                            employee.photo
                        )

                }))
                : [];


        console.log(
            "Employees loaded:",
            employees
        );


        employees.forEach(employee => {

            console.log(
                `Employee ${employee.id} - ${employee.name} - Photo length: ${employee.photo.length}`
            );

        });


        applyFilters();

        updateDashboard();

    }
    catch (error) {

        console.error(
            "Load employees error:",
            error
        );

        showToast(
            "Unable to load employees.",
            "error"
        );

    }

}


// ============================================================
// NORMALIZE PHOTO
// ============================================================

function normalizePhoto(photo) {

    if (!photo) {
        return "";
    }


    const value =
        String(photo).trim();


    if (
        value === "" ||
        value === "null" ||
        value === "undefined"
    ) {

        return "";

    }


    // Already a complete Data URL

    if (
        value.startsWith("data:image/")
    ) {

        return value;

    }


    // Raw Base64

    const cleanBase64 =
        value.replace(/\s/g, "");


    if (
        cleanBase64.length > 100 &&
        /^[A-Za-z0-9+/=]+$/.test(
            cleanBase64
        )
    ) {

        return (
            "data:image/jpeg;base64," +
            cleanBase64
        );

    }


    return "";

}


// ============================================================
// FILTER + SORT
// ============================================================

function applyFilters() {

    const searchInput =
        document.getElementById(
            "searchInput"
        );

    const statusFilter =
        document.getElementById(
            "statusFilter"
        );

    const sortFilter =
        document.getElementById(
            "sortFilter"
        );


    const search =
        searchInput
            ? searchInput.value
                .trim()
                .toLowerCase()
            : "";


    const status =
        statusFilter
            ? statusFilter.value
            : "All";


    const sort =
        sortFilter
            ? sortFilter.value
            : "newest";


    filteredEmployees =
        employees.filter(employee => {

            const matchesSearch =
                !search ||
                employee.name
                    .toLowerCase()
                    .includes(search) ||
                String(employee.id)
                    .includes(search);


            const matchesStatus =
                status === "All" ||
                employee.status === status;


            return (
                matchesSearch &&
                matchesStatus
            );

        });


    // SORT

    if (sort === "newest") {

        filteredEmployees.sort(
            (a, b) =>
                Number(b.id) -
                Number(a.id)
        );

    }
    else if (sort === "oldest") {

        filteredEmployees.sort(
            (a, b) =>
                Number(a.id) -
                Number(b.id)
        );

    }
    else if (sort === "nameAsc") {

        filteredEmployees.sort(
            (a, b) =>
                a.name.localeCompare(
                    b.name
                )
        );

    }
    else if (sort === "nameDesc") {

        filteredEmployees.sort(
            (a, b) =>
                b.name.localeCompare(
                    a.name
                )
        );

    }


    const totalPages =
        Math.max(
            1,
            Math.ceil(
                filteredEmployees.length /
                employeesPerPage
            )
        );


    if (currentPage > totalPages) {

        currentPage =
            totalPages;

    }


    displayEmployees();

    displayRecentEmployees();

    updatePagination();

    updateEmployeeCount();

}


// ============================================================
// DISPLAY EMPLOYEES
// ============================================================

function displayEmployees() {

    const tableBody =
        document.getElementById(
            "employeeTableBody"
        );


    if (!tableBody) {

        console.error(
            "employeeTableBody not found."
        );

        return;

    }


    tableBody.innerHTML = "";


    const start =
        (currentPage - 1) *
        employeesPerPage;


    const end =
        start +
        employeesPerPage;


    const pageEmployees =
        filteredEmployees.slice(
            start,
            end
        );


    if (
        pageEmployees.length === 0
    ) {

        tableBody.innerHTML = `

            <tr>

                <td
                    colspan="5"
                    class="empty-state">

                    No employees found

                </td>

            </tr>

        `;

        return;

    }


    pageEmployees.forEach(
        employee => {

            tableBody.insertAdjacentHTML(
                "beforeend",
                createEmployeeRow(
                    employee
                )
            );

        }
    );

}


// ============================================================
// CREATE EMPLOYEE ROW
// ============================================================

function createEmployeeRow(employee) {

    const safeName =
        escapeHtml(
            employee.name
        );


    const safeStatus =
        escapeHtml(
            employee.status
        );


    const photo =
        normalizePhoto(
            employee.photo
        );


    let employeePhotoHTML;


    if (photo) {

        employeePhotoHTML = `

            <img
                src="${photo}"
                alt="${safeName}"
                class="employee-avatar-image"
                onerror="this.style.display='none'; this.nextElementSibling.style.display='flex';">

            <div
                class="employee-avatar employee-avatar-fallback"
                style="display:none;">

                ${getInitials(
                    employee.name
                )}

            </div>

        `;

    }
    else {

        employeePhotoHTML = `

            <div class="employee-avatar">

                ${getInitials(
                    employee.name
                )}

            </div>

        `;

    }


    return `

        <tr>

            <td>
                ${employee.id}
            </td>


            <td>

                <div class="employee-name-container">

                    <div class="employee-photo-wrapper">

                        ${employeePhotoHTML}

                    </div>


                    <div class="employee-name-text">

                        <strong>
                            ${safeName}
                        </strong>

                    </div>

                </div>

            </td>


            <td>
                ${employee.age}
            </td>


            <td>

                <span
                    class="status-badge ${employee.status.toLowerCase()}">

                    <span class="status-dot"></span>

                    ${safeStatus}

                </span>

            </td>


            <td>

                <div class="employee-actions">

                    <button
                        type="button"
                        class="edit-btn"
                        onclick="openEditModal(${employee.id})">

                        Edit

                    </button>


                    <button
                        type="button"
                        class="delete-btn"
                        onclick="openDeleteModal(${employee.id})">

                        Delete

                    </button>

                </div>

            </td>

        </tr>

    `;

}


// ============================================================
// RECENT EMPLOYEES
// ============================================================

function displayRecentEmployees() {

    const container =
        document.getElementById(
            "recentEmployees"
        );


    if (!container) {
        return;
    }


    container.innerHTML = "";


    const recentEmployees =
        [...employees]
            .sort(
                (a, b) =>
                    Number(b.id) -
                    Number(a.id)
            )
            .slice(0, 5);


    recentEmployees.forEach(
        employee => {

            const photo =
                normalizePhoto(
                    employee.photo
                );


            let avatar;


            if (photo) {

                avatar = `

                    <img
                        src="${photo}"
                        alt="${escapeHtml(employee.name)}"
                        class="employee-avatar-image">

                `;

            }
            else {

                avatar = `

                    <div class="employee-avatar">

                        ${getInitials(
                            employee.name
                        )}

                    </div>

                `;

            }


            container.insertAdjacentHTML(
                "beforeend",
                `

                <tr>

                    <td>
                        ${employee.id}
                    </td>

                    <td>

                        <div class="employee-name-container">

                            <div class="employee-photo-wrapper">

                                ${avatar}

                            </div>

                            <div class="employee-name-text">

                                <strong>
                                    ${escapeHtml(
                                        employee.name
                                    )}
                                </strong>

                            </div>

                        </div>

                    </td>

                    <td>
                        ${employee.age}
                    </td>

                    <td>

                        <span
                            class="status-badge ${employee.status.toLowerCase()}">

                            <span class="status-dot"></span>

                            ${escapeHtml(
                                employee.status
                            )}

                        </span>

                    </td>

                </tr>

                `
            );

        }
    );

}


// ============================================================
// OPEN ADD MODAL
// ============================================================

function openAddModal() {

    editingEmployeeId = null;


    const form =
        document.getElementById(
            "employeeForm"
        );


    const title =
        document.getElementById(
            "modalTitle"
        );


    const idInput =
        document.getElementById(
            "employeeId"
        );


    const statusInput =
        document.getElementById(
            "employeeStatus"
        );


    if (form) {
        form.reset();
    }


    if (title) {

        title.textContent =
            "Add Employee";

    }


    if (idInput) {

        idInput.value =
            "Auto generated";

        idInput.disabled =
            true;

    }


    if (statusInput) {

        statusInput.value =
            "Active";

    }


    hidePhotoPreview();


    const photoInput =
        document.getElementById(
            "employeePhoto"
        );


    if (photoInput) {
        photoInput.value = "";
    }


    const modal =
        document.getElementById(
            "employeeModal"
        );


    if (modal) {

        modal.style.display =
            "flex";

    }

}


// ============================================================
// OPEN EDIT MODAL
// ============================================================

function openEditModal(id) {

    const employee =
        employees.find(
            e =>
                Number(e.id) ===
                Number(id)
        );


    if (!employee) {

        showToast(
            "Employee not found.",
            "error"
        );

        return;

    }


    editingEmployeeId =
        Number(employee.id);


    const modal =
        document.getElementById(
            "employeeModal"
        );


    const title =
        document.getElementById(
            "modalTitle"
        );


    const idInput =
        document.getElementById(
            "employeeId"
        );


    const nameInput =
        document.getElementById(
            "employeeName"
        );


    const ageInput =
        document.getElementById(
            "employeeAge"
        );


    const statusInput =
        document.getElementById(
            "employeeStatus"
        );


    const photoInput =
        document.getElementById(
            "employeePhoto"
        );


    if (title) {

        title.textContent =
            "Edit Employee";

    }


    if (idInput) {

        idInput.value =
            employee.id;

        idInput.disabled =
            true;

    }


    if (nameInput) {

        nameInput.value =
            employee.name;

    }


    if (ageInput) {

        ageInput.value =
            employee.age;

    }


    if (statusInput) {

        statusInput.value =
            employee.status;

    }


    if (photoInput) {

        photoInput.value =
            "";

    }


    if (employee.photo) {

        showPhotoPreview(
            employee.photo
        );

    }
    else {

        hidePhotoPreview();

    }


    if (modal) {

        modal.style.display =
            "flex";

    }

}


// ============================================================
// CLOSE MODAL
// ============================================================

function closeEmployeeModal() {

    const modal =
        document.getElementById(
            "employeeModal"
        );


    if (modal) {

        modal.style.display =
            "none";

    }


    editingEmployeeId =
        null;

}


// Alias for compatibility

function closeModal() {
    closeEmployeeModal();
}


// ============================================================
// PHOTO PREVIEW
// ============================================================

function previewEmployeePhoto(event) {

    const file =
        event.target.files &&
        event.target.files[0];


    if (!file) {
        return;
    }


    if (
        !file.type.startsWith(
            "image/"
        )
    ) {

        showToast(
            "Please select an image file.",
            "error"
        );

        event.target.value = "";

        return;

    }


    if (
        file.size >
        5 * 1024 * 1024
    ) {

        showToast(
            "Photo must be smaller than 5 MB.",
            "error"
        );

        event.target.value = "";

        return;

    }


    const reader =
        new FileReader();


    reader.onload =
        function (event) {

            showPhotoPreview(
                event.target.result
            );

        };


    reader.readAsDataURL(file);

}


// ============================================================
// SHOW PHOTO PREVIEW
// ============================================================

function showPhotoPreview(photo) {

    const container =
        document.getElementById(
            "photoPreviewContainer"
        );


    const preview =
        document.getElementById(
            "photoPreview"
        );


    const safePhoto =
        normalizePhoto(photo);


    if (
        !container ||
        !preview
    ) {

        return;

    }


    if (!safePhoto) {

        hidePhotoPreview();

        return;

    }


    preview.src =
        safePhoto;


    container.style.display =
        "flex";

}


// ============================================================
// HIDE PHOTO PREVIEW
// ============================================================

function hidePhotoPreview() {

    const container =
        document.getElementById(
            "photoPreviewContainer"
        );


    const preview =
        document.getElementById(
            "photoPreview"
        );


    if (preview) {

        preview.src = "";

    }


    if (container) {

        container.style.display =
            "none";

    }

}


// ============================================================
// GET PHOTO AS BASE64
// ============================================================

function getPhotoAsBase64(file) {

    return new Promise(
        (resolve, reject) => {

            if (!file) {

                resolve("");

                return;

            }


            if (
                !file.type.startsWith(
                    "image/"
                )
            ) {

                reject(
                    new Error(
                        "Selected file is not an image."
                    )
                );

                return;

            }


            const reader =
                new FileReader();


            reader.onload =
                function () {

                    const image =
                        new Image();


                    image.onload =
                        function () {

                            const maxSize =
                                500;

                            let width =
                                image.width;

                            let height =
                                image.height;


                            if (
                                width >
                                maxSize ||
                                height >
                                maxSize
                            ) {

                                if (
                                    width >
                                    height
                                ) {

                                    height =
                                        Math.round(
                                            height *
                                            maxSize /
                                            width
                                        );

                                    width =
                                        maxSize;

                                }
                                else {

                                    width =
                                        Math.round(
                                            width *
                                            maxSize /
                                            height
                                        );

                                    height =
                                        maxSize;

                                }

                            }


                            const canvas =
                                document.createElement(
                                    "canvas"
                                );


                            canvas.width =
                                width;

                            canvas.height =
                                height;


                            const ctx =
                                canvas.getContext(
                                    "2d"
                                );


                            ctx.drawImage(
                                image,
                                0,
                                0,
                                width,
                                height
                            );


                            const compressed =
                                canvas.toDataURL(
                                    "image/jpeg",
                                    0.80
                                );


                            resolve(
                                compressed
                            );

                        };


                    image.onerror =
                        function () {

                            reject(
                                new Error(
                                    "Unable to process image."
                                )
                            );

                        };


                    image.src =
                        reader.result;

                };


            reader.onerror =
                function () {

                    reject(
                        new Error(
                            "Unable to read image."
                        )
                    );

                };


            reader.readAsDataURL(file);

        }
    );

}


// ============================================================
// SAVE / UPDATE EMPLOYEE
// ============================================================

async function saveEmployee() {

    try {

        const idInput =
            document.getElementById(
                "employeeId"
            );


        const nameInput =
            document.getElementById(
                "employeeName"
            );


        const ageInput =
            document.getElementById(
                "employeeAge"
            );


        const statusInput =
            document.getElementById(
                "employeeStatus"
            );


        const photoInput =
            document.getElementById(
                "employeePhoto"
            );


        const name =
            nameInput?.value
                .trim() || "";


        const age =
            Number(
                ageInput?.value
            );


        const status =
            statusInput?.value ===
            "Inactive"
                ? "Inactive"
                : "Active";


        // ====================================================
        // VALIDATION
        // ====================================================

        if (!name) {

            showToast(
                "Employee name is required.",
                "error"
            );

            return;

        }


        if (name.length > 100) {

            showToast(
                "Name cannot exceed 100 characters.",
                "error"
            );

            return;

        }


        if (
            !Number.isInteger(age) ||
            age < 1 ||
            age > 100
        ) {

            showToast(
                "Age must be between 1 and 100.",
                "error"
            );

            return;

        }


        // ====================================================
        // PHOTO
        // ====================================================

        let photo = "";


        const selectedFile =
            photoInput?.files &&
            photoInput.files.length > 0
                ? photoInput.files[0]
                : null;


        console.log(
            "Selected photo file:",
            selectedFile
        );


        if (selectedFile) {

            console.log(
                "Reading photo..."
            );


            photo =
                await getPhotoAsBase64(
                    selectedFile
                );


            console.log(
                "PHOTO LENGTH BEFORE SEND:",
                photo.length
            );


            console.log(
                "PHOTO START:",
                photo.substring(
                    0,
                    80
                )
            );

        }
        else if (
            editingEmployeeId !== null
        ) {

            // Keep existing photo

            const existingEmployee =
                employees.find(
                    employee =>
                        Number(employee.id) ===
                        Number(
                            editingEmployeeId
                        )
                );


            photo =
                existingEmployee?.photo ||
                "";


            console.log(
                "Keeping existing photo. Length:",
                photo.length
            );

        }


        // ====================================================
        // FINAL DEBUG
        // ====================================================

        console.log(
            "================================"
        );

        console.log(
            "SENDING EMPLOYEE"
        );

        console.log(
            "Name:",
            name
        );

        console.log(
            "Age:",
            age
        );

        console.log(
            "Status:",
            status
        );

        console.log(
            "Photo length:",
            photo.length
        );

        console.log(
            "Photo start:",
            photo
                ? photo.substring(0, 80)
                : "[EMPTY]"
        );

        console.log(
            "================================"
        );


        // ====================================================
        // REQUEST
        // ====================================================

        let response;


        const employeeData = {

            name:
                name,

            age:
                age,

            status:
                status,

            photo:
                photo

        };


        if (
            editingEmployeeId !== null
        ) {

            response =
                await fetch(
                    `/api/employees/${editingEmployeeId}`,
                    {

                        method:
                            "PUT",

                        headers: {

                            "Content-Type":
                                "application/json"

                        },

                        body:
                            JSON.stringify(
                                employeeData
                            )

                    }
                );

        }
        else {

            response =
                await fetch(
                    "/api/employees",
                    {

                        method:
                            "POST",

                        headers: {

                            "Content-Type":
                                "application/json"

                        },

                        body:
                            JSON.stringify(
                                employeeData
                            )

                    }
                );

        }


        const result =
            await response
                .json()
                .catch(
                    () => ({})
                );


        if (!response.ok) {

            throw new Error(
                result.message ||
                "Unable to save employee."
            );

        }


        showToast(

            editingEmployeeId !== null
                ? "Employee updated successfully."
                : "Employee added successfully.",

            "success"

        );


        closeEmployeeModal();


        await loadEmployees();


    }
    catch (error) {

        console.error(
            "Save employee error:",
            error
        );


        showToast(
            error.message ||
            "Unable to save employee.",
            "error"
        );

    }

}


// ============================================================
// DELETE
// ============================================================

function openDeleteModal(id) {

    employeeToDelete =
        Number(id);


    const employee =
        employees.find(
            e =>
                Number(e.id) ===
                Number(id)
        );


    const confirmed =
        confirm(
            `Delete employee "${employee?.name || id}"?`
        );


    if (!confirmed) {

        employeeToDelete =
            null;

        return;

    }


    deleteEmployee();

}


async function deleteEmployee() {

    if (
        employeeToDelete === null
    ) {

        return;

    }


    try {

        const response =
            await fetch(
                `/api/employees/${employeeToDelete}`,
                {
                    method:
                        "DELETE"
                }
            );


        const result =
            await response
                .json()
                .catch(
                    () => ({})
                );


        if (!response.ok) {

            throw new Error(
                result.message ||
                "Unable to delete employee."
            );

        }


        showToast(
            "Employee deleted successfully.",
            "success"
        );


        employeeToDelete =
            null;


        await loadEmployees();

    }
    catch (error) {

        console.error(error);


        showToast(
            error.message ||
            "Unable to delete employee.",
            "error"
        );

    }

}


// ============================================================
// EXPORT CSV
// ============================================================

async function exportCSV() {

    try {

        const response =
            await fetch(
                "/api/employees/export"
            );


        if (!response.ok) {

            throw new Error(
                "Unable to export employees."
            );

        }


        const blob =
            await response.blob();


        const url =
            window.URL.createObjectURL(
                blob
            );


        const link =
            document.createElement(
                "a"
            );


        link.href =
            url;


        link.download =
            "employees.xlsx";


        document.body.appendChild(
            link
        );


        link.click();


        link.remove();


        window.URL.revokeObjectURL(
            url
        );


        showToast(
            "Employees exported successfully.",
            "success"
        );

    }
    catch (error) {

        console.error(
            "Export error:",
            error
        );


        showToast(
            error.message ||
            "Unable to export employees.",
            "error"
        );

    }

}
// ============================================================
// IMPORT CSV
// ============================================================

async function importCSV(event) {

    const file =
        event.target.files &&
        event.target.files[0];


    if (!file) {
        return;
    }


    try {

        const text =
            await file.text();


        const rows =
            parseCSV(text);


        if (rows.length < 2) {

            throw new Error(
                "CSV file is empty."
            );

        }


        const headers =
            rows[0].map(
                header =>
                    header
                        .trim()
                        .toLowerCase()
            );


        const idIndex =
            headers.indexOf("id");


        const nameIndex =
            headers.indexOf("name");


        const ageIndex =
            headers.indexOf("age");


        const statusIndex =
            headers.indexOf("status");


        const photoIndex =
            headers.indexOf("photo");


        if (
            idIndex === -1 ||
            nameIndex === -1 ||
            ageIndex === -1
        ) {

            throw new Error(
                "CSV must contain ID, Name and Age columns."
            );

        }


        let imported = 0;


        for (
            let i = 1;
            i < rows.length;
            i++
        ) {

            const row =
                rows[i];


            if (!row.length) {
                continue;
            }


            const employee = {

                id:
                    Number(
                        row[idIndex]
                    ),

                name:
                    row[nameIndex] ||
                    "",

                age:
                    Number(
                        row[ageIndex]
                    ),

                status:
                    statusIndex !== -1 &&
                    row[statusIndex] ===
                    "Inactive"
                        ? "Inactive"
                        : "Active",

                photo:
                    photoIndex !== -1
                        ? normalizePhoto(
                            row[photoIndex]
                        )
                        : ""

            };


            const response =
                await fetch(
                    "/api/employees/import",
                    {

                        method:
                            "POST",

                        headers: {

                            "Content-Type":
                                "application/json"

                        },

                        body:
                            JSON.stringify(
                                employee
                            )

                    }
                );


            if (response.ok) {

                imported++;

            }

        }


        showToast(
            `${imported} employee(s) imported successfully.`,
            "success"
        );


        await loadEmployees();

    }
    catch (error) {

        console.error(error);


        showToast(
            error.message ||
            "Unable to import CSV.",
            "error"
        );

    }
    finally {

        event.target.value =
            "";

    }

}


// ============================================================
// CSV PARSER
// ============================================================

function parseCSV(text) {

    const rows = [];

    let row = [];

    let value = "";

    let insideQuotes = false;


    for (
        let i = 0;
        i < text.length;
        i++
    ) {

        const char =
            text[i];


        const next =
            text[i + 1];


        if (
            char === '"' &&
            insideQuotes &&
            next === '"'
        ) {

            value += '"';

            i++;

        }
        else if (
            char === '"'
        ) {

            insideQuotes =
                !insideQuotes;

        }
        else if (
            char === "," &&
            !insideQuotes
        ) {

            row.push(value);

            value = "";

        }
        else if (
            (
                char === "\n" ||
                char === "\r"
            ) &&
            !insideQuotes
        ) {

            if (
                char === "\r" &&
                next === "\n"
            ) {

                i++;

            }


            row.push(value);

            rows.push(row);

            row = [];

            value = "";

        }
        else {

            value += char;

        }

    }


    if (
        value.length > 0 ||
        row.length > 0
    ) {

        row.push(value);

        rows.push(row);

    }


    return rows;

}


// ============================================================
// PAGINATION
// ============================================================

function updatePagination() {

    const totalPages =
        Math.max(
            1,
            Math.ceil(
                filteredEmployees.length /
                employeesPerPage
            )
        );


    const pagination =
        document.getElementById(
            "pagination"
        );


    const paginationInfo =
        document.getElementById(
            "paginationInfo"
        );


    if (paginationInfo) {

        if (
            filteredEmployees.length === 0
        ) {

            paginationInfo.textContent =
                "Showing 0 employees";

        }
        else {

            const start =
                (currentPage - 1) *
                employeesPerPage +
                1;


            const end =
                Math.min(
                    currentPage *
                    employeesPerPage,
                    filteredEmployees.length
                );


            paginationInfo.textContent =
                `Showing ${start}-${end} of ${filteredEmployees.length} employees`;

        }

    }


    if (!pagination) {
        return;
    }


    pagination.innerHTML =
        "";


    if (totalPages <= 1) {
        return;
    }


    for (
        let page = 1;
        page <= totalPages;
        page++
    ) {

        const button =
            document.createElement(
                "button"
            );


        button.type =
            "button";


        button.className =
            "page-button" +
            (
                page === currentPage
                    ? " active"
                    : ""
            );


        button.textContent =
            page;


        button.addEventListener(
            "click",
            function () {

                currentPage =
                    page;

                displayEmployees();

                updatePagination();

            }
        );


        pagination.appendChild(
            button
        );

    }

}


// ============================================================
// DASHBOARD
// ============================================================

function updateDashboard() {

    const total =
        employees.length;


    const active =
        employees.filter(
            employee =>
                employee.status ===
                "Active"
        ).length;


    const inactive =
        employees.filter(
            employee =>
                employee.status ===
                "Inactive"
        ).length;


    const ages =
        employees
            .map(
                employee =>
                    Number(
                        employee.age
                    )
            )
            .filter(
                age =>
                    Number.isFinite(age) &&
                    age > 0
            );


    const average =
        ages.length > 0
            ? (
                ages.reduce(
                    (sum, age) =>
                        sum + age,
                    0
                ) /
                ages.length
            ).toFixed(1)
            : 0;


    setText(
        "totalEmployees",
        total
    );


    setText(
        "heroEmployeeCount",
        total
    );


    setText(
        "activeEmployees",
        active
    );


    setText(
        "inactiveEmployees",
        inactive
    );


    setText(
        "averageAge",
        average
    );

}


// ============================================================
// EMPLOYEE COUNT
// ============================================================

function updateEmployeeCount() {

    setText(
        "employeeCount",
        `${filteredEmployees.length} employee${filteredEmployees.length === 1 ? "" : "s"}`
    );

}


// ============================================================
// HELPER
// ============================================================

function setText(id, value) {

    const element =
        document.getElementById(
            id
        );


    if (element) {

        element.textContent =
            value;

    }

}


// ============================================================
// INITIALS
// ============================================================

function getInitials(name) {

    if (!name) {
        return "?";
    }


    const parts =
        name
            .trim()
            .split(/\s+/)
            .filter(Boolean);


    if (
        parts.length === 1
    ) {

        return parts[0]
            .substring(0, 2)
            .toUpperCase();

    }


    return (
        parts[0][0] +
        parts[parts.length - 1][0]
    ).toUpperCase();

}


// ============================================================
// HTML ESCAPE
// ============================================================

function escapeHtml(value) {

    return String(value || "")
        .replace(
            /&/g,
            "&amp;"
        )
        .replace(
            /</g,
            "&lt;"
        )
        .replace(
            />/g,
            "&gt;"
        )
        .replace(
            /"/g,
            "&quot;"
        )
        .replace(
            /'/g,
            "&#039;"
        );

}


// ============================================================
// TOAST
// ============================================================

function showToast(
    message,
    type = "success"
) {

    const existing =
        document.querySelector(
            ".peopleflow-toast"
        );


    if (existing) {

        existing.remove();

    }


    const toast =
        document.createElement(
            "div"
        );


    toast.className =
        `peopleflow-toast ${type}`;


    toast.textContent =
        message;


    document.body.appendChild(
        toast
    );


    requestAnimationFrame(
        () => {

            toast.classList.add(
                "show"
            );

        }
    );


    setTimeout(
        () => {

            toast.classList.remove(
                "show"
            );


            setTimeout(
                () => {

                    toast.remove();

                },
                300
            );

        },
        3000
    );

}


// ============================================================
// GLOBAL FUNCTIONS
// ============================================================

window.showDashboard =
    showDashboard;

window.showEmployees =
    showEmployees;

window.openAddModal =
    openAddModal;

window.openEditModal =
    openEditModal;

window.openDeleteModal =
    openDeleteModal;

window.closeEmployeeModal =
    closeEmployeeModal;

window.closeModal =
    closeModal;

window.exportCSV =
    exportCSV;

window.importCSV =
    importCSV;

window.saveEmployee =
    saveEmployee;