using CMGDBEditor.Data;
using CMGDBEditor.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace CMGDBEditor.Helpers
{
    /// <summary>
    /// EF Core version of ListEnsembles - ready to replace the existing one
    /// </summary>
    public static class ListEnsemblesEfCore
    {
        public static async Task<ObservableCollection<Ensemble>> Execute()
        {
            try
            {
                using var context = new CmgDbContext();

                // Load all ensembles with their voices in a single query
                var ensembles = await context.Ensembles
                    .Include(e => e.Voices)
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                return new ObservableCollection<Ensemble>(ensembles);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error listing ensembles: {ex.Message}");
                return new ObservableCollection<Ensemble>();
            }
        }
    }

    /// <summary>
    /// EF Core version of ListVoices - ready to replace the existing one
    /// </summary>
    public static class ListVoicesEfCore
    {
        public static async Task<ObservableCollection<Voice>> Execute()
        {
            try
            {
                using var context = new CmgDbContext();

                // Load all voices, optionally with their ensembles
                var voices = await context.Voices
                    // Uncomment to also load which ensembles use each voice:
                    // .Include(v => v.Ensembles)
                    .OrderBy(v => v.Name)
                    .ToListAsync();

                return new ObservableCollection<Voice>(voices);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error listing voices: {ex.Message}");
                return new ObservableCollection<Voice>();
            }
        }
    }
}
