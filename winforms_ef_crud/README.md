Fitness Tracker (WinForms + EF Core)

Short description
- A minimal Windows Forms demo application that shows CRUD operations for fitness activities using Entity Framework Core and BindingList-based UI synchronization.

Project structure
- Program.cs                - application entry, runs Form1
- Form1.cs                 - main form logic and UI construction, tabs for Manage / ListView / DataGridView, CRUD handlers and RefreshData() synchronization
- Form1.Designer.cs        - designer-generated form scaffold (kept minimal)
- FitnessActivity.cs       - entity model (FitnessActivityId, Title, Duration, CaloriesBurned, Intensity, Date)
- FitnessDbContext.cs      - EF Core DbContext (DbSet< FitnessActivity >) and OnConfiguring using LocalDB
- winforms_ef_crud.csproj  - project file (targets .NET Framework 4.7.2)

How it works
- On startup the app recreates the LocalDB database (EnsureDeleted + EnsureCreated) and seeds example data.
- RefreshData() loads entities into a BindingList<FitnessActivity> and sets that list as DataSource for the ListBox, ComboBox and DataGridView; the ListView is populated manually.
- Add / Update / Delete operations modify the DbContext and call RefreshData() so all UI views stay synchronized.

Requirements
- Visual Studio with .NET Framework 4.7.2
- LocalDB (comes with Visual Studio)
- NuGet packages: Microsoft.EntityFrameworkCore and Microsoft.EntityFrameworkCore.SqlServer (install if not present)

Notes
- Database is recreated on each run for demo purposes. Remove EnsureDeleted/EnsureCreated calls in Form1.OnLoad for persistent data.
- If you want async operations, the DbContext usage and UI handlers can be converted to async/await.

License
- MIT (sample/demo code)
