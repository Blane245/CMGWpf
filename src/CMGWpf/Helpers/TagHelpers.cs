using CMGWpf.Data;
using CMGWpf.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace CMGWpf.Helpers
{
    public static class TagHelpers
    {
        public static async Task<ObservableCollection<Tag>> List()
        {
            using var context = new CmgDbContext();
            var tags = await context.Tags
                .ToListAsync();
            return new ObservableCollection<Tag>(tags);
        }
        public static async Task<Tag?> Get(string name)
        {
            using var context = new CmgDbContext();
            var tag = await context.Tags
                .AsNoTracking()
                .Include(e => e.NoteSequences)  // Eager load the related note sequences
                .FirstOrDefaultAsync((e) => e.Name == name);
            context.Entry(tag!).State = EntityState.Detached;
            return tag;
        }
        public static async Task<bool> Delete(string name)
        {
            using var context = new CmgDbContext();
            var tag = await context.Tags
                .FirstOrDefaultAsync(e => e.Name == name);
            if (tag == null)
                return false;
            context.Tags.Remove(tag);
            await context.SaveChangesAsync();
            return true;
        }
        public static async Task<bool> Add(Tag tag)
        {
            try
            {
                using var context = new CmgDbContext();
                context.Tags.Add(tag);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding tag: {ex.Message}");
                return false;
            }
        }
        public static async Task<bool> Modify(Tag newTag, string newName)
        {
            try
            {
                using var context = new CmgDbContext();
                var existingTag = await context.Tags
                    .FirstOrDefaultAsync(e => e.Name == newTag.Name);
                if (existingTag == null)
                    return false;

                if (existingTag.Name != newName) // the name is changed delete the existing tag and add a new one with the new name
                {
                    try
                    {
                        context.Remove(existingTag);
                        await context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error removing tag: {ex.Message}");
                        return false;

                    }
                    newTag.Name = newName;
                    bool addSuccessed = await Add(newTag);
                    return addSuccessed;
                }
                else return true; // nothing to do here
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error modifying tag: {ex.Message}");
                return false;
            }
        }
    }
}
