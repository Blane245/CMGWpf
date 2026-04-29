using CMGWpf.Types;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace CMGDBEditor.Model
{
    public class NoteSequence
    {
        string Name { get; set; } = string.Empty;
        string Attribute { get; set; } = "note";
        string Tags { get; set; } = string.Empty;
        Item[] Items { get; set; } = [];
        public string Encode()
        {
            return JsonSerializer.Serialize(Items);
        }
        public void Decode(string input)
        {
            Items = JsonSerializer.Deserialize<Item[]>(input) ?? [];
        }
        public DBTypes.DbErrorType[] Validate(string[] tagList)
        {
            DBTypes.DbErrorType[] errors = [];
            if (Name.Trim() == "")
            {
                errors = [.. errors, new DBTypes.DbErrorType()
                {
                    type = DBTypes.DBRESPONSETYPE.error,
                    message = "Note sequence name must not be blank"
                }];
            }

            // tags may be blank but if not they exist in the tag list
            string[] tags = Tags.Split(',').Select(t => t.Trim()).Where(t => t != "").ToArray();
            for (int i = 0; i < tags.Length; i++)
            {
                if (!tagList.Contains(tags[i]))
                {
                    errors = [.. errors, new DBTypes.DbErrorType()
                    {
                        type = DBTypes.DBRESPONSETYPE.error,
                        message = $"Tag '{tags[i]}' does not exist in the tag list"
                    }];
                }
            }
            return errors;
        }
    }
}
