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
    protected Vector3 position;
    protected GameObject gameObject;
    protected Vector3 initPos;
    protected NavMeshAgent agent;
    protected float thirst;

    protected const int thirstThreshold = 5;

    protected bool greetedAtDoor = false;
    private bool canBeServed = false;

    private Quaternion fixedRotation;
    Vector3 lookAt;
    bool calledLookAt = false;

    protected GameManager gameState;

    //methods
    protected Character(string name, Genders gender, Transform obj, GameManager gameState)
    {
        this.initPos = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5));
        this.fixedRotation = obj.rotation;

        this.name = name;
        this.gender = gender;
        this.position = obj.position;
        this.gameObject = obj.gameObject;
        this.agent = this.gameObject.GetComponent<NavMeshAgent>();
        this.agent.updateRotation = false;
        this.gameState = gameState;

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

    public Vector3 getPos()
    {
        return this.position;
    }

    public Roles getRole()
    {
        return this.role;
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

    public virtual bool isInState(string state)
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

    protected void WalkingToBar()
    {
        Debug.Log("[" + name + ", " + getRole() + "] Walking to bar");
        Vector3 barPos = new Vector3(0, 1 - gameState.getBarQueue(this));
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
        gameState.reduceBarQueue(this);
    }

    protected void createMessage(string text, Color color)
    {
        clearTexts(this);

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
        newText.AddComponent<CanvasRenderer>();

        newTextComp.text = text;
        newTextComp.color = color;
        newTextComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        newTextComp.alignment = TextAnchor.MiddleCenter;
        newTextComp.fontSize = 30;

        newText.transform.SetParent(this.gameObject.GetComponentInChildren<Canvas>().transform);

        newText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
        newText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);
        newText.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        newText.transform.localPosition = new Vector3(0, 1, 0);
    }

    protected void clearTexts(Character c)
    {
        foreach (Text txt in c.gameObject.GetComponentsInChildren<Text>())
        {
            GameObject.Destroy(txt.gameObject);
        }
    }
}
