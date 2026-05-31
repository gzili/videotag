using System.Data;
using Dapper;

namespace VideoTag.Server.SqlTypeHandlers;

class GuidHandler : SqlMapper.TypeHandler<Guid>
{
    // Parameters are converted by Microsoft.Data.Sqlite
    public override void SetValue(IDbDataParameter parameter, Guid value) => parameter.Value = value;
    
    public override Guid Parse(object value) => Guid.Parse((string)value);
}
