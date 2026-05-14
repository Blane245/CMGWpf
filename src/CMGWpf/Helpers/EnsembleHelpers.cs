using CMGWpf.Data;
using CMGWpf.Model.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace CMGWpf.Helpers
{
    public static class EnsembleHelpers
    {
        public static async Task<ObservableCollection<Ensemble>> List()
        {
            using var context = new CmgDbContext();
            var ensembles = await context.Ensembles
                //.Include(e => e.Voices)
                .ToListAsync();
            return new ObservableCollection<Ensemble>(ensembles);
        }
        public static async Task<Ensemble?> Get(string name)
        {
            using var context = new CmgDbContext();
            var ensemble = await context.Ensembles
                .AsNoTracking()
                .Include(e => e.Voices)  // Eager load the related voices
                .FirstOrDefaultAsync((e) => e.Name == name);
            context.Entry(ensemble!).State = EntityState.Detached;
            return ensemble;
        }
        public static async Task<bool> Delete(string name)
        {
            using var context = new CmgDbContext();
            var ensemble = await context.Ensembles
                .FirstOrDefaultAsync(e => e.Name == name);
            if (ensemble == null)
                return false;
            context.Ensembles.Remove(ensemble);
            await context.SaveChangesAsync();
            return true;
        }
        public static async Task<bool> Add(Ensemble ensemble)
        {
            try
            {
                using var context = new CmgDbContext();
                // Get only the SELECTED voices from the current context
                var selectedVoiceNames = ensemble.Voices.Select(v => v.Name).ToList();
                var selectedVoices = await context.Voices
                    .Where(v => selectedVoiceNames.Contains(v.Name))
                    .ToListAsync();

                ensemble.Voices.Clear();
                // Add the selected voices
                foreach (var voice in selectedVoices)
                {
                    ensemble.Voices.Add(voice);
                }

                context.Ensembles.Add(ensemble);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding ensemble: {ex.Message}");
                return false;
            }
        }
        public static async Task<bool> Modify(Ensemble newEnsemble, string newName)
        {
            try
            {
                using var context = new CmgDbContext();
                var existingEnsemble = await context.Ensembles
                    .Include(e => e.Voices)
                    .FirstOrDefaultAsync(e => e.Name == newEnsemble.Name);
                if (existingEnsemble == null)
                    return false;

                if (existingEnsemble.Name != newName) // the name is changed delete the existing ensemble and add a new one with the new name
                {
                    try
                    {
                        context.Remove(existingEnsemble);
                        await context.SaveChangesAsync();
                    } catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error removing ensemble: {ex.Message}");
                        return false;

                    }
                    newEnsemble.Name = newName;
                    bool addSuccessed = await Add(newEnsemble);
                    return addSuccessed;
                }
                else // ensemble name did not change
                {
                    // Clear existing voices
                    existingEnsemble.Voices.Clear();

                    // Get only the SELECTED voices from the current context
                    var selectedVoiceNames = newEnsemble.Voices.Select(v => v.Name).ToList();
                    var selectedVoices = await context.Voices
                        .Where(v => selectedVoiceNames.Contains(v.Name))
                        .ToListAsync();

                    // Add the selected voices
                    foreach (var voice in selectedVoices)
                    {
                        existingEnsemble.Voices.Add(voice);
                    }

                    existingEnsemble.Description = newEnsemble.Description;

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
