console.log("App.js loaded successfully");
// --- INITIAL USER LOADING ---
document.addEventListener("DOMContentLoaded", () => {
    // Define it once at the start of the block
    const userContainer = document.getElementById("user-container");

    if (userContainer) {
        userContainer.innerHTML = "Loading users...";

        fetch("http://localhost:5232/api/users")
            .then(res => res.json())
            .then(data => {
                userContainer.innerHTML = ""; 
                // data.forEach logic here...
            })
            .catch(err => {
                console.error("Error loading users:", err);
                // Now userContainer is definitely defined here
                userContainer.innerHTML = "Failed to load users.";
            });
    }
});
function testBooking() {
    const bookingData = {
        UserID: 3,         
        ScheduleID: 1,     
        SeatNumber: "A12",
        Price: 500
    };

    fetch("http://localhost:5232/api/bookings", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(bookingData)
    })
    .then(response => response.json().then(data => ({ status: response.status, body: data })))
    .then(res => {
        if (res.status === 200) {
            // If successful, show the message
            alert("SUCCESS: " + res.body.message);
        } else {
            // If it failed (400 error), show the error detail
            alert("SERVER ERROR: " + (res.body.error || res.body.message || "Unknown Error"));
        }
    })
    .catch(error => {
        alert("CONNECTION ERROR: Could not reach the backend.");
        console.error(error);
    });
}
async function loadSchedules() {
    const tableBody = document.getElementById("schedule-table-body");
    if (!tableBody) return;

    try {
        const response = await fetch("http://localhost:5232/api/schedules/available-schedules");
        const data = await response.json();

        tableBody.innerHTML = "";
        data.forEach(s => {
            const row = document.createElement("tr");
            row.innerHTML = `
                <td>${s.from}</td>
                <td>${s.to}</td>
                <td>${s.distance}</td>
                <td>${s.departureTime}</td>
                <td>${s.arrivalTime}</td>
                <td>${s.plate}</td>
                <td>${s.availableSeats}</td>
                <td>KES ${s.price}</td>
                <td><button onclick="bookSeat(${s.scheduleID})">Book</button></td>
            `;
            tableBody.appendChild(row);
        });
    } catch (e) { console.error("Load error:", e); }
}
function bookNow(schedId, price) {

    const userId = localStorage.getItem("currentUserID");

    if (!userId) {
        alert("Please login first.");
        window.location.href = "login.html";
        return;
    }

    const bookingData = {
        UserID: parseInt(userId),
        ScheduleID: parseInt(schedId),
        SeatNumber: "S-" + Math.floor(Math.random() * 60),
        BookingStatus: "Confirmed",
        Price: parseFloat(price)
    };

    console.log("Sending:", bookingData);

    fetch("http://localhost:5232/api/bookings", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(bookingData)
    })
    .then(async res => {
        const data = await res.json();

        if (!res.ok) {
            console.error(data);
            alert("Booking failed: " + (data.error || "Unknown error"));
            return;
        }

        alert("Booking successful!");
        window.location.reload();
    })
    .catch(err => {
        console.error("Fetch error:", err);
    });
}
function newFunction() {
    console.error("Error:", err);
    tableBody.innerHTML = "<tr><td colspan='6'>Error loading bookings.</td></tr>";
}

// 1. Fetch EVERY booking in the system
function loadAllBookings() {
    const table = document.getElementById("all-bookings-table");
    // We'll create a new endpoint in the backend for this shortly
    fetch("http://localhost:5232/api/bookings/admin/all")
        .then(res => res.json())
        .then(data => {
            table.innerHTML = "";
            data.forEach(b => {
                table.innerHTML += `
                    <tr>
                        <td>${b.bookingID}</td>
                        <td>${b.userName}</td>
                        <td>${b.route}</td>
                        <td>${b.plate}</td>
                        <td>${b.time}</td>
                    </tr>`;
            });
        });
}

// 2. Register a new bus to the database
function addNewBus() {
    const busData = {
        PlateNumber: document.getElementById("newPlate").value,
        Capacity: parseInt(document.getElementById("newCapacity").value),
        Status: "Active"
    };

    fetch("http://localhost:5232/api/buses", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(busData)
    })
    .then(res => res.ok ? alert("Bus Registered!") : alert("Failed"));
}
async function loginUser(event) {
    event.preventDefault();
    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;

    try {
        const response = await fetch(`http://localhost:5232/api/users/login?email=${email}&password=${password}`);
        if (response.ok) {
            const userData = await response.json();
            
            // Force the key name to be exactly "user"
            localStorage.setItem("user", JSON.stringify(userData));
            
            console.log("Session Saved Successfully:", userData);
            window.location.href = "bookings.html"; 
        } else {
            alert("Invalid Login");
        }
    } catch (err) { console.error(err); }
}
function logoutUser() {
    // 1. Clear the specific 'user' data or everything
    localStorage.removeItem("user");
    
    // 2. Optional: Clear all storage to be safe
    localStorage.clear();

    alert("You have been logged out.");
    
    // 3. Redirect to login page
    window.location.href = "login.html";
}
function downloadTxtTicket(userName, scheduleID) {
    const content = `TICKET: ${userName}\nSchedule: ${scheduleID}\nDate: ${new Date()}`;
    const blob = new Blob([content], { type: "text/plain" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = "BusTicket.txt";
    link.click();
}
function handleLogin() {
    const email = document.getElementById("loginEmail").value;
    const pass = document.getElementById("loginPass").value;

    if (!email || !pass) {
        alert("Please enter both email and password.");
        return;
    }

    // Call the C# API we created earlier. 
    // encodeURIComponent ensures special characters in emails don't break the URL.
    fetch(`http://localhost:5232/api/users/login?email=${encodeURIComponent(email)}&password=${encodeURIComponent(pass)}`)
        .then(async res => {
            const data = await res.json();
            
            if (res.ok) {
                // 1. Save data to browser storage so other pages can use it
                localStorage.setItem("currentUserID", data.userID);
                localStorage.setItem("userRole", data.role);
                localStorage.setItem("userName", data.fullName);

                // 2. The Traffic Cop: Redirect based on role
                if (data.role === "Admin") {
                    window.location.href = "admin.html";
                } else {
                    window.location.href = "index.html"; // Student homepage
                }
            } else {
                // Backend returned an error (e.g., wrong password)
                alert("Login Failed: " + (data.error || "Invalid credentials"));
            }
        })
        .catch(err => {
            console.error(err);
            alert("Cannot connect to the server. Is Visual Studio running?");
        });
}
function handleRegister() {
    const name = document.getElementById("regName").value;
    const email = document.getElementById("regEmail").value;
    const pass = document.getElementById("regPass").value;

    if (!name || !email || !pass) {
        alert("Please fill in all fields");
        return;
    }

    const data = {
        FullName: name,
        Email: email,
        Password: pass
    };

    fetch("http://localhost:5232/api/users/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
    })
    .then(async res => {
    const data = await res.json();
    if (res.ok) {
        alert("Registration successful!");
        window.location.href = "login.html";
    } else {
        // This will now show the REAL reason from the C# Exception
        alert("Registration Failed: " + data.error);
    }
})
    .catch(err => alert("Error: Could not connect to the server."));
}
// 1. Load all registered users for the Admin
function loadSystemUsers() {
    const tableBody = document.getElementById("all-users-table");
    fetch("http://localhost:5232/api/users")
        .then(res => res.json())
        .then(data => {
            tableBody.innerHTML = "";
            data.forEach(user => {
                tableBody.innerHTML += `
                    <tr>
                        <td>${user.userID}</td>
                        <td>${user.fullName}</td>
                        <td>${user.role}</td>
                    </tr>`;
            });
        })
        .catch(err => console.error("Error loading users:", err));
}

// 2. Load the bus fleet for the Admin
function loadBusFleet() {
    const busList = document.getElementById("bus-list");
    fetch("http://localhost:5232/api/buses") // Make sure you have this endpoint in BusesController
        .then(res => res.json())
        .then(data => {
            busList.innerHTML = "<ul>" + data.map(bus => 
                `<li><strong>${bus.plateNumber}</strong> - Capacity: ${bus.capacity} [${bus.status}]</li>`
            ).join('') + "</ul>";
        });
}
function loadUsers() {
    const userContainer = document.getElementById("user-container");

    if (!userContainer) {
        console.error("user-container not found in HTML.");
        return;
    }

    userContainer.innerHTML = "Loading users...";

    fetch("http://localhost:5232/api/users")
        .then(response => response.json())
        .then(data => {
            userContainer.innerHTML = "";

            data.forEach(user => {
                userContainer.innerHTML += `
                    <p><strong>${user.fullName}</strong> (${user.role})</p>
                `;
            });
        })
        .catch(error => {
            console.error("User Load Error:", error);
            userContainer.innerHTML = "Error loading users.";
        });
}
async function bookSeat(scheduleID) {
    // 1. Try to fetch the session
    const session = localStorage.getItem("user");
    
    // 2. If it's null, the browser forgot the data
    if (!session) {
        console.error("DEBUG: LocalStorage is empty!");
        alert("Session not found. Please log in again.");
        window.location.href = "login.html";
        return;
    }

    const user = JSON.parse(session);
    // Support both C# naming (UserID) and JS naming (userID)
    const uid = user.userID || user.UserID;

    try {
        const response = await fetch("http://localhost:5232/api/schedules/book", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ 
                userID: parseInt(uid), 
                scheduleID: parseInt(scheduleID) 
            })
        });

        if (response.ok) {
            alert("Booking Confirmed!");
            downloadTicket(user.fullName || user.FullName, scheduleID);
        }
    } catch (e) { alert("Error connecting to server"); }
}
// NEW: Function to generate and "Download" the ticket
function generateTicket(userName, scheduleID) {
    // Create a hidden print window
    const ticketWindow = window.open("", "_blank");
    
    // Design the ticket layout
    ticketWindow.document.write(`
        <html>
        <head>
            <title>Bus Ticket - ${userName}</title>
            <style>
                body { font-family: sans-serif; text-align: center; padding: 20px; }
                .ticket-box { border: 2px dashed #333; padding: 20px; display: inline-block; }
                .header { color: #004d99; font-size: 24px; font-weight: bold; }
                .details { margin-top: 15px; text-align: left; }
                .footer { margin-top: 20px; font-size: 12px; color: #666; }
            </style>
        </head>
        <body>
            <div class="ticket-box">
                <div class="header">CAMPUS TRANSPORT SYSTEM</div>
                <hr>
                <div class="details">
                    <p><strong>Passenger:</strong> ${userName}</p>
                    <p><strong>Ticket ID:</strong> TKT-${Math.floor(Math.random() * 10000)}</p>
                    <p><strong>Schedule ID:</strong> ${scheduleID}</p>
                    <p><strong>Date:</strong> ${new Date().toLocaleDateString()}</p>
                </div>
                <div class="footer">Please present this ticket to the bus driver.</div>
            </div>
        </body>
        </html>
    `);

    ticketWindow.document.close();
    
    // Trigger the print/save dialog automatically
    ticketWindow.print();
}