using CMGWpf.Model;
using CMGWpf.Model.Generators;

namespace CMGWpf.Utilities
{
    public static class Uid
    {
        /// <summary>
        /// *Get a uid for a track or a generator depending on the type value
        /// </summary>
        /// <param name="type">"Track" - get a track uid; otherwise get a generator uid "</param>
        /// <param name="tracks">The list of curren tracks in the model.</param>
        /// <returns>a uid</returns>
        public static int Get(string type, List<Track> tracks)
        {
            if (type == "Track")
            {
                int next = 0;
                bool found = false;
                while (!found)
                {
                    int index = tracks.FindIndex((t) => t.Name == "T" + next.ToString());
                    if (index < 0)
                    {
                        found = true;
                    }
                    else
                        next++;
                }
                return next;
            }
            else
            {
                // construct a list of all generators on all track and find a uid
                List<Generator> list = [];
                foreach (Track t in tracks)
                {
                    list.AddRange(t.Generators);
                }
                int next = 0;
                bool found = false;
                while (!found)
                {
                    int index = list.FindIndex((g) => g.Name == "G" + next.ToString());
                    if (index < 0)
                    {
                        found = true;
                    }
                    else
                        next++;
                }
                return next;
            }


        }
    }
}
