using CMGWpf.Data;
using CMGWpf.Model.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace CMGWpf.Helpers
{
    /// <summary>
    /// Helper class demonstrating how to use Entity Framework Core with your models
    /// </summary>
    public static class EfCoreExamples
    {
        /// <summary>
        /// Example: List all ensembles using EF Core
        /// </summary>
        public static async Task<ObservableCollection<Ensemble>> ListEnsemblesWithEfCore()
        {
            using var context = new CmgDbContext();

            // Load ensembles with their related voices
            var ensembles = await context.Ensembles
                .Include(e => e.Voices)  // Eager load the related voices
                .ToListAsync();

            return new ObservableCollection<Ensemble>(ensembles);
        }

        /// <summary>
        /// Example: List all voices using EF Core
        /// </summary>
        public static async Task<ObservableCollection<Voice>> ListVoicesWithEfCore()
        {
            using var context = new CmgDbContext();

            var voices = await context.Voices.ToListAsync();

            return new ObservableCollection<Voice>(voices);
        }

        /// <summary>
        /// Example: Add a new ensemble with EF Core
        /// </summary>
        public static async Task<bool> AddEnsembleWithEfCore(Ensemble ensemble)
        {
            try
            {
                using var context = new CmgDbContext();

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

        /// <summary>
        /// Example: Update an ensemble with EF Core
        /// </summary>
        public static async Task<bool> UpdateEnsembleWithEfCore(string ensembleName, string newDescription)
        {
            try
            {
                using var context = new CmgDbContext();

                var ensemble = await context.Ensembles
                    .FirstOrDefaultAsync(e => e.Name == ensembleName);

                if (ensemble == null)
                    return false;

                ensemble.Description = newDescription;
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating ensemble: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Example: Delete an ensemble with EF Core
        /// </summary>
        public static async Task<bool> DeleteEnsembleWithEfCore(string ensembleName)
        {
            try
            {
                using var context = new CmgDbContext();

                var ensemble = await context.Ensembles
                    .FirstOrDefaultAsync(e => e.Name == ensembleName);

                if (ensemble == null)
                    return false;

                context.Ensembles.Remove(ensemble);
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting ensemble: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Example: Add voices to an ensemble with EF Core
        /// </summary>
        public static async Task<bool> AddVoicesToEnsembleWithEfCore(string ensembleName, List<string> voiceNames)
        {
            try
            {
                using var context = new CmgDbContext();

                var ensemble = await context.Ensembles
                    .Include(e => e.Voices)
                    .FirstOrDefaultAsync(e => e.Name == ensembleName);

                if (ensemble == null)
                    return false;

                foreach (var voiceName in voiceNames)
                {
                    var voice = await context.Voices
                        .FirstOrDefaultAsync(v => v.Name == voiceName);

                    if (voice != null && !ensemble.Voices.Contains(voice))
                    {
                        ensemble.Voices.Add(voice);
                    }
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding voices to ensemble: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Example: Get ensemble with all its voices
        /// </summary>
        public static async Task<Ensemble?> GetEnsembleWithVoicesWithEfCore(string ensembleName)
        {
            using var context = new CmgDbContext();

            return await context.Ensembles
                .Include(e => e.Voices)
                .FirstOrDefaultAsync(e => e.Name == ensembleName);
        }

        /// <summary>
        /// Example: Search ensembles by name (LINQ query)
        /// </summary>
        public static async Task<ObservableCollection<Ensemble>> SearchEnsemblesWithEfCore(string searchTerm)
        {
            using var context = new CmgDbContext();

            var ensembles = await context.Ensembles
                .Where(e => e.Name.Contains(searchTerm) || e.Description.Contains(searchTerm))
                .Include(e => e.Voices)
                .ToListAsync();

            return new ObservableCollection<Ensemble>(ensembles);
        }
    }
}
