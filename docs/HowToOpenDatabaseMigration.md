# How to Open the Database Migration Dialog

## From the Application Menu

The easiest way to access the Database Migration Dialog is through the application's menu system:

### Steps:

1. **Launch your CMGWpf application**

2. **Open the Tools menu**
   - Click on "**Tools**" in the menu bar at the top of the window

3. **Click "Database Migration..."**
   - You'll see it listed below the "Generator and Calculator Tools" option
   - There's a separator line before it for visual clarity

4. **The Database Migration Dialog will open**
   - You can now enter your MySQL connection string and perform the migration

---

## Menu Location

```
Menu Bar
  └─ Tools
      ├─ Generator and Calculator Tools... (Ctrl+G)
      ├─ ──────────────────────────── (separator)
      └─ Database Migration...         ← Click here!
```

---

## Alternative: Open Programmatically

If you want to open it from code elsewhere in your application:

```csharp
using CMGWpf.Dialogs;

// In any event handler or method:
var migrationDialog = new DatabaseMigrationDialog();
migrationDialog.ShowDialog();  // Modal dialog
// or
migrationDialog.Show();        // Non-modal dialog
```

---

## Using the Dialog

Once the dialog opens:

### 1. Enter MySQL Connection String
Default: `Server=localhost;Port=3306;Database=cmg;Uid=root;Pwd=;`

Modify as needed for your MySQL setup.

### 2. (Optional) Export to JSON Backup
- Click **"Export to JSON (Backup)"**
- Choose a directory to save backup files
- This creates:
  - `tags.json`
  - `voices.json`
  - `ensembles.json`
  - `notesequences.json`

### 3. Migrate to SQLite
- Click **"Migrate to SQLite"**
- Watch the progress bar
- Review the results in the status area

### 4. Verify
Your data is now in SQLite at:
```
%LOCALAPPDATA%\CMGWpf\cmg.db
```
Or typically:
```
C:\Users\[YourUsername]\AppData\Local\CMGWpf\cmg.db
```

---

## Troubleshooting

### Can't see the menu item?
- Make sure you've rebuilt the application
- Check that you're running the latest version
- The menu item appears in the **Tools** menu

### Dialog won't open?
- Check the Output window in Visual Studio for any errors
- Make sure the `DatabaseMigrationDialog.xaml` file is set to build action "Page"

### Migration fails?
- Verify your MySQL server is running
- Check the connection string is correct
- Ensure you have permission to access the MySQL database
- Try exporting to JSON first to verify connectivity

---

## What Happens During Migration?

1. **Connects to MySQL** using your connection string
2. **Reads all data** from the four main tables (Tag, Voice, Ensemble, NoteSequence)
3. **Creates SQLite database** if it doesn't exist (at %LOCALAPPDATA%\CMGWpf\cmg.db)
4. **Imports all records** while checking for duplicates
5. **Recreates relationships** (many-to-many between Ensembles/Voices and NoteSequences/Tags)
6. **Reports results** with counts of migrated records

---

## After Migration

Once migration is complete:

✅ Your application now uses the SQLite database  
✅ All data is preserved with relationships intact  
✅ No MySQL server needed for future runs  
✅ Database is a single file - easy to backup  

You can:
- Continue using your application normally
- Back up the SQLite file by copying it
- Share the database by copying the .db file
- Remove MySQL server if you no longer need it

---

## Quick Reference

**Menu Path:** Tools → Database Migration...

**Connection String Format:**
```
Server=localhost;Port=3306;Database=cmg;Uid=root;Pwd=yourpassword;
```

**SQLite Database Location:**
```
%LOCALAPPDATA%\CMGWpf\cmg.db
```

**Backup Files Created:**
- tags.json
- voices.json  
- ensembles.json
- notesequences.json
