
using CMGWpf.Model;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using static CMGWpf.Model.Generators.StochasticTypes;
using static CMGWpf.Types.DBTypes;

namespace CMGWpf.Utilities
{
    public static class NoteSequenceUtilities
    {
        public static async Task<ObservableCollection<string>> GetNoteSequenceNamesAsync()
        {
            DbResult<DbNoteSequenceValidNamesType> dbResponse = await CMGDB.FetchAsync<DbNoteSequenceValidNamesType>($"note/valid", "GET");

            if (dbResponse.IsSuccess && dbResponse.Value.value != null)
            {
                return [.. dbResponse.Value.value.Select(dbNoteSequence => dbNoteSequence.name)];
            }
            else
            {
                return [];
            }
        }
        public static async Task<Sequence> GetNoteSequenceAsync(string name)
        {
            DbResult<DbNoteSequenceValueType> dbResponse = await CMGDB.FetchAsync<DbNoteSequenceValueType>($"note/{name}", "GET");
            if (dbResponse.IsSuccess)
            {
                Sequence sequence = new()
                {
                    Name = dbResponse.Value.value.name,
                    Items = [..dbResponse.Value.value.items],
                };
                return sequence;
            }
            else
            {
                return new Sequence { Name = name, Items = [] };
            }

        }
    }
}
