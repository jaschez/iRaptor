using System.Collections;
using System.Collections.Generic;

/*
 * Data structure that represents the rooms of a level
 * and the connections between them.
 * 
 * Will be used later by the World Generator to position
 * each room in an specific position in the world
 */
public class WorldGraph
{
    public Node Start;

    System.Random random;

    //The number of each corresponding room must be specified before
    Dictionary<RoomType, int> remainingRooms = new Dictionary<RoomType, int>();

    int seed;

    int maximumLoops = 3;
    int maximumBranchDepth = 3;

    int loopMaximumLength = 6;
    int loopMinimumLength = 3;

    int minimumNormalRooms = 10;
    int maximumNormalRooms = 13;

    int minimumBossDepth = 3;
    int minimumMinibossDepth = 4;
    int minimumShopDepth = 4;
    int minimumRewardDepth = 4;

    int normalRoomNumber = 0;
    int currentLoopIndex = 0;
    int currentLoopLength = 0;
    int currentDepth = 1;

    float splitProbability = 0.2f;
    float tripleSplitProbability = 0.2f;
    float loopProbability = 0.3f;
    float rewardInLoopProbability = 0.5f;
    float shopInLoopProbability = 0.5f;

    bool inLoop = false;

    //TODO: Probar que funciona: Init y luego Generate
    //Usar variables InLoopProbability?
    //Hay habitaciones que quedan sin colocarse
    //Despues visualizar
    //Después, asegurarse de que están todos los elementos,
    //usando para ello la búsqueda por profundidad del nodo
    //con menos depth y añadirle a este los elementos restantes.

    public void Init()
    {
        seed = 1728799526;//new System.Random().Next();
        random = new System.Random(seed); //There should have a seed
        normalRoomNumber = random.Next(minimumNormalRooms, maximumNormalRooms);
    }

    public int GetSeed()
    {
        return seed;
    }

    public void Generate()
    {
        Stack<Node> stack = new Stack<Node>();

        Node currentLoopParent = null;

        int currentBranchDepth = 0;
        int numberOfLoops = 0;

        remainingRooms.Add(RoomType.Normal, normalRoomNumber);
        remainingRooms.Add(RoomType.Reward, 2);
        remainingRooms.Add(RoomType.Shop, 1);
        remainingRooms.Add(RoomType.Miniboss, 1);
        remainingRooms.Add(RoomType.Boss, 1);

        Start = new Node(RoomType.Entrance, currentDepth, false);

        stack.Push(Start);

        while (stack.Count > 0)
        {
            Node currentNode = stack.Pop();

            currentDepth = currentNode.Depth;

            if (GetProbabilityResult(splitProbability))
            {
                //Split the generation in 2 or 3 childs
                int maxChilds;

                //We put 1 or 2 because they will be enqueued now, unlike the child from a loop or the child from the current branch.
                if (GetProbabilityResult(tripleSplitProbability))
                {
                    maxChilds = 2;
                }
                else
                {
                    maxChilds = 1;
                }

                //We have to assume that next childs (generated in a split) are not suitable for loops,
                //so when we choose for a possible room type, the method assumes the possibility of not being in an adequate scenario
                bool currentLoopState = inLoop;
                inLoop = false;

                for (int i = 0; i < maxChilds; i++)
                {
                    AddNewChild(stack, currentNode);
                }

                //Then, we recover the last loop state
                inLoop = currentLoopState;
                currentBranchDepth = 0;
            }

            if (!inLoop && numberOfLoops < maximumLoops && GetProbabilityResult(loopProbability))
            {
                currentLoopLength = random.Next(loopMinimumLength, loopMaximumLength);
                currentLoopIndex = 0;
                currentBranchDepth = 0;
                currentNode.BelongToLoop(true);
                currentLoopParent = currentNode;
                numberOfLoops++;

                inLoop = true;

                AddNewChild(stack, currentNode);

                currentLoopIndex++;

            }
            else if(inLoop)
            {
                if (currentLoopIndex == currentLoopLength)
                {
                    currentNode.AddChild(currentLoopParent);
                    inLoop = false;
                }
                else
                {
                    AddNewChild(stack, currentNode);
                    currentLoopIndex++;
                }
            }
            else
            {
                //Add 1 to the depth of the current branch
                if (currentBranchDepth < maximumBranchDepth)
                {
                    Node child = AddNewChild(stack, currentNode);

                    if (child != null) {
                        if (child.roomType != RoomType.Miniboss && child.roomType != RoomType.Boss) {
                            currentBranchDepth++;
                        }
                        else
                        {
                            currentBranchDepth = 0;
                        }
                    }
                }
                else
                {
                    currentBranchDepth = 0;
                }
            }
        }
    }

    Node AddNewChild(Stack<Node> stack, Node currentNode)
    {
        //Choose a type of room
        RoomType chosenType = ChoosePossibleRoomType();
        Node newNode = null;

        if (chosenType != RoomType.Null) {

            if (!inLoop)
            {
                newNode = new Node(chosenType, currentNode.Depth + 1, inLoop);

                if (chosenType != RoomType.Miniboss && chosenType != RoomType.Boss)
                {
                    stack.Push(newNode);
                }
            }
            else
            {
                if (currentNode.LoopID == -1)
                {
                    newNode = new Node(chosenType, currentNode.Depth + 1, inLoop);
                    newNode.CalculateLoopID();
                }
                else
                {
                    newNode = new Node(chosenType, currentNode.Depth + 1, inLoop, currentNode.LoopID);
                }

                stack.Push(newNode);
            }

            currentNode.AddChild(newNode);
        }

        return newNode;
    }

    List<RoomType> CheckForAvailableRooms() {
        List<RoomType> rooms = new List<RoomType>();

        foreach (RoomType roomType in remainingRooms.Keys)
        {
            switch (roomType)
            {
                case RoomType.Normal:
                    rooms.Add(roomType);
                    break;

                case RoomType.Shop:
                    if (inLoop) {
                        if (currentLoopIndex == currentLoopLength - 1) {
                            rooms.Add(roomType);
                        }
                    }
                    else if(currentDepth >= minimumShopDepth){
                        rooms.Add(roomType);
                    }
                    break;

                case RoomType.Reward:
                    if (inLoop)
                    {
                        if (currentLoopIndex == currentLoopLength - 1)
                        {
                            rooms.Add(roomType);
                        }
                    }
                    else if (currentDepth >= minimumRewardDepth)
                    {
                        rooms.Add(roomType);
                    }
                    break;

                case RoomType.Miniboss:
                    if (currentDepth >= minimumMinibossDepth && !inLoop)
                    {
                        rooms.Add(roomType);
                    }
                    break;

                case RoomType.Boss:
                    if (currentDepth >= minimumBossDepth && !inLoop)
                    {
                        rooms.Add(roomType);
                    }
                    break;

                case RoomType.Lore:
                    if (!inLoop) {
                        rooms.Add(roomType);
                    }
                    break;

                default:
                    break;
            }
        }

        return rooms;
    }

    //With the actual conditions, choose one of the possible room types;
    RoomType ChoosePossibleRoomType()
    {
        List<RoomType> currentPossibleTypes = CheckForAvailableRooms();

        RoomType chosenType = RoomType.Null;

        if (currentPossibleTypes.Count > 0)
        {
            int chosenIndex = random.Next(0, currentPossibleTypes.Count);

            chosenType = currentPossibleTypes[chosenIndex];

            remainingRooms[chosenType]--;

            if (remainingRooms[chosenType] == 0)
            {
                remainingRooms.Remove(chosenType);
            }
        }

        return chosenType;
    }

    bool GetProbabilityResult(float probability)
    {
        return random.Next(0, 100) < probability * 100;
    }
}

public class Node
{
    public List<Node> Childs { get; private set; }
    public int ID { get; private set; }
    public int Depth { get; private set; }

    public int LoopID { get; private set; }
    public bool FromLoop { get; private set; }

    public RoomType roomType;

    private static int IDCount = 0;
    private static int LoopCount = 0;

    public Node(RoomType roomType, int depth, bool fromLoop, int loopID = -1){
        this.roomType = roomType;
        this.FromLoop = fromLoop;
        LoopID = loopID;
        Depth = depth;
        ID = IDCount;
        IDCount++;
    }

    public void BelongToLoop(bool fromLoop)
    {
        this.FromLoop = fromLoop;
    }

    public void CalculateLoopID()
    {
        LoopID = LoopCount;
        LoopCount++;
    }

    public void AddChild(Node child)
    {
        if (Childs == null)
        {
            Childs = new List<Node>();
        }

        Childs.Add(child);
    }
}
