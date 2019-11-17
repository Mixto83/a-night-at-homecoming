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

    [SerializeField] private bool forceDoorAttended = false;
    [SerializeField] private bool doorAttended = false;
    [SerializeField] private bool forceBarAttended = false;
    [SerializeField] private bool barAttended = false;

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
        foreach (int i in friendsGroups)
        {
            groups.Add(new Group(i));
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
            if(c.getRole() == Roles.CalmStudent )
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
            c.Move(new Vector3(0, 1 - queueNum + i));
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
