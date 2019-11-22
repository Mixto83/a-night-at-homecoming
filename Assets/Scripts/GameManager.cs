using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;

public enum Roles { CalmStudent, MessyStudent, Teacher, OrganizerStudent }
public enum Genders { Male, Female }

public class GameManager : MonoBehaviour
{
    protected readonly int MALENAMES_NUM = 7;
    protected readonly int FOODS_NUM = 10;
    protected readonly int ANIMALS_NUM = 12;
    protected readonly int HOBBIES_NUM = 12;
    protected readonly int MUSICS_NUM = 9;

    [SerializeField] bool paused = false;

    private bool forceDoorAttended = false;
    private bool doorAttended = false;
    private bool forceBarAttended = false;
    private bool barAttended = false;
    private volatile bool barBeingSabotaged = false;
    [HideInInspector] public string soundingMusic;
    private SimpleTimer musicTimer;
    public SimpleTimer sabotageAvailable;


    List<GameObject> Agents;
    List<Character> People;
    [SerializeField] bool debugMode;
    [SerializeField] GameObject door;
    [SerializeField] GameObject bar;

    public List<Sprite> bocadillos;

    string[] Names = new string[] { "Diego", "Mario", "Juan", "Cesar", "Daniel", "Pedro", "Manuel", "Maria", "Marta", "Carmen", "Raquel", "Lucia", "Ana", "Laura" };
    string[] Hobbies = new string[] { "Comic books", "Videogames", "Movies", "Books", "Cooking", "Board games", "Trading card games", "Sports", "School shooting", "Music", "Dancing", "Gym" };
    string[] Animals = new string[] { "Dog", "Cat", "Bird", "Bee", "Sheep", "Whale", "Possum", "Crocodile", "Bat", "Spider", "Lizard", "Turtle" };
    string[] Foods = new string[] { "Hamburgers", "Pizza", "Pasta", "Sandwich", "Fish", "Eggs", "Salad", "Chocolate", "Children", "Apple" };
    string[] Music = new string[] { "Rock", "Rap", "Classical", "Punk", "Techno", "Disco", "Romantic", "Pop", "Rumba" }; 

    [SerializeField] List<int> friendsGroups;
    List<Group> groups;
    List<Character> queue;

    [HideInInspector] public List<float> possiblePosGym;
    [HideInInspector] public List<float> possiblePosBench;
    [HideInInspector] public List<float> possiblePosOutside;

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
        possiblePosGym = new List<float> {
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

        possiblePosBench = new List<float> {-16f,7f,  -15f,7f,  -14f,7f,  -13f,7f,  -12f,7f,
                                            -16f,6f,  -15f,6f,  -14f,6f,  -13f,6f,  -12f,6f,
                                            -16f,5f,  -15f,5f,  -14f,5f,  -13f,5f,  -12f,5f,
                                            -16f,4f,  -15f,4f,  -14f,4f,  -13f,4f,  -12f,4f,
                                            -16f,3f,  -15f,3f,  -14f,3f,  -13f,3f,  -12f,3f,
                                            -16f,2f,  -15f,2f,  -14f,2f,  -13f,2f,  -12f,2f,
                                            -16f,1f,  -15f,1f,  -14f,1f,  -13f,1f,  -12f,1f,
                                            -16f,0f,  -15f,0f,  -14f,0f,  -13f,0f,  -12f,0f,
                                            -16f,-1f, -15f,-1f, -14f,-1f, -13f,-1f, -12f,-1f,
                                            -16f,-2f, -15f,-2f, -14f,-2f, -13f,-2f, -12f,-2f,
                                            -16f,-3f, -15f,-3f, -14f,-3f, -13f,-3f, -12f,-3f,
                                            -16f,-4f, -15f,-4f, -14f,-4f, -13f,-4f, -12f,-4f,
                                            -16f,-5f, -15f,-5f, -14f,-5f, -13f,-5f, -12f,-5f,
                                            -16f,-6f, -15f,-6f, -14f,-6f, -13f,-6f, -12f,-6f,
        };

        possiblePosOutside = new List<float> {-30f,7f,   -29f,7f,   -28f,7f,   -27f,7f,   -26f,7f,   -25f,7f,   -24f,7f,   -23f,7f,   -22f,7f,   -21f,7f,   -20f,7f,   -19f,7f,   -18f,7f,
                                              -30f,6f,   -29f,6f,   -28f,6f,   -27f,6f,   -26f,6f,   -25f,6f,   -24f,6f,   -23f,6f,   -22f,6f,   -21f,6f,   -20f,6f,   -19f,6f,   -18f,6f,
                                              -30f,5f,   -29f,5f,   -28f,5f,   -27f,5f,   -26f,5f,   -25f,5f,   -24f,5f,   -23f,5f,   -22f,5f,   -21f,5f,   -20f,5f,   -19f,5f,   -18f,5f,
                                              -30f,4f,   -29f,4f,   -28f,4f,   -27f,4f,   -26f,4f,   -25f,4f,   -24f,4f,   -23f,4f,   -22f,4f,   -21f,4f,   -20f,4f,   -19f,4f,   -18f,4f,
                                              -30f,3f,   -29f,3f,   -28f,3f,   -27f,3f,   -26f,3f,   -25f,3f,   -24f,3f,   -23f,3f,   -22f,3f,   -21f,3f,   -20f,3f,   -19f,3f,   -18f,3f,
                                              -30f,2f,   -29f,2f,   -28f,2f,   -27f,2f,   -26f,2f,   -25f,2f,   -24f,2f,   -23f,2f,   -22f,2f,   -21f,2f,   -20f,2f,   -19f,2f,   -18f,2f,
                                              -30f,1f,   -29f,1f,   -28f,1f,   -27f,1f,   -26f,1f,   -25f,1f,   -24f,1f,   -23f,1f,   -22f,1f,   -21f,1f,   -20f,1f,   -19f,1f,   -18f,1f,
                                              -30f,0f,   -29f,0f,   -28f,0f,   -27f,0f,   -26f,0f,   -25f,0f,   -24f,0f,   -23f,0f,   -22f,0f,   -21f,0f,   -20f,0f,   -19f,0f,   -18f,0f,
                                              -30f,-1f,  -29f,-1f,  -28f,-1f,  -27f,-1f,  -26f,-1f,  -25f,-1f,  -24f,-1f,  -23f,-1f,  -22f,-1f,  -21f,-1f,  -20f,-1f,  -19f,-1f,  -18f,-1f,
                                              -30f,-2f,  -29f,-2f,  -28f,-2f,  -27f,-2f,  -26f,-2f,  -25f,-2f,  -24f,-2f,  -23f,-2f,  -22f,-2f,  -21f,-2f,  -20f,-2f,  -19f,-2f,  -18f,-2f,
                                              -30f,-3f,  -29f,-3f,  -28f,-3f,  -27f,-3f,  -26f,-3f,  -25f,-3f,  -24f,-3f,  -23f,-3f,  -22f,-3f,  -21f,-3f,  -20f,-3f,  -19f,-3f,  -18f,-3f,
                                              -30f,-4f,  -29f,-4f,  -28f,-4f,  -27f,-4f,  -26f,-4f,  -25f,-4f,  -24f,-4f,  -23f,-4f,  -22f,-4f,  -21f,-4f,  -20f,-4f,  -19f,-4f,  -18f,-4f,
                                              -30f,-5f,  -29f,-5f,  -28f,-5f,  -27f,-5f,  -26f,-5f,  -25f,-5f,  -24f,-5f,  -23f,-5f,  -22f,-5f,  -21f,-5f,  -20f,-5f,  -19f,-5f,  -18f,-5f,
                                              -30f,-6f,  -29f,-6f,  -28f,-6f,  -27f,-6f,  -26f,-6f,  -25f,-6f,  -24f,-6f,  -23f,-6f,  -22f,-6f,  -21f,-6f,  -20f,-6f,  -19f,-6f,  -18f,-6f,
                                              -30f,-7f,  -29f,-7f,  -28f,-7f,  -27f,-7f,  -26f,-7f,  -25f,-7f,  -24f,-7f,  -23f,-7f,  -22f,-7f,  -21f,-7f,  -20f,-7f,  -19f,-7f,  -18f,-7f,
                                              -30f,-8f,  -29f,-8f,  -28f,-8f,  -27f,-8f,  -26f,-8f,  -25f,-8f,  -24f,-8f,  -23f,-8f,  -22f,-8f,  -21f,-8f,  -20f,-8f,  -19f,-8f,  -18f,-8f,  -17f,-8f,  -16f,-8f,  -15f,-8f,  -14f,-8f,  -13f,-8f,  -12f,-8f,  -11f,-8f,  -10f,-8f,  -9f,-8f,  -8f,-8f,  -7f,-8f,  -6f,-8f,  -5f,-8f,  -4f,-8f,  -3f,-8f,  -2f,-8f,                  /*ENTRADA*/                   4f,-8f,  5f,-8f,  6f,-8f,  7f,-8f,  8f,-8f,  9f,-8f,  10f,-8f,  11f,-8f,  12f,-8f,  13f,-8f,  14f,-8f,  15f,-8f,  16f,-8f,  17f,-8f,  18f,-8f,
                                              -30f,-9f,  -29f,-9f,  -28f,-9f,  -27f,-9f,  -26f,-9f,  -25f,-9f,  -24f,-9f,  -23f,-9f,  -22f,-9f,  -21f,-9f,  -20f,-9f,  -19f,-9f,  -18f,-9f,  -17f,-9f,  -16f,-9f,  -15f,-9f,  -14f,-9f,  -13f,-9f,  -12f,-9f,  -11f,-9f,  -10f,-9f,  -9f,-9f,  -8f,-9f,  -7f,-9f,  -6f,-9f,  -5f,-9f,  -4f,-9f,  -3f,-9f,  -2f,-9f,                  /*ENTRADA*/                   4f,-9f,  5f,-9f,  6f,-9f,  7f,-9f,  8f,-9f,  9f,-9f,  10f,-9f,  11f,-9f,  12f,-9f,  13f,-9f,  14f,-9f,  15f,-9f,  16f,-9f,  17f,-9f,  18f,-9f,
                                              -30f,-10f, -29f,-10f, -28f,-10f, -27f,-10f, -26f,-10f, -25f,-10f, -24f,-10f, -23f,-10f, -22f,-10f, -21f,-10f, -20f,-10f, -19f,-10f, -18f,-10f, -17f,-10f, -16f,-10f, -15f,-10f, -14f,-10f, -13f,-10f, -12f,-10f, -11f,-10f, -10f,-10f, -9f,-10f, -8f,-10f, -7f,-10f, -6f,-10f, -5f,-10f, -4f,-10f, -3f,-10f, -2f,-10f,                 /*ENTRADA*/                   4f,-10f, 5f,-10f, 6f,-10f, 7f,-10f, 8f,-10f, 9f,-10f, 10f,-10f, 11f,-10f, 12f,-10f, 13f,-10f, 14f,-10f, 15f,-10f, 16f,-10f, 17f,-10f, 18f,-10f,
                                              -30f,-11f, -29f,-11f, -28f,-11f, -27f,-11f, -26f,-11f, -25f,-11f, -24f,-11f, -23f,-11f, -22f,-11f, -21f,-11f, -20f,-11f, -19f,-11f, -18f,-11f, -17f,-11f, -16f,-11f, -15f,-11f, -14f,-11f, -13f,-11f, -12f,-11f, -11f,-11f, -10f,-11f, -9f,-11f, -8f,-11f, -7f,-11f, -6f,-11f, -5f,-11f, -4f,-11f, -3f,-11f, -2f,-11f,                 /*ENTRADA*/                   4f,-11f, 5f,-11f, 6f,-11f, 7f,-11f, 8f,-11f, 9f,-11f, 10f,-11f, 11f,-11f, 12f,-11f, 13f,-11f, 14f,-11f, 15f,-11f, 16f,-11f, 17f,-11f, 18f,-11f,
                                              -30f,-12f, -29f,-12f, -28f,-12f, -27f,-12f, -26f,-12f, -25f,-12f, -24f,-12f, -23f,-12f, -22f,-12f, -21f,-12f, -20f,-12f, -19f,-12f, -18f,-12f, -17f,-12f, -16f,-12f, -15f,-12f, -14f,-12f, -13f,-12f, -12f,-12f, -11f,-12f, -10f,-12f, -9f,-12f, -8f,-12f, -7f,-12f, -6f,-12f, -5f,-12f, -4f,-12f, -3f,-12f, -2f,-12f,                 /*ENTRADA*/                   4f,-12f, 5f,-12f, 6f,-12f, 7f,-12f, 8f,-12f, 9f,-12f, 10f,-12f, 11f,-12f, 12f,-12f, 13f,-12f, 14f,-12f, 15f,-12f, 16f,-12f, 17f,-12f, 18f,-12f,
                                              -30f,-13f, -29f,-13f, -28f,-13f, -27f,-13f, -26f,-13f, -25f,-13f, -24f,-13f, -23f,-13f, -22f,-13f, -21f,-13f, -20f,-13f, -19f,-13f, -18f,-13f, -17f,-13f, -16f,-13f, -15f,-13f, -14f,-13f, -13f,-13f, -12f,-13f, -11f,-13f, -10f,-13f, -9f,-13f, -8f,-13f, -7f,-13f, -6f,-13f, -5f,-13f, -4f,-13f, -3f,-13f, -2f,-13f,                 /*ENTRADA*/                   4f,-13f, 5f,-13f, 6f,-13f, 7f,-13f, 8f,-13f, 9f,-13f, 10f,-13f, 11f,-13f, 12f,-13f, 13f,-13f, 14f,-13f, 15f,-13f, 16f,-13f, 17f,-13f, 18f,-13f,
                                              -30f,-14f, -29f,-14f, -28f,-14f, -27f,-14f, -26f,-14f, -25f,-14f, -24f,-14f, -23f,-14f, -22f,-14f, -21f,-14f, -20f,-14f, -19f,-14f, -18f,-14f, -17f,-14f, -16f,-14f, -15f,-14f, -14f,-14f, -13f,-14f, -12f,-14f, -11f,-14f, -10f,-14f, -9f,-14f, -8f,-14f, -7f,-14f, -6f,-14f, -5f,-14f, -4f,-14f, -3f,-14f, -2f,-14f, -1f,-14f, 0f,-14f, 1f,-14f, 2f,-14f, 3f,-14f, 4f,-14f, 5f,-14f, 6f,-14f, 7f,-14f, 8f,-14f, 9f,-14f, 10f,-14f, 11f,-14f, 12f,-14f, 13f,-14f, 14f,-14f, 15f,-14f, 16f,-14f, 17f,-14f, 18f,-14f,
                                              -30f,-15f, -29f,-15f, -28f,-15f, -27f,-15f, -26f,-15f, -25f,-15f, -24f,-15f, -23f,-15f, -22f,-15f, -21f,-15f, -20f,-15f, -19f,-15f, -18f,-15f, -17f,-15f, -16f,-15f, -15f,-15f, -14f,-15f, -13f,-15f, -12f,-15f, -11f,-15f, -10f,-15f, -9f,-15f, -8f,-15f, -7f,-15f, -6f,-15f, -5f,-15f, -4f,-15f, -3f,-15f, -2f,-15f, -1f,-15f, 0f,-15f, 1f,-15f, 2f,-15f, 3f,-15f, 4f,-15f, 5f,-15f, 6f,-15f, 7f,-15f, 8f,-15f, 9f,-15f, 10f,-15f, 11f,-15f, 12f,-15f, 13f,-15f, 14f,-15f, 15f,-15f, 16f,-15f, 17f,-15f, 18f,-15f,
                                              -30f,-16f, -29f,-16f, -28f,-16f, -27f,-16f, -26f,-16f, -25f,-16f, -24f,-16f, -23f,-16f, -22f,-16f, -21f,-16f, -20f,-16f, -19f,-16f, -18f,-16f, -17f,-16f, -16f,-16f, -15f,-16f, -14f,-16f, -13f,-16f, -12f,-16f, -11f,-16f, -10f,-16f, -9f,-16f, -8f,-16f, -7f,-16f, -6f,-16f, -5f,-16f, -4f,-16f, -3f,-16f, -2f,-16f, -1f,-16f, 0f,-16f, 1f,-16f, 2f,-16f, 3f,-16f, 4f,-16f, 5f,-16f, 6f,-16f, 7f,-16f, 8f,-16f, 9f,-16f, 10f,-16f, 11f,-16f, 12f,-16f, 13f,-16f, 14f,-16f, 15f,-16f, 16f,-16f, 17f,-16f, 18f,-16f,
                                              -30f,-17f, -29f,-17f, -28f,-17f, -27f,-17f, -26f,-17f, -25f,-17f, -24f,-17f, -23f,-17f, -22f,-17f, -21f,-17f, -20f,-17f, -19f,-17f, -18f,-17f, -17f,-17f, -16f,-17f, -15f,-17f, -14f,-17f, -13f,-17f, -12f,-17f, -11f,-17f, -10f,-17f, -9f,-17f, -8f,-17f, -7f,-17f, -6f,-17f, -5f,-17f, -4f,-17f, -3f,-17f, -2f,-17f, -1f,-17f, 0f,-17f, 1f,-17f, 2f,-17f, 3f,-17f, 4f,-17f, 5f,-17f, 6f,-17f, 7f,-17f, 8f,-17f, 9f,-17f, 10f,-17f, 11f,-17f, 12f,-17f, 13f,-17f, 14f,-17f, 15f,-17f, 16f,-17f, 17f,-17f, 18f,-17f,
                                              -30f,-18f, -29f,-18f, -28f,-18f, -27f,-18f, -26f,-18f, -25f,-18f, -24f,-18f, -23f,-18f, -22f,-18f, -21f,-18f, -20f,-18f, -19f,-18f, -18f,-18f, -17f,-18f, -16f,-18f, -15f,-18f, -14f,-18f, -13f,-18f, -12f,-18f, -11f,-18f, -10f,-18f, -9f,-18f, -8f,-18f, -7f,-18f, -6f,-18f, -5f,-18f, -4f,-18f, -3f,-18f, -2f,-18f, -1f,-18f, 0f,-18f, 1f,-18f, 2f,-18f, 3f,-18f, 4f,-18f, 5f,-18f, 6f,-18f, 7f,-18f, 8f,-18f, 9f,-18f, 10f,-18f, 11f,-18f, 12f,-18f, 13f,-18f, 14f,-18f, 15f,-18f, 16f,-18f, 17f,-18f, 18f,-18f,
                                              -30f,-19f, -29f,-19f, -28f,-19f, -27f,-19f, -26f,-19f, -25f,-19f, -24f,-19f, -23f,-19f, -22f,-19f, -21f,-19f, -20f,-19f, -19f,-19f, -18f,-19f, -17f,-19f, -16f,-19f, -15f,-19f, -14f,-19f, -13f,-19f, -12f,-19f, -11f,-19f, -10f,-19f, -9f,-19f, -8f,-19f, -7f,-19f, -6f,-19f, -5f,-19f, -4f,-19f, -3f,-19f, -2f,-19f, -1f,-19f, 0f,-19f, 1f,-19f, 2f,-19f, 3f,-19f, 4f,-19f, 5f,-19f, 6f,-19f, 7f,-19f, 8f,-19f, 9f,-19f, 10f,-19f, 11f,-19f, 12f,-19f, 13f,-19f, 14f,-19f, 15f,-19f, 16f,-19f, 17f,-19f, 18f,-19f,
        };

        foreach (int i in friendsGroups)
        {
            var index = Random.Range(0, possiblePosGym.Count / 2 - 1) * 2;
            var randomCoordinates = possiblePosGym.GetRange(index, 2);
            possiblePosGym.RemoveRange(index, 2);
            groups.Add(new Group(i, randomCoordinates));
        }

        foreach (Transform child in transform)
        {
            Agents.Add(child.gameObject);

            Genders gender = (Genders) Random.Range(0, 2);

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
                    newStudent.sexuality = (Genders)Random.Range(0, 2);
                    newStudent.beauty = Random.Range(1, 11);
                    newStudent.beautyThreshold = Random.Range(1, 11);
                    newStudent.affinityThreshold = Random.Range(1, 11);
                    newStudent.DanceAffinityThreshold = Random.Range(1, 11);

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
                    for(int i = 0; i < Music.Length - 1; i++)
                    {
                        string newMusic = Music[Random.Range(0, MUSICS_NUM)];
                        newStudent.musicLikes.Add(newMusic);
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

        changeMusic();
        musicTimer = new SimpleTimer(10);
        sabotageAvailable = new SimpleTimer(10);//No empieza de primeras
        musicTimer.start();
    }

    // Update is called once per frame
    void Update()
    {
        if (!paused)
        {
            var changeDoorState = false;
            var changeBarState = false;
            var gameStateDesc = "";

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

                gameStateDesc += character.Description();

                character.animationUpdate();
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

            musicTimer.Update();
            if (musicTimer.isFinished())
            {
                changeMusic();
                musicTimer.reset();
            }

            sabotageAvailable.Update();
            if (sabotageAvailable.isFinished()){
                barBeingSabotaged = false;
            }

            gameStateDesc += "MUSIC: " + soundingMusic;

            if (debugMode)
            {
                createMessageOnGUI(gameStateDesc, Color.blue);
            } else
            {
                clearTexts();
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

    public bool getBarSabotaged()
    {
        return barBeingSabotaged;
    }

    public void setBarSabotaged(bool bar)
    {
        barBeingSabotaged = bar;
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

    public void changeMusic()
    {
        soundingMusic = Music[Random.Range(0, MUSICS_NUM)];
        //createMessageOnGUI(soundingMusic, Color.blue);
    }

    public Character GetCharacter(GameObject obj)
    {
        return People[Agents.IndexOf(obj)];
    }

    public List<Character> GetPeople()
    {
        return People;
    }

    private void createMessageOnGUI(string text, Color color)
    {
        clearTexts();

        if (color == null)
        {
            color = Color.green;
        }
        if (text == null)
        {
            text = "";
        }

        GameObject newText = new GameObject(text.Replace(" ", "-"), typeof(RectTransform));
        var newTextComp = newText.AddComponent<Text>();
        if(newText.GetComponent<CanvasRenderer>() == null) newText.AddComponent<CanvasRenderer>();

        newTextComp.text = text;
        newTextComp.color = color;
        newTextComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        newTextComp.alignment = TextAnchor.UpperRight;
        newTextComp.fontSize = 10;

        newText.transform.SetParent(GameObject.FindGameObjectWithTag("CanvasPrincipal").transform);

        newText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 700);
        newText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 350);
        newText.transform.localPosition = new Vector3(0, 0, 0);
    }

    private void clearTexts()
    {
        foreach (Text txt in GameObject.FindGameObjectWithTag("CanvasPrincipal").GetComponentsInChildren<Text>())
        {
            GameObject.Destroy(txt.gameObject);
        }
    }
}
