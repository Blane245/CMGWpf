using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.Services;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.IO;
using static CMGWpf.Types.PlayTypes;
using Track = CMGWpf.Model.Track;

namespace CMGWpf.PlayFunctions.Utilities
{
    public static class ReportWriter
    {
        private static ObservableCollection<InstrumentSource> InstrumentSources => PlayViewModel.Instance.InstrumentSources;
        /// <summary>
        /// This will use the information gathered during the play process to write a report to a file in HTML format. 
        /// The report will include details about the instruments used, their parameters, and any relevant information 
        /// that can help with debugging or analysis.
        /// </summary>
        /// <param name="filePath">Path to the output HTML file</param>
        public static void WriteReport(string filePath)
        {
            try
            {
                string content = BuildHtmlReport();
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing report: {ex.Message}");
                throw;
            }
        }

        private static string BuildHtmlReport()
        {
            string fileName = GlobalService.Instance.FileName;
            string version = CMGWpf.Properties.Settings.Default.Version;
            CMGFile file = FileViewModel.Instance.File;
            ObservableCollection<Generator> generators = PlayViewModel.Instance.PlayGenerators;
            double duration = PlayViewModel.Instance.PlayDuration;

            // sort the sources by start time for better readability
            List<InstrumentSource> sources = [.. InstrumentSources.OrderBy(s => s.StartTime)];

            string html = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>CMG Composition Report</title>
                    <style>{{GetStyles()}}</style>
                </head>
                <body>
                    <div class="report">
                        <h1>File: CMG Version: {{version}} File Report</h1>
                        <p><strong>{{fileName}}</strong> </p>
                            <div class="summary">
                            <p><strong>Duration:</strong> {{duration:F2}} seconds <strong>Generators:</strong> {{generators.Count}} <strong>Instrument Sources:</strong> {{sources.Count}}</p>
                        </div>
                        {{RenderTracks(file.Tracks, generators, sources)}}
                        {{RenderAllSources(sources)}}
                    </div>
                </body>
                </html>
                """;

            return html;
        }

        private static string GetStyles()
        {
            return """
                body {
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    margin: 20px;
                    background-color: #f5f5f5;
                    width: 24in;
                }
                .report {
                    max-width: 1200px;
                    margin: 0 auto;
                    background-color: white;
                    padding: 30px;
                    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
                    border-radius: 8px;
                }
                h1 {
                    color: #2c3e50;
                    border-bottom: 3px solid #3498db;
                    padding-bottom: 10px;
                }
                h2 {
                    color: #2c3e50;
                    margin-top: 30px;
                    border-bottom: 2px solid #95a5a6;
                    padding-bottom: 5px;
                }
                h3 {
                    color: #2c3e50;
                    margin-top: 20px;
                }
                .summary {
                    background-color: #ecf0f1;
                    padding: 5px;
                    border-radius: 5px;
                    margin-bottom: 20px;
                }
                .summary p {
                    margin: 5px 0;
                }
                table {
                    border-collapse: collapse;
                    width: 100%;
                    margin: 15px 0;
                    background-color: white;
                }
                th, td {
                    border: 1px solid #bdc3c7;
                    padding: 10px;
                    text-align: left;
                }
                th {
                    background-color: #3498db;
                    color: white;
                    font-weight: 600;
                }
                tr:nth-child(even) {
                    background-color: #f9f9f9;
                }
                tr:hover {
                    background-color: #e8f4f8;
                }
                .track {
                    margin-bottom: 30px;
                    padding: 15px;
                    border-left: 4px solid #3498db;
                    background-color: #fafafa;
                }
                .generator {
                    margin: 10px 0;
                    padding: 10px;
                    background-color: #fff;
                    border-left: 3px solid #95a5a6;
                }
                .generator-type {
                    display: inline-block;
                    padding: 3px 8px;
                    border-radius: 3px;
                    font-size: 0.85em;
                    font-weight: bold;
                    margin-left: 10px;
                }
                .type-algorithmic {
                    background-color: #3498db;
                    color: white;
                }
                .type-stochastic {
                    background-color: #e74c3c;
                    color: white;
                }
                hr {
                    border: none;
                    border-top: 2px solid #bdc3c7;
                    margin: 30px 0;
                }
                """;
        }

        private static string RenderTracks(List<Track> tracks, ObservableCollection<Generator> playGenerators, List<InstrumentSource> sources)
        {
            if (tracks.Count == 0)
                return "<p>No tracks found.</p>";

            return $$"""
                <h2>Tracks ({{tracks.Count}})</h2>
                {{string.Join("\n", tracks.Select(t => RenderTrack(t, playGenerators, sources)))}}
                """;
        }

        private static string RenderTrack(Track track, ObservableCollection<Generator> playGenerators, List<InstrumentSource> sources)
        {
            var trackGenerators = playGenerators.Where(g => g.Parent == track).ToList();

            return $$"""
                <div class="track">
                    <h3>Track: {{track.Name}} {{ (track.Mute? "Muted" : "") }} {{ (track.Solo? "Soloed" : "") }} Volume (dB): {{track.Volume}} Active Generators {{trackGenerators.Count}}</h3>
                    {{RenderGenerators(trackGenerators, sources)}}
                </div>
                """;
        }

        private static string RenderGenerators(List<Generator> generators, List<InstrumentSource> sources)
        {
            if (generators.Count == 0)
                return "<p><em>No active generators in this track.</em></p>";

            return string.Join("\n", generators.Select(g => RenderGenerator(g, sources)));
        }

        private static string RenderGenerator(Generator gen, List<InstrumentSource> sources)
        {
            string typeName = gen switch
            {
                Algorithmic => "Algorithmic",
                Stochastic => "Stochastic",
                _ => "Unknown"
            };

            string typeClass = gen switch
            {
                Algorithmic => "type-algorithmic",
                Stochastic => "type-stochastic",
                _ => ""
            };

            return $$"""
                <div class="generator">
                    <strong>{{gen.Name}}</strong>
                    <span class="generator-type {{typeClass}}">{{typeName}}</span> Time: {{gen.StartTime:F2}}s — {{gen.StopTime:F2}}s ({{gen.StopTime - gen.StartTime:F2}}s)
                </div>
                {{RenderGeneratorDetails(gen)}}
                <hr/>
                {{RenderGeneratorSources(gen, sources)}}
                """;
        }

        private static string RenderGeneratorDetails(Generator gen)
        {
            return gen switch
            {
                Algorithmic algo => RenderAlgorithmicDetails(algo),
                Stochastic stoch => RenderStochasticDetails(stoch),
                _ => ""
            };
        }

        private static string RenderAlgorithmicDetails(Algorithmic algo)
        {
            return $$"""
                <table>
                    <thead>
                        <th>Start Time (sec)</th>
                        <th>Stop Time (sec)</th>
                        <th>SoundFont</th>
                        <th>Preset</th>
                        <th>Microtones?</th>
                        <th>Looping?</th>
                        <th>Attack?</th>
                        <th>Measure Length (beats)</th>
                        <th>Beat Count</th>
                        <th>Beat Offset (beats)</th>
                        <th>Notes in Octave (tones)</th>
                        <th>Note Offset (tones)</th>
                        <th>Noise Seed</th>
                        <th>Noise Frequency (Hz)</th>
                        <th>Noise Amplitude (dB)</th>
                        <th>Reverb Duration (sec)</th>
                        <th>Reverb Decay (dB)</th>
                        <th>Tremolo Frequency (Hz)</th>
                        <th>Tremolo Depth (dB)</th>
                        <th>Vibrato Frequency (Hz)</th>
                        <th>Vibrato Depth (dB)</th>
                    </thead>
                    <tbody>
                        <tr>
                            <td>{{algo.StartTime:F2}}</td>
                            <td>{{algo.StopTime:F2}}</td>
                            <td>{{algo.SoundFontFileName}}</td>
                            <td>{{algo.PresetName}}</td>
                            <td>{{(algo.Microtones ? "Yes" : "No")}}</td>
                            <td>{{(algo.IsLooping ? "Yes" : "No")}}</td>
                            <td>{{algo.MeasureLength}}</td>
                            <td>{{algo.OffsetSequence}}</td>
                            <td>{{algo.NoteCount}}</td>
                            <td>{{algo.OffsetNotes}}</td>
                            <td>{{algo.NoiseSeed}}</td>
                            <td>{{algo.NoiseFrequency:F2}}</td>
                            <td>{{algo.NoiseAmplitude:F2}}</td>
                            <td>{{algo.ReverbDelay:F2}}</td>
                            <td>{{algo.ReverbDecay}}</td>
                            <td>{{algo.Tremolo.Speed:F2}}</td>
                            <td>{{algo.Tremolo.Depth:F2}}</td>
                            <td>{{algo.Vibrato.Speed:F2}}</td>
                            <td>{{algo.Vibrato.Depth:F2}}</td>
                        </tr>   
                </table>
                """;
        }

        private static string RenderStochasticDetails(Stochastic stoch)
        {
            return $$"""
                <div>
                <strong>Composition Parameters</strong>
                <table>
                    <thead>
                        <th>Ensemble Type</th>
                        <th>Composition Duration (sec)</th>
                        <th>Number of Time Cells</th>
                        <th>Lambda</th>
                        <th>Composition Seed</th>
                    </thead>
                    <tbody>
                        <tr>
                            <td>{{stoch.Ensemble}}</td>
                            <td>{{stoch.CompositionDuration:F2}}</td>
                            <td>{{stoch.NumberOfTimeCells}}</td>
                            <td>{{stoch.Lambda:F2}}</td>
                            <td>{{stoch.CompositionSeed}}</td>
                        </tr>
                    </tbody>
                </table>
                </div>
                <div>
                <strong>Dynamic Parameters</strong>
                <table>
                    <thead>
                            <th>Intensity Option</th>
                            <th>Intensity Transition Option</th>
                            <th>Intensity Cycle Time (sec)</th>
                            <th>Pan Option</th>
                            <th>Pan Algorithm</th>
                            <th>Pan Cycle Time (sec)</th>
                            <th>Reverb Delay (msec)</th>
                            <th>Reverb Decay (dB)</th>
                                        </thead>
                        <tbody>
                            <tr>
                                <td>{{stoch.IntensityOption}}</td>
                                <td>{{stoch.IntensityTransitionOption}}</td>
                                <td>{{stoch.IntensityParameters.CycleTime:F2}}</td>
                                <td>{{stoch.PanOption}}</td>
                                <td>{{stoch.PanAlgorithm}}</td>
                                <td>{{stoch.PanParameters.CycleTime:F2}}</td>
                                <td>{{stoch.ReverbParameters.Delay:F2}}</td>
                                <td>{{stoch.ReverbParameters.Decay:F2}}</td>
                            </tr>
                        </tbody>
                </table>
                </div>
                <div>
                    <strong>Voices</strong>
                    <table>
                        <thead>
                            <th>Voice Name</th>
                            <th>Description</th>
                            <th>SoundFont</th>
                            <th>Preset</th>
                            <th>Timbre</th>
                            <th>Register</th>
                            <th>Duration</th>
                        </thead>
                        <tbody>
                            {{string.Join("\n", stoch.Voices.Select(v => $"<tr><td>{v.Name}</td><td>{v.Description}</td><td>{v.SoundFontFileName}</td><td>{v.PresetName}</td><td>{v.Timbre}</td><td>{v.RegisterLo:F1} → {v.RegisterHi:F1}</td><td>{v.Duration:F1}</td></tr>"))}}
                        </tbody>
                    </table>
                </div>
                <div>
                <strong>Composition</strong>
                {{RenderStochasticDetailsCompositon(stoch)}}
                </div>
                """;
        }
        private static string RenderStochasticDetailsCompositon(Stochastic stoch)
        {
            if (stoch.Composition.Length == 0)
                return "<p>No composition details found.</p>";

            // build the header which has a time column and one columf for each unmuted voice
            string header = "<thead><tr><th>Time (sec)</th>";
            foreach(var voice in stoch.Voices.Where(v => !v.Muted) ) {
                header += $"<th>{voice.Name}</th>";
            }
            header += "<th>Sum</th></tr></thead>";
            // build the body which has one row for each time cell, the first column is the time (time cell index * composition duration / number of time cells) and the other columns are the active voices at that time cell (comma separated if more than one)
            double time = 0;
            double deltaT = stoch.GetDeltaT();
            string body = "<tbody>";
            foreach (var row in stoch.Composition) {
                body+= $"<tr><td>{time:F2}</td>";
                foreach((var voice, var i) in stoch.Voices.Select((v, i) => (v, i))) {
                    if (!voice.Muted) {
                        body+= $"<td>{row[i]}</td>";
                    }
                }
                body += $"<td>{row.Sum():F0}</td</tr>";
                time += deltaT;
            }
            body += "</tr></tbody>";
            return $$"""
                <table>
                    {{ header }}
                    {{ body }}
                </table>
                """;
        }

        private static string RenderGeneratorSources(Generator generator, List<InstrumentSource> sources)
        {
            // extract the sources that belong to this generator
            if (sources.Count == 0)
                return "<p>No instrument sources found.</p>";
            var genSources = sources.Where(s => s.Generator!.Name == generator.Name).ToList();
            if (genSources.Count == 0)
                return "<p>No instrument sources found.</p>";

            return $$"""
                <h3>Instrument Sources ({{genSources.Count}})</h3>
                <table>
                    <thead>
                        <tr>
                            <th>Generator</th>
                            <th>Time</th>
                            <th>SoundFont</th>
                            <th>Preset</th>
                            <th>Pitch Range</th>
                            <th>Instrument</th>
                            <th>Looping?</th>
                            <th>Loop Samples</th>
                            <th>Root Key (midi)</th>
                            <th>Cents</th>
                            <th>Sample Rate (bps)</th>
                            <th>Sample Count</th>
                            <th>Attack Enabled?</th>
                            <th>Envelope</th>
                                        </tr>
                    </thead>
                    <tbody>
                        {{string.Join("\n", sources.Select(RenderSourceRow))}}
                    </tbody>
                </table>
                """;
        }

        private static string RenderAllSources(List<InstrumentSource> sources)
        {
            if (sources.Count == 0)
                return "<p>No instrument sources found.</p>";

            return $$"""
                <h2>Instrument Sources ({{sources.Count}})</h3>
                <table>
                    <thead>
                        <tr>
                            <th>Generator</th>
                            <th>Time</th>
                            <th>SoundFont</th>
                            <th>Preset</th>
                            <th>Pitch Range</th>
                            <th>Instrument</th>
                            <th>Looping?</th>
                            <th>Loop Samples</th>
                            <th>Root Key (midi)</th>
                            <th>Cents</th>
                            <th>Sample Rate (bps)</th>
                            <th>Sample Count</th>
                            <th>Attack Enabled?</th>
                            <th>Envelope</th>
                                        </tr>
                    </thead>
                    <tbody>
                        {{string.Join("\n", sources.Select(RenderSourceRow))}}
                    </tbody>
                </table>
                """;
        }

        private static string RenderSourceRow(InstrumentSource source)
        {
            string pitchRange = source.StartPitch == source.EndPitch
                ? $"{source.StartPitch:F2}"
                : $"{source.StartPitch:F2} → {source.EndPitch:F2}";
            string loopRange = $"{source.LoopStart} → {source.LoopEnd}";
            string centsRange = $"{source.StartCents:F0} → {source.EndCents:F0}";
            int ENVELOPEWIDTH = 500;
            int ENVELOPEHEIGHT = 50;
            double maxTime = source.Envelope.Length > 0 ? source.Envelope[^1].Time : 0;
            double xScale = ENVELOPEWIDTH / maxTime;
            double yScale = ENVELOPEHEIGHT / 1;
            string LineTo(double time, double gain)
            {
                return $"L{time * xScale} {(1 - gain) * yScale} ";
            }
            string Path(GainEnvelope[] envelope)
            {
                string path = $"M0 {ENVELOPEHEIGHT} ";
                foreach(var e in envelope)
                {
                    path += LineTo(e.Time, e.Gain);
                }
                path += $"L {source.Envelope[^1].Time * xScale} {ENVELOPEHEIGHT}";
                return path;
            }
            string EnvelopeGraph(GainEnvelope[] envelope)
            {
                return $$"""
                    <svg
                        xmlns="http://www.w3.org/2000/svg"
                        height={{ENVELOPEHEIGHT.ToString() + "px"}}
                        width={{ENVELOPEWIDTH.ToString() + "px"}}
                    >
                        <path d="{{Path(source.Envelope)}}" fill="Black" />
                    </svg>
                    <p style="text-align:right;">{{maxTime:F2}} (sec)</p>
                    """;
            }

            return $$"""
                        <tr>
                            <td>{{source.Generator?.Name ?? "Unknown"}}</td>
                            <td>{{source.StartTime:F2}}s — {{source.StopTime:F2}}s</td>
                            <td>{{source.SoundFontName}}</td>
                            <td>{{source.PresetName}}</td>
                            <td>{{pitchRange}}</td>
                            <td>{{source.Name}}</td>
                            <td>{{(source.LoopEnabled ? "Yes" : "No")}}</td>
                            <td>{{loopRange}}</td>
                            <td>{{source.RootKey}}</td>
                            <td>{{centsRange}}</td>
                            <td>{{source.SampleRate}}</td>
                            <td>{{source.SampleCount}}</td>
                            <td>{{(source.AttackEnabled ? "Yes" : "No")}}</td>
                            <td>{{EnvelopeGraph(source.Envelope)}}</td>                                
                        </tr>
                """;
        }
    }
}
