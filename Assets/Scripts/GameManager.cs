using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Roles { CalmStudent, MessyStudent, Teacher, OrganizerStudent }
public enum Genders { Male, Female, Other }

public class GameManager : MonoBehaviour
{
    protected readonly int MALENAMES_NUM = 7;
    protected readonly int FOODS_NUM = 10;
    protected readonly int ANIMALS_NUM = 12;
    protected readonly int HOBBIES_NUM = 12;

    [SerializeField] private bool forceDoorAttended = false;
    [SerializeField] private bool doorAttended = false;
    [SerializeField] private bool barAttended = false;

    [SerializeField] bool paused = false;

    [SerializeField] List<GameObject> Agents;
    List<Character> People;
    [SerializeField] bool debugMode;
    [SerializeField] GameObject door;
    [SerializeField] GameObject bar;

    string[] Names = new string[] { "Diego", "Mario", "Juan", "Cesar", "Daniel", "Pedro", "Manuel", "Maria", "Marta", "Carmen", "Raquel", "Lucia", "Ana", "Laura" };
    string[] Hobbies = new string[] { "Comic books", "Videogames", "Movies", "Books", "Cooking", "Board games", "Trading card games", "Sports", "School shooting", "Music", "Dancing", "Gym" };
    string[] Animals = new string[] { "Dog", "Cat", "Bird", "Bee", "Sheep", "Whale", "Possum", "Crocodile", "Bat", "Spider", "Lizard", "Turtle" };
    string[] Foods = new string[] { "Hamburgers", "Pizza", "Pasta", "Sandwich", "Fish", "Eggs", "Salad", "Chocolate", "Children", "Apple" };

    private void Awake()
    {
        Agents = new List<GameObject>();
        People = new List<Character>();
    }

    // Start is called before the first frame update
    void Start()
    {
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

                if (character.isInState("Door", "Waiting for someone"))
                {
                    changeDoorState = true;
                }

                if (character.isInState("Bar", "Waiting for client"))
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

            barAttended = changeBarState;
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

    public Character GetCharacter(GameObject obj)
    {
        return People[Agents.IndexOf(obj)];
    }
}
