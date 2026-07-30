using System.Data;
using Dapper;

namespace PopForums.Sql;

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
	public override void SetValue(IDbDataParameter parameter, DateOnly value)
	{
		parameter.DbType = DbType.Date;
		parameter.Value = value.ToDateTime(TimeOnly.MinValue);
	}

	public override DateOnly Parse(object value)
	{
		return DateOnly.FromDateTime((DateTime)value);
	}
}
