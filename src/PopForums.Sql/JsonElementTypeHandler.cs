namespace PopForums.Sql;

public class JsonElementTypeHandler : SqlMapper.TypeHandler<JsonElement>
{
	public override void SetValue(IDbDataParameter parameter, JsonElement value)
	{
		parameter.DbType = DbType.String;
		parameter.Size = int.MaxValue;
		parameter.Value = value.ToString();
	}

	public override JsonElement Parse(object value)
	{
		var o = JsonSerializer.Deserialize<dynamic>((string)value);
		var element = JsonSerializer.SerializeToElement(o, new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
		return element;
	}
}