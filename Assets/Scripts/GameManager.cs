using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public enum Roles { CalmStudent, MessyStudent, Teacher, OrganizerStudent }
public enum Genders { Male, Female, Other }

public class GameManager : MonoBehaviour
{
    protected readonly int MALENAMES_NUM = 7;
    protected readonly int FOODS_NUM = 10;
    protected readonly int ANIMALS_NUM = 12;
    protected readonly int HOBBIES_NUM = 12;

    [SerializeField] bool paused = false;

    private bool forceDoorAttended = false;
    private bool doorAttended = false;
    private bool forceBarAttended = false;
    private bool barAttended = false;

    [SerializeField] List<GameObject> Agents;
    List<Character> People;
    [SerializeField] bool debugMode;
    [SerializeField] GameObject door;
    [SerializeField] GameObject bar;

    string[] Names = new string[] { "Diego", "Mario", "Juan", "Cesar", "Daniel", "Pedro", "Manuel", "Maria", "Marta", "Carmen", "Raquel", "Lucia", "Ana", "Laura" };
    string[] Hobbies = new string[] { "Comic books", "Videogames", "Movies", "Books", "Cooking", "Board games", "Trading card games", "Sports", "School shooting", "Music", "Dancing", "Gym" };
    string[] Animals = new string[] { "Dog", "Cat", "Bird", "Bee", "Sheep", "Whale", "Possum", "Crocodile", "Bat", "Spider", "Lizard", "Turtle" };
    string[] Foods = new string[] { "Hamburgers", "Pizza", "Pasta", "Sandwich", "Fish", "Eggs", "Salad", "Chocolate", "Children", "Apple" };

    [SerializeField] List<int> friendsGroups;
    List<Group> groups;
    List<Character> queue;

    [HideInInspector] public List<float> possiblePositionsGym;
    [HideInInspector] public List<float> limitedPossiblePosGym;

    private int queueNum;

    private void Awake()
    {
        Agents = new List<GameObject>();
        People = new List<Character>();
        groups = new List<Group>();
        queue = new List<Character>();

        Interlocked.Add(ref queueNum, -1);
    }

    // Start is called before the first frame update
    void Start()
    {
        //possiblePositionsGym = new List<float> {-10f,7f,  -9f,7f,  -8f,7f,  -7f,7f,  -6f,7f,  -5f,7f,  -4f,7f,  -3f,7f,  -2f,7f,                                       /*Escenario*/                                      10f,7f,  11f,7f,  12f,7f,  13f,7f,  14f,7f,  15f,7f,  16f,7f,  17f,7f,  18f,7f,
        //                                    -10f,6f,  -9f,6f,  -8f,6f,  -7f,6f,  -6f,6f,  -5f,6f,  -4f,6f,  -3f,6f,  -2f,6f,                                        /*Escenario*/                                      10f,6f,  11f,6f,  12f,6f,  13f,6f,  14f,6f,  15f,6f,  16f,6f,  17f,6f,  18f,6f,
        //                                    -10f,5f,  -9f,5f,  -8f,5f,  -7f,5f,  -6f,5f,  -5f,5f,  -4f,5f,  -3f,5f,  -2f,5f,                                        /*Escenario*/                                      10f,5f,  11f,5f,  12f,5f,  13f,5f,  14f,5f,  15f,5f,  16f,5f,  17f,5f,  18f,5f,
        //                                    -10f,4f,  -9f,4f,  -8f,4f,  -7f,4f,  -6f,4f,  -5f,4f,  -4f,4f,  -3f,4f,  -2f,4f,                                        /*Escenario*/                                      10f,4f,  11f,4f,  12f,4f,  13f,4f,  14f,4f,  15f,4f,  16f,4f,  17f,4f,  18f,4f,
        //                                    -10f,3f,  -9f,3f,  -8f,3f,  -7f,3f,  -6f,3f,  -5f,3f,  -4f,3f,  -3f,3f,  -2f,3f,                                        /*Escenario*/                                      10f,3f,  11f,3f,  12f,3f,  13f,3f,  14f,3f,  15f,3f,  16f,3f,  17f,3f,  18f,3f,
        //                                    -10f,2f,  -9f,2f,  -8f,2f,  -7f,2f,  -6f,2f,  -5f,2f,  -4f,2f,  -3f,2f,  -2f,2f,  -1f,2f,  0f,2f,  1f,2f,  2f,2f,  3f,2f,  4f,2f,  5f,2f,  6f,2f,  7f,2f,  8f,2f,  9f,2f,  10f,2f,  11f,2f,  12f,2f,  13f,2f,  14f,2f,  15f,2f,  16f,2f,  17f,2f,  18f,2f,
        //                                    -10f,1f,  -9f,1f,  -8f,1f,  -7f,1f,  -6f,1f,  -5f,1f,  -4f,1f,  -3f,1f,  -2f,1f,  -1f,1f,  0f,1f,  1f,1f,  2f,1f,  3f,1f,  4f,1f,  5f,1f,  6f,1f,  7f,1f,  8f,1f,  9f,1f,  10f,1f,  11f,1f,  12f,1f,  13f,1f,  14f,1f,  15f,1f,  16f,1f,  17f,1f,  18f,1f,
        //                                    -10f,0f,  -9f,0f,  -8f,0f,  -7f,0f,  -6f,0f,  -5f,0f,  -4f,0f,  -3f,0f,  -2f,0f,  -1f,0f,  0f,0f,  1f,0f,  2f,0f,  3f,0f,  4f,0f,  5f,0f,  6f,0f,  7f,0f,  8f,0f,  9f,0f,  10f,0f,  11f,0f,  12f,0f,  13f,0f,  14f,0f,  15f,0f,  16f,0f,  17f,0f,  18f,0f,
        //                                    -10f,-1f, -9f,-1f, -8f,-1f, -7f,-1f, -6f,-1f, -5f,-1f, -4f,-1f, -3f,-1f, -2f,-1f, -1f,-1f, 0f,-1f, 1f,-1f, 2f,-1f, 3f,-1f, 4f,-1f, 5f,-1f, 6f,-1f, 7f,-1f, 8f,-1f, 9f,-1f, 10f,-1f, 11f,-1f, 12f,-1f, 13f,-1f, 14f,-1f, 15f,-1f, 16f,-1f, 17f,-1f, 18f,-1f,
        //                                    -10f,-2f, -9f,-2f, -8f,-2f, -7f,-2f, -6f,-2f, -5f,-2f, -4f,-2f, -3f,-2f, -2f,-2f, -1f,-2f, 0f,-2f, 1f,-2f, 2f,-2f, 3f,-2f, 4f,-2f, 5f,-2f, 6f,-2f, 7f,-2f, 8f,-2f, 9f,-2f, 10f,-2f, 11f,-2f, 12f,-2f, 13f,-2f, 14f,-2f, 15f,-2f, 16f,-2f,   /*Barra*/
        //                                    -10f,-3f, -9f,-3f, -8f,-3f, -7f,-3f, -6f,-3f, -5f,-3f, -4f,-3f, -3f,-3f, -2f,-3f, -1f,-3f, 0f,-3f, 1f,-3f, 2f,-3f, 3f,-3f, 4f,-3f, 5f,-3f, 6f,-3f, 7f,-3f, 8f,-3f, 9f,-3f,                          /*Cola barra*/                          /*Barra*/
        //                                    -10f,-4f, -9f,-4f, -8f,-4f, -7f,-4f, -6f,-4f, -5f,-4f, -4f,-4f, -3f,-4f, -2f,-4f, -1f,-4f, 0f,-4f, 1f,-4f, 2f,-4f, 3f,-4f, 4f,-4f, 5f,-4f, 6f,-4f, 7f,-4f, 8f,-4f, 9f,-4f, 10f,-4f, 11f,-4f, 12f,-4f, 13f,-4f, 14f,-4f, 15f,-4f, 16f,-4f,   /*Barra*/
        //                                    -10f,-5f, -9f,-5f, -8f,-5f, -7f,-5f, -6f,-5f, -5f,-5f, -4f,-5f, -3f,-5f, -2f,-5f, -1f,-5f, 0f,-5f, 1f,-5f, 2f,-5f, 3f,-5f, 4f,-5f, 5f,-5f, 6f,-5f, 7f,-5f, 8f,-5f, 9f,-5f, 10f,-5f, 11f,-5f, 12f,-5f, 13f,-5f, 14f,-5f, 15f,-5f, 16f,-5f, 17f,-5f, 18f,-5f,
        //                                    -10f,-6f, -9f,-6f, -8f,-6f, -7f,-6f, -6f,-6f, -5f,-6f, -4f,-6f,         /*Puerta*/         0f,-6f, 1f,-6f, 2f,-6f, 3f,-6f, 4f,-6f, 5f,-6f, 6f,-6f, 7f,-6f, 8f,-6f, 9f,-6f, 10f,-6f, 11f,-6f, 12f,-6f, 13f,-6f, 14f,-6f, 15f,-6f, 16f,-6f, 17f,-6f, 18f,-6f,
        //};

        limitedPossiblePosGym = new List<float> {
                                            -9f,6f,  -8f,6f,  -7f,6f,  -6f,6f,  -5f,6f,  -4f,6f,  -3f,6f,                                                 /*Escenario*/                                               11f,6f,  12f,6f,  13f,6f,  14f,6f,  15f,6f,  16f,6f,  17f,6f,  18f,6f,
                                            -9f,5f,  -8f,5f,  -7f,5f,  -6f,5f,  -5f,5f,  -4f,5f,  -3f,5f,                                                 /*Escenario*/                                               11f,5f,  12f,5f,  13f,5f,  14f,5f,  15f,5f,  16f,5f,  17f,5f,  18f,5f,
                                            -9f,4f,  -8f,4f,  -7f,4f,  -6f,4f,  -5f,4f,  -4f,4f,  -3f,4f,                                                 /*Escenario*/                                               11f,4f,  12f,4f,  13f,4f,  14f,4f,  15f,4f,  16f,4f,  17f,4f,  18f,4f,
                                            -9f,3f,  -8f,3f,  -7f,3f,  -6f,3f,  -5f,3f,  -4f,3f,  -3f,3f,                                                 /*Escenario*/                                               11f,3f,  12f,3f,  13f,3f,  14f,3f,  15f,3f,  16f,3f,  17f,3f,  18f,3f,
                                            -9f,2f,  -8f,2f,  -7f,2f,  -6f,2f,  -5f,2f,  -4f,2f,  -3f,2f,                                                 /*Escenario*/                                               11f,2f,  12f,2f,  13f,2f,  14f,2f,  15f,2f,  16f,2f,  17f,2f,  18f,2f,
                                            -9f,1f,  -8f,1f,  -7f,1f,  -6f,1f,  -5f,1f,  -4f,1f,  -3f,1f,  -2f,1f,  -1f,1f,  0f,1f,  1f,1f,  2f,1f,  3f,1f,  4f,1f,  5f,1f,  6f,1f,  7f,1f,  8f,1f,  9f,1f,  10f,1f,  11f,1f,  12f,1f,  13f,1f,  14f,1f,  15f,1f,  16f,1f,  17f,1f,  18f,1f,
                                            -9f,0f,  -8f,0f,  -7f,0f,  -6f,0f,  -5f,0f,  -4f,0f,  -3f,0f,  -2f,0f,  -1f,0f,  0f,0f,  1f,0f,  2f,0f,  3f,0f,  4f,0f,  5f,0f,  6f,0f,  7f,0f,  8f,0f,  9f,0f,  10f,0f,  11f,0f,  12f,0f,  13f,0f,  14f,0f,  15f,0f,  16f,0f,  17f,0f,  18f,0f,
                                            -9f,-1f, -8f,-1f, -7f,-1f, -6f,-1f, -5f,-1f, -4f,-1f, -3f,-1f, -2f,-1f, -1f,-1f, 0f,-1f, 1f,-1f, 2f,-1f, 3f,-1f, 4f,-1f, 5f,-1f, 6f,-1f, 7f,-1f, 8f,-1f, 9f,-1f, 10f,-1f, 11f,-1f, 12f,-1f, 13f,-1f, 14f,-1f, 15f,-1f,          /*Barra*/
                                            -9f,-2f, -8f,-2f, -7f,-2f, -6f,-2f, -5f,-2f, -4f,-2f, -3f,-2f, -2f,-2f, -1f,-2f, 0f,-2f, 1f,-2f, 2f,-2f, 3f,-2f, 4f,-2f, 5f,-2f, 6f,-2f, 7f,-2f, 8f,-2f,                                /*Cola barra*/                          /*Barra*/
                                            -9f,-3f, -8f,-3f, -7f,-3f, -6f,-3f, -5f,-3f, -4f,-3f, -3f,-3f, -2f,-3f, -1f,-3f, 0f,-3f, 1f,-3f, 2f,-3f, 3f,-3f, 4f,-3f, 5f,-3f, 6f,-3f, 7f,-3f, 8f,-3f,                                /*Cola barra*/                          /*Barra*/
                                            -9f,-4f, -8f,-4f, -7f,-4f, -6f,-4f, -5f,-4f, -4f,-4f, -3f,-4f, -2f,-4f, -1f,-4f, 0f,-4f, 1f,-4f, 2f,-4f, 3f,-4f, 4f,-4f, 5f,-4f, 6f,-4f, 7f,-4f, 8f,-4f,                                /*Cola barra*/                          /*Barra*/
                                            -9f,-5f, -8f,-5f, -7f,-5f, -6f,-5f, -5f,-5f,                  /*Puerta*/                 1f,-5f, 2f,-5f, 3f,-5f, 4f,-5f, 5f,-5f, 6f,-5f, 7f,-5f, 8f,-5f, 9f,-5f, 10f,-5f, 11f,-5f, 12f,-5f, 13f,-5f, 14f,-5f, 15f,-5f,          /*Barra*/
        };

        foreach (int i in friendsGroups)
        {
            var index = Random.Range(0, limitedPossiblePosGym.Count / 2 - 1) * 2;
            var randomCoordinates = limitedPossiblePosGym.GetRange(index, 2);
            limitedPossiblePosGym.RemoveRange(index, 2);
            groups.Add(new Group(i, randomCoordinates));
        }

        foreach (Transform child in transform)
        {
            Agents.Add(child.gameObject);

            Genders gender = (Genders) Random.Range(0, 3);

            string name;
            switch (gender)
            {
                case Genders.Male:
                    name = Names[Random.Range(0, MALENAMES_NUM)];
                    break;
                case Genders.Female:
                    name = Names[Random.Range(MALENAMES_NUM, Names.Length)];
                    break;
                default:
                    name = Names[Random.Range(0, Names.Length)];
                    break;
            }

            string[] hobbies = new string[] { };
            string[] animals = new string[] { };
            string[] foods = new string[] { };

            switch (child.tag)
            {
                case "CalmStudent":
                    CalmStudent newStudent = new CalmStudent(name, gender, child, this);
                    for (int i = 0; i < 3; i++)
                    {
                        string newHobbie = Hobbies[Random.Range(0, HOBBIES_NUM)];
                        string newAnimal = Animals[Random.Range(0, ANIMALS_NUM)];
                        string newFood = Foods[Random.Range(0, FOODS_NUM)];

                        //Should be checked to avoid duplicates!
                        newStudent.Hobbies.Add(newHobbie);
                        newStudent.FavAnimals.Add(newAnimal);
                        newStudent.FavFoods.Add(newFood);
                    }
                    
                    var j = 0;
                    foreach (Group g in groups)
                    {
                        j++;
                        if (g.pushFriend(newStudent))
                        {
                            newStudent.setGroup(g);
                            break;
                        }

                        if(j >= groups.Count)
                        {
                            Debug.Log("[Error] All groups are full");
                        }
                    }
                    People.Add(newStudent);

                    break;
                case "MessyStudent":
                    MessyStudent newStudent2 = new MessyStudent(name, gender, child, this);
                    for (int i = 0; i < 3; i++)
                    {
                        string newHobbie = Hobbies[Random.Range(0, HOBBIES_NUM)];
                        string newAnimal = Animals[Random.Range(0, ANIMALS_NUM)];
                        string newFood = Foods[Random.Range(0, FOODS_NUM)];

                        //Should be checked to avoid duplicates!
                        newStudent2.Hobbies.Add(newHobbie);
                        newStudent2.FavAnimals.Add(newAnimal);
                        newStudent2.FavFoods.Add(newFood);
                    }
                    People.Add(newStudent2);
                    break;
                case "Teacher":
                    Teacher newTeacher = new Teacher(name, gender, child, this);
                    People.Add(newTeacher);
                    break;
                case "OrgStudent":
                    OrganizerStudent newOrgStudent = new OrganizerStudent(name, gender, child, this);
                    People.Add(newOrgStudent);
                    break;
            }
        }
        foreach (Group g in groups) {
            Debug.Log(g.toString());
        }
        foreach (Character c in People)
        {
            c.CreateStateMachine();
            if(c.getRole() == Roles.CalmStudent)
                Debug.Log(c.getName());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!paused)
        {
            var changeDoorState = false;
            var changeBarState = false;

            foreach (Character character in People)
            {
                character.Update();
                character.lockCanvasRotation();
                character.RotationUpdate();

                if (character.isInState("Door"))
                {
                    changeDoorState = true;
                }

                if (character.isInState("Serve Drink"))
                {
                    changeBarState = true;
                }
            }

            if (forceDoorAttended)
            {
                doorAttended = true;
            } else
            {
                doorAttended = changeDoorState;
            }

            if (forceBarAttended)
            {
                barAttended = true;
            }
            else
            {
                barAttended = changeBarState;
            }
        }
    }

    public bool getDoorAttended()
    {
        return doorAttended;
    }

    public bool getBarAttended()
    {
        return barAttended;
    }

    public float getQueueNum()
    {
        return (float) queueNum;
    }

    public float getBarQueue(Character client)
    {
        queue.Add(client);
        return (float) Interlocked.Increment(ref queueNum);
    }

    public void reduceBarQueue(Character client)
    {
        var i = queue.Count;
        foreach(Character c in queue)
        {
            c.Move(new Vector3(GameObject.FindGameObjectWithTag("Bar").transform.position.x - 0.75f - queueNum + i, GameObject.FindGameObjectWithTag("Bar").transform.position.y));
            i--;
        }
        Interlocked.Decrement(ref queueNum);
        queue.Remove(client);
    }

    public Character GetCharacter(GameObject obj)
    {
        return People[Agents.IndexOf(obj)];
    }

    public List<Character> GetPeople()
    {
        return People;
    }
}
