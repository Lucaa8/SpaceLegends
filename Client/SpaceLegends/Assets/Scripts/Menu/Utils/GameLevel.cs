using Newtonsoft.Json.Linq;
using System;
using UnityEditor;

public class GameLevel
{

	public enum Difficulty
	{
		EASY, NORMAL, HARD
	}

	private string _collection;

	public int CollectionNo
	{
		get
		{
			int.TryParse(_collection.Split("_")[0], out int x);
            return x; 
		}
	}

	public string CollectionName
	{
		get
		{
			return _collection.Split("_")[1];
        }
	}

	public int LevelID { get; private set; }

    public int UnlockRequirements { get; private set; }

    public Difficulty LevelDifficulty { get; private set; }

	public int Level { get; private set; }

	public float[] Probabilities { get; private set; }
	
	public int[] Progression { get; private set; }

	public GameLevel(JObject json)
	{
		_collection = json.Value<string>("collection");
		LevelID = json.Value<int>("id");
		Enum.TryParse(json.Value<string>("difficulty"), out Difficulty difficulty);
		LevelDifficulty = difficulty;
		Level = json.Value<int>("level");
		UnlockRequirements = json.Value<int>("unlock_requirements");
		if(json.ContainsKey("progress"))
		{
            JObject progress = json.Value<JObject>("progress");
            Progression = new int[] { progress.Value<int>("completions"), progress.Value<int>("kills"), progress.Value<int>("deaths"), progress.Value<int>("stars") };
        }
        else
        {
            Progression = new int[] { 0, 0, 0, 0 };
        }
        JArray proba = json.Value<JArray>("probabilities");
        Probabilities = new float[] { proba[0].Value<float>(), proba[1].Value<float>(), proba[2].Value<float>(), proba[3].Value<float>() };
    }
    public int Games
    {
        get
        {
            return Progression[0];
        }
    }
    public int Kills
    {
        get
        {
            return Progression[1];
        }
    }

    public int Deaths
    {
        get
        {
            return Progression[2];
        }
    }

    public int Stars
    {
        get
        {
            return Progression[3];
        }
    }

}
