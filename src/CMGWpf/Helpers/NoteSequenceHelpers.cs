using CMGWpf.Data;
using CMGWpf.Model.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace CMGWpf.Helpers
{
    public static class NoteSequenceHelpers
    {
        public static async Task<ObservableCollection<NoteSequence>> List()
        {
            using var context = new CmgDbContext();
            var noteSequences = await context.NoteSequences
                .ToListAsync();
            return new ObservableCollection<NoteSequence>(noteSequences);
        }
        public static async Task<NoteSequence?> Get(string name)
        {
            using var context = new CmgDbContext();
            var noteSequence = await context.NoteSequences
                .AsNoTracking()
                .Include(e => e.Tags)  // Eager load the related tags
                .FirstOrDefaultAsync((e) => e.Name == name);
            context.Entry(noteSequence!).State = EntityState.Detached;
            return noteSequence;
        }
        public static async Task<bool> Delete(string name)
        {
            using var context = new CmgDbContext();
            var noteSequence = await context.NoteSequences
                .FirstOrDefaultAsync(e => e.Name == name);
            if (noteSequence == null)
                return false;
            context.NoteSequences.Remove(noteSequence);
            await context.SaveChangesAsync();
            return true;
        }
        public static async Task<bool> Add(NoteSequence noteSequence)
        {
            try
            {
                using var context = new CmgDbContext();
                // Get only the SELECTED tags from the current context
                var selectedTagNames = noteSequence.Tags.Select(t => t.Name).ToList();
                var selectedTags = await context.Tags
                    .Where(t => selectedTagNames.Contains(t.Name))
                    .ToListAsync();

                noteSequence.Tags.Clear();
                // Add the selected tags
                foreach (var tag in selectedTags)
                {
                    noteSequence.Tags.Add(tag);
                }

                context.NoteSequences.Add(noteSequence);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding note sequence: {ex.Message}");
                return false;
            }
        }
        public static async Task<bool> Modify(NoteSequence newNoteSequence, string newName)
        {
            try
            {
                using var context = new CmgDbContext();
                var existingNoteSequence = await context.NoteSequences
                    .Include(e => e.Tags)
                    .FirstOrDefaultAsync(e => e.Name == newNoteSequence.Name);
                if (existingNoteSequence == null)
                    return false;

                if (existingNoteSequence.Name != newName) // the name is changed delete the existing note sequence and add a new one with the new name
                {
                    try
                    {
                        context.Remove(existingNoteSequence);
                        await context.SaveChangesAsync();
                    } catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error removing note sequence: {ex.Message}");
                        return false;

                    }
                    newNoteSequence.Name = newName;
                    bool addSuccessed = await Add(newNoteSequence);
                    return addSuccessed;
                }
                else // note sequence name did not change
                {
                    // Clear existing tags
                    existingNoteSequence.Tags.Clear();

                    // Get only the SELECTED tags from the current context
                    var selectedTagNames = newNoteSequence.Tags.Select(t => t.Name).ToList();
                    var selectedTags = await context.Tags   
                        .Where(t => selectedTagNames.Contains(t.Name))
                        .ToListAsync();

                    // Add the selected tags
                    foreach (var tag in selectedTags)
                    {
                        existingNoteSequence.Tags.Add(tag);
                    }

                    //TODO make sure note items is serialized before we get here, otherwise we need to do it here before we save changes
                    existingNoteSequence.Items = newNoteSequence.Items;

                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error modifying ensemble: {ex.Message}");
                return false;
            }
        }
    }
}
