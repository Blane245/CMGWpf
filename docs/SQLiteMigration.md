# SQLite Migration Status

## ✅ PHASE 3: Update CmgDbContext - COMPLETED
## ✅ PHASE 4: Create Data Migration Tool - COMPLETED  
## ✅ PHASE 5: Generate SQLite Database Schema - COMPLETED

---

## Summary of Completed Work (Day 1)

### Phase 3: Updated CmgDbContext

#### 1. Updated Project File (CMGWpf.csproj)
**Added Packages:**
- `Microsoft.EntityFrameworkCore.Sqlite` (9.0.0)
- `Microsoft.EntityFrameworkCore.Design` (9.0.0)
- `Microsoft.EntityFrameworkCore.Tools` (9.0.0)
- `MySql.Data` (9.7.0) - Temporary, for migration utility

**Removed:**
- `Pomelo.EntityFrameworkCore.MySql` reference

#### 2. Updated CmgDbContext.cs
**Key Changes:**
- Switched from `UseMySql()` to `UseSqlite()`
- Changed database location to user's LocalApplicationData folder
  - Path: `%LOCALAPPDATA%\CMGWpf\cmg.db`
- Removed MySQL-specific configurations:
  - Removed `ServerVersion.AutoDetect()`
  - Removed retry policies (not needed for embedded database)
  - Removed explicit `float` column types (SQLite handles automatically)
- Added directory creation logic to ensure database folder exists
- Added explicit `using System.IO;` for Path and Directory classes

**Database Location:**
```
C:\Users\[username]\AppData\Local\CMGWpf\cmg.db
```

#### 3. Removed DatabaseClient.cs
- Removed `Services\DatabaseClient.cs` (TCP client for database server)
- No longer needed with embedded SQLite database

#### 4. Fixed XAML Design-Time Errors
Fixed incorrect DesignInstance declarations in:
- `GeneratorsEqualPanel.xaml`
- `GeneratorsAlignPanel.xaml`
- `GeneratorsStaggerPanel.xaml`

Changed from: `Type=viewmodel:ToolsViewModel.Instance`
To: `Type=viewmodel:ToolsViewModel`

---

### Phase 4: Created Data Migration Tool

#### Created Files:
1. **`Utilities/DatabaseMigrationUtility.cs`** - Core migration logic
   - `MigrateAllDataAsync()` - Migrates all data from MySQL to SQLite
   - `ExportToJsonAsync()` - Exports MySQL data to JSON for backup
   - Handles all tables: Tags, Voices, Ensembles, NoteSequences
   - Handles many-to-many relationships

2. **`Dialogs/DatabaseMigrationDialog.xaml`** - UI for migration
3. **`Dialogs/DatabaseMigrationDialog.xaml.cs`** - Dialog code-behind

#### Features:
- **Backup Capability**: Export all MySQL data to JSON files before migration
- **Progress Indicators**: Visual feedback during migration
- **Error Handling**: Comprehensive error reporting
- **Relationship Preservation**: Maintains all many-to-many relationships
- **Duplicate Prevention**: Checks for existing records before inserting

---

### Phase 5: Generated SQLite Database Schema

#### 1. Installed EF Core Tools
```powershell
dotnet tool install --global dotnet-ef
# Version 10.0.8 installed
```

#### 2. Created Initial Migration
```powershell
dotnet ef migrations add InitialSqliteDatabase --context CmgDbContext
```

**Generated Files:**
- `Migrations/20260514080948_InitialSqliteDatabase.cs`
- `Migrations/20260514080948_InitialSqliteDatabase.Designer.cs`
- `Migrations/CmgDbContextModelSnapshot.cs`

#### 3. Applied Migration
```powershell
dotnet ef database update --context CmgDbContext
```

**Created Tables:**
- `ensemble` (Name, Description)
- `voice` (Name, Description, Timbre, RegisterLo, RegisterHi, Duration, SoundFontFile, PresetName)
- `notesequence` (Name, Items)
- `tag` (Name)
- `ensemble_voice` (junction table for many-to-many)
- `notesequence_tag` (junction table for many-to-many)
- `__EFMigrationsHistory` (EF Core tracking table)

---

## How to Use the Migration Tool

### Step 1: Open Migration Dialog
Add a menu item or button in your application to open the migration dialog:

```csharp
var migrationDialog = new DatabaseMigrationDialog();
migrationDialog.ShowDialog();
```

### Step 2: Export MySQL Data (Optional but Recommended)
1. Enter your MySQL connection string (e.g., `Server=localhost;Port=3306;Database=cmg;Uid=root;Pwd=;`)
2. Click "Export to JSON (Backup)"
3. Choose a directory to save backup files
4. Files will be saved as: `tags.json`, `voices.json`, `ensembles.json`, `notesequences.json`

### Step 3: Migrate to SQLite
1. With the same MySQL connection string
2. Click "Migrate to SQLite"
3. Watch the progress indicator
4. Review the migration results

### Result
- SQLite database created at: `%LOCALAPPDATA%\CMGWpf\cmg.db`
- All data migrated with relationships preserved

---

## Next Steps

### Phase 6: Data Migration (Day 2)
**Status**: 🔶 Ready to Execute

**When you have existing MySQL data:**
1. Open the DatabaseMigrationDialog in your application
2. Export data to JSON as backup
3. Run the migration
4. Verify data integrity

### Phase 7: Update Application Code (Day 2-3)
**Status**: ⏳ Pending

- Remove all references to CMGDatabaseServer project
- Update any service initialization code
- Test all database operations with SQLite
- Update configuration files

### Phase 8: Testing (Day 3-4)
**Status**: ⏳ Pending

- Unit tests for database operations
- Integration tests
- Manual testing of all features

### Phase 9: Cleanup (Day 4)
**Status**: ⏳ Pending

- Remove CMGDatabaseServer project from solution
- Remove `MySql.Data` package reference
- Update documentation
- Remove MySQL server dependencies

---

## Build Status
✅ **Build Successful** - All compilation errors resolved  
✅ **Database Created** - SQLite schema applied  
✅ **Migration Tool Ready** - UI and logic complete

---

## Rollback Plan
If needed, original MySQL configuration can be restored:

```powershell
git checkout HEAD~1 -- src/CMGWpf/Data/CmgDbContext.cs
git checkout HEAD~1 -- src/CMGWpf/CMGWpf.csproj
```

---

## Benefits Achieved
✅ **No Server Required**: SQLite is embedded, no installation needed  
✅ **Single File Database**: Easy to backup and move  
✅ **Same EF Core Code**: All LINQ queries work identically  
✅ **Better Performance**: No network latency  
✅ **Simpler Deployment**: No database configuration for end users  
✅ **Migration Tool**: Easy data transfer from existing MySQL databases

---

## Technical Notes

### Database Location
The SQLite database is stored in the user's local application data folder:
- **Path**: `%LOCALAPPDATA%\CMGWpf\cmg.db`
- **Full Path Example**: `C:\Users\YourName\AppData\Local\CMGWpf\cmg.db`

### Data Types
SQLite automatically handles the following conversions:
- MySQL `VARCHAR` → SQLite `TEXT`
- MySQL `FLOAT` → SQLite `REAL`
- MySQL `INT` → SQLite `INTEGER`

### Entity Framework Compatibility
All EF Core features work with SQLite:
- LINQ queries
- Include/ThenInclude for eager loading
- Many-to-many relationships
- Migrations
- Change tracking

---

## Questions or Issues?

If you encounter any problems:
1. Check the migration dialog status messages
2. Review the JSON backup files
3. Check the EF Core migration files in the Migrations folder
4. Verify the SQLite database was created at `%LOCALAPPDATA%\CMGWpf\cmg.db`



### Create Initial Migration
Run the following commands in the Package Manager Console or terminal:

```powershell
cd C:\Users\blane\source\repos\CMGWpf\src\CMGWpf

# Create initial migration
dotnet ef migrations add InitialSqliteDatabase --context CmgDbContext

# Apply migration to create database
dotnet ef database update --context CmgDbContext
```

This will:
1. Create a `Migrations` folder with the initial schema
2. Create the SQLite database at `%LOCALAPPDATA%\CMGWpf\cmg.db`
3. Create all tables: `ensemble`, `voice`, `tag`, `notesequence`, and junction tables

---

## Future Steps

### Phase 4: Create Data Migration Tool
Create a utility to:
1. Export data from existing MySQL database
2. Import data into new SQLite database
3. Validate data integrity

### Phase 7: Update Application Code
- Remove any remaining references to CMGDatabaseServer
- Test all database operations with SQLite
- Update any MySQL-specific queries

### Phase 9: Cleanup
- Remove CMGDatabaseServer project from solution
- Update documentation
- Remove MySQL server dependencies from deployment

---

## Rollback Plan
If needed, the original MySQL configuration can be restored from Git history:
```powershell
git checkout SQLLite-migration-checkpoint~1 -- src/CMGWpf/Data/CmgDbContext.cs
```

## Notes
- Entity configurations (relationships, keys, etc.) remain unchanged
- All EF Core LINQ queries will work identically
- SQLite database is a single file, making backup and portability simple
- No server installation or configuration required for end users
