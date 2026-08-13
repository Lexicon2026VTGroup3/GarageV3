# Behörighetsmatris — GarageV3

Dokumentation för US10 (TASK-10.1): vilka roller som har åtkomst till vilka
controllers/actions i systemet.

| Controller | Skydd | Anonym | Member | Admin | Kommentar |
|---|---|---|---|---|---|
| `HomeController` | Inget | ✅ | ✅ | ✅ | Endast aggregerade siffror (lediga/upptagna platser), ingen fordonsdata visas |
| `MyVehiclesController` | `[Authorize]` | ❌ → login | ✅ (endast egna fordon) | ✅ (endast egna fordon) | Ägarskap filtreras i DB-frågan (`OwnerId == userId`) |
| `ParkingController` | `[Authorize]` | ❌ → login | ✅ (endast egna fordon/sessioner) | ✅ (endast egna, plus kan checka ut andras via admin-behörighet i `CheckOut`) | `Park`/`History` filtrerar på ägare; `CheckOut` tillåter admin ELLER ägare |
| `ParkingApiController` | `[Authorize]` | ❌ → 401 | ✅ | ✅ | Endast prisberäkning för egen bokning, ingen skrivning till databasen |
| `AdminParkingController` | `[Authorize(Roles="Admin")]` | ❌ → login | ❌ → 403 | ✅ | Ser alla aktiva parkeringar (avsiktligt, admin-funktion) |
| `AdminParkingSpotsController` | `[Authorize(Roles="Admin")]` | ❌ → login | ❌ → 403 | ✅ | CRUD för parkeringsplatser |
| `AdminVehiclesController` | `[Authorize(Roles="Admin")]` | ❌ → login | ❌ → 403 | ✅ | Ser/redigerar alla fordon (avsiktligt) |
| `AdminVehicleTypesController` | `[Authorize(Roles="Admin")]` | ❌ → login | ❌ → 403 | ✅ | CRUD för fordonstyper |
| `StatisticsController` | `[Authorize(Roles="Admin")]` | ❌ → login | ❌ → 403 | ✅ | Garagestatistik |
| `UserManagementController` | `[Authorize(Roles="Admin")]` | ❌ → login | ❌ → 403 | ✅ | Medlemsöversikt, roll­hantering |

## Ägarskapskontroll (TASK-10.4/10.5)

Följande actions hämtar en resurs via ett ID som skickas från klienten
(URL eller formulär), och måste därför verifiera ägarskap **efter** hämtning
— annars kan ett medlem manipulera ett ID i URL:en för att komma åt någon
annans data:

| Action | Kontroll |
|---|---|
| `MyVehiclesController.Details/Edit/Delete(id)` | `WHERE OwnerId == userId` i frågan |
| `ParkingController.CheckOut(id)` (GET) | Hämtar session, kontrollerar `isAdmin \|\| isOwner` efter hämtning, annars `Forbid()` |
| `ParkingController.Park(vehicleId)` (POST) | `WHERE OwnerId == userId` i frågan innan sessionen skapas |

## Testkonton (TASK-10.7)

| Roll | Konto |
|---|---|
| Admin | `admin@garage.com` / `Abc123!` |
| Member | (registrera ett nytt testkonto via `/Identity/Account/Register`) |
| Anonym | Ingen inloggning |

## Manuellt testprotokoll (TASK-10.7)

- [ ] Anonym användare omdirigeras till login vid försök att nå `/MyVehicles`, `/Parking/Park`, `/Admin*`
- [ ] Member nekas åtkomst (403/Forbid) vid direkt URL till `/AdminVehicles`, `/UserManagement`, etc.
- [ ] Member A kan inte se Member B:s fordon genom att ändra ett ID i URL:en (`/MyVehicles/Details/{annat-id}`)
- [ ] Admin kan nå samtliga sidor
- [ ] Navigationsmenyn visar endast länkar användaren faktiskt har åtkomst till