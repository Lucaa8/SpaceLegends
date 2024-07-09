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
	
	public int Kills { get; private set; }

    public int Deaths { get; private set; }

    public int Games { get; private set; }

    public int Completions { get; private set; }

    public bool[] Stars { get; private set; }

	public int StarsCount
	{
		get
		{
			int count = 0;
            foreach (bool item in Stars)
            {
				if (item) count++;
            }
			return count;
        }
	}

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
            Games = progress.Value<int>("games");
            Completions = progress.Value<int>("completions");
			Kills = progress.Value<int>("kills");
			Deaths = progress.Value<int>("deaths");
			JObject stars = progress.Value<JObject>("stars");
			Stars = new bool[] { stars.Value<bool>("star_1"), stars.Value<bool>("star_2"), stars.Value<bool>("star_3") };
        }
        else
        {
			Games = 0;
			Completions = 0;
			Kills = 0;
			Deaths = 0;
			Stars = new bool[] { false, false, false };
        }
        JArray proba = json.Value<JArray>("probabilities");
        Probabilities = new float[] { proba[0].Value<float>(), proba[1].Value<float>(), proba[2].Value<float>(), proba[3].Value<float>() };
    }

}
