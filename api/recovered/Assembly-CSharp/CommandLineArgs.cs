using System.Collections.Generic;
using System.Text;

public class CommandLineArgs
{
	private List<string> switches = new List<string>();

	private Dictionary<string, string> options = new Dictionary<string, string>();

	public CommandLineArgs(string[] args)
	{
		Init(args);
	}

	public void Init(string[] args)
	{
		for (int i = 0; i < args.Length; i++)
		{
			int num = i + 1;
			if (!args[i].StartsWith("-"))
			{
				continue;
			}
			if (num < args.Length)
			{
				if (args[num].StartsWith("-"))
				{
					switches.Add(args[i]);
					continue;
				}
				options.Add(args[i], args[i + 1]);
				i++;
			}
			else
			{
				switches.Add(args[i]);
			}
		}
	}

	public string GetOption(string name, string defaultValue)
	{
		if (!options.TryGetValue(name, out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public bool GetSwitch(string name, bool defaultValue)
	{
		if (string.IsNullOrEmpty(switches.Find((string _sw) => _sw == name)))
		{
			return defaultValue;
		}
		return true;
	}

	public string Dump()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string @switch in switches)
		{
			stringBuilder.AppendLine(@switch);
		}
		foreach (KeyValuePair<string, string> option in options)
		{
			stringBuilder.AppendLine(option.Key + ": " + option.Value);
		}
		return stringBuilder.ToString();
	}
}
