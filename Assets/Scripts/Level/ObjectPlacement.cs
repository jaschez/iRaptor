using System;
using System.Collections.Generic;

//Code based on Sebastian Lague's Poisson Disk Sampling Implementation
public class ObjectPlacement
{

	Random random;

	readonly TileType[,] map;

	List<Coord> points;
	List<Coord> spawnPoints;

	List<int> validCoordsID;

	int[,] flagGrid;

	int width;
	int height;

    public ObjectPlacement(int seed, TileType[,] map, List<Coord> validCoords, Coord origin)
    {
		random = new Random(seed);

		this.map = map;

		width = map.GetLength(0);
		height = map.GetLength(1);

		flagGrid = new int[width, height];

		points = new List<Coord>();
		spawnPoints = new List<Coord>();

		validCoordsID = new List<int>();

        foreach (Coord c in validCoords)
        {
			validCoordsID.Add(GetCoordID(c.x, c.y));
        }

		//Add initial spawn point
		spawnPoints.Add(origin);
	}

	public Coord GenerateNextPoint(int radius, int numSamplesBeforeRejection = 50)
	{	
		Coord candidate = new Coord(0, 0);

		if (spawnPoints.Count > 0)
		{
			int spawnIndex = random.Next(0, spawnPoints.Count);
			Coord spawnCentre = spawnPoints[spawnIndex];
			bool candidateAccepted = false;

			for (int i = 0; i < numSamplesBeforeRejection; i++)
			{
				double angle = (random.NextDouble() % 1) * Math.PI * 2f;
				double distance = random.NextDouble() % (2 * radius) + radius; 
				float dX = (float)Math.Sin(angle);
				float dY = (float)Math.Cos(angle);

				int candidateX = (int)(spawnCentre.x + dX * distance);
				int candidateY = (int)(spawnCentre.y + dY * distance);

				candidate = new Coord(candidateX, candidateY);

				if (IsValid(candidate, radius))
				{
					points.Add(candidate);
					spawnPoints.Add(candidate);
					flagGrid[candidate.x, candidate.y] = radius;
					candidateAccepted = true;
					break;
				}
			}

			if (!candidateAccepted)
			{
				spawnPoints.RemoveAt(spawnIndex);
			}
		}

		return candidate;
	}

	bool IsValid(Coord candidate, int radius)
	{
		if (candidate.x >= 0 && candidate.x < width && candidate.y >= 0 && candidate.y < height)
		{
			int coordID = GetCoordID(candidate.x, candidate.y);
			int neighbourID;

			if (validCoordsID.Contains(coordID)) {
				int searchStartX = Math.Max(0, candidate.x - radius);
				int searchEndX = Math.Min(candidate.x + radius, width - 1);
				int searchStartY = Math.Max(0, candidate.y - radius);
				int searchEndY = Math.Min(candidate.y + radius, height - 1);

				for (int x = searchStartX; x <= searchEndX; x++)
				{
					for (int y = searchStartY; y <= searchEndY; y++)
					{
						neighbourID = GetCoordID(x, y);

						if (flagGrid[x, y] > 0 || !validCoordsID.Contains(neighbourID))
						{
							int neighbourRadius = flagGrid[x, y];
							float sqrDst = (float)(Math.Pow(candidate.x - x, 2) + Math.Pow(candidate.y - y, 2));

							if (sqrDst < radius * radius || sqrDst < neighbourRadius * neighbourRadius)
							{
								return false;
							}
						}
					}
				}
				return true;
			}
		}
		return false;
	}

	public int GetCoordID(int x, int y)
	{
		return int.MinValue + (int)(Math.Pow(2, x) * (2 * y + 1));
	}
}
