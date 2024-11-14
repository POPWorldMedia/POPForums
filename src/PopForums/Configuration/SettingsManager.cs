namespace PopForums.Configuration;

public interface ISettingsManager
{
	Settings Current { get; }
	void SaveCurrent();
	void Save(Settings settings);
}

public class SettingsManager : ISettingsManager
{
	public SettingsManager(ISettingsRepository settingsRepository, IErrorLog errorLog)
	{
		_settingsRepository = settingsRepository;
		_errorLog = errorLog;
		_settingsRepository.OnSettingsInvalidated += () =>
		{
			_settings = null;
		};
	}

	private readonly ISettingsRepository _settingsRepository;
	private readonly IErrorLog _errorLog;
	private Settings _settings;

	public Settings Current
	{
		get
		{
			if (_settings == null)
				LoadSettings();
			return _settings;
		}
	}

	private void LoadSettings()
	{
		var dictionary = _settingsRepository.Get();
		var settings = new Settings();
		foreach (var setting in dictionary.Keys)
		{
			var property = settings.GetType().GetProperty(setting);
			if (property == null)
			{
				_errorLog.Log(null, ErrorSeverity.Warning, $"Settings repository returned a setting called {setting}, which does not exist in code.");
			}
			else
			{
				switch (property.PropertyType.FullName)
				{
					case "System.Boolean":
						property.SetValue(settings, Convert.ToBoolean(dictionary[setting]), null);
						break;
					case "System.String":
						property.SetValue(settings, dictionary[setting], null);
						break;
					case "System.Int32":
						property.SetValue(settings, Convert.ToInt32(dictionary[setting]), null);
						break;
					case "System.Double":
						property.SetValue(settings, Convert.ToDouble(dictionary[setting]), null);
						break;
					case "System.DateTime":
						property.SetValue(settings, Convert.ToDateTime(dictionary[setting]), null);
						break;
					default:
						throw new Exception($"Settings loader not coded to convert values of type {property.PropertyType.FullName}.");
				}
			}
		}
		_settings = settings;
	}

	public void Save(Settings settings)
	{
		_settings = settings;
		SaveCurrent();
	}

	public void SaveCurrent()
	{
		var dictionary = new Dictionary<string, object>();
		var properties = Current.GetType().GetProperties();
		foreach (var property in properties)
		{
			dictionary.Add(property.Name, property.GetValue(Current, null));
		}
		_settingsRepository.Save(dictionary);
	}
}