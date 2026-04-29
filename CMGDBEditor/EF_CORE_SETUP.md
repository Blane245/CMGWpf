# Entity Framework Core Setup for CMGDBEditor

## Overview

Your project now has Entity Framework Core configured to work with your existing MySQL database. This provides a modern, type-safe way to interact with your database instead of using raw SQL queries.

## What Was Added

1. **NuGet Packages**:
   - `Pomelo.EntityFrameworkCore.MySql` - MySQL provider for EF Core

2. **DbContext Class**: `CMGDBEditor/Data/CmgDbContext.cs`
   - Configures connection to your MySQL database
   - Defines `Ensembles` and `Voices` DbSets
   - Configures the many-to-many relationship via the `ensemble_voice` table

3. **Model Updates**:
   - Added `Voices` navigation property to `Ensemble` class
   - Added `Ensembles` navigation property to `Voice` class
   - These enable EF Core to manage the relationships automatically

4. **Example Helper Class**: `CMGDBEditor/Helpers/EfCoreExamples.cs`
   - Shows common patterns for using EF Core

## Quick Start

### Basic Usage

```csharp
using CMGDBEditor.Data;
using Microsoft.EntityFrameworkCore;

// List all ensembles
using var context = new CmgDbContext();
var ensembles = await context.Ensembles.ToListAsync();

// List ensembles with their voices
var ensemblesWithVoices = await context.Ensembles
    .Include(e => e.Voices)
    .ToListAsync();

// Add a new ensemble
var newEnsemble = new Ensemble 
{ 
    Name = "My Ensemble", 
    Description = "Test" 
};
context.Ensembles.Add(newEnsemble);
await context.SaveChangesAsync();
```

### Updating Your Existing Code

**Before (using DatabaseClient):**
```csharp
public static async Task<ObservableCollection<Ensemble>> Execute()
{
    var response = await Ensemble.List();
    ObservableCollection<Ensemble> ensembles = new ObservableCollection<Ensemble>();
    foreach (var data in response)
    {
        string ensembleName = data["name"]?.ToString() ?? string.Empty;
        string description = data["description"]?.ToString() ?? string.Empty;
        ensembles.Add(new Ensemble() { Name = ensembleName, Description = description });
    }
    return ensembles;
}
```

**After (using EF Core):**
```csharp
public static async Task<ObservableCollection<Ensemble>> Execute()
{
    using var context = new CmgDbContext();
    var ensembles = await context.Ensembles
        .Include(e => e.Voices)
        .ToListAsync();
    return new ObservableCollection<Ensemble>(ensembles);
}
```

## Common Operations

### Read Operations

```csharp
// Get all ensembles
var allEnsembles = await context.Ensembles.ToListAsync();

// Get ensemble by name
var ensemble = await context.Ensembles
    .FirstOrDefaultAsync(e => e.Name == "MyEnsemble");

// Get ensemble with voices
var ensembleWithVoices = await context.Ensembles
    .Include(e => e.Voices)
    .FirstOrDefaultAsync(e => e.Name == "MyEnsemble");

// Search ensembles
var searchResults = await context.Ensembles
    .Where(e => e.Name.Contains("test"))
    .ToListAsync();
```

### Create Operations

```csharp
// Add a new ensemble
var ensemble = new Ensemble 
{ 
    Name = "Orchestra", 
    Description = "Full orchestra" 
};
context.Ensembles.Add(ensemble);
await context.SaveChangesAsync();

// Add ensemble with voices
var ensemble = new Ensemble 
{ 
    Name = "String Quartet",
    Voices = new List<Voice>
    {
        await context.Voices.FirstOrDefaultAsync(v => v.Name == "Violin"),
        await context.Voices.FirstOrDefaultAsync(v => v.Name == "Viola")
    }
};
context.Ensembles.Add(ensemble);
await context.SaveChangesAsync();
```

### Update Operations

```csharp
// Update an ensemble
var ensemble = await context.Ensembles
    .FirstOrDefaultAsync(e => e.Name == "MyEnsemble");

if (ensemble != null)
{
    ensemble.Description = "Updated description";
    await context.SaveChangesAsync();
}

// Add voices to existing ensemble
var ensemble = await context.Ensembles
    .Include(e => e.Voices)
    .FirstOrDefaultAsync(e => e.Name == "MyEnsemble");

var voice = await context.Voices
    .FirstOrDefaultAsync(v => v.Name == "Flute");

if (ensemble != null && voice != null)
{
    ensemble.Voices.Add(voice);
    await context.SaveChangesAsync();
}
```

### Delete Operations

```csharp
// Delete an ensemble
var ensemble = await context.Ensembles
    .FirstOrDefaultAsync(e => e.Name == "MyEnsemble");

if (ensemble != null)
{
    context.Ensembles.Remove(ensemble);
    await context.SaveChangesAsync();
}

// Remove voice from ensemble
var ensemble = await context.Ensembles
    .Include(e => e.Voices)
    .FirstOrDefaultAsync(e => e.Name == "MyEnsemble");

var voiceToRemove = ensemble?.Voices
    .FirstOrDefault(v => v.Name == "Flute");

if (voiceToRemove != null)
{
    ensemble.Voices.Remove(voiceToRemove);
    await context.SaveChangesAsync();
}
```

## Benefits Over Raw SQL

1. **Type Safety**: Compile-time checking instead of runtime errors
2. **Less Code**: No manual mapping from dictionaries
3. **LINQ Queries**: Powerful, readable queries
4. **Change Tracking**: EF knows what changed and updates only that
5. **Navigation Properties**: Easy access to related data
6. **Migrations**: Version control for database schema

## Migration Path

You can migrate gradually:

1. **Keep existing code working**: Your old `DatabaseClient` code still works
2. **Start using EF Core for new features**: Use `CmgDbContext` for new code
3. **Gradually refactor**: Replace old code as you touch it

### Example: Updating ListEnsembles Helper

```csharp
// In CMGDBEditor/Helpers/ListEnsembles.cs
public static async Task<ObservableCollection<Ensemble>> Execute()
{
    // Option 1: Use EF Core directly
    using var context = new CmgDbContext();
    var ensembles = await context.Ensembles
        .Include(e => e.Voices)
        .ToListAsync();
    return new ObservableCollection<Ensemble>(ensembles);

    // Option 2: Use the example helper
    // return await EfCoreExamples.ListEnsemblesWithEfCore();
}
```

## Connection String

The DbContext uses the same connection string as your DatabaseServer:
- Server: localhost
- Port: 3306
- Database: cmg
- User: root
- Password: (empty)

To change it, modify the `OnConfiguring` method in `CmgDbContext.cs`.

## Next Steps

1. **Try the examples**: Use `EfCoreExamples` class to test EF Core
2. **Update ListEnsembles.cs**: Replace with EF Core version
3. **Update ListVoices.cs**: Replace with EF Core version
4. **Refactor CRUD operations**: Replace Add/Modify/Delete methods in models

## Need Help?

The `EfCoreExamples.cs` file contains working examples of all common operations. Use it as a reference when updating your code.
