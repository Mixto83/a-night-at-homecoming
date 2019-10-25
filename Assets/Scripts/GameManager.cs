using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Roles { CalmStudent, MessyStudent, Teacher, OrganizerStudent }
public enum Genders { Male, Female, Other }

public class GameManager : MonoBehaviour
{
    [SerializeField] List<GameObject> Agents;
    List<Character> People;

    string[] Names = new string[] { "Diego", "Mario", "Juan", "Cesar", "Daniel", "Pedro", "Manuel", "Maria", "Marta", "Carmen", "Raquel", "Lucia", "Ana", "Laura" };

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
                    name = Names[Random.Range(0, 7)];
                    break;
                case Genders.Female:
                    name = Names[Random.Range(7, Names.Length)];
                    break;
                default:
                    name = Names[Random.Range(0, Names.Length)];
                    break;
            }

            switch (child.tag)
            {
                case "CalmStudent":
                    People.Add(new CalmStudent(name, gender, child.position));
                    break;
                case "MessyStudent":
                    People.Add(new MessyStudent(name, gender, child.position));
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
        
    }
}
