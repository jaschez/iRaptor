using System.Linq;
using System.Collections.Generic;

/*
 * Data structure which gets the output from Graph Generator
 * and transforms it into useful game world data.
 * 
 * Receives a mathematical graph input,
 * outputs an abstract game map.
 */
public class WorldGraphGenerator
{
    System.Random random;

    WorldGenerationParameters parameters;

    Dictionary<RoomType, int> keyRoomList;
    List<RootedNode> availableRooms;
    List<RootedNode> availableLeaves;
    List<RootedNode> gameRooms;

    GraphOutput graphOutput;
    Dictionary<RoomType, List<RootedNode>> filteredRooms;

    public WorldGraphGenerator(WorldGenerationParameters parameters)
    {
        this.parameters = parameters;

        random = new System.Random(parameters.GraphParameters.Seed);
        graphOutput = new GraphGenerator(parameters.GraphParameters).GenerateGraph();

        //Generate key room list
        keyRoomList = new Dictionary<RoomType, int>();

        keyRoomList.Add(RoomType.Reward, parameters.SpecialRoomList.Rewards);
        keyRoomList.Add(RoomType.Shop, parameters.SpecialRoomList.Shops);
        keyRoomList.Add(RoomType.Lore, parameters.SpecialRoomList.Lore);
        keyRoomList.Add(RoomType.Miniboss, parameters.SpecialRoomList.Minibosses);
    }

    public WorldGraphOutput GenerateWorldGraph()
    {
        filteredRooms = new Dictionary<RoomType, List<RootedNode>>();

        availableRooms = new List<RootedNode>(graphOutput.Nodes).OrderBy(node => node.Depth).ToList();
        availableLeaves = new List<RootedNode>(graphOutput.Leaves).OrderBy(node => node.Depth).ToList();
        gameRooms = new List<RootedNode>();

        LocateEntrance();
        LocateBoss();
        LocateMiniBosses();
        ExamineLoops();
        ExamineLeaves();
        ExamineRemainingNodes();
        FilterGraph();

        return new WorldGraphOutput(filteredRooms, graphOutput);
    }

    void LocateEntrance()
    {
        RootedNode entranceNode = availableRooms[0];
        availableRooms.Remove(entranceNode);

        entranceNode.SetRoomType(RoomType.Entrance);

        gameRooms.Add(entranceNode);
    }

    void LocateBoss()
    {
        RootedNode bossNode = graphOutput.DeepestLeaf;
        availableRooms.Remove(bossNode);
        availableLeaves.Remove(bossNode);

        bossNode.SetRoomType(RoomType.Boss);

        gameRooms.Add(bossNode);
    }

    void LocateMiniBosses()
    {
        int minibosses = keyRoomList[RoomType.Miniboss];

        int suitableLeafIndex = FindMinimumDepthIndex(availableLeaves, parameters.MinimumBossDepthFactor);
        int suitableRoomIndex = FindMinimumDepthIndex(availableRooms, parameters.MinimumBossDepthFactor);

        for (int i = 0; i < minibosses; i++)
        {
            RootedNode minibossNode;

            if (suitableLeafIndex != -1 && availableLeaves.Count > suitableLeafIndex)
            {
                minibossNode = availableLeaves[random.Next(suitableLeafIndex, availableLeaves.Count)];
                availableLeaves.Remove(minibossNode);
                availableRooms.Remove(minibossNode);
            }
            else
            {
                minibossNode = availableRooms[random.Next(suitableRoomIndex, availableRooms.Count)];
                availableLeaves.Remove(minibossNode);
                availableRooms.Remove(minibossNode);
            }

            minibossNode.SetRoomType(RoomType.Miniboss);
            gameRooms.Add(minibossNode);

            keyRoomList[RoomType.Miniboss]--;
        }

        if (keyRoomList[RoomType.Miniboss] == 0)
        {
            keyRoomList.Remove(RoomType.Miniboss);
        }
    }

    void ExamineLoops()
    {
        foreach (List<RootedNode> loop in graphOutput.Loops)
        {
            RootedNode keyRoom = null;

            int middleIndex = loop.Count / 2;
            int nearestIndex = -1;

            float randomResult = (float)(random.NextDouble() % 1);

            if (parameters.ChanceOfOneWayLoop < randomResult) {
                for (int i = 0; i < loop.Count; i++)
                {
                    RootedNode node = loop[i];

                    if (node.Childs.Count > 1 && i != 0 && i != loop.Count - 1)
                    {
                        int distance = System.Math.Abs(middleIndex - i);

                        if (distance < System.Math.Abs(middleIndex - nearestIndex))
                        {
                            nearestIndex = i;
                        }
                    }
                }

                if (System.Math.Abs(middleIndex - nearestIndex) <= loop.Count * .3f)
                {
                    foreach (RootedNode child in loop[nearestIndex].Childs)
                    {
                        if (child != loop[nearestIndex + 1])
                        {
                            keyRoom = child;
                        }
                    }
                }
                else
                {
                    keyRoom = loop[middleIndex];
                }
            }
            else
            {
                keyRoom = loop[loop.Count - 1];
            }

            //Decide on which room type should be set
            RoomType roomChoice = ChooseRoomType();
            keyRoom.SetRoomType(roomChoice);

            if (keyRoomList.ContainsKey(roomChoice)) {

                keyRoomList[roomChoice]--;

                if (keyRoomList[roomChoice] == 0)
                {
                    keyRoomList.Remove(roomChoice);
                }
            }

            availableRooms.Remove(keyRoom);

            if (availableLeaves.Contains(keyRoom)) {
                availableLeaves.Remove(keyRoom);
            }

            gameRooms.Add(keyRoom);
        }
    }

    void ExamineLeaves()
    {
        RootedNode leaf;

        RoomType roomChoice;

        int suitableLeafIndex = FindMinimumDepthIndex(availableLeaves, parameters.MinimumKeyRoomDepthFactor);
        int leafIndexChoice;

        int rewardsLeft = keyRoomList.ContainsKey(RoomType.Reward)? keyRoomList[RoomType.Reward] : 0;
        int shopsLeft = keyRoomList.ContainsKey(RoomType.Shop) ? keyRoomList[RoomType.Shop] : 0;

        while(availableLeaves.Count > 0 && (availableLeaves.Count - suitableLeafIndex) > 0 && rewardsLeft > 0 && suitableLeafIndex >= 0) {
            leafIndexChoice = random.Next(suitableLeafIndex, availableLeaves.Count);

            leaf = availableLeaves[leafIndexChoice];

            if (rewardsLeft > 0)
            {
                roomChoice = RoomType.Reward;
                rewardsLeft--;
            }
            else
            {
                roomChoice = RoomType.Shop;
                shopsLeft--;
            }

            leaf.SetRoomType(roomChoice);
            availableLeaves.Remove(leaf);
            availableRooms.Remove(leaf);

            keyRoomList[roomChoice]--;

            gameRooms.Add(leaf);
        }
    }

    void ExamineRemainingNodes()
    {
        int rewardsLeft = keyRoomList.ContainsKey(RoomType.Reward) ? keyRoomList[RoomType.Reward] : 0;
        int shopsLeft = keyRoomList.ContainsKey(RoomType.Shop) ? keyRoomList[RoomType.Shop] : 0;

        if (rewardsLeft + shopsLeft > 0)
        {
            int suitableKeyIndex = FindMinimumDepthIndex(availableRooms, parameters.MinimumKeyRoomDepthFactor);

            while (rewardsLeft + shopsLeft > 0)
            {
                RoomType roomChoice;
                RootedNode node;

                if (rewardsLeft > 0)
                {
                    roomChoice = RoomType.Reward;
                    rewardsLeft--;
                }
                else
                {
                    roomChoice = RoomType.Shop;
                    shopsLeft--;
                }

                do {
                    node = availableRooms[random.Next(suitableKeyIndex, availableRooms.Count)];
                } while (SeenInLoop(node));

                node.SetRoomType(roomChoice);

                keyRoomList[roomChoice]--;

                availableRooms.Remove(node);
                gameRooms.Add(node);
            }
        }

        //Hub rooms location
        List<RootedNode> hubList = new List<RootedNode>(availableRooms);
        int maxChilds;

        //Take the rooms with more childs
        hubList = hubList.OrderByDescending(room => room.Childs.Count).ToList();
        maxChilds = hubList[0].Childs.Count;

        while (hubList[hubList.Count - 1].Childs.Count != maxChilds)
        {
            hubList.RemoveAt(hubList.Count - 1);
        }

        //Order by depth and take a middle-depth room
        hubList = hubList.OrderByDescending(room => room.Depth).ToList();
        
        RootedNode hub = hubList[hubList.Count / 2];
        hub.SetRoomType(RoomType.Hub);

        gameRooms.Add(hub);
        availableRooms.Remove(hub);

        //Set normal room location

        foreach (RootedNode node in availableRooms)
        {
            //Could be chosen between normal and hub room
            node.SetRoomType(RoomType.Normal);
            gameRooms.Add(node);
        }
    }

    void FilterGraph()
    {
        foreach (RootedNode node in gameRooms)
        {
            if (!filteredRooms.ContainsKey(node.Type))
            {
                filteredRooms.Add(node.Type, new List<RootedNode>());
            }

            filteredRooms[node.Type].Add(node);
        }
    }

    RoomType ChooseRoomType()
    {
        RoomType roomChoice = RoomType.Null;

        int totalRooms = 0;
        int roomIndex = 0;
        int unfairFrequency;
        int indexChoice;

        foreach (RoomType key in keyRoomList.Keys)
        {
            totalRooms += keyRoomList[key];
        }

        unfairFrequency = (int)System.Math.Floor(totalRooms * parameters.ChanceOfUnfairness);
        indexChoice = random.Next(-unfairFrequency, totalRooms);

        if (indexChoice >= 0) {
            foreach (RoomType key in keyRoomList.Keys)
            {
                roomIndex += keyRoomList[key];

                if (roomIndex > indexChoice)
                {
                    roomChoice = key;
                }
            }
        }
        else
        {
            roomChoice = RoomType.Normal;
        }

        return roomChoice;
    }

    bool SeenInLoop(RootedNode node)
    {
        foreach (List<RootedNode> loop in graphOutput.Loops)
        {
            if (loop.Contains(node))
            {
                return true;
            }
        }

        return false;
    }

    int FindMinimumDepthIndex(List<RootedNode> nodeList, float depth)
    {
        int suitableIndex = -1;

        for (int i = 0; i < nodeList.Count && suitableIndex == -1; i++)
        {
            RootedNode node = nodeList[i];
            float depthFactor = node.Depth / (float)graphOutput.DeepestLeaf.Depth;

            if (depthFactor >= depth)
            {
                suitableIndex = i;
            }

        }

        return suitableIndex;
    }
}

public class RoomNode
{
    RoomType Type;
    List<RoomNode> Neighbours;

    public RoomNode(RoomType Type)
    {
        this.Type = Type;
    }
}