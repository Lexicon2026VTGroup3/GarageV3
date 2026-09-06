# 🚗 GarageV3
ASP.NET Core MVC • EF Core • Identity • Roles • Parking Sessions • Statistics

GarageV3 is a full parking management system built with ASP.NET Core MVC and ASP.NET Core Identity. It provides vehicle registration, parking sessions, receipts, administration tools, user management, and extended premium features.

## 🧩 Domain Model
GarageV3 separates vehicle registration from parking events:
* Vehicle – Registered vehicle owned by an Identity user. Always kept in the system.
* ParkingSession – A parking event with check‑in, check‑out, and total price. Stored as history.
* ParkingSpot – Physical parking spot reused over time, one active session at a time (base requirement).
* Check‑out sets CheckOutTime + TotalPrice. No deletion of Vehicle or ParkingSession.

## ✅ Core Features
### 🔐 Identity & Authorization
* User registration and login
* Role system: Admin + User
* Admin user management
* Protected views, actions, and resources

### 🗄️ EF Core & Data
* Relational model, seed data, price configuration
* Manage vehicle types & parking spots
* Vehicle ownership via Identity

🚘 Vehicle Management
* Register vehicles
* List, view details, edit
* Unique license plate validation
* Vehicle type selection via dropdown
* Validation: required fields, max lengths, wheel count
* Search by license plate with clear feedback

### 🅿️ Parking Flow
* Park a registered vehicle
* System‑generated check‑in time
* Check out + receipt (owner, vehicle, times, duration, price)
* Keep full parking history

### 🔍 Search & Overview
* Search active parkings
* Search members
* Garage statistics

## ⭐ Extra Features (Implemented)
### 🅿️ Shared & Multi‑Spot Parking
* Support for vehicles occupying multiple spots
* Support for shared spots (e.g., multiple motorcycles)

### 💳 Pro Membership & Discounts
* Pro membership tier
* Automatic discount calculation
* Discounted receipts and revenue tracking

### 🔁 Password Reset
* Identity password reset flow
* Email‑based recovery
* Secure token validation

### 🔐 Admin Two‑Factor Authentication
* Optional 2FA for administrators
* Enhanced security for sensitive operations

### 📈 Revenue & Customer Insights
* Total revenue statistics
* Most profitable customers
* Historical income charts

## 🧱 Tech Stack
* ASP.NET Core MVC
* EF Core Code First
* ASP.NET Core Identity
* LINQ + ViewModels
* GitHub Project (GitHub Kanban board: 17 parent issues, 123 sub‑issues)

## ▶️ Run the Project
cd GarageV3
dotnet run
