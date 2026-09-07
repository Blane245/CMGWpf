using CMGWpf.Data;
using CMGWpf.Model.Database;
using CMGWpf.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace CMGWpf.Helpers
{
    public static class ChordSequenceHelpers
    {
        public static async Task<ObservableCollection<ChordSequence>> List()
        {
            using var context = new CmgDbContext();
            var chordSequences = await context.ChordSequences
                .ToListAsync();
            return new ObservableCollection<ChordSequence>(chordSequences);
        }
        public static async Task<ChordSequence?> Get(string name)
        {
            using var context = new CmgDbContext();
            var chordSequence = await context.ChordSequences
                .AsNoTracking()
                .FirstOrDefaultAsync((e) => e.Name == name);
            if (chordSequence != null)context.Entry(chordSequence!).State = EntityState.Detached;
            return chordSequence;
        }
        public static async Task<bool> Delete(string name)
        {
            using var context = new CmgDbContext();
            var chordSequence = await context.ChordSequences
                .FirstOrDefaultAsync(e => e.Name == name);
            if (chordSequence == null)
                return false;
            context.ChordSequences.Remove(chordSequence);
            await context.SaveChangesAsync();
            return true;
        }
        public static async Task<bool> Add(ChordSequence chordSequence)
        {
            try
            {
                using var context = new CmgDbContext();
                context.ChordSequences.Add(chordSequence);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                DebugLog.Write($"Error adding chord sequence: {ex.Message}");
                return false;
            }
        }
        public static async Task<bool> Modify(ChordSequence newChordSequence, string newName)
        {
            try
            {
                using var context = new CmgDbContext();
                var existingChordSequence = await context.ChordSequences
                    .FirstOrDefaultAsync(e => e.Name == newChordSequence.Name);
                if (existingChordSequence == null)
                    return false;
                context.Remove(existingChordSequence);
                newChordSequence.Name = newName;
                context.ChordSequences.Add(newChordSequence);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                DebugLog.Write($"Error modifying chord sequence: {ex.Message}");
                return false;
            }
        }
    }
}
