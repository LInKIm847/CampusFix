# CampusFix repair notes

This copy contains targeted fixes for request ownership, role-based status updates, API data exposure, EF Core package alignment, database migration startup, and the maintenance dashboard.

## Before running

1. Open `CampusFix.slnx` in Visual Studio.
2. Confirm the .NET 10 SDK is installed.
3. Confirm SQL Server is running and the `DefaultConnection` in `appsettings.json` matches your local SQL Server instance.
4. Restore NuGet packages and build the solution.
5. Run the app. Startup applies pending EF Core migrations and seeds request categories and roles.

## Test accounts

Public registration creates Student accounts only. To create a Maintenance account for a local demonstration, set these user secrets in the project directory (do not commit the password):

```powershell
dotnet user-secrets set "CampusFix:MaintenanceEmail" "maintenance@example.com"
dotnet user-secrets set "CampusFix:MaintenancePassword" "Use-A-Strong-Demo-Password-123!"
```

Restart the app. It creates the account if needed and assigns the Maintenance role. Replace the example password with your own demo password.

## Demonstration routes

- Register / log in: `/Identity/Account/Register` and `/Identity/Account/Login`
- Student request list: `/MaintenanceRequests`
- Staff-only dashboard: `/MaintenanceRequests/MaintenanceDashboard`
- JSON list: `/api/maintenance-requests`
- JSON item: `/api/maintenance-requests/1` (replace `1` with an existing request ID)

The API requires authentication. Students receive only their own requests; Maintenance and Administrator users receive all requests. The maintenance dashboard is role-restricted and can be used to demonstrate access denied when signed in as a Student.

## Docker warning

The supplied Dockerfile uses Windows containers. The current connection string uses Windows Integrated Security (`Trusted_Connection=True`) and a host-specific SQL Server name (`LINDSAY`). That may work for local Visual Studio hosting but will not automatically work from inside a container. Container hosting needs a SQL Server connection reachable from the container and a supported authentication method. Do not put database credentials in source control.

## Verification status

The code was inspected and patched, but this environment does not have the .NET SDK installed, so a `dotnet build` or live database test could not be run here. Build and run it in Visual Studio, then address any environment-specific errors shown in the Error List or Output window.
