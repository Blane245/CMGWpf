
using CMGWpf.Types;

namespace CMGDBEditor.Model
{
    public class Tag
    {
        public string Name { get; set; } = "";

        public DBTypes.DbErrorType[] Validate(string[] tagList)
        {
            DBTypes.DbErrorType[] errors = [];
            if (Name.Trim() == "" || Name.Contains(','))
            {
                errors = [.. errors, new DBTypes.DbErrorType()
                {
                    type = DBTypes.DBRESPONSETYPE.error,
                    message = "Tag name must not be blank or contain commas"
                }];
            }
            // check that the tag name is unique
            for (int i = 0; i < tagList.Length; i++)
            {
                if (Name == tagList[i])
                {
                    errors = [.. errors, new DBTypes.DbErrorType()
                    {
                        type = DBTypes.DBRESPONSETYPE.error,
                        message = "Tag name must be unique"
                    }];
                    break;
                }
            }
            return errors;
        }
    }
}
