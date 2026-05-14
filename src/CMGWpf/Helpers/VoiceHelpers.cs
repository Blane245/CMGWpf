using CMGWpf.Data;
using CMGWpf.Model.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace CMGWpf.Helpers
{
    public static class VoiceHelpers
    {
        public static async Task<ObservableCollection<Voice>> List()
        {
            using var context = new CmgDbContext();
            var voices = await context.Voices.ToListAsync();
            return new ObservableCollection<Voice>(voices);
        }
        public static async Task<Voice?> Get(string name)
        {
            using var context = new CmgDbContext();
            var voice = await context.Voices
                .AsNoTracking()  // Avoid tracking for read-only operation
                .Include(e => e.Ensembles)  // Eager load the related voices
                .FirstOrDefaultAsync((e) => e.Name == name);
            context.Entry(voice!).State = EntityState.Detached;  // Detach the entity to prevent tracking issues
            return voice;
        }
        public static async Task<bool> Delete(string name)
        {
            using var context = new CmgDbContext();
            var voice = await context.Voices
                .FirstOrDefaultAsync(v => v.Name == name);
            if (voice == null) return false;
            context.Voices.Remove(voice);
            await context.SaveChangesAsync();
            return true;
        }
        public static async Task<bool> Add(Voice voice)
        {
            try
            {
                using var context = new CmgDbContext();
                context.Voices.Add(voice);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding voice '{voice.Name}': {ex.Message}");
                return false;
            }
        }
        public static async Task<bool> Modify(Voice newVoice, string newName)
        {
            try
            {
                using var context = new CmgDbContext();
                var existingVoice = await context.Voices
                    .FirstOrDefaultAsync(v => v.Name == newVoice.Name);
                if (existingVoice == null)
                    return false;
                if (newVoice.Name != newName) // the name is changed delete the existing voice and add a new one with the new name. Ensembles will be updated to point to the new voice
                {
                    // hold the ensembles in a variable before deleting the existing voice
                    var ensembles = existingVoice.Ensembles.ToList();
                        context.Remove(existingVoice);
                        await context.SaveChangesAsync();
                    newVoice.Name = newName;
                    newVoice.Ensembles = ensembles; // assign the ensembles to the new voice
                    bool addSuccedded = await Add(newVoice);
                    return addSuccedded;
                }
                else // voice name did not change
                {
                    existingVoice.Description = newVoice.Description;
                    existingVoice.RegisterLo = newVoice.RegisterLo;
                    existingVoice.RegisterHi = newVoice.RegisterHi;
                    existingVoice.Duration = newVoice.Duration;
                    existingVoice.Timbre = newVoice.Timbre;
                    existingVoice.SoundFontFile = newVoice.SoundFontFile;
                    existingVoice.PresetName = newVoice.PresetName;
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error modifying voice: {ex.Message}");
                return false;
            }
        }
    }
}
