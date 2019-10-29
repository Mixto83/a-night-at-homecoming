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

    [SerializeField] List<GameObject> Agents;
    List<Character> People;
    public bool debugMode = true;

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
                    CalmStudent newStudent = new CalmStudent(name, gender, child.position);             
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
                    MessyStudent newStudent2 = new MessyStudent(name, gender, child.position);
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
                    People.Add(new Teacher(name, gender, child.position));
                    break;
                case "OrgStudent":
                    People.Add(new OrganizerStudent(name, gender, child.position));
                    break;
            }
        }

        foreach (Character character in People.FindAll(x => x.getRole() == Roles.CalmStudent)) character.Flirt();
        foreach (Character character in People.FindAll(x => x.getRole() == Roles.MessyStudent)) character.Trouble();
        foreach (Character character in People.FindAll(x => x.getRole() == Roles.Teacher)) character.Patrol();
        foreach (Character character in People.FindAll(x => x.getRole() == Roles.OrganizerStudent)) character.Enjoying();
    }

    // Update is called once per frame
    void Update()
    {
        if (debugMode)
        {
            debugKeyboard();
        }

        if (Input.anyKeyDown)
        {
            foreach (Character character in People) character.FSM();
        }
    }

    //Only available in Debug Mode
    void debugKeyboard()
    {
        //General Student FSM
        if (Input.GetKeyDown(KeyCode.Z))
        {
            foreach (MessyStudent student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentState = MessyStudent.messStates.start;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            foreach (MessyStudent student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentState = MessyStudent.messStates.rest;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            foreach (MessyStudent student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentState = MessyStudent.messStates.drink;
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            foreach (MessyStudent student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentState = MessyStudent.messStates.breath;
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            foreach (MessyStudent student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentState = MessyStudent.messStates.punishment;
        }

        //Drink HFSM
        if (Input.GetKeyDown(KeyCode.A))
        {
            foreach (Student student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentDrink = Student.drinkStates.walkToBar;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            foreach (Student student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentDrink = Student.drinkStates.waitQueue;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            foreach (Student student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentDrink = Student.drinkStates.drinking;
        }

        //Rest HFSM
        if (Input.GetKeyDown(KeyCode.F))
        {
            foreach (Student student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentRest = Student.restStates.walkToBenches;
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            foreach (Student student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentRest = Student.restStates.satInBench;
        }

        //Breath HFSM
        if (Input.GetKeyDown(KeyCode.H))
        {
            foreach (Student student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentBreath = Student.breathStates.walkOutside;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            foreach (Student student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentBreath = Student.breathStates.stayOutside;
        }

        //Punishment HFSM
        if (Input.GetKeyDown(KeyCode.K))
        {
            foreach (Student student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentPunishment = Student.punishmentStates.waitEndOfPunishment;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            foreach (Student student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentPunishment = Student.punishmentStates.scapeFromPunishment;
        }


        //Behaviour HFSM (Messy, Calm, Organizer)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            foreach (MessyStudent student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentState = MessyStudent.messStates.lookForMess;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            foreach (MessyStudent student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentState = MessyStudent.messStates.sabotageDrink;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (MessyStudent student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentState = MessyStudent.messStates.negotiateOrganizer;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (MessyStudent student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentState = MessyStudent.messStates.runAway;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            foreach (MessyStudent student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentState = MessyStudent.messStates.botherTeacher;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            foreach (MessyStudent student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentState = MessyStudent.messStates.checkAffinity;
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            foreach (MessyStudent student in People.FindAll(x => x.getRole() == Roles.MessyStudent)) student.currentState = MessyStudent.messStates.fightStudent;
        }
    }
}
