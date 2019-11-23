using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Character
{
    //parameters
    protected StateMachineEngine drinkSubFSM;

    protected Perception isInStateDrink;
    protected Perception timeOut;

    protected float distanceToBar;

    protected string name;
    protected Roles role;
    protected Genders gender;
    protected Vector3 destination;
    protected GameObject gameObject;
    protected Vector3 initPos;
    protected NavMeshAgent agent;
    protected float thirst;
    protected List<float> currentOcuppiedPos;
    protected List<float> currentOcuppiedBench;
    protected List<float> currentOcuppiedOutside;
    protected List<float> currentOcuppiedBathroom;
    protected List<float> currentOcuppiedPunishment;
    public int beauty;

    protected const int thirstThreshold = 5;

    protected bool greetedAtDoor = false;
    private bool canBeServed = false;

    private Quaternion fixedRotation;
    Vector3 lookAt;
    bool calledLookAt = false;

    protected GameManager gameState;

    protected Animator animationController;

    //methods
    protected Character(string name, Genders gender, Transform obj, GameManager gameState)
    {
        this.gameState = gameState;
        var index = Random.Range(0, this.gameState.possiblePosGym.Count / 2 - 1) * 2;
        var randomCoordinates = this.gameState.possiblePosGym.GetRange(index, 2);
        this.gameState.possiblePosGym.RemoveRange(index, 2);
        this.initPos = new Vector3(randomCoordinates[0], randomCoordinates[1]);
        this.fixedRotation = obj.rotation;

        this.name = name;
        this.gender = gender;
        this.destination = obj.position;
        this.gameObject = obj.gameObject;
        this.agent = this.gameObject.GetComponent<NavMeshAgent>();
        this.agent.updateRotation = false;

        CreateDrinkSubStateMachine();
    }

    private void CreateDrinkSubStateMachine()
    {
        drinkSubFSM = new StateMachineEngine(true);

        // Perceptions
        Perception gotToBar = drinkSubFSM.CreatePerception<ValuePerception>(() => distanceToBar < 0.3f);
        Perception startDrinking = drinkSubFSM.CreatePerception<PushPerception>();

        // States
        State walkingToBarState = drinkSubFSM.CreateEntryState("WalkingToBar", WalkingToBar);
        State waitingQueueState = drinkSubFSM.CreateState("WaitingQueue", WaitingQueue);
        State drinkState = drinkSubFSM.CreateState("Drink", Drinking);

        // Transitions
        drinkSubFSM.CreateTransition("Join queue", walkingToBarState, gotToBar, waitingQueueState);
        drinkSubFSM.CreateTransition("Served", waitingQueueState, startDrinking, drinkState);
    }

    public string getName()
    {
        return this.name;
    }

    public Vector3 getDestination()
    {
        return this.destination;
    }

    public Roles getRole()
    {
        return this.role;
    }

    public Genders getGender()
    {
        return gender;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public bool getGreeted()
    {
        return greetedAtDoor;
    }

    public void setGreeted(bool greeted)
    {
        greetedAtDoor = greeted;
    }

    public bool getCanBeServed()
    {
        return canBeServed;
    }

    public void setServed()
    {
        canBeServed = false;
        drinkSubFSM.Fire("Served");
    }

    public void Move(Vector3 to)
    {
        destination = to;
        agent.SetDestination(to);
    }

    public void LookAt(Transform target)
    {
        Vector3 targetPos = new Vector3(target.position.x, target.position.y);
        lookAt = (targetPos - this.gameObject.transform.position).normalized;
        calledLookAt = true;
    }

    public void RotationUpdate()
    {
        Quaternion rot;

        if (agent.velocity.sqrMagnitude > Mathf.Epsilon)
        {
            rot = Quaternion.LookRotation(agent.velocity.normalized, Vector3.back);
        } else
        {
            if (calledLookAt) {
                rot = Quaternion.LookRotation(lookAt, Vector3.back);
            } else
            {
                rot = this.gameObject.transform.rotation;
            }

            calledLookAt = false;
        }

        this.gameObject.transform.rotation = rot;
    }

    public void lockCanvasRotation()
    {
        this.gameObject.GetComponentInChildren<Canvas>().gameObject.transform.rotation = fixedRotation;
    }

    public virtual bool isInState(params string[] states)
    {
        return false;
    }

    public virtual void setGroup(Group group)
    {

    }

    //Common behaviours to be overridden
    public virtual void CreateStateMachine()
    {

    }

    public virtual void Update()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Behaviour not defined");
    }

    public virtual void Flirt() {
        Debug.Log("[" + name + ", " + getRole() + "] Behaviour not defined");
    }

    public virtual void LookForTrouble() {
        Debug.Log("[" + name + ", " + getRole() + "] Behaviour not defined");
    }

    protected void Walking(string tag, Vector3 offset)
    {
        Debug.Log("[" + name + ", " + getRole() + "] Walking to " + tag);

        this.Move(GameObject.FindGameObjectWithTag(tag).transform.position + offset);

        clearSprites();
    }

    protected void WalkingToBar()
    {
        if (currentOcuppiedBench != null) this.gameState.possiblePosBench.AddRange(currentOcuppiedBench);
        createMessage(6);
        Debug.Log("[" + name + ", " + getRole() + "] Walking to bar");
        Vector3 barPos = new Vector3(GameObject.FindGameObjectWithTag("Bar").transform.position.x - 0.75f - gameState.getBarQueue(this), GameObject.FindGameObjectWithTag("Bar").transform.position.y);
        Move(barPos);
    }

    protected void WaitingQueue()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Waiting at queue");
        canBeServed = true;
    }

    protected void Drinking()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Drinking!");
        this.thirst = 0.0f;
        gameState.reduceBarQueue(this);
    }

    protected void createMessage(int index)
    {
        /*
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
        if (newText.GetComponent<CanvasRenderer>() == null) newText.AddComponent<CanvasRenderer>();

        newTextComp.text = text;
        newTextComp.color = color;
        newTextComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        newTextComp.alignment = TextAnchor.MiddleCenter;
        newTextComp.fontSize = 30;

        newText.transform.SetParent(this.gameObject.GetComponentInChildren<Canvas>().transform);

        newText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
        newText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);
        newText.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        newText.transform.localPosition = new Vector3(0, 1, 0);*/

        clearSprites();

        GameObject newSprite = new GameObject(gameState.bocadillos[index].name);
        var newSpriteComp = newSprite.AddComponent<SpriteRenderer>();
        if (newSprite.GetComponent<CanvasRenderer>() == null) newSprite.AddComponent<CanvasRenderer>();

        newSpriteComp.sprite = gameState.bocadillos[index];

        newSprite.transform.SetParent(this.gameObject.GetComponentInChildren<Canvas>().transform);
        newSprite.transform.localPosition = new Vector3(0, 1.5f, 0);
    }

    protected void clearSprites()
    {
        foreach (SpriteRenderer spr in this.gameObject.GetComponentInChildren<Canvas>().GetComponentsInChildren<SpriteRenderer>())
        {
            GameObject.Destroy(spr.gameObject);
        }
    }

    public virtual void FireFlirt(CalmStudent me)
    {

    }

    public virtual string Description()
    {
        return "\n";
    }

    public void animationUpdate()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            animationController.SetBool("isWalking", true);
        }
        else
        {
            animationController.SetBool("isWalking", false);
        }
    }

    public void thirstIncrement()
    {
        thirst++;
    }

    public virtual bool Fire(string transition)
    {
        return false;
    }
}
