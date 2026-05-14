using CMGWpf.Helpers;
using CMGWpf.Types;
using System.Collections.ObjectModel;
namespace CMGWpf.Model.Database
{
    public class Ensemble
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        // EF Core navigation property for many-to-many relationship
        public virtual ICollection<Voice> Voices { get; set; } = new List<Voice>();

        public Ensemble Clone() { return new Ensemble { Name = Name, Description = Description, Voices = new List<Voice>(Voices) }; }
        public static ObservableCollection<Message> Validate(Ensemble ensemble, string newName, ObservableCollection<Ensemble> allEnsembles)
        {
            ObservableCollection<Message> errors = new ObservableCollection<Message>();
            // check that the ensemble name is not blank and is unique
            if (newName.Trim(' ', '\t') == "")
            {
                Messages.Add(errors, "Ensemble name must not be blank", true);
            }
            // find the ensemble object so it can be skipped
            int ensembleIndex = -1;
            for (int i = 0; i < allEnsembles.Count; i++)
            {
                if (allEnsembles[i].Name == ensemble.Name) { ensembleIndex = i; break; }
            }
            // see if there is another ensemble with the same name
            for (int i = 0; i < allEnsembles.Count; i++)
            {
                if (i == ensembleIndex) continue;
                if (newName == allEnsembles[i].Name)
                {
                    Messages.Add(errors, $"Ensemble name '{newName}' must be unique", true);
                    break;
                }
            }
            return errors;
        }
    }
}
